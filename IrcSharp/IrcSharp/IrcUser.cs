// Copyright (C) 2011-2013, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using IrcSharp.Ctcp;
using IrcSharp.Internals;
using IrcSharp.Validation;

namespace IrcSharp
{
    /// <summary>
    /// An IRC user.
    /// </summary>
    [DebuggerDisplay( "{Nickname}" )]
    public sealed class IrcUser : ConnectedObject
    {
        #region Constants
        /// <summary>
        /// The non-letter, non-digit chars that are allowed in nicknames.
        /// </summary>
        private static readonly char[] SpecialNicknameChars = { '[', ']', '\\', '`', '_', '^', '{', '|', '}' };

        /// <summary>
        /// The non-letter, non-digit char that is allowed in nicknames, except at the beginning.
        /// </summary>
        private const char SpecialNoBeginningNicknameChar = '-';
        #endregion

        #region Private members
        /// <summary>
        /// Indicates whether the <see cref="IrcUser"/> is active, i.e. has triggered any events.
        /// </summary>
        private bool _isActive;
        #endregion

        #region Internal properties
        internal ObservableCollection<IrcChannel> ChannelsInternal { get; private set; }
        #endregion

        #region Property-backing fields
        private string _nickname;
        private string _userName;
        private string _host;
        private string _realName;
        private readonly ObservableCollection<char> _modes;
        #endregion

        #region Public properties
        /// <summary>
        /// Gets the <see cref="IrcNetwork"/> the <see cref="IrcUser"/> is connected to.
        /// </summary>
        public IrcNetwork Network { get; private set; }

        /// <summary>
        /// Gets the nickname of the <see cref="IrcUser"/>.
        /// </summary>
        public string Nickname
        {
            get { return this._nickname; }
            internal set { this.SetProperty( ref this._nickname, value ); }
        }

        /// <summary>
        /// Gets the user name of the <see cref="IrcUser"/>, used when connecting.
        /// </summary>
        public string UserName
        {
            get { return this._userName; }
            internal set { this.SetProperty( ref this._userName, value ); }
        }

        /// <summary>
        /// Gets the host of the <see cref="IrcUser"/>.
        /// Usually, either a structured host, an IP address or a semi-random one.
        /// </summary>
        public string Host
        {
            get { return this._host; }
            internal set { this.SetProperty( ref this._host, value ); }
        }

        /// <summary>
        /// Gets the real name of the <see cref="IrcUser"/>. 
        /// The real name is user-defined.
        /// </summary>
        public string RealName
        {
            get { return this._realName; }
            internal set { this.SetProperty( ref this._realName, value ); }
        }

        /// <summary>
        /// Gets the mode flags of the <see cref="IrcUser"/>.
        /// </summary>
        /// <remarks>This property will most likely return a wrong value for anyone but the current user; there is no way to get an user's mode.</remarks>
        public ReadOnlyObservableCollection<char> Modes { get; private set; }

        /// <summary>
        /// Gets the <see cref="IrcChannel"/>s in which the <see cref="IrcUser"/> is known to be.
        /// </summary>
        public ReadOnlyObservableCollection<IrcChannel> Channels { get; private set; }

        /// <summary>
        /// Gets CTCP extensions to the <see cref="IrcUser"/>.
        /// </summary>
        public CtcpUserExtension Ctcp { get; private set; }
        #endregion

        internal IrcUser( IrcClient client, IrcNetwork network )
            : base( client )
        {
            this.Network = network;

            this.ChannelsInternal = new ObservableCollection<IrcChannel>();
            this.Channels = new ReadOnlyObservableCollection<IrcChannel>( this.ChannelsInternal );

            this._modes = new ObservableCollection<char>();
            this.Modes = new ReadOnlyObservableCollection<char>( this._modes );

            this.Ctcp = new CtcpUserExtension( this );
        }

        #region Events
        /// <summary>
        /// Occurs when a private message is sent by the <see cref="IrcUser"/> to the current <see cref="IrcUser"/>.
        /// </summary>
        public event UserEventHandler<PrivateMessageReceivedEventArgs> MessageReceived;
        internal void OnMessageReceived( string message )
        {
            this.Raise( ref this.MessageReceived, new PrivateMessageReceivedEventArgs( message ) );
        }

        /// <summary>
        /// Occurs when a notice is sent by the <see cref="IrcUser"/> to the current <see cref="IrcUser"/>.
        /// </summary>
        public event UserEventHandler<PrivateMessageReceivedEventArgs> NoticeReceived;
        internal void OnNoticeReceived( string message )
        {
            this.Raise( ref this.NoticeReceived, new PrivateMessageReceivedEventArgs( message ) );
        }

        /// <summary>
        /// Occurs when the <see cref="IrcUser"/> changes their nickname.
        /// </summary>
        public event UserEventHandler<NicknameChangedEventArgs> NicknameChanged;
        internal void OnNicknameChanged( string oldNick )
        {
            this.Raise( ref this.NicknameChanged, new NicknameChangedEventArgs( oldNick ) );
        }

        /// <summary>
        /// Occurs when the <see cref="IrcUser"/> has its mode set.
        /// </summary>
        public event UserEventHandler<ModeChangedEventArgs> ModeChanged;
        internal void OnModeChanged( string mode, IrcUser setter )
        {
            this.Raise( ref this.ModeChanged, new ModeChangedEventArgs( mode, setter ) );
        }

        /// <summary>
        /// Occurs when information about the <see cref="IrcUser"/> is received.
        /// </summary>
        public event UserEventHandler<UserInformationReceivedEventArgs> InformationReceived;
        internal void OnInformationReceived( UserInformationReceivedEventArgs args )
        {
            this.Raise( ref this.InformationReceived, args );
        }

        /// <summary>
        /// Occurs when the <see cref="IrcUser"/> leaves the network.
        /// </summary>
        public event UserEventHandler<UserQuitEventArgs> Quit;
        internal void OnQuit( string reason )
        {
            this.Raise( ref this.Quit, new UserQuitEventArgs( reason ) );
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Sends a message to the <see cref="IrcUser"/>.
        /// </summary>
        /// <param name="text">The message text.</param>
        public void SendMessage( string text )
        {
            Validate.IsNotNull( text, "text" );

            // PRIVMSG <user name> :<text>
            this.Send( "PRIVMSG", this.Nickname, ":", text );
        }

        /// <summary>
        /// Sends a notice to the <see cref="IrcUser"/>.
        /// </summary>
        /// <param name="text">The notice text.</param>
        public void SendNotice( string text )
        {
            Validate.IsNotNull( text, "text" );

            // NOTICE <user name> :<text>
            this.Send( "NOTICE", this.UserName, ":", text );
        }

        /// <summary>
        /// Queries information about the <see cref="IrcUser"/>: user name, host, channels, channel privileges, ...
        /// </summary>
        public void GetInformation()
        {
            // WHOIS <user name>
            this.Send( "WHOIS", this.Nickname );
        }

        /// <summary>
        /// Attempts to add the specified flag to the <see cref="IrcUser"/>'s mode.
        /// </summary>
        /// <param name="mode">The mode flag.</param>
        /// <remarks>This method only works if the <see cref="IrcUser"/> is the current <see cref="IrcUser"/>, or the current <see cref="IrcUser"/> is a server operator.</remarks>
        public void AddMode( char mode )
        {
            // MODE <user name> +<mode char>
            this.Send( "MODE", this.Nickname, "+" + mode.ToString() );
        }

        /// <summary>
        /// Attempts to remove the specified flag from the <see cref="IrcUser"/>'s mode.
        /// </summary>
        /// <param name="mode">The mode flag.</param>
        /// <remarks>This method only works if the <see cref="IrcUser"/> is the current <see cref="IrcUser"/> (unless the flag is a privilege-restricting one), or the current <see cref="IrcUser"/> is a server operator.</remarks>
        public void RemoveMode( char mode )
        {
            // MODE <user name> -<mode char>
            this.Send( "MODE", this.Nickname, "+" + mode.ToString() );
        }

        /// <summary>
        /// Attempts to set the <see cref="IrcUser"/>'s mode.
        /// </summary>
        /// <param name="mode">The mode, as a string.</param>
        /// <remarks>This method will only work if the current <see cref="IrcUser"/> is a server operator.</remarks>
        public void SetMode( string mode )
        {
            Validate.HasText( mode, "mode" );

            this.Send( "MODE", this.Nickname, mode );
        }

        /// <summary>
        /// Gets the <see cref="IrcUser"/>'s channel mode on the specified <see cref="IrcChannel"/>.
        /// </summary>
        /// <param name="channel">The <see cref="IrcChannel"/>.</param>
        /// <returns>The <see cref="IrcUser"/>'s channel mode on the specified <see cref="IrcChannel"/>.</returns>
        /// <remarks>This method may not return a correct result if the current <see cref="IrcUser"/> is not in the specified <see cref="IrcChannel"/>.</remarks>
        public IrcChannelUserModes GetChannelMode( IrcChannel channel )
        {
            Validate.IsNotNull( channel, "channel" );

            if ( channel.Users.Contains( this ) )
            {
                return channel.UserModes[this];
            }
            return IrcChannelUserModes.Normal;
        }

        /// <summary>
        /// Attempts to disconnect the <see cref="IrcUser"/> from the network.
        /// </summary>
        /// <param name="reason">Optional. The reason, if any.</param>
        /// <remarks>This method only works if the current <see cref="IrcUser"/> is a network operator.</remarks>
        public void Disconnect( string reason = "" )
        {
            Validate.IsNotNull( reason, "reason" );

            reason = reason.HasText() ? ":" + reason : string.Empty;
            // KILL <user name> [:<reason>]
            this.Send( "KILL", this.Nickname, reason );
        }
        #endregion

        #region Public static methods
        /// <summary>
        /// Checks whether the specified nickname is valid.
        /// </summary>
        /// <param name="nickname">The nickname.</param>
        /// <returns>A value indicating whether the specified nickname is valid.</returns>
        public static bool IsNicknameValid( string nickname )
        {
            Validate.HasText( nickname, "nickname" );

            return IsFirstNicknameLetterValid( nickname[0] )
                && nickname.ToCharArray().Skip( 1 ).All( IsNicknameLetterValid );
        }

        /// <summary>
        /// Attempts to get a valid nickname from a potentially valid one.
        /// </summary>
        /// <param name="candidate">The nickname candidate.</param>
        /// <returns>A valid nickname, or null if the candidate cannot be made valid.</returns>
        public static string GetValidNickname( string candidate )
        {
            Validate.HasText( candidate, "nickname" );

            candidate = new string( candidate.ToCharArray().Where( IsNicknameLetterValid ).ToArray() );

            int begin = 0;
            while ( begin < candidate.Length && !IsFirstNicknameLetterValid( candidate[begin] ) )
            {
                begin++;
            }

            return begin < candidate.Length - 1 ? candidate.Substring( begin ) : null;
        }
        #endregion

        #region Internal methods
        /// <summary>
        /// Sets this user's mode.
        /// </summary>
        internal void SetModeInternal( string modeString )
        {
            foreach ( var mode in IrcUserModes.SplitMode( modeString ) )
            {
                if ( mode.IsAdded )
                {
                    this._modes.Add( mode.Flag );
                }
                else
                {
                    this._modes.Remove( mode.Flag );
                }
            }
        }

        /// <summary>
        /// Adds a mode.
        /// </summary>
        internal void AddModeInternal( char modeChar )
        {
            this._modes.Add( modeChar );
        }

        /// <summary>
        /// Raises an event.
        /// </summary>
        /// <remarks>
        /// Without 'ref', handlers added to <paramref name="handler"/> in a <see cref="IrcNetwork.OnUserDiscovered"/> handler will not be fired.
        /// </remarks>
        internal void Raise<T>( ref UserEventHandler<T> handler, T args )
            where T : EventArgs
        {
            if ( !this._isActive )
            {
                this._isActive = true;
                this.Network.OnUserDiscovered( this );
            }

            // avoid race conditions
            var handlerCopy = handler;
            if ( handlerCopy != null )
            {
                handlerCopy( this, args );
            }
        }
        #endregion

        #region Private static methods
        /// <summary>
        /// Indicates whether the given letter is a valid first letter for a nickname.
        /// </summary>
        private static bool IsFirstNicknameLetterValid( char letter )
        {
            return CharHelper.IsBasicLetter( letter ) || SpecialNicknameChars.Contains( letter );
        }

        /// <summary>
        /// Indicates whether the given letter is a valid non-first letter for a nickname.
        /// </summary>
        private static bool IsNicknameLetterValid( char letter )
        {
            return CharHelper.IsBasicLetter( letter )
                || char.IsDigit( letter )
                || SpecialNicknameChars.Contains( letter )
                || letter == SpecialNoBeginningNicknameChar;
        }
        #endregion
    }
}