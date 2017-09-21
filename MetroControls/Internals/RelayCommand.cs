// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Windows.Input;

// Original code: "WPF Apps with the Model-View-ViewModel Pattern" by Josh Smith
// http://msdn.microsoft.com/en-us/magazine/dd419663.aspx

namespace MetroControls.Internals
{
    internal sealed class RelayCommand : ICommand
    {
        #region Fields
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;
        #endregion

        #region Constructors
        public RelayCommand( Action<object> execute )
            : this( execute, null )
        {
        }

        public RelayCommand( Action<object> execute, Predicate<object> canExecute )
        {
            if ( execute == null )
            {
                throw new ArgumentNullException( "execute" );
            }

            this._execute = execute;
            this._canExecute = canExecute;
        }
        #endregion

        #region ICommand Members
        public bool CanExecute( object parameter )
        {
            return this._canExecute == null ? true : this._canExecute( parameter );
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute( object parameter )
        {
            this._execute( parameter );
        }
        #endregion
    }
}