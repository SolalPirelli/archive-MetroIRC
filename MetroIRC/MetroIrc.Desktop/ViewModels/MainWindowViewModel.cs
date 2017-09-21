// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using BasicMvvm;
using BasicMvvm.Dialogs;
using MetroIrc.ViewModels;

namespace MetroIrc.Desktop.ViewModels
{
    public sealed class MainWindowViewModel : ObservableObject, IDisposable
    {
        #region Property-backing fields
        private ObservableCollection<IrcNetworkViewModel> _networks = new ObservableCollection<IrcNetworkViewModel>();
        private IrcNetworkViewModel _selectedNetwork;
        #endregion

        #region Public properties
        public ObservableCollection<IrcNetworkViewModel> Networks
        {
            get { return this._networks; }
            private set { this.SetProperty( ref this._networks, value ); }
        }

        public IrcNetworkViewModel SelectedNetwork
        {
            get { return this._selectedNetwork; }
            set
            {
                if ( this._selectedNetwork == value )
                {
                    return;
                }

                if ( this._selectedNetwork != null )
                {
                    this._selectedNetwork.IsSelected = false;
                    this._selectedNetwork.HasUnreadMessages = this._selectedNetwork.Conversations.Any( c => c.UnreadMessagesCount > 0 );
                    this._selectedNetwork.HasHighlightedUnreadMessages = this._selectedNetwork.Conversations.Any( c => c.HasUnreadImportantMessage );
                }

                this._selectedNetwork = value;
                this.FirePropertyChanged();

                if ( this._selectedNetwork != null )
                {
                    this._selectedNetwork.IsSelected = true;
                    this._selectedNetwork.HasUnreadMessages = false;
                    this._selectedNetwork.HasHighlightedUnreadMessages = false;
                    this._selectedNetwork.SelectedConversation.UnreadMessagesCount = 0;
                    this._selectedNetwork.SelectedConversation.HasUnreadImportantMessage = false;
                }
            }
        }
        #endregion

        public MainWindowViewModel()
        {
            Messenger.Register<AddNetworkMessage>( AddNetworkMessageReceived );
            Messenger.Register<QuitNetworkMessage>( QuitNetworkMessageReceived );
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
                    this._addNetworkCommand = new RelayCommand( AddNetworkCommandExecuted );
                }
                return this._addNetworkCommand;
            }
        }

        private void AddNetworkCommandExecuted( object parameter )
        {
            var viewModel = new ManageNetworksWindowViewModel();
            if ( Locator.Get<IDialogService>().ShowDialog( viewModel ) == true )
            {
                this.AddNetwork( viewModel.SelectedNetwork );
            }
        }
        #endregion

        public ICommand ShowSettingsCommand
        {
            get { return new RelayCommand( _ => Locator.Get<IDialogService>().ShowDialog( new SettingsWindowViewModel() ) ); }
        }

        public ICommand ShowAboutWindowCommand
        {
            get { return new RelayCommand( _ => Locator.Get<IDialogService>().ShowDialog( new AboutWindowViewModel() ) ); }
        }

        #region RemoveNetworkCommand
        private RelayCommand _removeNetworkCommand;
        public ICommand RemoveNetworkCommand
        {
            get
            {
                if ( this._removeNetworkCommand == null )
                {
                    this._removeNetworkCommand = new RelayCommand( RemoveNetworkCommandExecuted );
                }
                return this._removeNetworkCommand;
            }
        }

        private void RemoveNetworkCommandExecuted( object parameter )
        {
            var network = (IrcNetworkViewModel) parameter;
            this.Networks.Remove( network );
            network.Dispose();
        }
        #endregion

        #region SwitchNetworkCommand
        public ICommand SwitchNetworkCommand
        {
            get { return new RelayCommand( SwitchNetworkCommandExecuted ); }
        }

        private void SwitchNetworkCommandExecuted( object parameter )
        {
            int index = this.Networks.IndexOf( this.SelectedNetwork );
            index = ( index + 1 + this.Networks.Count ) % this.Networks.Count;
            this.SelectedNetwork = this.Networks[index];
        }
        #endregion
        #endregion

        #region Public methods
        public void ConnectToStartupNetworks()
        {
            foreach ( var network in Locator.Get<ISettings>().Networks.Where( n => n.JoinOnStartup ) )
            {
                this.AddNetwork( network );
            }

            if ( this.Networks.Any() )
            {
                this.SelectedNetwork = this.Networks[0];
            }
        }

        public void AddNetwork( IrcNetworkInfo info )
        {
            if ( info == null )
            {
                throw new ArgumentNullException( "info" );
            }

            var existingNetwork = this._networks.FirstOrDefault( n => n.NetworkInfo.HostName == info.HostName );
            if ( existingNetwork != null )
            {
                existingNetwork.Connect();
                return;
            }

            var network = new IrcNetworkViewModel( info );
            this.Networks.Add( network );
            this.SelectedNetwork = network;
            network.Connect();
        }
        #endregion

        #region Messages handling
        private void AddNetworkMessageReceived( AddNetworkMessage message )
        {
            var info = new IrcNetworkInfo
            {
                HostName = message.Name,
                PortNumber = message.Port,
                Password = message.Password
            };

            this.AddNetwork( info );

            if ( !Locator.Get<ISettings>().Networks.Any( i => i.HostName == info.HostName ) )
            {
                Locator.Get<ISettings>().Networks.Add( info );
            }
        }

        private void QuitNetworkMessageReceived( QuitNetworkMessage message )
        {
            var network = this.Networks.First( n => n.Network == message.Network );
            this.Networks.Remove( network );
            network.Dispose( false );
        }
        #endregion

        #region IDisposable implementation
        public void Dispose()
        {
            foreach ( var item in this._networks )
            {
                item.Dispose();
            }
        }
        #endregion
    }
}