// Copyright (C) 2011-2013, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using IrcSharp.Internals;

namespace IrcSharp
{
    /// <summary>
    /// Provides data for events dealing with raw data.
    /// </summary>
    public sealed class RawDataEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the data that was sent.
        /// </summary>
        public string Data { get; private set; }

        internal RawDataEventArgs( string data )
        {
            this.Data = data;
        }
    }

    /// <summary>
    /// Provides data for events related to an <see cref="IrcChannel"/>.
    /// </summary>
    public sealed class ChannelEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the <see cref="IrcChannel"/> concerned by the event.
        /// </summary>
        public IrcChannel Channel { get; private set; }

        internal ChannelEventArgs( IrcChannel channel )
        {
            this.Channel = channel;
        }
    }

    /// <summary>
    /// Provides data for events related to an <see cref="IrcUser"/>.
    /// </summary>
    public sealed class UserEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the <see cref="IrcUser"/> concerned by the event.
        /// </summary>
        public IrcUser User { get; private set; }

        internal UserEventArgs( IrcUser user )
        {
            this.User = user;
        }
    }

    /// <summary>
    /// Provides data for events related to messages received in an <see cref="IrcChannel"/>.
    /// </summary>
    public sealed class MessageReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the <see cref="IrcUser"/> who sent the message.
        /// </summary>
        public IrcUser Sender { get; private set; }

        /// <summary>
        /// Gets the message content.
        /// </summary>
        public string Message { get; private set; }

        internal MessageReceivedEventArgs( IrcUser sender, string message )
        {
            this.Sender = sender;
            this.Message = message;
        }
    }

    /// <summary>
    /// Provides data for events related to messages received from an <see cref="IrcUser"/>.
    /// </summary>
    public sealed class PrivateMessageReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the message content.
        /// </summary>
        public string Message { get; private set; }

        internal PrivateMessageReceivedEventArgs( string message )
        {
            this.Message = message;
        }
    }

    /// <summary>
    /// Provides data for the <see cref="IrcNetwork.InformationReceived"/> and <see cref="IrcNetwork.ErrorReceived"/> events.
    /// </summary>
    public sealed class InformationReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the message content.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Gets the message command. 
        /// May be an integer.
        /// </summary>
        public string Command { get; private set; }

        internal InformationReceivedEventArgs( string message, string command )
        {
            this.Message = message;
            this.Command = command;
        }
    }

    /// <summary>
    /// Provides data for the <see cref="IrcChannel.InviteReceived"/> event.
    /// </summary>
    public sealed class InviteReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the <see cref="IrcUser"/> who sent the message.
        /// </summary>
        public IrcUser Sender { get; private set; }

        internal InviteReceivedEventArgs( IrcUser sender )
        {
            this.Sender = sender;
        }
    }

    /// <summary>
    /// Provides data for the <see cref="IrcNetwork.ChannelListReceived"/> event.
    /// </summary>
    public sealed class ChannelListReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the <see cref="IrcChannel"/>s.
        /// </summary>
        public ReadOnlyCollection<IrcChannel> Channels { get; private set; }

        internal ChannelListReceivedEventArgs( List<IrcChannel> channels )
        {
            this.Channels = new ReadOnlyCollection<IrcChannel>( channels );
        }
    }

    /// <summary>
    /// Provides data for the <see cref="IrcUser.ModeChanged"/> and <see cref="IrcChannel.ModeChanged"/> events.
    /// </summary>
    public sealed class ModeChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the mode that was set.
        /// </summary>
        public string Mode { get; private set; }

        /// <summary>
        /// Gets the <see cref="IrcUser"/> who set the mode.
        /// </summary>
        public IrcUser Setter { get; private set; }

        internal ModeChangedEventArgs( string mode, IrcUser setter )
        {
            this.Mode = mode;
            this.Setter = setter;
        }
    }


    /// <summary>
    /// Provides data for the <see cref="IrcUser.NicknameChanged"/> event.
    /// </summary>
    public sealed class NicknameChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the old nickname of the <see cref="IrcUser"/>.
        /// </summary>
        public string OldNickname { get; private set; }

        internal NicknameChangedEventArgs( string oldNick )
        {
            this.OldNickname = oldNick;
        }
    }

    /// <summary>
    /// Provides data for the <see cref="IrcNetwork.NicknameCollision"/> event.
    /// </summary>
    public sealed class NicknameCollisionEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the nickname that is already used.
        /// </summary>
        public string UsedNickname { get; private set; }

        internal NicknameCollisionEventArgs( string usedNickname )
        {
            this.UsedNickname = usedNickname;
        }
    }

    /// <summary>
    /// Provides data for the <see cref="IrcChannel.UserKicked"/> event.
    /// </summary>
    public sealed class UserKickedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the <see cref="IrcUser"/> who was kicked.
        /// </summary>
        public IrcUser KickedUser { get; private set; }

        /// <summary>
        /// Gets the <see cref="IrcUser"/> who kicked.
        /// </summary>
        public IrcUser Kicker { get; private set; }

        /// <summary>
        /// Gets the reason, if any, of the kick.
        /// </summary>
        public string Reason { get; private set; }

        internal UserKickedEventArgs( IrcUser kickedUser, IrcUser kicker, string reason )
        {
            this.KickedUser = kickedUser;
            this.Kicker = kicker;
            this.Reason = reason;
        }
    }

    /// <summary>
    /// Provides data for the <see cref="IrcChannel.UserLeft"/> event.
    /// </summary>
    public sealed class UserLeftEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the <see cref="IrcUser"/> who left the channel.
        /// </summary>
        public IrcUser User { get; private set; }

        /// <summary>
        /// Gets the reason, if any, of the leave. 
        /// The default is usually the <see cref="IrcUser"/>'s nickname.
        /// </summary>
        public string Reason { get; private set; }

        internal UserLeftEventArgs( IrcUser user, string reason )
        {
            this.User = user;
            this.Reason = reason;
        }
    }

    /// <summary>
    /// Provides data for the <see cref="IrcUser.Quit"/> event.
    /// </summary>
    public sealed class UserQuitEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the reason, if any.
        /// </summary>
        public string Reason { get; private set; }

        internal UserQuitEventArgs( string reason )
        {
            this.Reason = reason;
        }
    }

    /// <summary>
    /// Provides data for the <see cref="IrcUser.InformationReceived"/> event.
    /// </summary>
    public sealed class UserInformationReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the name of the server to which the <see cref="IrcUser"/> is connected.
        /// May be null.
        /// </summary>
        public string ConnectionServer { get; internal set; }

        /// <summary>
        /// Gets the idle time, in seconds, of the <see cref="IrcUser"/>.
        /// May be null.
        /// </summary>
        public int? IdleTime { get; internal set; }

        /// <summary>
        /// Gets the date at which the <see cref="IrcUser"/> connected to the network.
        /// May be null.
        /// </summary>
        public DateTime? LoginDate { get; internal set; }

        /// <summary>
        /// Gets a list of the <see cref="IrcChannel"/>s the <see cref="IrcUser"/> is on.
        /// May be empty.
        /// </summary>
        public ReadOnlyCollection<IrcChannel> Channels
        {
            get { return new ReadOnlyCollection<IrcChannel>( this.ChannelsInternal ); }
        }

        /// <summary>
        /// Gets additional, server-defined messages.
        /// </summary>
        public ReadOnlyCollection<string> AdditionalMessages
        {
            get { return new ReadOnlyCollection<string>( this.AdditionalMessagesInternal ); }
        }


        /// <summary>
        /// Gets a read-write list of the <see cref="IrcChannel"/>s the <see cref="IrcUser"/> is on.
        /// </summary>
        internal List<IrcChannel> ChannelsInternal { get; private set; }

        /// <summary>
        /// Gets a read-write list of additional, server-defined messages.
        /// </summary> 
        internal List<string> AdditionalMessagesInternal { get; private set; }

        internal UserInformationReceivedEventArgs()
        {
            this.ChannelsInternal = new List<IrcChannel>();
            this.AdditionalMessagesInternal = new List<string>();
        }
    }

    /// <summary>
    /// Provides data for the <see cref="IrcClient.UnknownCommandReceived"/> event.
    /// </summary>
    public sealed class UnknownCommandReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the <see cref="IrcUser"/> who sent the command.
        /// </summary>
        public IrcUser Sender { get; private set; }

        /// <summary>
        /// Gets the command.
        /// </summary>
        public string Command { get; private set; }

        /// <summary>
        /// Gets the command arguments.
        /// </summary>
        public ReadOnlyCollection<string> CommandArguments { get; private set; }

        /// <summary>
        /// Gets the message content.
        /// </summary>
        public string Content { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this event was handled.
        /// </summary>
        public bool Handled { get; set; }

        internal UnknownCommandReceivedEventArgs( IrcMessage message )
        {
            this.Sender = message.Sender;
            this.Command = message.Command;
            this.CommandArguments = new ReadOnlyCollection<string>( message.CommandArguments );
            this.Content = message.Content;
        }
    }


    /// <summary>
    /// Provides data for the <see cref="TcpListenerWrapper.ConnectionReceived"/> event.
    /// </summary>
    public sealed class ConnectionReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets a <see cref="TcpWrapper"/> around the connection.
        /// </summary>
        public TcpWrapper ConnectionWrapper { get; private set; }

        internal ConnectionReceivedEventArgs( TcpWrapper wrapper )
        {
            this.ConnectionWrapper = wrapper;
        }
    }


    /// <summary>
    /// Provides data for the <see cref="IdentServer.UserNameNeeded"/> event.
    /// </summary>
    internal sealed class UserNameNeededEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the user name.
        /// If this is not set, a random name will be chosen.
        /// </summary>
        public string UserName { get; set; }
    }
}