// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Collections.Generic;
using System.Linq;
using BasicMvvm;
using MetroIrc.Internals;
using IrcSharp;
using MetroIrc.Services;
using CM = System.ComponentModel;

namespace MetroIrc.ViewModels
{
    public sealed class IrcChannelViewModel : IrcConversationViewModel
    {
        #region Properties
        public IrcChannel Channel { get; private set; }

        public bool CanEditTopic
        {
            get
            {
                return this.Channel.UserModes[this.Channel.Network.CurrentUser] >= IrcChannelUserModes.Op
                    || !this.Channel.Modes.Contains( IrcChannelModes.TopicSetByOps );
            }
        }
        #endregion

        #region IrcConversationViewModel overrides
        public override string Title
        {
            get { return this.Channel.Name; }
        }

        public override string TargetName
        {
            get { return this.Channel.FullName; }
        }

        public override Type ConversationType
        {
            get { return typeof( IrcChannel ); }
        }

        public override IEnumerable<IrcUser> Users
        {
            get { return this.Channel.Users; }
        }
        #endregion

        public IrcChannelViewModel( IrcChannel channel, IrcNetworkViewModel networkVM )
            : base( networkVM )
        {
            this.Channel = channel;
            this.IsVisible = true;

            PropertyChangedEventManager.AddHandler( this.Channel, Channel_TopicChanged, o => o.Topic );
            var currentPair = this.Channel.UserModes.First( p => p.User == this.Channel.Network.CurrentUser );
            PropertyChangedEventManager.AddHandler( currentPair, ( s, e ) => this.CanEditTopicChanged(), o => o.Mode );
            CollectionChangedEventManager.AddHandler( this.Channel.Modes, ( s, e ) => this.CanEditTopicChanged() );

            this.Channel.MessageReceived += Channel_MessageReceived;
            this.Channel.UserJoined += Channel_UserJoined;
            this.Channel.UserLeft += Channel_UserLeft;
            this.Channel.UserKicked += Channel_UserKicked;
            this.Channel.ModeChanged += Channel_ModeChanged;
            this.Channel.InviteReceived += Channel_InviteReceived;

            this.Channel.Ctcp.ActionReceived += Channel_Ctcp_ActionReceived;
        }

        #region Event handlers
        private void Channel_MessageReceived( IrcChannel channel, MessageReceivedEventArgs args )
        {
            var message = new IrcMessage( channel.Network, MessageDirection.FromServer, args.Sender, IrcMessageType.Normal, args.Message );
            this.AddMessage( message );
        }

        private void Channel_UserJoined( IrcChannel channel, UserEventArgs args )
        {
            string text = Locator.Get<ITranslationService>().Translate( "Conversation", "Join", args.User.Nickname, channel.FullName );
            var message = new IrcMessage( channel.Network, MessageDirection.FromServer, args.User, IrcMessageType.Join, text );
            this.AddMessage( message );
        }

        private void Channel_UserLeft( IrcChannel channel, UserLeftEventArgs args )
        {
            if ( args.User == channel.Network.CurrentUser )
            {
                Messenger.Send( new ConversationEndMessage( this.Channel.Network, this ) );
                return;
            }

            string text = Locator.Get<ITranslationService>().Translate( "Conversation", "Part", args.User.Nickname, channel.FullName );
            if ( args.Reason.HasText() )
            {
                text += Locator.Get<ITranslationService>().Translate( "Conversation", "Reason", args.Reason );
            }
            var message = new IrcMessage( channel.Network, MessageDirection.FromServer, args.User, IrcMessageType.Part, text );
            this.AddMessage( message );
        }

        private void Channel_UserKicked( IrcChannel channel, UserKickedEventArgs args )
        {
            string text = Locator.Get<ITranslationService>().Translate( "Conversation", "Kick", args.KickedUser.Nickname, args.Kicker.Nickname );
            if ( args.Reason.HasText() )
            {
                text += Locator.Get<ITranslationService>().Translate( "Conversation", "Reason", args.Reason );
            }
            var message = new IrcMessage( channel.Network, MessageDirection.FromServer, args.Kicker, IrcMessageType.Kick, text );
            this.AddMessage( message );
        }

        private void Channel_ModeChanged( IrcChannel channel, ModeChangedEventArgs args )
        {
            foreach ( string text in IrcModeTranslator.TranslateChannelMode( args.Setter, args.Mode ) )
            {
                var message = new IrcMessage( channel.Network, MessageDirection.FromServer, args.Setter, IrcMessageType.ChannelMode, text );
                this.AddMessage( message );
            }
        }

        private void Channel_InviteReceived( IrcChannel channel, InviteReceivedEventArgs args )
        {
            string text = Locator.Get<ITranslationService>().Translate( "Conversation", "InviteReceived", args.Sender.Nickname, channel.FullName );
            var message = new IrcMessage( channel.Network, MessageDirection.FromServer, args.Sender, IrcMessageType.Invite, text );
            var msg = new GlobalMessageReceivedMessage( message );
            Messenger.Send( msg );
        }

        private void Channel_Ctcp_ActionReceived( IrcChannel channel, MessageReceivedEventArgs args )
        {
            string text = args.Sender.Nickname + " " + args.Message;
            var message = new IrcMessage( channel.Network, MessageDirection.FromServer, args.Sender, IrcMessageType.Action, text );
            this.AddMessage( message );
        }

        #endregion

        #region Weak event handlers
        private void Channel_TopicChanged( object sender, CM.PropertyChangedEventArgs e )
        {
            this.FirePropertyChanged( "ChannelTopic" );
        }

        private void CanEditTopicChanged()
        {
            this.FirePropertyChanged( "CanEditTopic" );
        }
        #endregion

        public override void Dispose()
        {
            this.Channel.Leave();

            this.Channel.MessageReceived -= Channel_MessageReceived;
            this.Channel.UserJoined -= Channel_UserJoined;
            this.Channel.UserLeft -= Channel_UserLeft;
            this.Channel.UserKicked -= Channel_UserKicked;
            this.Channel.ModeChanged -= Channel_ModeChanged;
            this.Channel.InviteReceived -= Channel_InviteReceived;

            this.Channel.Ctcp.ActionReceived -= Channel_Ctcp_ActionReceived;
        }
    }
}