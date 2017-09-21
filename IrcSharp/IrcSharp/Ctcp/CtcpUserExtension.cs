// Copyright (C) 2012-2013, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Collections.Generic;
using IrcSharp.Internals;
using IrcSharp.Validation;

namespace IrcSharp.Ctcp
{
    /// <summary>
    /// A class providing CTCP functionality for <see cref="IrcUser"/>s.
    /// </summary>
    public sealed class CtcpUserExtension
    {
        #region Private members
        /// <summary>
        /// The user these <see cref="CtcpUserExtension"/> are for.
        /// </summary>
        private readonly IrcUser _user;
        #endregion

        #region Internal properties
        /// <summary>
        /// Gets the ping times.
        /// </summary>
        internal Queue<long> PingTimes { get; private set; }
        #endregion

        internal CtcpUserExtension( IrcUser user )
        {
            this._user = user;
            this.PingTimes = new Queue<long>();
        }

        #region Events
        /// <summary>
        /// Occurs when the <see cref="IrcUser"/> sends an action message in a private conversation with the current user.
        /// </summary>
        public event UserEventHandler<PrivateMessageReceivedEventArgs> ActionReceived;
        internal void OnActionReceived( string message )
        {
            this._user.Raise( ref this.ActionReceived, new PrivateMessageReceivedEventArgs( message ) );
        }

        /// <summary>
        /// Occurs when a reply to <see cref="GetLocalTime"/> is received.
        /// </summary>
        public event UserEventHandler<LocalTimeReceivedEventArgs> LocalTimeReceived;
        internal void OnLocalTimeReceived( string time )
        {
            this._user.Raise( ref this.LocalTimeReceived, new LocalTimeReceivedEventArgs( time ) );
        }

        /// <summary>
        /// Occurs when a reply to <see cref="GetInformation"/> is received.
        /// </summary>
        public event UserEventHandler<UserInfoReplyReceivedEventArgs> InformationReceived;
        internal void OnInformationReceived( string userInfo )
        {
            this._user.Raise( ref this.InformationReceived, new UserInfoReplyReceivedEventArgs( userInfo ) );
        }

        /// <summary>
        /// Occurs when a reply to <see cref="GetClientCapabilities"/> is received.
        /// </summary>
        public event UserEventHandler<ClientCapabilitiesReceivedEventArgs> ClientCapabilitiesReceived;
        internal void OnCapabilitiesReceived( string clientInfo )
        {
            this._user.Raise( ref this.ClientCapabilitiesReceived, new ClientCapabilitiesReceivedEventArgs( clientInfo ) );
        }

        /// <summary>
        /// Occurs when a reply to <see cref="Ping"/> is received.
        /// </summary>
        public event UserEventHandler<PingReplyReceivedEventArgs> PingReplyReceived;
        internal void OnPingReplyReceived( TimeSpan replyTime )
        {
            this._user.Raise( ref this.PingReplyReceived, new PingReplyReceivedEventArgs( replyTime ) );
        }

        /// <summary>
        /// Occurs when a reply to <see cref="GetClientLocation"/> is received.
        /// </summary>
        public event UserEventHandler<ClientLocationReceivedEventArgs> ClientLocationReceived;
        internal void OnSourceReplyReceived( string clientLocation )
        {
            this._user.Raise( ref this.ClientLocationReceived, new ClientLocationReceivedEventArgs( clientLocation ) );
        }


        /// <summary>
        /// Occurs when a reply to <see cref="GetClientInformation"/> is received.
        /// </summary>
        public event UserEventHandler<ClientInformationReceivedEventArgs> ClientInformationReceived;
        internal void OnVersionReplyReceived( string version )
        {
            this._user.Raise( ref this.ClientInformationReceived, new ClientInformationReceivedEventArgs( version ) );
        }

        /// <summary>
        /// Occurs when an error message is received from the <see cref="IrcUser"/> often in reaction to an invalid query.
        /// </summary>
        public event UserEventHandler<ErrorMessageReceivedEventArgs> ErrorMessageReceived;
        internal void OnErrorMessageReceived( string error )
        {
            this._user.Raise( ref this.ErrorMessageReceived, new ErrorMessageReceivedEventArgs( error ) );
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Sends an action message to the <see cref="IrcUser"/>.
        /// </summary>
        /// <param name="text">The action text.</param>
        public void SendAction( string text )
        {
            Validate.HasText( text, "text" );

            this.SendMessage( "ACTION " + text );
        }

        /// <summary>
        /// Requests the <see cref="IrcUser"/>'s local time.
        /// </summary>
        public void GetLocalTime()
        {
            this.SendMessage( "TIME" );
        }

        /// <summary>
        /// Requests information about the <see cref="IrcUser"/>.
        /// </summary>
        public void GetInformation()
        {
            this.SendMessage( "USERINFO" );
        }

        /// <summary>
        /// Requests information about the capabilities of the <see cref="IrcUser"/>'s IRC client.
        /// </summary>
        /// <param name="command">Optional. The command to query about, or an empty string to ask for a list of all supported commands.</param>
        public void GetClientCapabilities( string command = "" )
        {
            Validate.IsNotNull( command, "command" );

            command = command.HasText() ? ": " + command : string.Empty;
            this.SendMessage( "CLIENTINFO" + command );
        }

        /// <summary>
        /// Sends a ping to the <see cref="IrcUser"/>.
        /// This is used to measure the lag between the <see cref="IrcUser"/> and the current user.
        /// The lag is the time a message takes from one computer to another through the network.
        /// </summary>
        public void Ping()
        {
            long time = TimeHelper.DateTimeToUnixMilliseconds( DateTime.UtcNow );
            this.PingTimes.Enqueue( time );

            this.SendMessage( "PING " + time.ToString() );
        }

        /// <summary>
        /// Requests information about the download location of the <see cref="IrcUser"/>'s IRC client.
        /// </summary>
        public void GetClientLocation()
        {
            this.SendMessage( "SOURCE" );
        }

        /// <summary>
        /// Requests information about the <see cref="IrcUser"/>'s IRC client.
        /// </summary>
        public void GetClientInformation()
        {
            this.SendMessage( "VERSION" );
        }

        /// <summary>
        /// Sends an error message to the <see cref="IrcUser"/>. 
        /// This should only be used when answering a command.
        /// </summary>
        /// <param name="text">The message content.</param>
        public void SendErrorMessage( string text )
        {
            Validate.HasText( text, "text" );

            this.SendNotice( "ERRMSG :" + text );
        }

        /// <summary>
        /// Sends a CTCP private message.
        /// </summary>
        /// <param name="text">The message text.</param>
        /// <remarks>This is a low-level method that should be used to send custom CTCP commands if needed.</remarks>
        public void SendMessage( string text )
        {
            Validate.HasText( text, "text" );

            this._user.SendMessage( CtcpUtils.EncodeMessage( text ) );
        }

        /// <summary>
        /// Sends a CTCP notice.
        /// </summary>
        /// <param name="text">The notice text.</param>
        /// <remarks>This is a low-level method that should be used to send custom CTCP commands if needed.</remarks>
        public void SendNotice( string text )
        {
            Validate.HasText( text, "text" );

            this._user.SendNotice( CtcpUtils.EncodeMessage( text ) );
        }
        #endregion
    }
}