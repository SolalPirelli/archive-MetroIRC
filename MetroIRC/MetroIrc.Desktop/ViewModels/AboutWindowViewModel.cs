// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using BasicMvvm;
using BasicMvvm.Dialogs;
using MetroIrc.Services;

namespace MetroIrc.Desktop.ViewModels
{
    public sealed class AboutWindowViewModel : DialogViewModel
    {
        #region Property-backing fields
        private bool? _isUpdateAvailable = null;
        #endregion

        #region Private members
        // get it only once
        private Assembly _executingAssembly = Assembly.GetExecutingAssembly();
        #endregion

        #region Public properties
        public Version VersionNumber
        {
            get { return this._executingAssembly.GetName().Version; }
        }

        public string CopyrightText
        {
            get { return this._executingAssembly.GetCustomAttributes( false ).OfType<AssemblyCopyrightAttribute>().First().Copyright; }
        }

        public bool? IsUpdateAvailable
        {
            get { return this._isUpdateAvailable; }
            set { this.SetProperty( ref this._isUpdateAvailable, value ); }
        }
        #endregion

        #region Commands
        #region CheckForUpdateCommand
        private RelayCommand _checkForUpdateCommand;
        public ICommand CheckForUpdateCommand
        {
            get
            {
                if ( this._checkForUpdateCommand == null )
                {
                    this._checkForUpdateCommand = new RelayCommand( CheckForUpdateCommandExecuted, CanExecuteCheckForUpdateCommand );
                    this._checkForUpdateCommand.BindConditionToProperty( this, o => o.IsUpdateAvailable );
                }
                return this._checkForUpdateCommand;
            }
        }

        private void CheckForUpdateCommandExecuted( object parameter )
        {
            this._isUpdateAvailable = UpdateChecker.CheckForUpdate( true );
            if ( this._isUpdateAvailable == true )
            {
                Locator.Get<IDialogService>().ShowDialog( new UpdateWindowViewModel() );
            }
        }

        private bool CanExecuteCheckForUpdateCommand( object parameter )
        {
            return this.IsUpdateAvailable != false;
        }
        #endregion

        #region NavigateCommand
        private RelayCommand _navigateCommand;
        public ICommand NavigateCommand
        {
            get
            {
                if ( this._navigateCommand == null )
                {
                    this._navigateCommand = new RelayCommand( NavigateCommandExecuted );
                }
                return this._navigateCommand;
            }
        }

        private void NavigateCommandExecuted( object parameter )
        {
            Process.Start( new ProcessStartInfo( parameter.ToString() ) );
        }
        #endregion
        #endregion

        #region DialogViewModel overrides
        public override string Title
        {
            get { return Locator.Get<ITranslationService>().Translate( "AboutWindow", "Title" ); }
        }

        public override object Icon
        {
            get { return Locator.Get<IResourceService>().GetResource( "AboutWindowIcon" ); }
        }
        #endregion
    }
}