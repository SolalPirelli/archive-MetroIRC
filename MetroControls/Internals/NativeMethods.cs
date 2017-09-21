// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace MetroControls.Internals
{
    internal static class NativeMethods
    {
        #region SetWindowProperties
        /// <summary>
        /// Sets a window's size and position properties without the weird effect that happens when you set the window's properties not at the same time.
        /// </summary>
        public static void SetWindowProperties( Window window, int? left = null, int? top = null, int? width = null, int? height = null )
        {
            var handle = new WindowInteropHelper( window ).Handle;
            MoveWindow( handle, left ?? (int) window.Left, top ?? (int) window.Top, width ?? (int) window.Width, height ?? (int) window.Height, true );
        }

        [DllImport( "user32" )]
        private static extern bool MoveWindow( IntPtr handle, int left, int top, int width, int height, bool repaint );
        #endregion

        #region DragMove
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        /// <summary>
        /// A better version of Window.DragMove that always work.
        /// </summary>
        public static void DragMove( Window window )
        {
            var handle = new WindowInteropHelper( window ).Handle;
            NativeMethods.ReleaseCapture();
            NativeMethods.SendMessage( handle, NativeMethods.WM_NCLBUTTONDOWN, NativeMethods.HT_CAPTION, 0 );
        }

        [DllImport( "user32.dll" )]
        public static extern int SendMessage( IntPtr handle, int message, int param1, int param2 );
        [DllImport( "user32.dll" )]
        public static extern bool ReleaseCapture();
        #endregion

        #region FixWindowMaxSize
        private const int WM_GETMINMAXINFO = 0x24;
        private const int MONITOR_DEFAULTTONEAREST = 0x2;

        [StructLayout( LayoutKind.Sequential )]
        private struct Win32Point
        {
            public readonly int X;
            public readonly int Y;

            public Win32Point( int x, int y )
            {
                this.X = x;
                this.Y = y;
            }
        }

        [StructLayout( LayoutKind.Sequential )]
        private struct MinMaxInfo
        {
            public readonly Win32Point Reserved;
            public Win32Point MaxSize;
            public Win32Point MaxPosition;
            public Win32Point MinTrackSize;
            public Win32Point MaxTrackSize;
        };

        [StructLayout( LayoutKind.Sequential )]
        private sealed class MonitorInfo
        {
            public readonly int Size = Marshal.SizeOf( typeof( MonitorInfo ) );
            public readonly Win32Rect Monitor;
            public readonly Win32Rect WorkArea;
            public readonly int Flags = 0;
        }

        [StructLayout( LayoutKind.Sequential )]
        private struct Win32Rect
        {
            public readonly int Left;
            public readonly int Top;
            public readonly int Right;
            public readonly int Bottom;
        }

        public static void FixWindowMaxSize( Window window )
        {
            IntPtr handle = ( new WindowInteropHelper( window ) ).Handle;
            HwndSource.FromHwnd( handle ).AddHook( WindowMinMaxInfoHook );
        }

        private static IntPtr WindowMinMaxInfoHook( IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled )
        {
            if ( msg == NativeMethods.WM_GETMINMAXINFO )
            {
                NativeMethods.WmGetMinMaxInfo( hwnd, lParam );
                handled = true;
            }

            return IntPtr.Zero;
        }

        private static void WmGetMinMaxInfo( IntPtr hwnd, IntPtr param )
        {
            // Adjust the maximized size and position to fit the work area of the correct monitor
            IntPtr monitor = MonitorFromWindow( hwnd, MONITOR_DEFAULTTONEAREST );

            var minMax = (MinMaxInfo) Marshal.PtrToStructure( param, typeof( MinMaxInfo ) );

            if ( monitor != IntPtr.Zero )
            {
                var monitorInfo = new MonitorInfo();
                GetMonitorInfo( monitor, monitorInfo );
                Win32Rect workArea = monitorInfo.WorkArea;
                Win32Rect monitorArea = monitorInfo.Monitor;
                minMax.MaxPosition = new Win32Point( workArea.Left - monitorArea.Left, workArea.Left - monitorArea.Left );
                minMax.MaxSize = new Win32Point( workArea.Right - workArea.Left, workArea.Bottom - workArea.Top );
            }

            Marshal.StructureToPtr( minMax, param, true );
        }

        [DllImport( "user32" )]
        private static extern bool GetMonitorInfo( IntPtr monitor, MonitorInfo info );

        [DllImport( "User32" )]
        private static extern IntPtr MonitorFromWindow( IntPtr handle, int flags );
        #endregion
    }
}