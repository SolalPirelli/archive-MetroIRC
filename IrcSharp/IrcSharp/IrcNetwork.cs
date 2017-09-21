// Copyright (C) 2012-2013, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using IrcSharp.Internals;
using IrcSharp.Validation;

namespace IrcSharp
{
    /// <summary>
    /// A class representing an IRC network and its related properties.
    /// </summary>
    [DebuggerDisplay( "Network {HostName}, port {Port}" )]
    public sealed class IrcNetwork : ConnectedObject, IDisposable
    {
        #region Property-backing fields
        private bool _isConnected;
        private string _messageOfTheDay;
        #endregion

        #region Private members
        /// <summary>
        /// Whether this instance has been disposed.
        /// </summary>
        private bool _disposed;
        #endregion

        #region Public properties
        /// <summary>
        /// Gets the <see cref="IrcClient"/> associated with the <see cref="IrcNetwork"/>.
        /// </summary>
        public IrcClient Client { get; private set; }

        /// <summary>
        /// Gets the host name of the <see cref="IrcNetwork"/>.
        /// </summary>
        public string HostName { get; private set; }

        /// <summary>
        /// Gets the port used to communicate with the <see cref="IrcNetwork"/>.
        /// </summary>
        public int Port { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the connection to the <see cref="IrcNetwork"/> uses SSL.
        /// </summary>
        public bool UsesSsl { get; private set; }

        /// <summary>
        /// Gets the parameters used by the <see cref="IrcNetwork"/>.
        /// </summary>
        public IrcNetworkParameters Parameters { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the computer is connected to the <see cref="IrcNetwork"/>.
        /// </summary>
        public bool IsConnected
        {
            get { return _isConnected; }
            internal set { SetProperty( ref _isConnected, value ); }
        }

        /// <summary>
        /// Gets the current <see cref="IrcUser"/>.
        /// </summary>
        public IrcUser CurrentUser { get; private set; }

        /// <summary>
        /// Gets the server as an <see cref="IrcUser"/>, which is the one sending messages without nickname information.
        /// </summary>
        public IrcUser ServerUser { get; private set; }

        /// <summary>
        /// Gets the <see cref="IrcNetwork"/>'s "Message Of The Day" (or MOTD).
        /// Usually, this is a static message; it does not change every day.
        /// There may be one MOTD and some other information in notices.
        /// </summary>
        public string MessageOfTheDay
        {
            get { return this._messageOfTheDay; }
            internal set { this.SetProperty( ref this._messageOfTheDay, value ); }
        }

        /// <summary>
        /// Gets the list of known <see cref="IrcChannel"/>s.
        /// </summary>
        public ReadOnlyObservableCollection<IrcChannel> KnownChannels { get; private set; }

        /// <summary>
        /// Gets the list of known <see cref="IrcUser"/>s.
        /// </summary>
        public ReadOnlyObservableCollection<IrcUser> KnownUsers { get; private set; }
        #endregion

        #region Internal properties
        /// <summary>
        /// Gets a read-write list of the writeable known channels.
        /// </summary>
        internal ObservableCollection<IrcChannel> KnownChannelsInternal { get; private set; }

        /// <summary>
        /// Gets a read-write list of the writeable known users.
        /// </summary>
        internal ObservableCollection<IrcUser> KnownUsersInternal { get; private set; }

        /// <summary>
        /// Gets the previous nickname, used when there is a nickname collision.
        /// </summary>
        internal string PreviousNickname { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user has authenticated successfully.
        /// </summary>
        internal bool IsAuthenticated { get; set; }
        #endregion

        /// <summary>
        /// Creates a new <see cref="IrcNetwork"/> using the specified <see cref="TcpWrapper"/>.
        /// </summary>
        /// <param name="wrapper">The <see cref="TcpWrapper"/>.</param>
        public IrcNetwork( TcpWrapper wrapper )
        {
            this.Client = new IrcClient( this, wrapper );
            this.SetClient( this.Client );
            this.HostName = wrapper.HostName;
            this.Port = wrapper.Port;
            this.UsesSsl = wrapper.UsesSsl;
            this.CurrentUser = new IrcUser( this.Client, this );
            this.ServerUser = new IrcUser( this.Client, this ) { Nickname = this.HostName };

            this.KnownChannelsInternal = new ObservableCollection<IrcChannel>();
            this.KnownUsersInternal = new ObservableCollection<IrcUser>();
            this.KnownChannels = new ReadOnlyObservableCollection<IrcChannel>( this.KnownChannelsInternal );
            this.KnownUsers = new ReadOnlyObservableCollection<IrcUser>( this.KnownUsersInternal );
            this.KnownUsersInternal.Add( this.CurrentUser );
            this.KnownUsersInternal.Add( this.ServerUser );
            this.Parameters = new IrcNetworkParameters();
        }

        #region Events
        /// <summary>
        /// Occurs when the connection to the <see cref="IrcNetwork"/> is lost.
        /// </summary>
        public event NetworkEventHandler<EventArgs> ConnectionLost;
        internal void OnConnectionLost()
        {
            this.Raise( this.ConnectionLost, EventArgs.Empty );
        }

        /// <summary>
        /// Occurs when a new <see cref="IrcChannel"/> is discovered.
        /// </summary>
        /// <remarks>You should subscribe to all events on the <see cref="IrcChannel"/> because this event means they are now likely to be fired.</remarks>
        public event NetworkEventHandler<ChannelEventArgs> ChannelDiscovered;
        internal void OnChannelDiscovered( IrcChannel channel )
        {
            this.Raise( this.ChannelDiscovered, new ChannelEventArgs( channel ) );
        }

        /// <summary>
        /// Occurs when a new <see cref="IrcUser"/> is discovered.
        /// </summary>
        /// <remarks>
        /// You should subscribe to all events on the <see cref="IrcUser"/> because this event means they are now likely to be fired.
        /// This event is only fired when the <see cref="IrcUser"/> starts interacting directly with the current <see cref="IrcUser"/>.
        /// </remarks>
        public event NetworkEventHandler<UserEventArgs> UserDiscovered;
        internal void OnUserDiscovered( IrcUser user )
        {
            this.Raise( this.UserDiscovered, new UserEventArgs( user ) );
        }

        /// <summary>
        /// Occurs when an error message is received.
        /// </summary>
        public event NetworkEventHandler<InformationReceivedEventArgs> ErrorReceived;
        internal void OnErrorReceived( string errorMessage, string command )
        {
            this.Raise( this.ErrorReceived, new InformationReceivedEventArgs( errorMessage, command ) );
        }

        /// <summary>
        /// Occurs when an information message is received.
        /// </summary>
        public event NetworkEventHandler<InformationReceivedEventArgs> InformationReceived;
        internal void OnInformationReceived( string message, string command )
        {
            this.Raise( this.InformationReceived, new InformationReceivedEventArgs( message, command ) );
        }

        /// <summary>
        /// Occurs when the list of <see cref="IrcChannel"/>s is received.
        /// </summary>
        public event NetworkEventHandler<ChannelListReceivedEventArgs> ChannelListReceived;
        internal void OnChannelListReceived( List<IrcChannel> channels )
        {
            this.Raise( this.ChannelListReceived, new ChannelListReceivedEventArgs( channels ) );
        }

        /// <summary>
        /// Occurs when the nickname given to <see cref="ChangeNickname"/> or <see cref="ConnectAsync"/> is already in use. 
        /// If <see cref="IrcClient.RetryOnNicknameCollision"/> is set to true, another nickname (determined by the <see cref="IrcClient.NicknameCollisionTransform"/> function will be tried.
        /// </summary>
        public event NetworkEventHandler<NicknameCollisionEventArgs> NicknameCollision;
        internal void OnNicknameCollision( string usedNickname )
        {
            this.Raise( this.NicknameCollision, new NicknameCollisionEventArgs( usedNickname ) );
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Gets a known <see cref="IrcUser"/> from their name.
        /// </summary>
        /// <param name="nickname">The user name.</param>
        /// <returns>The <see cref="IrcUser"/>.</returns>
        public IrcUser GetUser( string nickname )
        {
            Validate.IsNotDisposed( this._disposed );
            Validate.HasText( nickname, "nickname" );
            IrcValidate.IsConnected( this.IsConnected );

            var user = this.KnownUsers.FirstOrDefault( u => this.Parameters.CaseMapping.AreEqual( u.Nickname, nickname ) );

            if ( user == null )
            {
                user = new IrcUser( this.Client, this ) { Nickname = nickname };
                this.KnownUsersInternal.Add( user );
            }

            return user;
        }

        /// <summary>
        /// Gets a known <see cref="IrcChannel"/> from its name.
        /// </summary>
        /// <param name="name">The channel name.</param>
        /// <returns>The <see cref="IrcChannel"/>.</returns>
        public IrcChannel GetChannel( string name )
        {
            Validate.IsNotDisposed( this._disposed );
            Validate.HasText( name, "name" );
            IrcValidate.IsChannelName( name, this.Parameters, "name" );
            IrcValidate.IsConnected( this.IsConnected );

            var channel = this.KnownChannels.FirstOrDefault( chan => this.Parameters.CaseMapping.AreEqual( chan.FullName, name ) );
            if ( channel == null )
            {
                channel = new IrcChannel( this.Client, this, name );
                this.KnownChannelsInternal.Add( channel );
            }

            return channel;
        }

        /// <summary>
        /// Asynchronously connects using the specified information.
        /// </summary>
        /// <param name="nickname">The desired nickname.</param>
        /// <param name="realName">The desired real name.</param>
        /// <param name="mode">The desired mode. Most modes must be set after connecting.</param>
        /// <param name="password">Optional. The password, if any.</param>
        /// <returns>True if the connection attempt was successful; false otherwise.</returns>
        public async Task<bool> ConnectAsync( string nickname, string realName, IrcUserLoginModes mode, string password = "" )
        {
            Validate.IsNotDisposed( this._disposed );
            Validate.HasText( nickname, "nickname" );
            Validate.HasText( realName, "realName" );
            Validate.IsEnumValue( mode, "mode" );
            Validate.IsNotNull( password, "password" );
            IrcValidate.IsNotConnected( this.IsConnected );

            if ( await this.Client.ConnectAsync() )
            {
                this.IsConnected = true;

                if ( password.HasText() )
                {
                    this.SendPassword( password );
                }
                this.SendUserNames( nickname, realName, mode );
                this.ChangeNickname( nickname );
                this.CurrentUser.Nickname = nickname;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Requests a nickname change to the server.
        /// </summary>
        /// <param name="newNickname">The desired nickname.</param>
        public void ChangeNickname( string newNickname )
        {
            Validate.IsNotDisposed( this._disposed );
            Validate.HasText( newNickname, "newNickname" );
            IrcValidate.IsConnected( this.IsConnected );

            this.PreviousNickname = this.CurrentUser.Nickname ?? newNickname;
            this.CurrentUser.Nickname = newNickname;

            // NICK <new nick>
            this.Send( "NICK", newNickname );
        }

        /// <summary>
        /// Requests operator rights to the server. 
        /// Optional part of the authentication process.
        /// </summary>
        /// <param name="userName">The username.</param>
        /// <param name="password">The operator password.</param>
        public void AuthenticateAsOperator( string userName, string password )
        {
            Validate.IsNotDisposed( this._disposed );
            Validate.HasText( userName, "userName" );
            Validate.HasText( password, "password" );
            IrcValidate.IsConnected( this.IsConnected );

            // OPER <user name> <password>
            this.Send( "OPER", userName, password );
        }

        /// <summary>
        /// Attempts to connect as a service.
        /// </summary>
        /// <param name="nickname">The nickname of the service.</param>
        /// <param name="distribution">A mask specifying which hosts have access to the service.</param>
        /// <param name="type">The service type.</param>
        /// <param name="info">Information about the service.</param>
        /// <returns>True if the connection attempt was successful; false otherwise.</returns>
        /// <remarks>This method should be called instead of <see cref="ConnectAsync"/> if you are a service.</remarks>
        public async Task<bool> ConnectAsServiceAsync( string nickname, string distribution, string type, string info )
        {
            Validate.IsNotDisposed( this._disposed );
            Validate.HasText( nickname, "nickname" );
            Validate.HasText( distribution, "distribution" );
            Validate.HasText( type, "type" );
            Validate.HasText( info, "info" );
            IrcValidate.IsNotConnected( this.IsConnected );

            if ( await this.Client.ConnectAsync() )
            {
                // SERVICE <nickname> * <distribution> <type> * :<info>
                // RFC 2812 doesn't say there is a ':' char before the "info" parameter, but the example has one, and it seems logical
                this.Send( "SERVICE", nickname, "*", distribution, type, "*", ":", info );

                return true;
            }

            return false;
        }

        /// <summary>
        /// Leaves all channels, but does not quit the <see cref="IrcNetwork"/>.
        /// </summary>
        public void LeaveAllChannels()
        {
            Validate.IsNotDisposed( this._disposed );
            IrcValidate.IsConnected( this.IsConnected );

            // JOIN 0
            // It means "part all channels but do not disconnect". Go figure.
            this.Send( "JOIN", "0" );
        }

        /// <summary>
        /// Quits the <see cref="IrcNetwork"/>.
        /// </summary>
        /// <param name="reason">Optional. The reason, if any.</param>
        public void Quit( string reason = "" )
        {
            Validate.IsNotDisposed( this._disposed );
            Validate.IsNotNull( reason, "reason" );
            IrcValidate.IsConnected( this.IsConnected );

            // QUIT[ :reason]
            if ( reason.HasText() )
            {
                this.Send( "QUIT", ":", reason );
            }
            else
            {
                this.Send( "QUIT" );
            }

            this.Client.Stop( false );
        }
        #endregion

        #region Internal methods
        /// <summary>
        /// Gets a known user from his/her full name, which may include the username and host.
        /// </summary>
        internal IrcUser GetUserFromFullName( string fullName )
        {
            bool hasUserSeparator = fullName.Contains( IrcUtils.UserNameSeparator );
            bool hasHostSeparator = fullName.Contains( IrcUtils.UserHostSeparator );

            if ( hasUserSeparator && hasHostSeparator )
            {
                // full name, e.g. "Alice!Bob@irc.example.com"
                string[] parts = fullName.Split( IrcUtils.FullNameSeparators, StringSplitOptions.None );
                string nick = parts[0];
                string userName = parts[1];
                string host = parts[2];

                var user = this.KnownUsers.FirstOrDefault( u => u.Host == host );

                if ( user == null && nick == this.PreviousNickname )
                {
                    user = this.CurrentUser;
                }
                else if ( user == null || user.Nickname != nick ) // sadly, hosts are not required to be unique
                {
                    user = this.GetUser( nick );
                    user.Host = host;
                    user.UserName = userName;
                }

                return user;
            }
            if ( hasUserSeparator || hasHostSeparator )
            {
                throw new ArgumentException( "The sender is invalid. Either none or both nick-user and user-host separators must be present." );
            }

            return this.GetUser( fullName );
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Sends a password to the server.
        /// This is an optional part of the authentication process.
        /// </summary>
        private void SendPassword( string password )
        {
            // PASS <password>
            this.Send( "PASS", password );
        }

        /// <summary>
        /// Authenticates using a nickname and a real name, as well as the desired mode.
        /// This is a mandatory part of the authentication process.
        /// </summary>
        private void SendUserNames( string userName, string realName, IrcUserLoginModes mode )
        {
            this.CurrentUser.UserName = userName;
            this.CurrentUser.RealName = realName;

            // USER <user name> <mode> * :<real name>
            this.Send( "USER", userName, ( (int) mode ).ToString(), "*", ":", realName );
        }

        /// <summary>
        /// Resets the state of the <see cref="IrcNetwork"/>.
        /// </summary>
        private void Reset()
        {
            foreach ( var channel in this.KnownChannels )
            {
                channel.UserModes.Clear();
            }
            this.MessageOfTheDay = null;
        }

        /// <summary>
        /// Raises an event.
        /// </summary>
        private void Raise<T>( NetworkEventHandler<T> handler, T args )
            where T : EventArgs
        {
            if ( handler != null )
            {
                handler( this, args );
            }
        }
        #endregion

        #region IDisposable implementation
        /// <summary>
        /// Finalizes this instance.
        /// </summary>
        ~IrcNetwork()
        {
            this.Dispose( false );
        }

        /// <summary>
        /// Disposes of the <see cref="IrcClient"/>'s resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose( true );
            GC.SuppressFinalize( this );
        }

        /// <summary>
        /// Disposes of the <see cref="IrcClient"/>'s managed and unmanaged resources.
        /// </summary>
        private void Dispose( bool isDisposing )
        {
            if ( !this._disposed )
            {
                this._disposed = true;
                this.IsConnected = false;
                this.Client.Dispose();
            }
        }
        #endregion
    }
}