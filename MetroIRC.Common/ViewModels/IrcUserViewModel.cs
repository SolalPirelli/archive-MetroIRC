// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Collections.Generic;
using System.Linq;
using BasicMvvm;
using MetroIrc.Internals;
using IrcSharp;
using IrcSharp.Ctcp;
using MetroIrc.Services;
using CM = System.ComponentModel;

namespace MetroIrc.ViewModels
{
    public sealed class IrcUserViewModel : IrcConversationViewModel
    {
        #region Properties
        public IrcUser User { get; private set; }
        #endregion

        #region IrcConversationViewModel overrides
        public override string Title
        {
            get { return this.User.Nickname; }
        }

        public override string TargetName
        {
            get { return this.User.Nickname; }
        }

        public override Type ConversationType
        {
            get { return typeof( IrcUser ); }
        }

        public override IEnumerable<IrcUser> Users
        {
            get { yield return this.User; }
        }
        #endregion

        public IrcUserViewModel( IrcUser user, IrcNetworkViewModel networkVM )
            : base( networkVM )
        {
            this.User = user;
            PropertyChangedEventManager.AddHandler( this.User, PartnerNicknameChanged, o => o.Nickname );

            this.User.MessageReceived += User_MessageReceived;
            this.User.NoticeReceived += User_NoticeReceived;
            this.User.InformationReceived += User_InformationReceived;
            this.User.ModeChanged += User_ModeChanged;
            this.User.NicknameChanged += User_NicknameChanged;
            this.User.Quit += User_Quit;

            this.User.Ctcp.ActionReceived += User_Ctcp_ActionReceived;
            this.User.Ctcp.ClientCapabilitiesReceived += User_Ctcp_ClientCapabilitiesReceived;
            this.User.Ctcp.ClientInformationReceived += User_Ctcp_ClientInformationReceived;
            this.User.Ctcp.ClientLocationReceived += User_Ctcp_ClientLocationReceived;
            this.User.Ctcp.PingReplyReceived += User_Ctcp_PingReplyReceived;
            this.User.Ctcp.LocalTimeReceived += User_Ctcp_LocalTimeReceived;
            this.User.Ctcp.InformationReceived += User_Ctcp_InformationReceived;
            this.User.Ctcp.ErrorMessageReceived += User_Ctcp_ErrorMessageReceived;
        }

        #region Event handlers
        private void User_MessageReceived( IrcUser user, PrivateMessageReceivedEventArgs args )
        {
            var message = new IrcMessage( user.Network, MessageDirection.FromUser, user, IrcMessageType.Normal, args.Message );
            this.AddMessage( message );
        }

        private void User_NoticeReceived( IrcUser user, PrivateMessageReceivedEventArgs args )
        {
            var message = new IrcMessage( user.Network, MessageDirection.FromUser, user, IrcMessageType.Notice, args.Message, user.Network.CurrentUser.Nickname );
            this.HandleGlobalMessage( message );
        }

        private void User_InformationReceived( IrcUser user, UserInformationReceivedEventArgs args )
        {
            var messages = new List<string>();
            messages.Add( Locator.Get<ITranslationService>().Translate( "UserInformation", "UserName", user.Nickname, user.UserName ) );
            messages.Add( Locator.Get<ITranslationService>().Translate( "UserInformation", "RealName", user.Nickname, user.RealName ) );
            messages.Add( Locator.Get<ITranslationService>().Translate( "UserInformation", "Host", user.Nickname, user.Host ) );

            if ( args.ConnectionServer != null )
            {
                messages.Add( Locator.Get<ITranslationService>().Translate( "UserInformation", "ConnectionServer", user.Nickname, args.ConnectionServer ) );
            }
            if ( args.IdleTime.HasValue )
            {
                messages.Add( Locator.Get<ITranslationService>().Translate( "UserInformation", "IdleTime", user.Nickname, args.IdleTime.Value ) );
            }
            if ( args.LoginDate.HasValue )
            {
                messages.Add( Locator.Get<ITranslationService>().Translate( "UserInformation", "LoginDate", user.Nickname, args.LoginDate.Value.ToString( "F" ) ) );
            }
            if ( args.Channels.Any() )
            {
                messages.Add( Locator.Get<ITranslationService>().Translate( "UserInformation", "Channels", user.Nickname, string.Join( ", ", args.Channels.Select( c => c.FullName ) ) ) );
            }
            // ignore other info; it's useless.

            foreach ( string message in messages )
            {
                var ircMessage = new IrcMessage( user.Network, MessageDirection.FromServer, user, IrcMessageType.Info, message );
                this.HandleGlobalMessage( ircMessage );
            }
        }

        private void User_ModeChanged( IrcUser user, ModeChangedEventArgs args )
        {
            foreach ( string text in IrcModeTranslator.TranslateUserMode( user, args.Setter, args.Mode ) )
            {
                var message = new IrcMessage( user.Network, MessageDirection.FromServer, args.Setter, IrcMessageType.UserMode, text );
                this.HandleUserMessage( message );
            }
        }

        private void User_NicknameChanged( IrcUser user, NicknameChangedEventArgs args )
        {
            string text = Locator.Get<ITranslationService>().Translate( "Conversation", "NickChange", args.OldNickname, user.Nickname );
            var message = new IrcMessage( user.Network, MessageDirection.FromServer, user, IrcMessageType.NickChange, text );
            this.HandleUserMessage( message );
        }

        private void User_Quit( IrcUser user, UserQuitEventArgs args )
        {
            string text = Locator.Get<ITranslationService>().Translate( "Conversation", "Quit", user.Nickname );
            if ( args.Reason.HasText() )
            {
                text += Locator.Get<ITranslationService>().Translate( "Conversation", "Reason", args.Reason );
            }
            var message = new IrcMessage( user.Network, MessageDirection.FromServer, user, IrcMessageType.Quit, text );
            this.HandleUserMessage( message );

            Messenger.Send( new ConversationEndMessage( this.User.Network, this ) );
        }

        private void User_Ctcp_ActionReceived( IrcUser user, PrivateMessageReceivedEventArgs args )
        {
            string text = user.Nickname + " " + args.Message;
            var message = new IrcMessage( user.Network, MessageDirection.FromUser, user, IrcMessageType.Action, text );
            this.AddMessage( message );
        }

        private void User_Ctcp_ClientCapabilitiesReceived( IrcUser user, ClientCapabilitiesReceivedEventArgs args )
        {
            string text = Locator.Get<ITranslationService>().Translate( "CtcpReplies", "ClientInfo", user.Nickname, args.Information );
            var message = new IrcMessage( user.Network, MessageDirection.FromUser, user, IrcMessageType.Info, text );
            this.HandleGlobalMessage( message );
        }

        private void User_Ctcp_ClientInformationReceived( IrcUser user, ClientInformationReceivedEventArgs args )
        {
            string text = Locator.Get<ITranslationService>().Translate( "CtcpReplies", "ClientVersion", user.Nickname, args.Information );
            var message = new IrcMessage( user.Network, MessageDirection.FromUser, user, IrcMessageType.Info, text );
            this.HandleGlobalMessage( message );
        }

        private void User_Ctcp_ClientLocationReceived( IrcUser user, ClientLocationReceivedEventArgs args )
        {
            string text = Locator.Get<ITranslationService>().Translate( "CtcpReplies", "ClientSource", user.Nickname, args.ClientLocation );
            var message = new IrcMessage( user.Network, MessageDirection.FromUser, user, IrcMessageType.Info, text );
            this.HandleGlobalMessage( message );
        }

        private void User_Ctcp_PingReplyReceived( IrcUser user, PingReplyReceivedEventArgs args )
        {
            string text = Locator.Get<ITranslationService>().Translate( "CtcpReplies", "Ping", user.Nickname, args.ReplyTime.TotalSeconds );
            var message = new IrcMessage( user.Network, MessageDirection.FromUser, user, IrcMessageType.Info, text );
            this.HandleGlobalMessage( message );
        }

        private void User_Ctcp_LocalTimeReceived( IrcUser user, LocalTimeReceivedEventArgs args )
        {
            string text = Locator.Get<ITranslationService>().Translate( "CtcpReplies", "Time", user.Nickname, args.Time );
            var message = new IrcMessage( user.Network, MessageDirection.FromUser, user, IrcMessageType.Info, text );
            this.HandleGlobalMessage( message );
        }

        private void User_Ctcp_InformationReceived( IrcUser user, UserInfoReplyReceivedEventArgs args )
        {
            string text = Locator.Get<ITranslationService>().Translate( "CtcpReplies", "UserInfo", user.Nickname, args.Information );
            var message = new IrcMessage( user.Network, MessageDirection.FromUser, user, IrcMessageType.Info, text );
            this.HandleGlobalMessage( message );
        }

        private void User_Ctcp_ErrorMessageReceived( IrcUser user, ErrorMessageReceivedEventArgs args )
        {
            string text = Locator.Get<ITranslationService>().Translate( "CtcpReplies", "Error", user.Nickname, args.Error );
            var message = new IrcMessage( user.Network, MessageDirection.FromUser, user, IrcMessageType.Error, text );
            this.HandleGlobalMessage( message );
        }
        #endregion

        #region Weak event handlers
        private void PartnerNicknameChanged( object sender, CM.PropertyChangedEventArgs e )
        {
            this.FirePropertyChanged( "Title" );
        }
        #endregion

        #region Private methods
        private void HandleUserMessage( IrcMessage message )
        {
            Messenger.Send( new UserMessageReceivedMessage( this.User, message ) );
        }
        private void HandleGlobalMessage( IrcMessage message )
        {
            Messenger.Send( new GlobalMessageReceivedMessage( message ) );
        }
        #endregion

        public override void Dispose()
        {
            this.User.MessageReceived -= User_MessageReceived;
            this.User.NoticeReceived -= User_NoticeReceived;
            this.User.InformationReceived -= User_InformationReceived;
            this.User.ModeChanged -= User_ModeChanged;
            this.User.NicknameChanged -= User_NicknameChanged;
            this.User.Quit -= User_Quit;

            this.User.Ctcp.ActionReceived -= User_Ctcp_ActionReceived;
            this.User.Ctcp.ClientCapabilitiesReceived -= User_Ctcp_ClientCapabilitiesReceived;
            this.User.Ctcp.ClientInformationReceived -= User_Ctcp_ClientInformationReceived;
            this.User.Ctcp.ClientLocationReceived -= User_Ctcp_ClientLocationReceived;
            this.User.Ctcp.PingReplyReceived -= User_Ctcp_PingReplyReceived;
            this.User.Ctcp.LocalTimeReceived -= User_Ctcp_LocalTimeReceived;
            this.User.Ctcp.InformationReceived -= User_Ctcp_InformationReceived;
            this.User.Ctcp.ErrorMessageReceived -= User_Ctcp_ErrorMessageReceived;
        }
    }
}