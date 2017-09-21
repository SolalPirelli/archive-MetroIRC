// Copyright (C) 2013, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Linq;
using IrcSharp.Tests.TestInternals;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IrcSharp.Tests
{
    [TestClass]
    public sealed class IrcClientCommandHandlerTests
    {
        private const string Nickname = "nick";
        private const string RealName = "real";
        private const string UserString = ":nick!nick@host";

        private TestTcpWrapper _wrapper;
        private IrcNetwork _network;

        [TestInitialize]
        public void Initialize()
        {
            var tup = Initializer.GetAuthenticatedNetwork( Nickname, RealName );
            this._network = tup.Item1;
            this._wrapper = tup.Item2;
        }

        [TestMethod]
        public void Ping()
        {
            string data = "data";
            this._wrapper.ReceiveLine( "PING :" + data );
            Assert.AreEqual( "PONG :" + data, this._wrapper.LinesSent[0] );
        }

        [TestMethod]
        public void Nick_CurrentUser_Forced()
        {
            string newNick = "newnick";
            bool fired = false;
            this._network.CurrentUser.NicknameChanged += ( s, e ) =>
            {
                fired = true;
                Assert.AreEqual( e.OldNickname, Nickname );
            };

            this._wrapper.ReceiveLine( UserString + " NICK " + newNick );

            Assert.AreEqual( newNick, this._network.CurrentUser.Nickname );
            Assert.IsTrue( fired );
        }

        [TestMethod]
        public void Nick_CurrentUser_Voluntary()
        {
            string newNick = "newnick";
            bool fired = false;
            this._network.CurrentUser.NicknameChanged += ( s, e ) =>
            {
                fired = true;
                Assert.AreEqual( e.OldNickname, Nickname );
            };

            this._network.ChangeNickname( newNick );
            this._wrapper.ReceiveLine( UserString + " NICK " + newNick );

            Assert.AreEqual( newNick, this._network.CurrentUser.Nickname );
            Assert.IsTrue( fired );
        }

        [TestMethod]
        public void Nick_OtherUser()
        {
            string newNick = "newnick";
            string otherNick = "other";
            string otherString = ":other!other@otherhost";
            var otherUser = this._network.GetUser( otherNick );
            bool fired = false;
            otherUser.NicknameChanged += ( s, e ) =>
            {
                fired = true;
                Assert.AreEqual( e.OldNickname, otherNick );
            };

            this._wrapper.ReceiveLine( otherString + " NICK " + newNick );
            Assert.AreEqual( newNick, otherUser.Nickname );
            Assert.IsTrue( fired );
        }

        [TestMethod]
        public void Mode_Channel()
        {
            string op = "op";
            var opUser = this._network.GetUser( op );
            string opString = ":op!op@ophost";
            string chanName = "#chan";
            bool fired = false;

            this._wrapper.ReceiveLine( opString + " JOIN " + chanName );

            var modePair = this._network.GetChannel( chanName ).UserModes.First( p => p.User == opUser );
            modePair.PropertyChanged += ( s, e ) =>
            {
                if ( e.PropertyName == "Mode" )
                {
                    fired = true;
                }
            };

            this._wrapper.ReceiveLine( "MODE " + chanName + " +o " + op );

            Assert.AreEqual( IrcChannelUserModes.Op, this._network.GetChannel( chanName ).UserModes[opUser] );
            Assert.IsTrue( fired );
        }

        [TestMethod]
        public void Mode_User()
        {
            bool fired = false;
            this._network.CurrentUser.ModeChanged += ( s, e ) =>
            {
                fired = true;
            };

            this._wrapper.ReceiveLine( "MODE " + Nickname + " +r" );

            CollectionAssert.Contains( this._network.CurrentUser.Modes, 'r' );
            Assert.IsTrue( fired );
        }

        [TestMethod]
        public void Quit_NoReason()
        {
            string otherNick = "quitter";
            string otherString = ":quitter!quitter@qhost";
            var otherUser = this._network.GetUser( otherNick );
            bool fired = false;

            otherUser.Quit += ( s, e ) =>
            {
                fired = true;
            };

            this._wrapper.ReceiveLine( otherString + " QUIT" );

            CollectionAssert.DoesNotContain( this._network.KnownUsers, otherUser );
            Assert.IsTrue( fired );
        }

        [TestMethod]
        public void Quit_WithReason()
        {
            string otherNick = "quitter";
            string otherString = ":quitter!quitter@qhost";
            string reason = "reason";
            var otherUser = this._network.GetUser( otherNick );
            bool fired = false;

            otherUser.Quit += ( s, e ) =>
            {
                fired = true;
                Assert.AreEqual( reason, e.Reason );
            };

            this._wrapper.ReceiveLine( otherString + " QUIT :" + reason );

            CollectionAssert.DoesNotContain( this._network.KnownUsers, otherUser );
            Assert.IsTrue( fired );
        }

        [TestMethod]
        public void Join_CurrentUser()
        {
            string chanName = "#chan";
            var channel = this._network.GetChannel( chanName );
            bool fired = false;
            channel.UserJoined += ( s, e ) =>
            {
                fired = true;
                Assert.AreEqual( this._network.CurrentUser, e.User );
            };

            this._wrapper.ReceiveLine( UserString + " JOIN " + chanName );

            CollectionAssert.Contains( channel.Users, this._network.CurrentUser );
            Assert.IsTrue( fired );
        }

        [TestMethod]
        public void Join_OtherUser()
        {
            string otherNick = "other";
            string otherString = ":other!other@otherhost";
            var otherUser = this._network.GetUser( otherNick );
            string chanName = "#chan";
            var channel = this._network.GetChannel( chanName );
            bool fired = false;

            channel.UserJoined += ( s, e ) =>
            {
                fired = true;
                Assert.AreEqual( otherUser, e.User );
            };

            this._wrapper.ReceiveLine( otherString + " JOIN " + chanName );

            CollectionAssert.Contains( channel.Users, otherUser );
            Assert.IsTrue( fired );
        }

        [TestMethod]
        public void Part_NoReason()
        {
            string chanName = "#chan";
            var channel = this._network.GetChannel( chanName );
            bool fired = false;

            channel.UserLeft += ( s, e ) =>
            {
                fired = true;
                Assert.AreEqual( this._network.CurrentUser, e.User );
                Assert.AreEqual( string.Empty, e.Reason );
            };

            this._wrapper.ReceiveLine( UserString + " JOIN " + chanName );
            this._wrapper.ReceiveLine( UserString + " PART " + chanName );

            CollectionAssert.DoesNotContain( channel.Users, this._network.CurrentUser );
            Assert.IsTrue( fired );
        }

        [TestMethod]
        public void Part_WithReason()
        {
            string chanName = "#chan";
            var channel = this._network.GetChannel( chanName );
            string reason = "reason";
            bool fired = false;

            channel.UserLeft += ( s, e ) =>
            {
                fired = true;
                Assert.AreEqual( this._network.CurrentUser, e.User );
                Assert.AreEqual( reason, e.Reason );
            };

            this._wrapper.ReceiveLine( UserString + " JOIN " + chanName );
            this._wrapper.ReceiveLine( UserString + " PART " + chanName + " :" + reason );

            CollectionAssert.DoesNotContain( channel.Users, this._network.CurrentUser );
            Assert.IsTrue( fired );
        }

        [TestMethod]
        public void Topic_NewTopic()
        {
            string chanName = "#chan";
            var channel = this._network.GetChannel( chanName );
            string topic = "topic";

            this._wrapper.ReceiveLine( "TOPIC " + chanName + " :" + topic );

            Assert.AreEqual( topic, channel.Topic.Text );
            Assert.IsNotNull( channel.Topic.SetDate );
            Assert.AreEqual( DateTime.Now.Ticks, channel.Topic.SetDate.Value.Ticks, 10000 );
            Assert.AreEqual( this._network.ServerUser, channel.Topic.Setter );
        }

        [TestMethod]
        public void Topic_ClearTopic()
        {
            string chanName = "#chan";
            var channel = this._network.GetChannel( chanName );

            this._wrapper.ReceiveLine( "TOPIC " + chanName + " :topic" );
            this._wrapper.ReceiveLine( "TOPIC " + chanName + " :" );

            Assert.AreEqual( string.Empty, channel.Topic.Text );
            Assert.AreEqual( DateTime.Now, channel.Topic.SetDate );
            Assert.AreEqual( this._network.ServerUser, channel.Topic.Setter );
        }

        [TestMethod]
        public void Invite()
        {
            string chanName = "#chan";
            var channel = this._network.GetChannel( chanName );
            bool fired = false;

            channel.InviteReceived += ( s, e ) =>
            {
                fired = true;
                Assert.AreEqual( this._network.ServerUser, e.Sender );
            };

            this._wrapper.ReceiveLine( "INVITE " + Nickname + " " + chanName );

            Assert.IsTrue( fired );
        }

        [TestMethod]
        public void Kick_NoReason()
        {
            string otherNick = "kicker";
            string otherString = ":kicker!kicker@kickerhost";
            var other = this._network.GetUser( otherNick );
            string chanName = "#chan";
            var channel = this._network.GetChannel( chanName );
            bool fired = false;

            channel.UserKicked += ( s, e ) =>
            {
                fired = true;
                Assert.AreEqual( this._network.CurrentUser, e.KickedUser );
                Assert.AreEqual( other, e.Kicker );
                Assert.AreEqual( string.Empty, e.Reason );
            };

            this._wrapper.ReceiveLine( UserString + " JOIN " + chanName );
            this._wrapper.ReceiveLine( otherString + " KICK " + chanName + " " + Nickname );

            CollectionAssert.DoesNotContain( channel.Users, this._network.CurrentUser );
            Assert.IsTrue( fired );
        }

        [TestMethod]
        public void Kick_WithReason()
        {
            string otherNick = "kicker";
            string otherString = ":kicker!kicker@kickerhost";
            var other = this._network.GetUser( otherNick );
            string chanName = "#chan";
            var channel = this._network.GetChannel( chanName );
            string reason = "reason";
            bool fired = false;

            channel.UserKicked += ( s, e ) =>
            {
                fired = true;
                Assert.AreEqual( this._network.CurrentUser, e.KickedUser );
                Assert.AreEqual( other, e.Kicker );
                Assert.AreEqual( reason, e.Reason );
            };

            this._wrapper.ReceiveLine( UserString + " JOIN " + chanName );
            this._wrapper.ReceiveLine( otherString + " KICK " + chanName + " " + Nickname + " :" + reason );

            CollectionAssert.DoesNotContain( channel.Users, this._network.CurrentUser );
            Assert.IsTrue( fired );
        }

        [TestMethod]
        public void PrivMsg_Channel()
        {
            string otherNick = "msger";
            string otherString = ":msger!msger@msgerhost";
            var other = this._network.GetUser( otherNick );
            string chanName = "#chan";
            var channel = this._network.GetChannel( chanName );
            string text = "message";
            bool fired = false;

            channel.MessageReceived += ( s, e ) =>
            {
                fired = true;
                Assert.AreEqual( other, e.Sender );
                Assert.AreEqual( text, e.Message );
            };

            this._wrapper.ReceiveLine( otherString + " PRIVMSG " + chanName + " :" + text );

            Assert.IsTrue( fired );
        }

        [TestMethod]
        public void PrivMsg_User()
        {
            string otherNick = "msger";
            string otherString = ":msger!msger@msgerhost";
            var other = this._network.GetUser( otherNick );
            string text = "message";
            bool fired = false;

            other.MessageReceived += ( s, e ) =>
            {
                fired = true;
                Assert.AreEqual( text, e.Message );
            };

            this._wrapper.ReceiveLine( otherString + " PRIVMSG " + Nickname + " :" + text );

            Assert.IsTrue( fired );
        }

        [TestMethod]
        public void Notice()
        {
            string otherNick = "msger";
            string otherString = ":msger!msger@msgerhost";
            var other = this._network.GetUser( otherNick );
            string text = "message";
            bool fired = false;

            other.NoticeReceived += ( s, e ) =>
            {
                fired = true;
                Assert.AreEqual( text, e.Message );
            };

            this._wrapper.ReceiveLine( otherString + " NOTICE " + Nickname + " :" + text );

            Assert.IsTrue( fired );
        }

        [TestMethod]
        public void Kill()
        {
            string otherString = ":msger!msger@msgerhost";

            this._wrapper.ReceiveLine( otherString + " KILL " + Nickname + " : goodbye." );

            Assert.AreEqual( false, this._network.IsConnected );
        }

        [TestMethod]
        public void ISupport()
        {
            // just one token to test - the rest of the ISupport tests are in CapabilityParserTests
            this._wrapper.ReceiveLine( "005 " + Nickname + " CASEMAPPING=ascii :are supported by this server" );
            Assert.AreEqual( IrcCaseMapping.Ascii, this._network.Parameters.CaseMapping );
        }

        [TestMethod]
        public void NamesReplies_None()
        {
            string chanName = "#channel";
            this._wrapper.ReceiveLine( "366 " + Nickname + " " + chanName + " :End of NAMES list" );

            Assert.AreEqual( 0, this._network.GetChannel( chanName ).Users.Count );
        }

        [TestMethod]
        public void NamesReplies_One()
        {
            string chanName = "#channel";
            var channel = this._network.GetChannel( chanName );
            string[] nicks = { "test", "user123", "another_user" };
            string nicksText = string.Join( " ", nicks );

            this._wrapper.ReceiveLine( "353 " + Nickname + " = " + chanName + " :" + nicksText );
            this._wrapper.ReceiveLine( "366 " + Nickname + " " + chanName + " :End of NAMES list" );

            Assert.AreEqual( nicks.Length, channel.Users.Count );
            foreach ( string nick in nicks )
            {
                var user = this._network.GetUser( nick );
                CollectionAssert.Contains( channel.Users, user );
            }
        }

        [TestMethod]
        public void NamesReplies_Many()
        {
            string chanName = "#channel";
            var channel = this._network.GetChannel( chanName );
            string[] nicks = { "test", "user123", "another_user" };

            foreach ( string nick in nicks )
            {
                this._wrapper.ReceiveLine( "353 " + Nickname + " = " + chanName + " :" + nick );
            }
            this._wrapper.ReceiveLine( "366 " + Nickname + " " + chanName + " :End of NAMES list" );

            Assert.AreEqual( nicks.Length, channel.Users.Count );
            foreach ( string nick in nicks )
            {
                var user = this._network.GetUser( nick );
                CollectionAssert.Contains( channel.Users, user );
            }
        }

        [TestMethod]
        public void NamesReply_Visibility_Normal()
        {
            string chanName = "#channel";
            var channel = this._network.GetChannel( chanName );

            this._wrapper.ReceiveLine( "353 " + Nickname + " = " + chanName + " :" );
            this._wrapper.ReceiveLine( "366 " + Nickname + " " + chanName + " :End of NAMES list" );

            Assert.AreEqual( IrcChannelVisibility.Normal, channel.Visibility );
        }

        [TestMethod]
        public void NamesReply_Visibility_Private()
        {
            string chanName = "#channel";
            var channel = this._network.GetChannel( chanName );

            this._wrapper.ReceiveLine( "353 " + Nickname + " * " + chanName + " :" );
            this._wrapper.ReceiveLine( "366 " + Nickname + " " + chanName + " :End of NAMES list" );

            Assert.AreEqual( IrcChannelVisibility.Private, channel.Visibility );
        }

        [TestMethod]
        public void NamesReply_Visibility_Secret()
        {
            string chanName = "#channel";
            var channel = this._network.GetChannel( chanName );

            this._wrapper.ReceiveLine( "353 " + Nickname + " @ " + chanName + " :" );
            this._wrapper.ReceiveLine( "366 " + Nickname + " " + chanName + " :End of NAMES list" );

            Assert.AreEqual( IrcChannelVisibility.Secret, channel.Visibility );
        }

        [TestMethod]
        public void TopicContent()
        {
            string chanName = "#channel";
            var channel = this._network.GetChannel( chanName );
            string topic = "test topic.";

            this._wrapper.ReceiveLine( "332 " + Nickname + " " + chanName + " :" + topic );

            Assert.AreEqual( topic, channel.Topic.Text );
        }

        [TestMethod]
        public void NoTopic()
        {
            string chanName = "#channel";
            var channel = this._network.GetChannel( chanName );
            string topic = "test topic.";

            this._wrapper.ReceiveLine( "332 " + Nickname + " " + chanName + " :" + topic );
            this._wrapper.ReceiveLine( "331 " + Nickname + " " + chanName + " :No topic is set" );

            Assert.AreEqual( string.Empty, channel.Topic.Text );
        }

        [TestMethod]
        public void TopicInfo()
        {
            string chanName = "#channel";
            var channel = this._network.GetChannel( chanName );
            string setterName = "setter";
            var setter = this._network.GetUser( setterName );
            DateTime setDate = DateTime.Now;
            int unixDate = (int) ( setDate - new DateTime( 1970, 1, 1 ) ).TotalSeconds;

            this._wrapper.ReceiveLine( "333 " + Nickname + " " + chanName + " " + setterName + " " + unixDate );

            Assert.AreEqual( setter, channel.Topic.Setter );
            Assert.IsTrue( ( channel.Topic.SetDate.Value - setDate ).TotalSeconds < 1 );
        }

        // TODO: rest of the tests
    }
}