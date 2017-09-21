// Copyright (C) 2011-2013, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BasicMvvm;
using BasicMvvm.Dialogs;
using MetroIrc.Internals;
using IrcSharp;
using MetroIrc.Log;
using MetroIrc.Services;
using CM = System.ComponentModel;

namespace MetroIrc.ViewModels
{
    /// <summary>
    /// The IrcNetworkViewModel class manages ViewModels related to IRC conversations
    /// It is the only class, apart from its <see cref="MetroIrc.MessageSender"/>, that talks with the <see cref="IrcSharp.IrcClient"/> directly.
    /// </summary>
    public sealed class IrcNetworkViewModel : ObservableObject, IDisposable
    {
        #region Constants
        private const int ConnectionRetryInterval = 5000; // in milliseconds
        private const int ConnectionAttemptsCount = 5;
        private const int ChannelsJoinDelay = 5000; // in milliseconds

        private static readonly int[] IgnoredReplies = 
        { 
            (int) IrcReplyCode.MyInfo,
            (int) IrcErrorCode.UnknownChannelMode,
            (int) IrcErrorCode.NotEnoughChannelPrivileges
        };
        #endregion

        #region Property-backing fields
        private ObservableCollection<IrcConversationViewModel> _conversations;
        private IrcConversationViewModel _selectedConversation;

        private string _writtenText;

        private bool _isSelected;
        private bool _isCurrentUserPrivileged;
        private bool _hasUnreadMessages;
        private bool _hasHighlightedUnreadMessages;
        #endregion

        #region Private members
        private IrcNetworkInformationViewModel _networkInfoViewModel;
        private IrcLogger _logger;
        // Amount of times we tried reconnecting after losing the connection
        private int _retryCount = 0;
        #endregion

        #region Public properties
        public IrcNetwork Network { get; private set; }

        public IrcNetworkInfo NetworkInfo { get; set; }

        /// <summary>
        /// Gets all conversations included in this network : the "info" one, the channels, and the private conversations with users.
        /// </summary>        
        public ObservableCollection<IrcConversationViewModel> Conversations
        {
            get { return this._conversations; }
            set { this.SetProperty( ref this._conversations, value ); }
        }

        public IrcConversationViewModel SelectedConversation
        {
            get { return this._selectedConversation; }
            set
            {
                if ( this._selectedConversation != value )
                {
                    if ( this._selectedConversation != null )
                    {
                        this._selectedConversation.IsSelected = false;
                    }
                    this._selectedConversation = value;
                    this._selectedConversation.IsSelected = true;
                    this.FirePropertyChanged();
                    this.SetCurrentPrivilegeStatus();
                }
            }
        }


        /// <summary>
        /// Gets or sets the text written in the message textbox.
        /// </summary>
        public string WrittenText
        {
            get { return this._writtenText; }
            set { this.SetProperty( ref this._writtenText, value ); }
        }

        /// <summary>
        /// Gets or sets the current user's Nickname. 
        /// Use this to attempt to change the Nickname.
        /// </summary>
        public string CurrentNickname
        {
            get { return this.Network.CurrentUser.Nickname; }
            set
            {
                if ( value.IsEmpty() || value == this.CurrentNickname )
                {
                    return;
                }
                this.Network.ChangeNickname( value );
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current user can kick other users (i.e. has at least chan half-operator rights).
        /// </summary>
        public bool IsCurrentUserPrivileged
        {
            get { return this._isCurrentUserPrivileged; }
            set { this.SetProperty( ref this._isCurrentUserPrivileged, value ); }
        }

        /// <summary>
        /// Gets this viewmodel's title.
        /// </summary>
        public string Title
        {
            get { return this.NetworkInfo.FriendlyName; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this network is selected.
        /// </summary>
        public bool IsSelected
        {
            get { return this._isSelected; }
            set { this.SetProperty( ref this._isSelected, value ); }
        }

        /// <summary>
        /// Gets a value indicating whether there are unread messages.
        /// </summary>
        public bool HasUnreadMessages
        {
            get { return this._hasUnreadMessages; }
            set { this.SetProperty( ref this._hasUnreadMessages, value ); }
        }

        /// <summary>
        /// Gets a value indicating whether there are unread messages containing HLs.
        /// </summary>
        public bool HasHighlightedUnreadMessages
        {
            get { return this._hasHighlightedUnreadMessages; }
            set { this.SetProperty( ref this._hasHighlightedUnreadMessages, value ); }
        }
        #endregion

        public IrcNetworkViewModel( IrcNetworkInfo info )
        {
            this.NetworkInfo = info;

            var wrapper = Locator.Get<ITcpService>().GetWrapper( info );
            this.Network = new IrcNetwork( wrapper );
            this.Network.Client.Encoding = info.Encoding;
            this.Network.Client.IdentUserName = info.Nickname;

            this._logger = Locator.Get<IrcLogger>();
            this._logger.Start( this.Network.Client );

            this._networkInfoViewModel = new IrcNetworkInformationViewModel( this.Network, this );

            this.Conversations = new ObservableCollection<IrcConversationViewModel>();
            this.Conversations.Add( this._networkInfoViewModel );

            this.SelectedConversation = this._networkInfoViewModel;

            this.Network.Connected += Network_Connected;
            this.Network.Authenticated += Network_Authenticated;
            this.Network.ConnectionLost += Network_ConnectionLost;

            this.Network.ChannelDiscovered += Network_ChannelDiscovered;
            this.Network.UserDiscovered += Network_UserDiscovered;

            this.Network.ChannelListReceived += Network_ChannelListReceived;
            this.Network.InformationReceived += Network_InformationReceived;
            this.Network.ErrorReceived += Network_ErrorReceived;
            this.Network.NicknameCollision += Network_NicknameCollision;

            PropertyChangedEventManager.AddHandler( this.Network.CurrentUser, CurrentUser_NicknameChanged, o => o.Nickname );
            PropertyChangedEventManager.AddHandler( this.NetworkInfo, NetworkInfo_EncodingChanged, o => o.Encoding );

            Messenger.Register<ChangeEncodingMessage>( ChangeEncodingHandler, m => m.Network == this.Network );
            Messenger.Register<JoinChannelMessage>( JoinChannelHandler, m => m.Network == this.Network || m.Network == null && this.IsSelected );
            Messenger.Register<LeaveChannelMessage>( LeaveChannelHandler, m => m.Network == this.Network );
            Messenger.Register<GlobalMessageSentMessage>( GlobalMessageSentHandler, m => m.Network == this.Network );
            Messenger.Register<UserMessageSentMessage>( UserMessageSentHandler, m => m.Network == this.Network );
            Messenger.Register<ChannelMessageSentMessage>( ChannelMessageSentHandler, m => m.Network == this.Network );
            Messenger.Register<GlobalMessageReceivedMessage>( GlobalMessageReceivedHandler, m => m.Network == this.Network );
            Messenger.Register<UserMessageReceivedMessage>( UserMessageReceivedHandler, m => m.Network == this.Network );
            Messenger.Register<OpenPrivateConversationMessage>( OpenPrivateConversationHandler, m => m.Network == this.Network );
            Messenger.Register<ConversationEndMessage>( ConversationEndHandler, m => m.Network == this.Network );
            Messenger.Register<UnreadMessageReceivedMessage>( UnreadMessageReceivedHandler, m => m.Network == this.Network );
        }

        #region Commands
        #region JoinChannelCommand
        private RelayCommand _joinChannelCommand;
        public ICommand JoinChannelCommand
        {
            get
            {
                if ( this._joinChannelCommand == null )
                {
                    this._joinChannelCommand = new RelayCommand( JoinChannelCommandExecuted, CanExecuteJoinChannelCommand );
                    this._joinChannelCommand.BindConditionToProperty( this.Network, o => o.ConnectionStatus );
                }
                return this._joinChannelCommand;
            }
        }

        private void JoinChannelCommandExecuted( object parameter )
        {
            IrcChannelInfo info = null;

            var channel = parameter as IrcChannel;
            if ( channel == null )
            {
                var vm = new JoinChannelWindowViewModel( this.Network );
                if ( Locator.Get<IDialogService>().ShowDialog( vm ) == true )
                {
                    info = vm.ChannelInfo;
                }
            }
            else
            {
                info = new IrcChannelInfo( channel.FullName );
            }

            if ( info != null )
            {
                if ( info.JoinOnStartup )
                {
                    this.NetworkInfo.FavoriteChannels.Add( info );
                }
                this.JoinChannel( info );
            }
        }

        private bool CanExecuteJoinChannelCommand( object parameter )
        {
            return this.Network.ConnectionStatus == ConnectionStatus.Authenticated;
        }
        #endregion

        #region SendMessageCommand
        private RelayCommand _sendMessageCommand;
        public ICommand SendMessageCommand
        {
            get
            {
                if ( this._sendMessageCommand == null )
                {
                    this._sendMessageCommand = new RelayCommand( SendMessageCommandExecuted, CanExecuteSendMessageCommand );
                    this._sendMessageCommand.BindConditionToProperty( this.Network, o => o.ConnectionStatus );
                }
                return this._sendMessageCommand;
            }
        }

        private void SendMessageCommandExecuted( object parameter )
        {
            if ( this.WrittenText.HasText() )
            {
                this.Send( this.WrittenText );
            }
            this.WrittenText = string.Empty;
        }

        private bool CanExecuteSendMessageCommand( object parameter )
        {
            return this.Network.ConnectionStatus == ConnectionStatus.Authenticated;
        }
        #endregion

        #region RemoveConversationCommand
        private RelayCommand _removeConversationCommand;
        public ICommand RemoveConversationCommand
        {
            get
            {
                if ( this._removeConversationCommand == null )
                {
                    this._removeConversationCommand = new RelayCommand( RemoveConversationCommandExecuted );
                }
                return this._removeConversationCommand;
            }
        }

        private void RemoveConversationCommandExecuted( object parameter )
        {
            var vm = parameter as IrcConversationViewModel;
            if ( vm != null )
            {
                this.RemoveConversation( vm );
            }
        }
        #endregion

        #region OpenPrivateConversationCommand
        private RelayCommand _openPrivateConversationCommand;
        public ICommand OpenPrivateConversationCommand
        {
            get
            {
                if ( this._openPrivateConversationCommand == null )
                {
                    this._openPrivateConversationCommand = new RelayCommand( OpenPrivateConversationCommandExecuted );
                }
                return this._openPrivateConversationCommand;
            }
        }

        private void OpenPrivateConversationCommandExecuted( object parameter )
        {
            var user = parameter as IrcUser;
            if ( user != null )
            {
                this.SelectedConversation = this.GetConversation( user );
            }
        }
        #endregion

        #region SendWhoIsCommand
        private RelayCommand _sendWhoIsCommand;
        public ICommand SendWhoIsCommand
        {
            get
            {
                if ( this._sendWhoIsCommand == null )
                {
                    this._sendWhoIsCommand = new RelayCommand( SendWhoIsCommandExecuted );
                }
                return this._sendWhoIsCommand;
            }
        }

        private void SendWhoIsCommandExecuted( object parameter )
        {
            var user = (IrcUser) parameter;
            user.GetInformation();
        }
        #endregion

        #region KickCommand
        private RelayCommand _kickCommand;
        public ICommand KickCommand
        {
            get
            {
                if ( this._kickCommand == null )
                {
                    this._kickCommand = new RelayCommand( KickCommandExecuted, CanExecuteKickCommand );
                    this._kickCommand.BindConditionToProperty( this, o => o.IsCurrentUserPrivileged );
                }
                return this._kickCommand;
            }
        }

        private void KickCommandExecuted( object parameter )
        {
            var user = (IrcUser) parameter;
            var chanVM = this.SelectedConversation as IrcChannelViewModel;
            if ( chanVM != null )
            {
                chanVM.Channel.KickUser( user );
            }
        }

        private bool CanExecuteKickCommand( object parameter )
        {
            return this.IsCurrentUserPrivileged == true;
        }
        #endregion

        #region BanCommand
        private RelayCommand _banCommand;
        public ICommand BanCommand
        {
            get
            {
                if ( this._banCommand == null )
                {
                    this._banCommand = new RelayCommand( BanCommandExecuted, CanExecuteBanCommand );
                    this._banCommand.BindConditionToProperty( this, o => o.IsCurrentUserPrivileged );
                }
                return this._banCommand;
            }
        }

        private void BanCommandExecuted( object parameter )
        {
            var user = (IrcUser) parameter;
            var chanVM = this.SelectedConversation as IrcChannelViewModel;
            if ( chanVM != null )
            {
                chanVM.Channel.BanUser( user, true );
            }
        }

        private bool CanExecuteBanCommand( object parameter )
        {
            return this.IsCurrentUserPrivileged == true;
        }
        #endregion

        #region SwitchConversationCommand
        private RelayCommand _switchConversationCommand;
        public ICommand SwitchConversationCommand
        {
            get
            {
                if ( this._switchConversationCommand == null )
                {
                    this._switchConversationCommand = new RelayCommand( SwitchChannelCommandExecuted );
                }
                return this._switchConversationCommand;
            }
        }

        private void SwitchChannelCommandExecuted( object parameter )
        {
            // GIANT HACK

            int index = this.Conversations.IndexOf( this.SelectedConversation );
            index = ( index + 1 ) % this.Conversations.Count;
            var conv = this.Conversations.Skip( index ).FirstOrDefault( c => c.IsVisible );
            if ( conv == null )
            {
                this.SelectedConversation = this.Conversations.First( c => c.IsVisible );
            }
            else
            {
                this.SelectedConversation = conv;
            }
        }
        #endregion
        #endregion

        #region Public methods
        public void Connect()
        {
            if ( this.Network.ConnectionStatus == ConnectionStatus.NotConnected
              || this.Network.ConnectionStatus == ConnectionStatus.ConnectionLost )
            {
                this.Network.Connect();
            }
        }
        #endregion

        #region Private methods
        private void JoinChannel( IrcChannelInfo info )
        {
            var channel = this.Network.GetChannel( info.Name );
            channel.Key = info.Key ?? string.Empty;
            channel.Join();
        }

        private IrcConversationViewModel GetConversation( IrcChannel channel )
        {
            var vm = this.Conversations.FirstOrDefault( c => c.TargetName == channel.FullName );

            if ( vm == null )
            {
                vm = new IrcChannelViewModel( channel, this );
                this.AddConversation( vm );
            }

            return vm;
        }
        private IrcConversationViewModel GetConversation( IrcUser user )
        {
            var vm = this.Conversations.FirstOrDefault( c => c.TargetName == user.Nickname );

            if ( vm == null )
            {
                vm = new IrcUserViewModel( user, this );
                this.AddConversation( vm );
            }

            return vm;
        }

        private void AddUserMessage( Message message, IrcUser user )
        {
            if ( user == this.Network.CurrentUser )
            {
                this._networkInfoViewModel.AddMessage( message );
            }

            foreach ( var vm in this.Conversations.Where( c => c.Users.Contains( user ) ) )
            {
                vm.AddMessage( message );
            }
        }
        private void AddGlobalMessage( Message message )
        {
            foreach ( var vm in this.Conversations )
            {
                vm.AddMessage( message );
            }
        }

        private void AddConversation( IrcConversationViewModel conversation )
        {
            Locator.Get<IUIService>().Execute( () =>
            {
                if ( !this.Conversations.Contains( conversation ) )
                {
                    this.Conversations.Add( conversation );
                }
            } );
        }
        private void RemoveConversation( IrcConversationViewModel conversation )
        {
            if ( !conversation.CanBeClosed )
            {
                return;
            }

            Locator.Get<IUIService>().Execute( () =>
            {
                if ( this.SelectedConversation == conversation )
                {
                    this.SelectedConversation = this.Conversations.TakeWhile( c => c != conversation )
                                                                  .Last( c => c.IsVisible );
                }

                this.Conversations.Remove( conversation );
                conversation.Dispose();
            } );
        }

        private void Send( string text )
        {
            MessageSender.SendMessage( this.Network, this.SelectedConversation.TargetName, text );
        }

        private void ProcessMessage( Message message, IrcConversationViewModel conversation )
        {
            if ( this.ShouldNotify( message, conversation ) )
            {
                string text = Locator.Get<ITranslationService>().Translate( "Conversation", "HL", message.Sender.Nickname );
                Locator.Get<INotificationService>().Notify( text );

                if ( !this.IsSelected )
                {
                    this.HasHighlightedUnreadMessages = true;
                }
            }

            if ( !this.IsSelected && !message.IsInformationMessage )
            {
                this.HasUnreadMessages = true;
            }
        }
        private bool ShouldNotify( Message message, IrcConversationViewModel conversation )
        {
            return message.Sender != null
                && ( message.IsImportantMessage || conversation is IrcUserViewModel )
                && !IsConversationVisible( conversation );
        }
        private bool IsConversationVisible( IrcConversationViewModel conversation )
        {
            return conversation.IsSelected
                && this.IsSelected
                && Locator.Get<IUIService>().IsAppToForeground();
        }
        private void SetCurrentPrivilegeStatus()
        {
            var channelVM = this.SelectedConversation as IrcChannelViewModel;
            if ( channelVM == null )
            {
                this.IsCurrentUserPrivileged = false;
            }
            else
            {
                var currentUserMode = this.Network.CurrentUser.GetChannelMode( channelVM.Channel );
                this.IsCurrentUserPrivileged = currentUserMode >= IrcChannelUserModes.HalfOp;
            }
        }
        #endregion

        #region Event handlers
        private void CurrentUser_NicknameChanged( object sender, CM.PropertyChangedEventArgs e )
        {
            this.FirePropertyChanged( "CurrentNickname" );
        }

        private void NetworkInfo_EncodingChanged( object sender, CM.PropertyChangedEventArgs e )
        {
            this.Network.Client.Encoding = this.NetworkInfo.Encoding;
        }


        private void Network_Connected( IrcNetwork network, EventArgs args )
        {
            string nick = this.NetworkInfo.GetActualNickname();
            string realName = this.NetworkInfo.GetActualRealName();
            this.Network.Authenticate( nick, realName, IrcUserLoginModes.None, this.NetworkInfo.Password ?? string.Empty );
        }

        private async void Network_Authenticated( IrcNetwork network, EventArgs args )
        {
            if ( this.NetworkInfo.JoinCommand.HasText() )
            {
                this.Send( this.NetworkInfo.JoinCommand );

                // Some bots *cough* freenode's NickServ *cough* are slow.
                // Sending the JOINs directly after the join command results in joining channels BEFORE authentication.
                await Task.Delay( ChannelsJoinDelay );

                if ( this._disposed || this.Network.ConnectionStatus != ConnectionStatus.Authenticated )
                {
                    return; // connection loss or network closed by user
                }
            }

            foreach ( var channel in this.NetworkInfo.FavoriteChannels )
            {
                this.JoinChannel( channel );
            }

            // Join the channels which we were on before a disconnection, if any
            foreach ( var vm in this.Conversations.OfType<IrcChannelViewModel>()
                                                  .Where( c => c.IsVisible ) )
            {
                vm.Channel.Join();
            }
        }

        private async void Network_ConnectionLost( IrcNetwork network, EventArgs args )
        {
            if ( this._disposed )
            {
                return;
            }
            // Go back to the network info view, more practical to display stuff
            this.SelectedConversation = this._networkInfoViewModel;

            if ( this._retryCount == ConnectionAttemptsCount )
            {
                this._retryCount = 0;
            }
            else
            {
                this._retryCount++;

                await Task.Delay( ConnectionRetryInterval );

                if ( !this._disposed )
                {
                    this.Connect();
                }
            }
        }


        private void Network_ChannelDiscovered( IrcNetwork network, ChannelEventArgs args )
        {
            this.SelectedConversation = this.GetConversation( args.Channel );
            this.SelectedConversation.IsVisible = true;
        }

        private void Network_UserDiscovered( IrcNetwork network, UserEventArgs args )
        {
            this.GetConversation( args.User );
        }

        private void Network_ChannelListReceived( IrcNetwork network, ChannelListReceivedEventArgs args )
        {
            if ( args.Channels.Count == 0 )
            {
                var errorText = Locator.Get<ITranslationService>().Translate( "ConversationErrors", "EmptyChannelList" );
                var ircMessage = new IrcMessage( this.Network, MessageDirection.FromServer, null, IrcMessageType.Error, errorText );
                this.SelectedConversation.AddMessage( ircMessage );
            }
            else
            {
                var channels = args.Channels.OrderBy( c => c.Name ).ToList();
                var message = new ChannelListMessage( this.Network, channels );
                this.SelectedConversation.AddMessage( message );
            }
        }

        private void Network_InformationReceived( IrcNetwork network, InformationReceivedEventArgs args )
        {
            int replyCode;
            if ( int.TryParse( args.Command, out replyCode ) && IgnoredReplies.Contains( replyCode ) )
            {
                return;
            }

            var message = new IrcMessage( this.Network, MessageDirection.FromServer, null, IrcMessageType.Info, args.Message );
            this.SelectedConversation.AddMessage( message );
        }

        private void Network_ErrorReceived( IrcNetwork network, InformationReceivedEventArgs args )
        {
            int replyCode;
            if ( int.TryParse( args.Command, out replyCode ) && IgnoredReplies.Contains( replyCode ) )
            {
                return;
            }

            var message = new IrcMessage( this.Network, MessageDirection.FromServer, null, IrcMessageType.Error, args.Message );
            this.SelectedConversation.AddMessage( message );
        }

        private void Network_NicknameCollision( IrcNetwork network, NicknameCollisionEventArgs args )
        {
            string text;
            if ( this.Network.ConnectionStatus < ConnectionStatus.Connected )
            {
                text = Locator.Get<ITranslationService>().Translate( "ConnectionErrors", "NicknameCollision", args.UsedNickname );
            }
            else
            {
                text = Locator.Get<ITranslationService>().Translate( "ConversationErrors", "NicknameTaken", args.UsedNickname );
            }

            var message = new IrcMessage( this.Network, MessageDirection.FromServer, null, IrcMessageType.Error, text );
            this.SelectedConversation.AddMessage( message );
        }
        #endregion

        #region Messages handling
        private void ChangeEncodingHandler( ChangeEncodingMessage message )
        {
            this.NetworkInfo.Encoding = message.Encoding;
        }

        private void JoinChannelHandler( JoinChannelMessage message )
        {
            var info = new IrcChannelInfo
            {
                Name = message.Channel.FullName,
                Key = message.Key
            };

            this.JoinChannel( info );
        }

        private void LeaveChannelHandler( LeaveChannelMessage message )
        {
            var conv = this.GetConversation( message.Channel );
            this.RemoveConversation( conv );
        }

        private void GlobalMessageSentHandler( GlobalMessageSentMessage message )
        {
            this.SelectedConversation.AddMessage( message.Message );
        }

        private void UserMessageSentHandler( UserMessageSentMessage message )
        {
            this.GetConversation( message.User ).AddMessage( message.Message );
        }

        private void ChannelMessageSentHandler( ChannelMessageSentMessage message )
        {
            this.GetConversation( message.Channel ).AddMessage( message.Message );
        }

        private void GlobalMessageReceivedHandler( GlobalMessageReceivedMessage message )
        {
            this.SelectedConversation.AddMessage( message.Message );
        }

        private void UserMessageReceivedHandler( UserMessageReceivedMessage message )
        {
            this.AddUserMessage( message.Message, message.User );
        }

        private void OpenPrivateConversationHandler( OpenPrivateConversationMessage message )
        {
            var conv = this.GetConversation( message.User );
            this.AddConversation( conv );
        }

        private void ConversationEndHandler( ConversationEndMessage message )
        {
            this.RemoveConversation( message.Conversation );
        }

        private void UnreadMessageReceivedHandler( UnreadMessageReceivedMessage message )
        {
            this.ProcessMessage( message.Message, message.Conversation );
        }
        #endregion

        #region IDisposable implementation
        private bool _disposed;

        public void Dispose()
        {
            this.Dispose( true );
        }

        public void Dispose( bool quitNetwork )
        {
            this._disposed = true;
            if ( quitNetwork && this.Network.ConnectionStatus >= ConnectionStatus.Connected )
            {
                this.Network.Quit();
            }
            this.Network.Dispose();
            this._logger.LogInfo( "Connection closed." );
            this._logger.Dispose();
        }
        #endregion
    }
}