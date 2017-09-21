using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using BasicMvvm;

namespace MetroIrc.Desktop.Services
{
    public sealed class WpfUIService : IUIService
    {
        public void Execute( Action action )
        {
            if ( Application.Current == null )
            {
                return; // when exiting the app...
            }

            Application.Current.Dispatcher.Invoke( action );
        }

        public T Execute<T>( Func<T> func )
        {
            if ( Application.Current == null )
            {
                return default( T ); // when exiting the app...
            }

            return Application.Current.Dispatcher.Invoke( func );
        }

        /// <summary>
        /// Returns a value indicating whether the current application is on the foreground.
        /// </summary>
        public bool IsAppToForeground()
        {
            return App.Current.Dispatcher.Invoke( () => NativeMethods.GetForegroundWindow() == new WindowInteropHelper( App.Current.MainWindow ).Handle );
        }

        private static class NativeMethods
        {
            [DllImport( "user32" )]
            public static extern IntPtr GetForegroundWindow();
        }
    }
}