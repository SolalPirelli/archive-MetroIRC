// Copyright (C) 2012-2013, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System.Collections.Specialized;
using System.Linq;
using IrcSharp.Internals;
using IrcSharp.Tests.TestInternals;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IrcSharp.Tests
{
    [TestClass]
    public class IrcIntegrationTests
    {
        private const string Nickname = "TestNick";
        private const string RealName = "TestUser";
        private const string ChannelName = "#TestChannel";

        private IrcNetwork _network;
        private IrcChannel _channel;
        private TestTcpWrapper _wrapper;

        [TestInitialize]
        public void Initialize()
        {
            var tup = Initializer.GetAuthenticatedNetwork( Nickname, RealName );
            this._network = tup.Item1;
            this._wrapper = tup.Item2;

            this._channel = this._network.GetChannel( ChannelName );
        }

        [TestMethod]
        public void Irc_ChannelJoin()
        {
            this._wrapper.ReceiveLine( GetUserString( this._network.CurrentUser ) + " JOIN " + ChannelName );

            Assert.AreEqual( 1, this._channel.Users.Count );
            Assert.AreEqual( this._network.CurrentUser, this._channel.Users[0] );
            Assert.IsTrue( this._network.CurrentUser.Channels.Contains( this._channel ) );
        }

        [TestMethod]
        public void Irc_ChannelJoin_CurrentUser_BeforeOtherUser()
        {
            string nick = "OtherNick", user = "OtherUser", host = "OtherHost";

            this._wrapper.ReceiveLine( GetUserString( this._network.CurrentUser ) + " JOIN " + ChannelName );
            this._wrapper.ReceiveLine( GetUserString( nick, user, host ) + " JOIN " + ChannelName );

            Assert.AreEqual( 2, this._channel.Users.Count );
            Assert.IsTrue( this._channel.Users.Contains( this._network.CurrentUser ) );
            Assert.IsTrue( this._channel.Users.Contains( this._network.GetUser( nick ) ) );
        }

        [TestMethod]
        public void Irc_ChannelJoin_OtherUser()
        {
            string nick = "OtherNick", user = "OtherUser", host = "OtherHost";

            this._wrapper.ReceiveLine( GetUserString( nick, user, host ) + " JOIN " + ChannelName );

            Assert.AreEqual( 1, this._channel.Users.Count );
        }

        [TestMethod]
        public void Irc_ChannelLeave_CurrentUser()
        {
            this._wrapper.ReceiveLine( GetUserString( this._network.CurrentUser ) + " JOIN " + ChannelName );
            this._wrapper.ReceiveLine( GetUserString( this._network.CurrentUser ) + " PART " + ChannelName );

            Assert.AreEqual( 0, this._channel.Users.Count );
            Assert.IsFalse( this._network.CurrentUser.Channels.Contains( this._channel ) );
        }

        [TestMethod]
        public void Irc_ChannelLeave_OtherUser()
        {
            string nick = "OtherNick", user = "OtherUser", host = "OtherHost";

            this._wrapper.ReceiveLine( GetUserString( nick, user, host ) + " JOIN " + ChannelName );
            this._wrapper.ReceiveLine( GetUserString( nick, user, host ) + " PART " + ChannelName );

            Assert.AreEqual( 0, this._channel.Users.Count );
        }

        [TestMethod]
        public void Irc_ChannelKick()
        {
            string nick = "OtherNick", user = "OtherUser", host = "OtherHost";
            string other = GetUserString( nick, user, host );
            string us = GetUserString( this._network.CurrentUser );

            this._wrapper.ReceiveLine( us + " JOIN " + ChannelName );
            this._wrapper.ReceiveLine( other + " JOIN " + ChannelName );
            this._wrapper.ReceiveLine( us + " KICK " + ChannelName + " " + nick );

            Assert.AreEqual( 1, this._channel.Users.Count );
            Assert.AreEqual( this._network.CurrentUser, this._channel.Users.First() );
        }

        [TestMethod]
        public void Irc_ChannelQuit()
        {
            string nick = "OtherNick", user = "OtherUser", host = "OtherHost";
            string us = GetUserString( this._network.CurrentUser );
            string other = GetUserString( nick, user, host );

            this._wrapper.ReceiveLine( us + " JOIN " + ChannelName );
            this._wrapper.ReceiveLine( other + " JOIN " + ChannelName );
            this._wrapper.ReceiveLine( other + " QUIT :bye." );

            Assert.AreEqual( 1, this._channel.Users.Count );
            Assert.AreEqual( this._network.CurrentUser, this._channel.Users[0] );
            Assert.AreEqual( 0, this._network.GetUser( nick ).Channels.Count );
        }

        [TestMethod]
        public void Irc_ChannelQuit_EventSent()
        {
            string nick = "OtherNick", user = "OtherUser", host = "OtherHost";
            string other = GetUserString( nick, user, host );
            bool userFlag = false;
            bool userModesFlag = false;
            ( (INotifyCollectionChanged) this._channel.Users ).CollectionChanged += ( s, e ) => userFlag = true;
            ( (INotifyCollectionChanged) this._channel.UserModes ).CollectionChanged += ( s, e ) => userModesFlag = true;

            this._wrapper.ReceiveLine( other + " JOIN " + ChannelName );
            this._wrapper.ReceiveLine( other + " QUIT :bye." );

            Assert.AreEqual( true, userFlag );
            Assert.AreEqual( true, userModesFlag );
        }

        [TestMethod]
        public void Irc_OperatorSet_CurrentUser()
        {
            string us = GetUserString( this._network.CurrentUser );

            this._wrapper.ReceiveLine( us + " JOIN " + ChannelName );
            this._wrapper.ReceiveLine( "MODE " + ChannelName + " +o " + this._network.CurrentUser.Nickname );

            Assert.AreEqual( IrcChannelUserModes.Op, this._network.CurrentUser.GetChannelMode( this._channel ) );
        }

        [TestMethod]
        public void Irc_OperatorSet_OtherUser()
        {
            string nick = "OtherNick", user = "OtherUser", host = "OtherHost";
            string other = GetUserString( nick, user, host );
            string us = GetUserString( this._network.CurrentUser );

            this._wrapper.ReceiveLine( us + " JOIN " + ChannelName );
            this._wrapper.ReceiveLine( other + " JOIN " + ChannelName );
            this._wrapper.ReceiveLine( us + " MODE " + ChannelName + " +o " + nick );

            Assert.AreEqual( IrcChannelUserModes.Op, this._network.GetUser( nick ).GetChannelMode( this._channel ) );
        }


        [TestMethod]
        public void Irc_RealLife_BeginConnection()
        {
            this._wrapper = new TestTcpWrapper();
            this._network = new IrcNetwork( this._wrapper );
            this._network.ConnectAsync( "Aethec", "Aethec", IrcUserLoginModes.None ).Wait();
            this._wrapper.ReceiveLine( ":main.smoothirc.net NOTICE Auth :*** Looking up your hostname..." );
            this._wrapper.ReceiveLine( ":main.smoothirc.net NOTICE Auth :*** Found your hostname (adsl-84-226-156-139.adslplus.ch) -- cached" );
            this._wrapper.ReceiveLine( ":main.smoothirc.net NOTICE Auth :Welcome to SmoothIRC!" );
            this._wrapper.ReceiveLine( ":main.smoothirc.net 001 Aethec :Welcome to the SmoothIRC IRC Network Aethec!Aethec@adsl-11-111-111-111.adslplus.ch" );
            this._wrapper.ReceiveLine( ":main.smoothirc.net 002 Aethec :Your host is main.smoothirc.net, running version InspIRCd-2.0" );
            this._wrapper.ReceiveLine( ":main.smoothirc.net 003 Aethec :This server was created 15:07:46 Dec 15 2012" );
            this._wrapper.ReceiveLine( ":main.smoothirc.net 004 Aethec main.smoothirc.net InspIRCd-2.0 BGHIRSWikorswx CFGMORSTabcefhijklmnopqrstuvz Fabefhjkloqv" );
            this._wrapper.ReceiveLine( ":main.smoothirc.net 005 Aethec AWAYLEN=201 :are supported by this server" );
            this._wrapper.ReceiveLine( ":main.smoothirc.net 042 Aethec 540AAACYH :your unique ID" );
            this._wrapper.ReceiveLine( ":main.smoothirc.net 375 Aethec :main.smoothirc.net message of the day" );
            this._wrapper.ReceiveLine( ":main.smoothirc.net 372 Aethec :- (MOTD cut)" );
            this._wrapper.ReceiveLine( ":main.smoothirc.net 372 Aethec :- (for brevity)" );
            this._wrapper.ReceiveLine( ":main.smoothirc.net 376 Aethec :End of message of the day." );
            this._wrapper.ReceiveLine( ":main.smoothirc.net 251 Aethec :There are 42 users and 15 invisible on 4 servers" );
            this._wrapper.ReceiveLine( ":main.smoothirc.net 252 Aethec 10 :operator(s) online" );
            this._wrapper.ReceiveLine( ":main.smoothirc.net 254 Aethec 20 :channels formed" );
            this._wrapper.ReceiveLine( ":main.smoothirc.net 255 Aethec :I have 18 clients and 3 servers" );
            this._wrapper.ReceiveLine( ":main.smoothirc.net 265 Aethec :Current Local Users: 18  Max: 46" );
            this._wrapper.ReceiveLine( ":main.smoothirc.net 266 Aethec :Current Global Users: 57  Max: 69" );
            this._wrapper.ReceiveLine( ":main.smoothirc.net 396 Aethec Smooth-un4lig.adslplus.ch :is now your displayed host" );
            this._wrapper.ReceiveLine( ":Aethec!Aethec@Smooth-un4lig.adslplus.ch MODE Aethec +x" );
        }


        [TestMethod]
        public void Irc_RealLife_NamesReply_CurrentUserPairRemains()
        {
            var channel = this._network.GetChannel( "#SmoothIRC" );
            var op = this._network.GetUser( "Aethec" );

            this._wrapper.ReceiveLine( ":Aethec!Aethec@Smooth-rtt.m6l.167.62.IP JOIN :#SmoothIRC" );
            var pair = channel.UserModes.First( p => p.User == op );
            this._wrapper.ReceiveLine( ":pinkiepie.smoothirc.net 353 Aethec = #SmoothIRC :+Prune Harakiri @Aethec" ); // cut for brevity
            this._wrapper.ReceiveLine( ":pinkiepie.smoothirc.net 366 Aethec #SmoothIRC :End of /NAMES list." );
            var newPair = channel.UserModes.First( p => p.User == op );

            Assert.AreSame( pair, newPair );
        }

        [TestMethod]
        public void Irc_RealLife_Names()
        {
            var channel = this._network.GetChannel( "#SmoothIRC" );
            var op = this._network.GetUser( "Aethec" );
            var voice = this._network.GetUser( "Prune" );
            var normal = this._network.GetUser( "Harakiri" );

            this._wrapper.ReceiveLine( ":Aethec!Aethec@Smooth-rtt.m6l.167.62.IP JOIN :#SmoothIRC" );
            this._wrapper.ReceiveLine( ":pinkiepie.smoothirc.net 353 Aethec = #SmoothIRC :+Prune Harakiri @Aethec" ); // cut for brevity
            this._wrapper.ReceiveLine( ":pinkiepie.smoothirc.net 366 Aethec #SmoothIRC :End of /NAMES list." );

            Assert.AreEqual( 3, channel.Users.Count );
            CollectionAssert.Contains( channel.Users, op );
            CollectionAssert.Contains( channel.Users, voice );
            CollectionAssert.Contains( channel.Users, normal );
            Assert.AreEqual( IrcChannelUserModes.Op, channel.UserModes[op] );
            Assert.AreEqual( IrcChannelUserModes.Voiced, channel.UserModes[voice] );
            Assert.AreEqual( IrcChannelUserModes.Normal, channel.UserModes[normal] );
            Assert.AreEqual( 1, op.Channels.Count );
            Assert.AreEqual( channel, op.Channels.First() );
        }

        [TestMethod]
        public void Irc_RealLife_NamesAndQuit()
        {
            var bot = this._network.GetUser( "QuizzBot" );
            var channel = this._network.GetChannel( "#Quizz" );

            this._wrapper.ReceiveLine( ":Aethec!Aethec@Smooth-rtt.m6l.167.62.IP JOIN :#Quizz" );
            this._wrapper.ReceiveLine( ":pinkiepie.smoothirc.net 353 Aethec = #Quizz :@QuizzBot Aethec" );
            this._wrapper.ReceiveLine( ":pinkiepie.smoothirc.net 366 Aethec #Quizz :End of /NAMES list." );
            this._wrapper.ReceiveLine( ":QuizzBot!QuizzBot@Smooth-rtt.m6l.167.62.IP QUIT :Connection closed" );

            Assert.AreEqual( 1, channel.Users.Count );
            Assert.AreEqual( 0, bot.Channels.Count );
        }


        private static string GetUserString( IrcUser user )
        {
            return GetUserString( user.Nickname, user.UserName ?? "user", user.Host ?? "host" );
        }

        private static string GetUserString( string nick, string userName, string host )
        {
            return IrcUtils.SenderIndicator + nick + IrcUtils.UserNameSeparator + userName + IrcUtils.UserHostSeparator + host;
        }
    }
}