// Copyright (C) 2011-2013, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using IrcSharp.Ctcp;
using IrcSharp.Internals;
using IrcSharp.Validation;

namespace IrcSharp
{
    /// <summary>
    /// An IRC channel.
    /// </summary>
    [DebuggerDisplay( "{FullName}" )]
    public sealed class IrcChannel : ConnectedObject
    {
        #region Constants
        /// <summary>
        /// The chars forbidden in channel names.
        /// </summary>
        internal static readonly char[] ForbiddenNameChars =  
        { 
            ' ', ',', ':', '\n', '\r', '\0', '\a' 
        };

        /// <summary>
        /// The chars forbidden in channel keys.
        /// </summary>
        private static readonly char[] ForbiddenKeyChars =
        {
            '\0', '\r', '\n', '\f', '\t', '\v', ' '
        };

        /// <summary>
        /// The standard channel prefix. 99% of channels are standard.
        /// </summary>
        internal const char StandardChannelPrefix = '#';

        /// <summary>
        /// The <see cref="IrcChannelVisibility"/> values corresponding to visibility identifiers.
        /// </summary>
        private static readonly Dictionary<char, IrcChannelVisibility> Visibilities = new Dictionary<char, IrcChannelVisibility>
        {
            { '=', IrcChannelVisibility.Normal },
            { '*', IrcChannelVisibility.Private },
            { '@', IrcChannelVisibility.Secret }
        };
        #endregion

        #region Private members
        /// <summary>
        /// Indicates whether the <see cref="IrcChannel"/> is active, i.e. has triggered any events.
        /// </summary>
        private bool _isActive;
        #endregion

        #region Property-backing fields
        private string _key;
        private IrcChannelVisibility _visibility;
        private DateTime? _creationDate;
        private int? _userLimit;
        private readonly ObservableCollection<char> _modes;
        #endregion

        #region Internal properties
        /// <summary>
        /// Gets a writeable collection of the ban masks set on the <see cref="IrcChannel"/>.
        /// </summary>
        internal ObservableCollectionEx<string> BanMasksInternal { get; private set; }

        /// <summary>
        /// Gets a writeable collection of the ban exceptions set on the <see cref="IrcChannel"/>.
        /// </summary>
        internal ObservableCollectionEx<string> BanExceptionsInternal { get; private set; }

        /// <summary>
        /// Gets a writeable collection of the invite exceptions set on the <see cref="IrcChannel"/>.
        /// </summary>
        internal ObservableCollectionEx<string> InviteExceptionsInternal { get; private set; }
        #endregion

        #region Public properties
        /// <summary>
        /// Gets the <see cref="IrcNetwork"/> the <see cref="IrcChannel"/> belongs to.
        /// </summary>
        public IrcNetwork Network { get; private set; }

        /// <summary>
        /// Gets the kind of the <see cref="IrcChannel"/>.
        /// </summary>
        public IrcChannelKind Kind { get; private set; }

        /// <summary>
        /// Gets the full name of of the <see cref="IrcChannel"/>, including the prefix.
        /// </summary>
        public string FullName { get; private set; }

        /// <summary>
        /// Gets the key used to join the <see cref="IrcChannel"/>.
        /// </summary>
        public string Key
        {
            get { return this._key; }
            set { this.SetProperty( ref this._key, value ); }
        }

        /// <summary>
        /// Gets the name of the <see cref="IrcChannel"/>.
        /// The name does not include the prefix.
        /// </summary>
        public string Name
        {
            get { return this.FullName.Substring( 1 ); }
        }

        /// <summary>
        /// Gets the <see cref="IrcChannel"/>'s topic.
        /// </summary>
        public IrcChannelTopic Topic { get; private set; }

        /// <summary>
        /// Gets the <see cref="IrcUser"/>s that are known to be on the <see cref="IrcChannel"/>.
        /// </summary>
        public ReadOnlyObservableCollection<IrcUser> Users
        {
            get { return this.UserModes.Users; }
        }

        /// <summary>
        /// Gets the modes of <see cref="IrcUser"/>s that are known to be on the <see cref="IrcChannel"/>.
        /// </summary>
        public IrcUserModeCollection UserModes { get; private set; }

        /// <summary>
        /// Gets the visibility of the <see cref="IrcChannel"/>.
        /// </summary>
        public IrcChannelVisibility Visibility
        {
            get { return this._visibility; }
            private set { this.SetProperty( ref this._visibility, value ); }
        }

        /// <summary>
        /// Gets the date at which the <see cref="IrcChannel"/> was created, if the network provided it.
        /// </summary>
        public DateTime? CreationDate
        {
            get { return this._creationDate; }
            set { this.SetProperty( ref this._creationDate, value ); }
        }

        /// <summary>
        /// Gets the user limit of the <see cref="IrcChannel"/>, which may be null.
        /// </summary>
        public int? UserLimit
        {
            get { return this._userLimit; }
            private set { this.SetProperty( ref this._userLimit, value ); }
        }

        /// <summary>
        /// Gets the modes set on of the <see cref="IrcChannel"/>.
        /// This does not include <see cref="IrcChannelUserModes"/> such as operator privileges.
        /// </summary>
        public ReadOnlyObservableCollection<char> Modes { get; private set; }

        /// <summary>
        /// Gets the known ban masks set on the <see cref="IrcChannel"/>.
        /// <see cref="IrcUser"/>s matching one of these masks will not be able to join it.
        /// </summary>
        public ReadOnlyObservableCollection<string> BanMasks { get; private set; }

        /// <summary>
        /// Gets the known ban exceptions set on the <see cref="IrcChannel"/>.
        /// <see cref="IrcUser"/>s matching one of the ban masks but also one of the ban exceptions will be able to join the channel normally.
        /// </summary>
        public ReadOnlyObservableCollection<string> BanExceptions { get; private set; }

        /// <summary>
        /// Gets the known invite exceptions set on the <see cref="IrcChannel"/>.
        /// If the <see cref="IrcChannel"/> is invite-only, <see cref="IrcUser"/>s matching one of these masks will be able to join it normally.
        /// </summary>
        public ReadOnlyObservableCollection<string> InviteExceptions { get; private set; }

        /// <summary>
        /// Gets CTCP extensions to the <see cref="IrcChannel"/>.
        /// </summary>
        public CtcpChannelExtension Ctcp { get; private set; }
        #endregion

        internal IrcChannel( IrcClient client, IrcNetwork network, string fullName )
            : base( client )
        {
            this.Network = network;
            this.Key = string.Empty;
            this.FullName = fullName;
            this.Kind = this.Network.Parameters.ChannelKinds[fullName[0]];
            this.Topic = new IrcChannelTopic();

            this._modes = new ObservableCollection<char>();
            this.BanMasksInternal = new ObservableCollectionEx<string>();
            this.BanExceptionsInternal = new ObservableCollectionEx<string>();
            this.InviteExceptionsInternal = new ObservableCollectionEx<string>();

            this.Modes = new ReadOnlyObservableCollection<char>( this._modes );
            this.UserModes = new IrcUserModeCollection();
            this.BanMasks = new ReadOnlyObservableCollection<string>( this.BanMasksInternal );
            this.BanExceptions = new ReadOnlyObservableCollection<string>( this.BanExceptionsInternal );
            this.InviteExceptions = new ReadOnlyObservableCollection<string>( this.InviteExceptionsInternal );

            this.Ctcp = new CtcpChannelExtension( this );
        }

        #region Events
        /// <summary>
        /// Occurs when a message is sent by an <see cref="IrcUser"/> to the <see cref="IrcChannel"/>.
        /// </summary>
        public event ChannelEventHandler<MessageReceivedEventArgs> MessageReceived;
        internal void OnMessageReceived( IrcUser sender, string message )
        {
            this.Raise( ref this.MessageReceived, new MessageReceivedEventArgs( sender, message ) );
        }

        /// <summary>
        /// Occurs when an invite to the <see cref="IrcChannel"/> is received.
        /// </summary>
        public event ChannelEventHandler<InviteReceivedEventArgs> InviteReceived;
        internal void OnInviteReceived( IrcUser sender )
        {
            this.Raise( ref this.InviteReceived, new InviteReceivedEventArgs( sender ) );
        }

        /// <summary>
        /// Occurs when an <see cref="IrcUser"/> joins the <see cref="IrcChannel"/>.
        /// </summary>
        public event ChannelEventHandler<UserEventArgs> UserJoined;
        internal void OnUserJoined( IrcUser user )
        {
            this.Raise( ref this.UserJoined, new UserEventArgs( user ) );
        }

        /// <summary>
        /// Occurs when an <see cref="IrcUser"/> leaves the <see cref="IrcChannel"/>.
        /// </summary>
        public event ChannelEventHandler<UserLeftEventArgs> UserLeft;
        internal void OnUserLeft( IrcUser user, string reason )
        {
            this.Raise( ref this.UserLeft, new UserLeftEventArgs( user, reason ) );
        }

        /// <summary>
        /// Occurs when an <see cref="IrcUser"/> is forced to leave the <see cref="IrcChannel"/> by another <see cref="IrcUser"/>.
        /// </summary>
        public event ChannelEventHandler<UserKickedEventArgs> UserKicked;
        internal void OnUserKicked( IrcUser kickedUser, IrcUser kicker, string reason )
        {
            this.Raise( ref this.UserKicked, new UserKickedEventArgs( kickedUser, kicker, reason ) );
        }

        /// <summary>
        /// Occurs when the <see cref="IrcChannel"/> has its mode set.
        /// </summary>
        public event ChannelEventHandler<ModeChangedEventArgs> ModeChanged;
        internal void OnModeChanged( string mode, IrcUser setter )
        {
            this.Raise( ref this.ModeChanged, new ModeChangedEventArgs( mode, setter ) );
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Joins the <see cref="IrcChannel"/>.
        /// </summary>
        /// <param name="key">
        /// Optional. The channel key. If set, the <see cref="Key"/> property will be set. 
        /// If the <see cref="Key"/> property is set and no parameter is given, the property will be used.
        /// </param>
        public void Join( string key = "" )
        {
            Validate.IsNotNull( key, "key" );
            if ( key.HasText() )
            {
                this.Key = key;
            }

            // JOIN <channel name> [key]
            this.Send( "JOIN", this.FullName, this.Key ?? string.Empty );
        }

        /// <summary>
        /// Leaves the <see cref="IrcChannel"/>.
        /// </summary>
        /// <param name="reason">Optional. The reason, if any.</param>
        public void Leave( string reason = "" )
        {
            Validate.IsNotNull( reason, "reason" );

            reason = reason.HasText() ? ":" + reason : string.Empty;
            // PART <channel name> [:reason]
            this.Send( "PART", this.FullName, reason );
        }

        /// <summary>
        /// Sends a message to the <see cref="IrcChannel"/>.
        /// </summary>
        /// <param name="text">The message text.</param>
        public void SendMessage( string text )
        {
            Validate.IsNotNull( text, "text" );

            // PRIVMSG <channel name> :<text>
            this.Send( "PRIVMSG", this.FullName, ":", text );
        }

        /// <summary>
        /// Invites the specified <see cref="IrcUser"/> to the <see cref="IrcChannel"/>.
        /// </summary>
        /// <param name="user">The <see cref="IrcUser"/>.</param>
        /// <remarks>
        /// If the <see cref="IrcChannel"/> is not empty, the current <see cref="IrcUser"/> has to be a member of it for this method to work. 
        /// If the <see cref="IrcChannel"/> has the <see cref="IrcChannelModes.InviteOnly"/> flag set, this method will only work if the current <see cref="IrcUser"/> is an operator on the <see cref="IrcChannel"/>.
        /// </remarks>
        public void InviteUser( IrcUser user )
        {
            Validate.IsNotNull( user, "user" );

            // INVITE <nickname> <channel name>
            this.Send( "INVITE", user.Nickname, this.FullName );
        }

        /// <summary>
        /// Queries the <see cref="IrcChannel"/>'s information: ban list, ban exception list and invite exception list.
        /// </summary>
        /// <remarks>On some non-standards-compliant servers, this may return one or more <see cref="IrcErrorCode.UnknownChannelMode"/> messages.</remarks>
        public void UpdateInformation()
        {
            // modes
            // MODE <channel name>
            this.Send( "MODE", this.FullName );

            // ban list
            // MODE <channel name> +b
            this.Send( "MODE", this.FullName, "+b" );

            if ( this.Network.Parameters.AreBanExceptionsEnabled )
            {
                // ban exception list
                // MODE <channel name> e
                string text = this.Network.Parameters.ChannelModes.BanExceptionMode.ToString();
                this.Send( "MODE", this.FullName, text );
            }

            if ( this.Network.Parameters.AreInviteExceptionsEnabled )
            {
                // invite exception list
                // MODE <channel name> I
                string text = this.Network.Parameters.ChannelModes.InviteExceptionMode.ToString();
                this.Send( "MODE", this.FullName, text );
            }

            // The '+' is only there for the ban list.
            // See http://tools.ietf.org/search/rfc2812#section-3.2.3
        }

        /// <summary>
        /// Attempts to set the <see cref="IrcChannel"/>'s topic.
        /// </summary>
        /// <param name="topic">The topic.</param>
        /// <remarks>
        /// This method will only work if the current <see cref="IrcUser"/> is an operator on the <see cref="IrcChannel"/> or if the <see cref="IrcChannel"/>'s mode does not include the <see cref="IrcChannelModes.TopicSetByOps"/> flag.
        /// </remarks>
        public void SetTopic( string topic )
        {
            Validate.IsNotNull( topic, "topic" );

            // TOPIC <channel name> :<topic message>
            this.Send( "TOPIC", this.FullName, ":", topic );
        }

        /// <summary>
        /// Attempts to clear the <see cref="IrcChannel"/>'s topic.
        /// </summary>
        /// <remarks>
        /// This method will only work if the current <see cref="IrcUser"/> is an operator on the <see cref="IrcChannel"/> or if the <see cref="IrcChannel"/>'s mode does not include the <see cref="IrcChannelModes.TopicSetByOps"/> flag.
        /// </remarks>
        public void ClearTopic()
        {
            this.SetTopic( string.Empty );
        }

        /// <summary>
        /// Attempts to kick the specified <see cref="IrcUser"/> from the <see cref="IrcChannel"/>.
        /// </summary>
        /// <param name="user">The <see cref="IrcUser"/>.</param>
        /// <param name="reason">Optional. The reason, if any.</param>
        /// <remarks>This method will only work if the current <see cref="IrcUser"/> is an operator on the <see cref="IrcChannel"/>.</remarks>
        public void KickUser( IrcUser user, string reason = "" )
        {
            Validate.IsNotNull( user, "user" );
            Validate.IsNotNull( reason, "reason" );

            reason = reason.HasText() ? ":" + reason : string.Empty;
            // KICK <channel name> <user name>[ :<reason>]
            this.Send( "KICK", this.FullName, user.Nickname, reason );
        }

        /// <summary>
        /// Attempts to ban the specified <see cref="IrcUser"/> from the <see cref="IrcChannel"/>.
        /// </summary>
        /// <param name="user">The <see cref="IrcUser"/>.</param>
        /// <param name="alsoKick">Optional. Whether the <see cref="IrcUser"/> should also be kicked out of the channel. True by default.</param>
        /// <param name="kickReason">Optional. If the <paramref name="alsoKick"/> parameter is set to true, the reason for the kick, if any.</param>
        /// <remarks>This method will only work if the current user is an operator on the <see cref="IrcChannel"/>.</remarks>
        public void BanUser( IrcUser user, bool alsoKick = true, string kickReason = "" )
        {
            Validate.IsNotNull( user, "user" );
            Validate.IsNotNull( kickReason, "kickReason" );

            // MODE <channel name> +b <user name>
            this.Send( "MODE", this.FullName, "+b", user.Nickname );

            if ( alsoKick )
            {
                this.KickUser( user, kickReason );
            }
        }

        /// <summary>
        /// Attempts to un-ban the specified <see cref="IrcUser"/> from the <see cref="IrcChannel"/>.
        /// </summary>
        /// <param name="user">The <see cref="IrcUser"/>.</param>
        public void UnbanUser( IrcUser user )
        {
            Validate.IsNotNull( user, "user" );

            // MODE <channel name> -b <user name>
            this.Send( "MODE", this.FullName, "-b", user.Nickname );
        }

        /// <summary>
        /// Attempts to add the specified flag to the <see cref="IrcChannel"/>'s mode.
        /// </summary>
        /// <param name="mode">The mode flag.</param>
        /// <remarks>This method will only work if the current <see cref="IrcUser"/> is an operator in the <see cref="IrcChannel"/>.</remarks>
        public void AddMode( char mode )
        {
            this.Send( "MODE", this.FullName, "+" + mode.ToString() );
        }

        /// <summary>
        /// Attempts to remove the specified flag from the <see cref="IrcChannel"/>'s mode.
        /// </summary>
        /// <param name="mode">The mode flag.</param>
        /// <remarks>This method will only work if the current <see cref="IrcUser"/> is an operator in the <see cref="IrcChannel"/>.</remarks>
        public void RemoveMode( char mode )
        {
            this.Send( "MODE", this.FullName, "-" + mode.ToString() );
        }

        /// <summary>
        /// Attempts to set the mode of the <see cref="IrcChannel"/>.
        /// </summary>
        /// <param name="mode">The mode, as a string.</param>
        /// <remarks>This method overrides all previous mode flags. It will only work if the current <see cref="IrcUser"/> is an operator in the <see cref="IrcChannel"/>.</remarks>
        public void SetMode( string mode )
        {
            Validate.HasText( mode, "mode" );

            this.Send( "MODE", this.FullName, mode );
        }

        /// <summary>
        /// Attempts to add the specified flag to the specified <see cref="IrcUser"/>'s mode on the <see cref="IrcChannel"/>.
        /// </summary>
        /// <param name="user">The <see cref="IrcUser"/>.</param>
        /// <param name="mode">The mode.</param>
        /// <remarks>This method will only work if the current <see cref="IrcUser"/> is an operator on the <see cref="IrcChannel"/>.</remarks>
        public void AddUserMode( IrcUser user, IrcChannelUserModes mode )
        {
            Validate.IsNotNull( user, "user" );
            Validate.IsEnumValue( mode, "mode" );

            if ( mode == IrcChannelUserModes.Normal )
            {
                return;
            }

            if ( !this.Network.Parameters.ChannelModes.UserModes.Any( p => p.Value == mode ) )
            {
                throw new ArgumentException( "The specified mode is not available on the network." );
            }

            string modeString = "+" + this.Network.Parameters.ChannelModes.UserModes.First( p => p.Value == mode ).Key.ToString();
            // MODE <channel name> +<mode char> <user name>
            this.Send( "MODE", this.FullName, modeString, user.Nickname );
        }

        /// <summary>
        /// Attempts to remove the specified flag from the specified <see cref="IrcUser"/>'s mode on the <see cref="IrcChannel"/>.
        /// </summary>
        /// <param name="user">The <see cref="IrcUser"/>.</param>
        /// <param name="mode">The mode.</param>
        /// <remarks>This method will only work if the current <see cref="IrcUser"/> is an operator on the <see cref="IrcChannel"/>.</remarks>
        public void RemoveUserMode( IrcUser user, IrcChannelUserModes mode )
        {
            Validate.IsNotNull( user, "user" );
            Validate.IsEnumValue( mode, "mode" );

            if ( mode == IrcChannelUserModes.Normal )
            {
                return;
            }

            if ( !this.Network.Parameters.ChannelModes.UserModes.Any( p => p.Value == mode ) )
            {
                throw new ArgumentException( "The specified mode is not available on the network." );
            }

            string modeString = "-" + this.Network.Parameters.ChannelModes.UserModes.First( p => p.Value == mode ).Key.ToString();
            // MODE <channel name> -<mode char> <user name>
            this.Send( "MODE", this.FullName, modeString, user.Nickname );
        }

        /// <summary>
        /// Requests the list of <see cref="IrcUser"/>s on the <see cref="IrcChannel"/>.
        /// </summary>
        public void GetUserList()
        {
            this.Send( "NAMES", this.FullName );
        }
        #endregion

        #region Public static methods
        /// <summary>
        /// Indicates whether the given string is a valid channel key.
        /// </summary>
        /// <param name="key">The key candidate.</param>
        /// <returns>A valid indicating whether the given string is a valid channel key.</returns>
        public static bool IsValidKey( string key )
        {
            return key.IndexOfAny( ForbiddenKeyChars ) == -1;
        }
        #endregion

        #region Internal methods
        /// <summary>
        /// Adds the specified <see cref="IrcUser"/> in the <see cref="IrcChannel"/>.
        /// </summary>
        internal void AddUser( IrcUser user )
        {
            var pair = new IrcUserChannelModePair( user, IrcChannelUserModes.Normal );
            this.UserModes.Add( pair );

            user.ChannelsInternal.Add( this );
        }

        /// <summary>
        /// Removes the specified <see cref="IrcUser"/> from the <see cref="IrcChannel"/>.
        /// </summary>
        internal void RemoveUser( IrcUser user )
        {
            this.UserModes.RemoveUser( user );
            user.ChannelsInternal.Remove( this );
        }

        /// <summary>
        /// Sets the users of the <see cref="IrcChannel"/> using a sequence of nicknames from a NAMES reply.
        /// </summary>
        internal void SetUsers( IEnumerable<string> prefixedNicknames )
        {
            // The trick here is that since IrcUserChannelModePairs implement INotifyPropertyChanged,
            // the existing ones have to be kept because someone may listen to their events
            // and obviously expect that if an user is still there, the objects representing them won't change.

            var pairs = new List<IrcUserChannelModePair>();
            foreach ( string nick in prefixedNicknames )
            {
                var split = IrcChannel.SplitName( nick, this.Network.Parameters );
                var user = this.Network.GetUser( split.Name );
                var mode = IrcChannel.GetUserMode( split.Modifier, this.Network.Parameters );
                var pair = this.UserModes.FirstOrDefault( p => p.User == user );

                if ( pair == null )
                {
                    pair = new IrcUserChannelModePair( user, mode );
                }
                else
                {
                    pair.Mode = mode;
                }

                pairs.Add( pair );

                if ( !user.Channels.Contains( this ) )
                {
                    user.ChannelsInternal.Add( this );
                }
            }

            this.UserModes.SetItems( pairs );
        }

        /// <summary>
        /// Sets the visibility of the <see cref="IrcChannel"/>.
        /// </summary>
        internal void SetVisibility( char visibilityChar )
        {
            this.Visibility = Visibilities[visibilityChar];
        }

        /// <summary>
        /// Sets the mode of the <see cref="IrcChannel"/>.
        /// </summary>
        internal void SetMode( string modeString, bool clear )
        {
            if ( clear )
            {
                this._modes.Clear();
            }

            if ( modeString.IsEmpty() )
            {
                return;
            }

            var modes = this.Network.Parameters.ChannelModes.SplitMode( modeString );

            foreach ( var mode in modes )
            {
                if ( this.Network.Parameters.ChannelModes.UserModes.ContainsKey( mode.Flag ) && mode.Argument.HasText() )
                {
                    var user = this.Network.GetUser( mode.Argument );

                    if ( user != null )
                    {
                        this.AddOrRemoveUserMode( user, mode.IsAdded, mode.Flag );
                    }
                }
                else
                {
                    this.HandleModeChange( mode.IsAdded, mode.Flag, mode.Argument );
                }
            }
        }

        /// <summary>
        /// Adds or removes a mode from the specified <see cref="IrcUser"/>'s mode on the <see cref="IrcChannel"/>.
        /// This method should be used for actual mode changes made using the MODE command.
        /// </summary>
        internal void AddOrRemoveUserMode( IrcUser user, bool add, char modeChar )
        {
            IrcChannelUserModes mode;
            if ( !this.Network.Parameters.ChannelModes.UserModes.TryGetValue( modeChar, out mode ) )
            {
                return;
            }

            if ( add )
            {
                this.UserModes[user] |= mode;
            }
            else
            {
                this.UserModes[user] &= ~mode;
            }
        }

        /// <summary>
        /// Raises an event.
        /// </summary>
        /// <remarks>
        /// Without 'ref', handlers added to <paramref name="handler"/> in a <see cref="IrcNetwork.OnUserDiscovered"/> handler will not be fired.
        /// </remarks>
        internal void Raise<T>( ref ChannelEventHandler<T> handler, T args )
            where T : EventArgs
        {
            if ( !this._isActive )
            {
                this._isActive = true;
                this.Network.OnChannelDiscovered( this );
            }

            // avoid race conditions
            var handlerCopy = handler;
            if ( handlerCopy != null )
            {
                handlerCopy( this, args );
            }
        }
        #endregion

        #region Internal static methods
        /// <summary>
        /// Attempts to find what the real name of an <see cref="IrcChannel"/> is from the specified channel name which might include modes.
        /// </summary>
        /// <remarks>
        /// The channel name may begin with the mode char associated with the user (e.g. @#example)
        /// Some networks send multiple mode chars. (e.g. @+#example)
        /// There is no way to distinguish a voiced user (+#example) from a weird but legal channel name (+#example).
        /// Thus, this method is not guaranteed to return an accurate result.
        /// </remarks>
        internal static ModifiedName GuessChannelName( string nameWithMode, IrcNetworkParameters parameters )
        {
            int prefixIndex = nameWithMode.IndexOf( StandardChannelPrefix );
            if ( prefixIndex == -1 )
            {
                do
                {
                    prefixIndex++;
                } while ( !parameters.IsChannelPrefix( nameWithMode[prefixIndex] ) );
            }

            if ( prefixIndex == 0 )
            {
                return new ModifiedName( nameWithMode, string.Empty );
            }

            string modes = nameWithMode.Substring( 0, prefixIndex );
            string name = nameWithMode.Substring( prefixIndex );
            return new ModifiedName( name, modes );
        }

        /// <summary>
        /// Gets a <see cref="IrcChannelUserModes"/> instance from prefixes preceding a name in a NAMES reply.
        /// </summary>
        internal static IrcChannelUserModes GetUserMode( string prefixes, IrcNetworkParameters parameters )
        {
            var mode = IrcChannelUserModes.Normal;

            foreach ( char c in prefixes )
            {
                mode |= parameters.ChannelModes.UserPrefixes[c];
            }

            return mode;
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Handles a mode change.
        /// </summary>
        private void HandleModeChange( bool isAdded, char modeChar, string argument )
        {
            switch ( modeChar )
            {
                case IrcChannelModes.Private:
                    this.Visibility = isAdded ? IrcChannelVisibility.Private : IrcChannelVisibility.Normal;
                    break;

                case IrcChannelModes.Secret:
                    this.Visibility = isAdded ? IrcChannelVisibility.Secret : IrcChannelVisibility.Normal;
                    break;

                case IrcChannelModes.Key:
                    this.Key = isAdded ? argument : string.Empty;
                    break;

                case IrcChannelModes.UserLimit:
                    this.UserLimit = isAdded ? (int?) int.Parse( argument ) : null;
                    break;

                case IrcChannelModes.Ban:
                case IrcChannelModes.BanException:
                case IrcChannelModes.InviteExceptions:
                    var collection = modeChar == IrcChannelModes.Ban ? this.BanMasksInternal
                                   : modeChar == IrcChannelModes.BanException ? this.BanExceptionsInternal
                                                                              : this.InviteExceptionsInternal;
                    if ( isAdded )
                    {
                        collection.Add( argument );
                    }
                    else
                    {
                        collection.Remove( argument );
                    }
                    break;

                default:
                    if ( isAdded )
                    {
                        this._modes.Add( modeChar );
                    }
                    else
                    {
                        this._modes.Remove( modeChar );
                    }
                    break;
            }
        }
        #endregion

        #region Private static methods
        /// <summary>
        /// Gets the prefixes and actual name from a prefixed channel user name.
        /// </summary>
        private static ModifiedName SplitName( string prefixedName, IrcNetworkParameters parameters )
        {
            int index = 0;
            while ( parameters.ChannelModes.UserPrefixes.ContainsKey( prefixedName[index] ) )
            {
                index++;
            }

            if ( index == 0 )
            {
                return new ModifiedName( prefixedName, string.Empty );
            }

            string prefixes = prefixedName.Substring( 0, index );
            string name = prefixedName.Substring( index );
            return new ModifiedName( name, prefixes );
        }
        #endregion
    }
}