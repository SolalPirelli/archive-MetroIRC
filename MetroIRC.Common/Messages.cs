// Copyright (C) 2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.using System.Text;

using System.Text;
using BasicMvvm;
using IrcSharp;
using MetroIrc.ViewModels;

namespace MetroIrc
{
    public sealed class ChangeEncodingMessage:IMessage
    {
        public IrcNetwork Network { get; private set; }
        public Encoding Encoding { get; private set; }

        public ChangeEncodingMessage( IrcNetwork network, Encoding encoding )
        {
            this.Network = network;
            this.Encoding = encoding;
        }
    }

    public class GlobalMessageSentMessage : IMessage
    {
        public IrcNetwork Network { get; private set; }
        public IrcMessage Message { get; private set; }

        public GlobalMessageSentMessage( IrcMessage message )
        {
            this.Network = message.Network;
            this.Message = message;
        }
    }

    public sealed class ChannelMessageSentMessage : GlobalMessageSentMessage
    {
        public IrcChannel Channel { get; private set; }

        public ChannelMessageSentMessage( IrcChannel channel, IrcMessage message )
            : base( message )
        {
            this.Channel = channel;
        }
    }

    public sealed class UserMessageSentMessage : GlobalMessageSentMessage
    {
        public IrcUser User { get; private set; }

        public UserMessageSentMessage( IrcUser user, IrcMessage message )
            : base( message )
        {
            this.User = user;
        }
    }

    public sealed class UserMessageReceivedMessage : IMessage
    {
        public IrcNetwork Network { get; private set; }
        public IrcUser User { get; private set; }
        public IrcMessage Message { get; private set; }

        public UserMessageReceivedMessage( IrcUser user, IrcMessage message )
        {
            this.Network = user.Network;
            this.User = user;
            this.Message = message;
        }
    }

    public sealed class GlobalMessageReceivedMessage : IMessage
    {
        public IrcNetwork Network { get; private set; }
        public IrcMessage Message { get; private set; }

        public GlobalMessageReceivedMessage( IrcMessage message )
        {
            this.Network = message.Network;
            this.Message = message;
        }
    }

    public sealed class OpenPrivateConversationMessage : IMessage
    {
        public IrcNetwork Network { get; private set; }
        public IrcUser User { get; private set; }

        public OpenPrivateConversationMessage( IrcUser user )
        {
            this.Network = user.Network;
            this.User = user;
        }
    }

    public sealed class JoinChannelMessage : IMessage
    {
        public IrcNetwork Network { get; private set; }
        public IrcChannel Channel { get; private set; }
        public string Key { get; private set; }

        public JoinChannelMessage( IrcChannel channel, string key )
        {
            this.Network = channel.Network;
            this.Channel = channel;
            this.Key = key;
        }
    }

    public sealed class LeaveChannelMessage : IMessage
    {
        public IrcNetwork Network { get; private set; }
        public IrcChannel Channel { get; private set; }

        public LeaveChannelMessage( IrcChannel channel )
        {
            this.Network = channel.Network;
            this.Channel = channel;
        }
    }

    public sealed class AddNetworkMessage : IMessage
    {
        public string Name { get; private set; }
        public int Port { get; private set; }
        public string Password { get; private set; }

        public AddNetworkMessage( string name, int port, string password )
        {
            this.Name = name;
            this.Port = port;
            this.Password = password;
        }
    }

    public sealed class QuitNetworkMessage : IMessage
    {
        public IrcNetwork Network { get; private set; }

        public QuitNetworkMessage( IrcNetwork network )
        {
            this.Network = network;
        }
    }

    public sealed class ConversationEndMessage : IMessage
    {
        public IrcNetwork Network { get; private set; }
        public IrcConversationViewModel Conversation { get; private set; }

        public ConversationEndMessage( IrcNetwork network, IrcConversationViewModel conversation )
        {
            this.Network = network;
            this.Conversation = conversation;
        }
    }

    public sealed class UnreadMessageReceivedMessage : IMessage
    {
        public IrcNetwork Network { get; private set; }
        public IrcConversationViewModel Conversation { get; private set; }
        public Message Message { get; private set; }

        public UnreadMessageReceivedMessage( IrcConversationViewModel conversation, Message message )
        {
            this.Network = message.Network;
            this.Conversation = conversation;
            this.Message = message;
        }
    }
}