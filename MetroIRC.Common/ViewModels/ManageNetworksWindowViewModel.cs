// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using BasicMvvm;
using BasicMvvm.Dialogs;
using MetroIrc.Services;

namespace MetroIrc.ViewModels
{
    public sealed class ManageNetworksWindowViewModel : DialogViewModel
    {
        #region Property-backing fields
        private IrcNetworkInfo _selectedNetwork;
        #endregion

        #region Properties
        public IrcNetworkInfo SelectedNetwork
        {
            get { return this._selectedNetwork; }
            set { this.SetProperty( ref this._selectedNetwork, value ); }
        }
        #endregion

        public ManageNetworksWindowViewModel()
        {
            if ( Locator.Get<ISettings>().Networks.Any() )
            {
                this.SelectedNetwork = Locator.Get<ISettings>().Networks[0];
            }
        }

        #region Commands
        #region AddNetworkCommand
        private RelayCommand _addNetworkCommand;
        public ICommand AddNetworkCommand
        {
            get
            {
                if ( this._addNetworkCommand == null )
                {
                    this._addNetworkCommand = new RelayCommand( AddNetworkCommandExecuted, CanExecuteAddNetworkCommand );
                    this._addNetworkCommand.BindConditionToProperty( Locator.Get<ISettings>(), o => o.HasErrors );
                }
                return this._addNetworkCommand;
            }
        }

        private void AddNetworkCommandExecuted( object parameter )
        {
            var viewModel = new EditNetworkWindowViewModel();
            if ( Locator.Get<IDialogService>().ShowDialog( viewModel ) == true )
            {
                Locator.Get<ISettings>().Networks.Add( viewModel.NetworkInfo );
                this.SelectedNetwork = viewModel.NetworkInfo;
            }
        }

        private bool CanExecuteAddNetworkCommand( object parameter )
        {
            return !Locator.Get<ISettings>().HasErrors;
        }
        #endregion

        #region RemoveNetworkCommand
        private RelayCommand _removeNetworkCommand;
        public ICommand RemoveNetworkCommand
        {
            get
            {
                if ( this._removeNetworkCommand == null )
                {
                    this._removeNetworkCommand = new RelayCommand( RemoveNetworkCommandExecuted, CanExecuteRemoveNetworkCommand );
                    this._removeNetworkCommand.BindConditionToProperty( this, o => o.SelectedNetwork );
                }
                return this._removeNetworkCommand;
            }
        }

        private void RemoveNetworkCommandExecuted( object parameter )
        {
            string title = Locator.Get<ITranslationService>().Translate( "Global", "Confirmation" );
            string text = Locator.Get<ITranslationService>().Translate( "ManageNetworksWindow", "RemoveNetworkConfirmation", this.SelectedNetwork.FriendlyName );

            if ( Locator.Get<IDialogService>().ShowMessageBox( title, text, AvailableDialogButtons.YesNo ) == true )
            {
                Locator.Get<ISettings>().Networks.Remove( this.SelectedNetwork );
            }
        }

        private bool CanExecuteRemoveNetworkCommand( object parameter )
        {
            return this.SelectedNetwork != null;
        }
        #endregion

        #region EditNetworkCommand
        private RelayCommand _editNetworkCommand;
        public ICommand EditNetworkCommand
        {
            get
            {
                if ( this._editNetworkCommand == null )
                {
                    this._editNetworkCommand = new RelayCommand( EditNetworkCommandExecuted, CanExecuteEditNetworkCommand );
                    this._editNetworkCommand.BindConditionToProperty( this, o => o.SelectedNetwork );
                }
                return this._editNetworkCommand;
            }
        }

        private void EditNetworkCommandExecuted( object parameter )
        {
            var newInfo = this.SelectedNetwork.Clone();
            var viewModel = new EditNetworkWindowViewModel( newInfo );

            if ( Locator.Get<IDialogService>().ShowDialog( viewModel ) == true )
            {
                Locator.Get<ISettings>().Networks[Locator.Get<ISettings>().Networks.IndexOf( this.SelectedNetwork )] = newInfo;
                this.SelectedNetwork = newInfo;
            }
        }

        private bool CanExecuteEditNetworkCommand( object parameter )
        {
            return this.SelectedNetwork != null;
        }
        #endregion

        private RelayCommand _cancelCommand;
        public ICommand CancelCommand
        {
            get
            {
                if ( this._cancelCommand == null )
                {
                    this._cancelCommand = new RelayCommand( _ => this.FireRequestClose( false ),
                                                            _ => !Locator.Get<ISettings>().HasErrors );
                    this._cancelCommand.BindConditionToProperty( Locator.Get<ISettings>(), o => o.HasErrors );
                }
                return this._cancelCommand;
            }
        }

        private RelayCommand _connectCommand;
        public ICommand ConnectCommand
        {
            get
            {
                if ( this._connectCommand == null )
                {
                    this._connectCommand = new RelayCommand( _ => this.FireRequestClose( true ),
                                                             _ => this.SelectedNetwork != null && !Locator.Get<ISettings>().HasErrors );
                    this._connectCommand.BindConditionToProperty( this, o => o.SelectedNetwork );
                    this._connectCommand.BindConditionToProperty( Locator.Get<ISettings>(), o => o.HasErrors );
                }
                return this._connectCommand;
            }
        }
        #endregion

        #region DialogViewModel overrides
        public override string Title
        {
            get { return Locator.Get<ITranslationService>().Translate( "ManageNetworksWindow", "Title" ); }
        }

        public override object Icon
        {
            get { return Locator.Get<IResourceService>().GetResource( "ManageNetworksWindowIcon" ); }
        }

        public override void BaseWindowClosing( object sender, CancelEventArgs e )
        {
            e.Cancel = Locator.Get<ISettings>().HasErrors;

            if ( !e.Cancel )
            {
                Locator.Get<ISettings>().Save();
            }
        }
        #endregion
    }
}