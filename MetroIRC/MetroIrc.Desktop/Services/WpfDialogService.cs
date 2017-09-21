// Copyright (C) 2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System.Collections.Generic;
using System.Windows;
using BasicMvvm.Dialogs;
using MetroIrc.Desktop.Views;
using Microsoft.Win32;

namespace MetroIrc.Desktop.Services
{
    public sealed class WpfDialogService : IDialogService
    {
        // the currently top-most dialog window, used as parent for new dialog windows
        private Window _topmostDialogWindow;

        /// <summary>
        /// Displays a viewmodel as a dialog.
        /// </summary>
        /// <returns>The returned value depends on the viewmodel.</returns>
        public bool? ShowDialog<TDialogViewModel>( TDialogViewModel viewModel )
            where TDialogViewModel : DialogViewModel
        {
            var window = new MetroDialogWindowView( viewModel );

            var oldTopmostDialogWindow = _topmostDialogWindow;
            if ( _topmostDialogWindow == null )
            {
                _topmostDialogWindow = App.Current.MainWindow; // can't do that in the static constructor, MainWindow might not be defined.
            }

            // TEMP
            if ( window != _topmostDialogWindow )
            {
                window.Owner = _topmostDialogWindow;
            }

            _topmostDialogWindow = window;
            bool? result = window.ShowDialog();
            _topmostDialogWindow = oldTopmostDialogWindow;
            return result;
        }

        /// <summary>
        /// Displays a message with a title, text and buttons.
        /// </summary>
        /// <returns>True for OK or Yes, False for Cancel or No, null if the user closed the window.</returns>
        public bool? ShowMessageBox( string title, string text, AvailableDialogButtons buttons )
        {
            return ShowDialog( new DialogBoxViewModel( title, text, buttons ) );
        }

        /// <summary>
        /// Prompts the user for input and returns the text the user entered.
        /// </summary>
        /// <param name="displayText">The text to be displayed in the input box.</param>
        public string ShowInputBox( string displayText )
        {
            var viewModel = new InputBoxViewModel( displayText );
            if ( ShowDialog( viewModel ) == true )
            {
                return viewModel.InputText;
            }
            return null;
        }

        /// <summary>
        /// Obtains a file whose name matches one of the specified filters.
        /// </summary>
        /// <returns>The file path.</returns>
        public string GetFile( IEnumerable<string> filters )
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = string.Join( ", ", filters ) + "|" + string.Join( ";", filters ); // no localization, just the filters
            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }
    }
}