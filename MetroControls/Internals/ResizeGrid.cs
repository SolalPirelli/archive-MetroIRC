// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MetroControls.Internals
{
    internal sealed class ResizeGrid : Control
    {
        #region Window DependencyProperty
        public Window Window
        {
            get { return (Window) GetValue( WindowProperty ); }
            set { SetValue( WindowProperty, value ); }
        }

        public static readonly DependencyProperty WindowProperty =
            DependencyProperty.Register( "Window", typeof( Window ), typeof( ResizeGrid ), new UIPropertyMetadata() );
        #endregion

        #region ResizeCommand
        private RelayCommand _resizeCommand;
        public ICommand ResizeCommand
        {
            get
            {
                if ( this._resizeCommand == null )
                {
                    this._resizeCommand = new RelayCommand( ResizeCommandExecuted );
                }
                return this._resizeCommand;
            }
        }

        private void ResizeCommandExecuted( object parameter )
        {
            var info = (DragInfo) parameter;

            // So...we need to move the window if it was resized to the top or left, and resize the window anyway.
            // But we also need to prevent the width from becoming smaller than minwidth
            // The operation must not be canceled if the result would be smaller ; we only resize it until we can't.
            // And, to avoid a stutter effect, we have to use the SetWindowPos Win32 API that sets everything at the same time.

            int? newWidth = null, newHeight = null, newLeft = null, newTop = null;

            double horizontalDelta = info.IsLeft ? Math.Min( this.Window.Width - this.Window.MinWidth, info.HorizontalDelta ) : info.HorizontalDelta;
            if ( info.Direction.HasFlag( DragDirections.Horizontal ) )
            {
                newWidth = (int) Math.Max( this.Window.MinWidth, this.Window.Width + ( info.IsLeft ? -1 : 1 ) * horizontalDelta );

                if ( info.IsLeft )
                {
                    newLeft = (int) ( this.Window.Left + horizontalDelta );
                }
            }

            double verticalDelta = info.IsTop ? Math.Min( this.Window.Height - this.Window.MinHeight, info.VerticalDelta ) : info.VerticalDelta;
            if ( info.Direction.HasFlag( DragDirections.Vertical ) )
            {
                newHeight = (int) Math.Max( this.Window.MinHeight, this.Window.Height + ( info.IsTop ? -1 : 1 ) * verticalDelta );

                if ( info.IsTop )
                {
                    newTop = (int) ( this.Window.Top + verticalDelta );
                }
            }

            NativeMethods.SetWindowProperties( this.Window, newLeft, newTop, newWidth, newHeight );
        }
        #endregion
    }
}