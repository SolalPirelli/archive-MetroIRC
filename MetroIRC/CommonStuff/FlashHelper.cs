// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

// This seems a bit clunky. The supposed flash-till-foreground 12 flag doesn't work...

namespace CommonStuff
{
    public static class FlashHelper
    {
        /// <summary>
        /// Flashes the specified window once.
        /// </summary>
        public static void Flash( Window window )
        {
            NativeMethods.FlashWindow( window, NativeMethods.FLASHW_ALL, 1, (uint) 0 );
        }

        private static class NativeMethods
        {
            // Stop flashing. The system restores the window to its original state.
            public const int FLASHW_STOP = 0;
            // Flash the window caption.
            public const int FLASHW_CAPTION = 1;
            // Flash the taskbar button.
            public const int FLASHW_TRAY = 2;
            // Flash both the window caption and taskbar button.
            // This is equivalent to setting the FLASHW_CAPTION | FLASHW_TRAY flags.
            public const int FLASHW_ALL = 3;

            [StructLayout( LayoutKind.Sequential )]
            public struct FlashInfo
            {
                /// <summary>
                /// The size of the structure in bytes.
                /// </summary>
                public uint Size;
                /// <summary>
                /// A Handle to the Window to be Flashed. The window can be either opened or minimized.
                /// </summary>
                public IntPtr Handle;
                /// <summary>
                /// The Flash Status.
                /// </summary>
                public uint Flags;
                /// <summary>
                /// The number of times to Flash the window.
                /// </summary>
                public uint Count;
                /// <summary>
                /// The rate at which the Window is to be flashed, in milliseconds. If Zero, the function uses the default cursor blink rate.
                /// </summary>
                public uint Timeout;

                public FlashInfo( IntPtr handle, uint flags, uint count, uint timeout )
                {
                    this.Size = Convert.ToUInt32( Marshal.SizeOf( typeof( FlashInfo ) ) );
                    this.Handle = handle;
                    this.Flags = flags;
                    this.Count = count;
                    this.Timeout = timeout;
                }
            }

            public static void FlashWindow( Window window, uint flags, uint count, uint timeout )
            {
                var helper = new WindowInteropHelper( window );
                var info = new FlashInfo( helper.Handle, flags, count, timeout );
                FlashWindowEx( ref info );
            }

            [DllImport( "user32.dll" )]
            [return: MarshalAs( UnmanagedType.Bool )]
            internal static extern bool FlashWindowEx( ref FlashInfo info );
        }
    }
}