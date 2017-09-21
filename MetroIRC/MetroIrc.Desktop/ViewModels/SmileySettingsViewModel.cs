// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System.IO;
using System.Linq;
using System.Windows.Input;
using BasicMvvm;
using BasicMvvm.Dialogs;
using MetroIrc.Desktop.Services;
using MetroIrc.Services;

namespace MetroIrc.Desktop.ViewModels
{
    public sealed class SmileySettingsViewModel : ObservableObject
    {
        #region Property-backing fields
        private Smiley _selectedSmiley;
        #endregion

        #region Properties
        public Smiley SelectedSmiley
        {
            get { return this._selectedSmiley; }
            set { this.SetProperty( ref this._selectedSmiley, value ); }
        }
        #endregion

        #region Commands
        #region AddPackCommand
        public ICommand AddPackCommand
        {
            get { return new RelayCommand( AddPackCommandExecuted ); }
        }

        private void AddPackCommandExecuted( object parameter )
        {
            string packName = GetNewPackName( "NewPackName" );
            if ( packName == null )
            {
                return;
            }

            SmileyManager.CurrentPack = SmileyManager.AddPack( packName );
        }
        #endregion

        #region DuplicatePackCommand
        public ICommand DuplicatePackCommand
        {
            get { return new RelayCommand( DuplicatePackCommandExecuted ); }
        }

        private void DuplicatePackCommandExecuted( object parameter )
        {
            string packName = GetNewPackName( "DuplicatePackName" );
            if ( packName == null )
            {
                return;
            }

            SmileyManager.CurrentPack = SmileyManager.CopyPack( SmileyManager.CurrentPack, packName );
        }
        #endregion

        #region RemovePackCommand
        private RelayCommand _removePackCommand;
        public ICommand RemovePackCommand
        {
            get
            {
                if ( this._removePackCommand == null )
                {
                    this._removePackCommand = new RelayCommand( RemovePackCommandExecuted, CanExecuteRemovePackCommand );
                    this._removePackCommand.BindConditionToProperty( typeof( SmileyManager ) );
                    this._removePackCommand.BindConditionToProperty( SmileyManager.CurrentPack, o => o.IsDefault );
                }
                return this._removePackCommand;
            }
        }

        private void RemovePackCommandExecuted( object parameter )
        {
            string title = Locator.Get<ITranslationService>().Translate( "Global", "Confirmation" );
            string text = Locator.Get<ITranslationService>().Translate( "SmileySettings", "DeletePackConfirmation", SmileyManager.CurrentPack.Name );

            var packToDelete = SmileyManager.CurrentPack;
            if ( Locator.Get<IDialogService>().ShowMessageBox( title, text, AvailableDialogButtons.YesNo ) == true )
            {
                SmileyManager.CurrentPack = SmileyManager.Packs.First( p => p.IsDefault );
                SmileyManager.DeletePack( packToDelete );
            }
        }

        private bool CanExecuteRemovePackCommand( object parameter )
        {
            return !SmileyManager.CurrentPack.IsDefault;
        }
        #endregion

        #region ChangeSelectedSmileyImageCommand
        private RelayCommand _changeSelectedSmileyImageCommand;
        public ICommand ChangeSelectedSmileyImageCommand
        {
            get
            {
                if ( this._changeSelectedSmileyImageCommand == null )
                {
                    this._changeSelectedSmileyImageCommand = new RelayCommand( ChangeSelectedSmileyImageCommandExecuted, CanExecuteChangeSelectedSmileyImageCommand );
                    this._changeSelectedSmileyImageCommand.BindConditionToProperty( this, o => o.SelectedSmiley );
                }
                return this._changeSelectedSmileyImageCommand;
            }
        }

        private void ChangeSelectedSmileyImageCommandExecuted( object parameter )
        {
            if ( !CheckAccessToCurrentPack() )
            {
                return;
            }
            // TODO fix that
            string fileName = ( (WpfDialogService) Locator.Get<IDialogService>() ).GetFile( new[] { "*.*" } );
            if ( fileName != null )
            {
                SmileyManager.SetSmileyImage( SmileyManager.CurrentPack, this.SelectedSmiley, fileName );
            }
        }

        private bool CanExecuteChangeSelectedSmileyImageCommand( object parameter )
        {
            return this.SelectedSmiley != null;
        }
        #endregion

        #region AddSmileyCommand
        public ICommand AddSmileyCommand
        {
            get { return new RelayCommand( AddSmileyCommandExecuted ); }
        }

        private void AddSmileyCommandExecuted( object parameter )
        {
            if ( !CheckAccessToCurrentPack() )
            {
                return;
            }

            var vm = new AddSmileyWindowViewModel();
            if ( Locator.Get<IDialogService>().ShowDialog( vm ) == true )
            {
                this.SelectedSmiley = SmileyManager.AddSmiley( vm.ImagePath, vm.Shortcuts, SmileyManager.CurrentPack );
            }
        }
        #endregion

        #region RemoveSmileyCommand
        private RelayCommand _removeSmileyCommand;
        public ICommand RemoveSmileyCommand
        {
            get
            {
                if ( this._removeSmileyCommand == null )
                {
                    this._removeSmileyCommand = new RelayCommand( RemoveSmileyCommandExecuted, CanExecuteRemoveSmileyCommand );
                    this._removeSmileyCommand.BindConditionToProperty( this, o => o.SelectedSmiley );
                }
                return this._removeSmileyCommand;
            }
        }

        private void RemoveSmileyCommandExecuted( object parameter )
        {
            if ( !CheckAccessToCurrentPack() )
            {
                return;
            }

            string title = Locator.Get<ITranslationService>().Translate( "Global", "Confirmation" );
            string text = Locator.Get<ITranslationService>().Translate( "SmileySettings", "DeleteSmileyConfirmation" );
            if ( Locator.Get<IDialogService>().ShowMessageBox( title, text, AvailableDialogButtons.YesNo ) == true )
            {
                SmileyManager.RemoveSmiley( this.SelectedSmiley, SmileyManager.CurrentPack );
                this.SelectedSmiley = null;
            }
        }

        private bool CanExecuteRemoveSmileyCommand( object parameter )
        {
            return this.SelectedSmiley != null;
        }
        #endregion
        #endregion

        #region Private static methods
        private static bool CheckAccessToCurrentPack()
        {
            if ( SmileyManager.CurrentPack.IsDefault )
            {
                string title = Locator.Get<ITranslationService>().Translate( "Global", "Error" );
                string text = Locator.Get<ITranslationService>().Translate( "SmileySettings", "CannotChangeDefaultPack" );
                Locator.Get<IDialogService>().ShowMessageBox( title, text, AvailableDialogButtons.Ok );
                return false;
            }
            return true;
        }

        private static string GetNewPackName( string messageKey )
        {
            string packName;
            bool isOkay;
            do
            {
                isOkay = true;
                packName = Locator.Get<IDialogService>().ShowInputBox( Locator.Get<ITranslationService>().Translate( "SmileySettings", messageKey ) );

                if ( packName == null )
                {
                    return null; // user clicked cancel button
                }

                if ( packName.Intersect( Path.GetInvalidPathChars() ).Any() )
                {
                    isOkay = false;
                    string title = Locator.Get<ITranslationService>().Translate( "Global", "Error" );
                    string invalidChars = string.Join( string.Empty, Path.GetInvalidPathChars() );
                    string text = Locator.Get<ITranslationService>().Translate( "SmileySettings", "InvalidPackName", packName, invalidChars );
                    Locator.Get<IDialogService>().ShowMessageBox( title, text, AvailableDialogButtons.Ok );
                }

                if ( isOkay && SmileyManager.Packs.Any( pack => packName == pack.Name ) )
                {
                    isOkay = false;
                    string title = Locator.Get<ITranslationService>().Translate( "Global", "Error" );
                    string text = Locator.Get<ITranslationService>().Translate( "SmileySettings", "PackNameAlreadyExists", packName );
                    Locator.Get<IDialogService>().ShowMessageBox( title, text, AvailableDialogButtons.Ok );
                }
            } while ( !isOkay );

            return packName;
        }
        #endregion
    }
}