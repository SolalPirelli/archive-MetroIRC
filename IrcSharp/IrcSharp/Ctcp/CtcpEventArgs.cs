// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using IrcSharp.Internals;

namespace IrcSharp.Ctcp
{
    /// <summary>
    /// Provides data for the <see cref="CtcpUserExtension.ClientCapabilitiesReceived"/> event.
    /// </summary>
    public sealed class ClientCapabilitiesReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets information about the capabilities of the <see cref="IrcUser"/>'s client.
        /// </summary>
        public string Information { get; private set; }

        internal ClientCapabilitiesReceivedEventArgs( string clientInfo )
        {
            this.Information = clientInfo;
        }
    }

    /// <summary>
    /// Provides data for the <see cref="CtcpUserExtension.ErrorMessageReceived"/> event.
    /// </summary>
    public sealed class ErrorMessageReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the error message.
        /// </summary>
        public string Error { get; private set; }

        internal ErrorMessageReceivedEventArgs( string error )
        {
            this.Error = error;
        }
    }

    /// <summary>
    /// Provides data for the <see cref="CtcpUserExtension.PingReplyReceived"/> event.
    /// </summary>
    public sealed class PingReplyReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the time taken by a message sent to the <see cref="IrcUser"/> to arrive.
        /// </summary>
        public TimeSpan ReplyTime { get; private set; }

        internal PingReplyReceivedEventArgs( TimeSpan replyTime )
        {
            this.ReplyTime = replyTime;
        }
    }

    /// <summary>
    /// Provides data for the <see cref="CtcpUserExtension.ClientLocationReceived"/> event.
    /// </summary>
    public sealed class ClientLocationReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the location at which the <see cref="IrcUser"/>'s IRC client can be downloaded, such as an FTP server or a website.
        /// </summary>
        public string ClientLocation { get; private set; }

        internal ClientLocationReceivedEventArgs( string clientLocation )
        {
            this.ClientLocation = clientLocation;
        }
    }

    /// <summary>
    /// Provides data for the <see cref="CtcpUserExtension.LocalTimeReceived"/> event.
    /// </summary>
    public sealed class LocalTimeReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the local time of the <see cref="IrcUser"/>. The format is not standardised.
        /// </summary>
        public string Time { get; private set; }

        internal LocalTimeReceivedEventArgs( string time )
        {
            this.Time = time;
        }
    }

    /// <summary>
    /// Provides data for the <see cref="CtcpUserExtension.InformationReceived"/> event.
    /// </summary>
    public sealed class UserInfoReplyReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets information about the <see cref="IrcUser"/>.
        /// </summary>
        public string Information { get; private set; }

        internal UserInfoReplyReceivedEventArgs( string info )
        {
            this.Information = info;
        }
    }

    /// <summary>
    /// Provides data for the <see cref="CtcpUserExtension.ClientInformationReceived"/> event.
    /// </summary>
    public sealed class ClientInformationReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets information about the IRC client of the <see cref="IrcUser"/>.
        /// </summary>
        public string Information { get; private set; }

        internal ClientInformationReceivedEventArgs( string clientVersion )
        {
            this.Information = clientVersion;
        }
    }

    /// <summary>
    /// Provides data for the <see cref="CtcpClient.UnknownCommandReceived"/> event.
    /// </summary>
    public sealed class UnknownCommandReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the <see cref="IrcChannel"/> in which the command was sent, if any.
        /// </summary>
        public IrcChannel Channel { get; private set; }

        /// <summary>
        /// Gets the <see cref="IrcUser"/> who sent the command.
        /// </summary>
        public IrcUser Sender { get; private set; }

        /// <summary>
        /// Gets the command.
        /// </summary>
        public string Command { get; private set; }

        /// <summary>
        /// Gets the message content. 
        /// May be empty, contain command arguments or a text message.
        /// </summary>
        public string Content { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the message is a query.
        /// Messages that are not queries must not be answered.
        /// </summary>
        public bool IsQuery { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this event was handled.
        /// If this event is not handled, the <see cref="CtcpClient"/> will inform the sender that the command is unknown.
        /// </summary>
        public bool Handled { get; set; }

        internal UnknownCommandReceivedEventArgs( CtcpMessage message )
        {
            this.Channel = message.Channel;
            this.Sender = message.Sender;
            this.IsQuery = message.IsQuery;
            this.Command = message.Command;
            this.Content = message.Content;
        }
    }
}