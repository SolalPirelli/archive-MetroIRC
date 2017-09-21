// Copyright (C) 2012, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using IrcSharp.Internals;
using IrcSharp.Validation;

namespace IrcSharp.Ctcp
{
    /// <summary>
    /// A class providing CTCP functionality for <see cref="IrcChannel"/>s.
    /// </summary>
    public sealed class CtcpChannelExtension
    {
        #region Private members
        /// <summary>
        /// The <see cref="IrcChannel"/> the <see cref="CtcpChannelExtension"/> is for.
        /// </summary>
        private readonly IrcChannel _channel;
        #endregion

        internal CtcpChannelExtension( IrcChannel channel )
        {
            this._channel = channel;
        }

        #region Events
        /// <summary>
        /// Occurs when an action message is received in the <see cref="IrcChannel"/>.
        /// </summary>
        public event ChannelEventHandler<MessageReceivedEventArgs> ActionReceived;
        internal void OnActionReceived( IrcUser sender, string message )
        {
            this._channel.Raise( ref this.ActionReceived, new MessageReceivedEventArgs( sender, message ) );
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Sends an action message to the <see cref="IrcChannel"/>.
        /// </summary>
        /// <param name="text">The action text.</param>
        public void SendAction( string text )
        {
            Validate.HasText( text, "text" );

            this.SendMessage( "ACTION " + text );
        }

        /// <summary>
        /// Sends a CTCP message to the <see cref="IrcChannel"/>.
        /// </summary>
        /// <param name="text">The message text.</param>
        /// <remarks>This is a low-level method that should be used to send custom CTCP commands if needed.</remarks>
        public void SendMessage( string text )
        {
            Validate.HasText( text, "text" );

            this._channel.SendMessage( CtcpUtils.EncodeMessage( text ) );
        }
        #endregion
    }
}