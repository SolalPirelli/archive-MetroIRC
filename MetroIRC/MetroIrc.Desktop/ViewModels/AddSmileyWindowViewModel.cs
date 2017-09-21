// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using BasicMvvm;
using BasicMvvm.Dialogs;
using MetroIrc.Desktop.Services;
using MetroIrc.Services;

namespace MetroIrc.Desktop.ViewModels
{
    public sealed class AddSmileyWindowViewModel : DialogViewModel
    {
        #region Property-backing fields
        private string _imagePath;
        private ObservableCollection<string> _shortcuts = new ObservableCollection<string>();
        #endregion

        #region Public properties
        public string ImagePath
        {
            get { return this._imagePath; }
            set { this.SetProperty( ref this._imagePath, value ); }
        }

        public ObservableCollection<string> Shortcuts
        {
            get { return this._shortcuts; }
            set { this.SetProperty( ref this._shortcuts, value ); }
        }
        #endregion

        #region Commands
        #region BrowseCommand
        public ICommand BrowseCommand
        {
            get { return new RelayCommand( BrowseCommandExecuted ); }
        }

        private void BrowseCommandExecuted( object parameter )
        {
            // TODO fix that
            string fileName = ( (WpfDialogService) Locator.Get<IDialogService>() ).GetFile( new[] { "*.*" } );
            if ( fileName != null )
            {
                this.ImagePath = fileName;
            }
        }
        #endregion

        public ICommand OkCommand
        {
            get
            {
                return new RelayCommand( _ => this.FireRequestClose( true ),
                                         _ => File.Exists( this.ImagePath ) );
            }
        }

        public ICommand CancelCommand
        {
            get { return new RelayCommand( _ => this.FireRequestClose( false ) ); }
        }
        #endregion

        #region DialogViewModel overrides
        public override string Title
        {
            get { return Locator.Get<ITranslationService>().Translate( "SmileySettings", "AddSmiley" ); }
        }

        public override object Icon
        {
            get { return Locator.Get<IResourceService>().GetResource( "AddSmileyWindowIcon" ); }
        }
        #endregion
    }
}