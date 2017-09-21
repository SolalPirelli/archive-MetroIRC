// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MetroControls.Internals
{
    internal sealed class WindowControls : Control
    {
        #region Window DependencyProperty
        /// <summary>
        /// Gets or sets the window bound to the controls.
        /// </summary>
        public MetroWindow Window
        {
            get { return (MetroWindow) GetValue( WindowProperty ); }
            set { SetValue( WindowProperty, value ); }
        }

        /// <summary>
        /// See the <see cref="WindowControls.Window"/> property.
        /// </summary>
        public static readonly DependencyProperty WindowProperty =
            DependencyProperty.Register( "Window", typeof( MetroWindow ), typeof( WindowControls ), new UIPropertyMetadata() );
        #endregion

        #region CloseCommand
        /// <summary>
        /// Gets or sets the command invoked when the "Close" button is clicked.
        /// </summary>
        public ICommand CloseCommand
        {
            get { return new RelayCommand( CloseCommandExecuted ); }
        }

        private void CloseCommandExecuted( object parameter )
        {
            try
            {
                this.Window.DialogResult = null;
            }
            catch ( InvalidOperationException )
            {
                // the above code will throw an exception if the window isn't shown as a dialog
                // but we have to set the DialogResult to false for dialog windows
            }
            finally
            {
                this.Window.Close();
            }
        }
        #endregion

        #region MaximizeOrRestoreCommand
        /// <summary>
        /// Gets or sets the command invoked when the "Maximize/Restore" button is clicked.
        /// </summary>
        public ICommand MaximizeOrRestoreCommand
        {
            get { return new RelayCommand( MaximizeOrRestoreCommandExecuted ); }
        }

        private void MaximizeOrRestoreCommandExecuted( object parameter )
        {
            this.Window.WindowState = this.Window.WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
        }
        #endregion

        #region MinimizeCommand
        /// <summary>
        /// Gets or sets the command invoked when the "Minimize" button is clicked.
        /// </summary>
        public ICommand MinimizeCommand
        {
            get { return new RelayCommand( MinimizeCommandExecuted ); }
        }

        private void MinimizeCommandExecuted( object parameter )
        {
            this.Window.WindowState = WindowState.Minimized;
        }
        #endregion
    }
}