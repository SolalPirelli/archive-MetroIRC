// Copyright (C) 2013, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using IrcSharp.Tests.TestInternals;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IrcSharp.Tests
{
    [TestClass]
    public class IrcChannelTests
    {
        private IrcNetwork _network;
        private IrcChannel _channel;
        private TestTcpWrapper _wrapper;

        [TestInitialize]
        public void Initialize()
        {
            var tup = Initializer.GetAuthenticatedNetwork();
            this._network = tup.Item1;
            this._wrapper = tup.Item2;
            this._channel = this._network.GetChannel( "#channel" );
        }

        [TestMethod]
        public void Join_NoKey()
        {
            this._channel.Join();
            AssertSent( "JOIN #channel" );
        }

        [TestMethod]
        public void Join_Key_FromProperty()
        {
            this._channel.Key = "key";
            this._channel.Join();
            AssertSent( "JOIN #channel key" );
        }

        [TestMethod]
        public void Join_Key_FromParam()
        {
            this._channel.Join( "key" );
            AssertSent( "JOIN #channel key" );
        }

        [TestMethod]
        public void Join_Key_FromParam_OverridesProperty()
        {
            this._channel.Key = "not_the_key";
            this._channel.Join( "key" );
            AssertSent( "JOIN #channel key" );
        }

        [TestMethod]
        public void Leave_NoReason()
        {
            this._channel.Leave();
            AssertSent( "PART #channel" );
        }

        [TestMethod]
        public void Leave_Reason()
        {
            this._channel.Leave( "the reason" );
            AssertSent( "PART #channel :the reason" );
        }

        [TestMethod]
        public void SendMessage()
        {
            this._channel.SendMessage( "the message" );
            Assert.AreEqual( 1, this._wrapper.LinesSent.Count );
            Assert.AreEqual( "PRIVMSG #channel :the message", this._wrapper.LinesSent[0] );
        }

        [TestMethod]
        public void InviteUser()
        {
            var user = this._network.GetUser( "user" );
            this._channel.InviteUser( user );
            AssertSent( "INVITE user #channel" );
        }

        [TestMethod]
        public void UpdateInformation_Default()
        {
            // by default, invite exceptions and ban exceptions are not enabled
            this._channel.UpdateInformation();
            AssertSent( "MODE #channel", "MODE #channel +b" );
        }

        [TestMethod]
        public void UpdateInformation_WithBanExceptions()
        {
            // don't use the default char, make sure we don't hardcode it
            this._network.Parameters.ChannelModes.BanExceptionMode = 'X';
            this._network.Parameters.AreBanExceptionsEnabled = true;
            this._channel.UpdateInformation();
            AssertSent( "MODE #channel", "MODE #channel +b", "MODE #channel X" );
        }

        [TestMethod]
        public void UpdateInformation_WithInviteExceptions()
        {
            // don't use the default char, make sure we don't hardcode it
            this._network.Parameters.ChannelModes.InviteExceptionMode = 'Y';
            this._network.Parameters.AreInviteExceptionsEnabled = true;
            this._channel.UpdateInformation();
            AssertSent( "MODE #channel", "MODE #channel +b", "MODE #channel Y" );
        }

        [TestMethod]
        public void UpdateInformation_WithBothExceptions()
        {
            this._network.Parameters.ChannelModes.BanExceptionMode = 'X';
            this._network.Parameters.AreBanExceptionsEnabled = true;
            this._network.Parameters.ChannelModes.InviteExceptionMode = 'Y';
            this._network.Parameters.AreInviteExceptionsEnabled = true;
            this._channel.UpdateInformation();

            try
            {
                AssertSent( "MODE #channel", "MODE #channel +b", "MODE #channel X", "MODE #channel Y" );
            }
            catch ( AssertFailedException )
            {
                AssertSent( "MODE #channel", "MODE #channel +b", "MODE #channel Y", "MODE #channel X" );
            }
        }

        [TestMethod]
        public void SetTopic()
        {
            this._channel.SetTopic( "the topic" );
            AssertSent( "TOPIC #channel :the topic" );
        }

        [TestMethod]
        public void ClearTopic()
        {
            this._channel.ClearTopic();
            AssertSent( "TOPIC #channel :" );
        }

        [TestMethod]
        public void KickUser_NoReason()
        {
            var user = this._network.GetUser( "user" );
            this._channel.KickUser( user );
            AssertSent( "KICK #channel user" );
        }

        [TestMethod]
        public void KickUser_WithReason()
        {
            var user = this._network.GetUser( "user" );
            this._channel.KickUser( user, "the reason" );
            AssertSent( "KICK #channel user :the reason" );
        }


        [TestMethod]
        public void BanUser_NoKick()
        {
            var user = this._network.GetUser( "user" );
            this._channel.BanUser( user, false );
            AssertSent( "MODE #channel +b user" );
        }

        [TestMethod]
        public void BanUser_WithKick_NoReason()
        {
            var user = this._network.GetUser( "user" );
            this._channel.BanUser( user, true );
            AssertSent( "MODE #channel +b user", "KICK #channel user" );
        }

        [TestMethod]
        public void BanUser_WithKick_WithReason()
        {
            var user = this._network.GetUser( "user" );
            this._channel.BanUser( user, true, "the reason" );
            AssertSent( "MODE #channel +b user", "KICK #channel user :the reason" );
        }

        [TestMethod]
        public void UnbanUser()
        {
            var user = this._network.GetUser( "user" );
            this._channel.UnbanUser( user );
            AssertSent( "MODE #channel -b user" );
        }

        [TestMethod]
        public void AddMode()
        {
            this._channel.AddMode( 'X' );
            AssertSent( "MODE #channel +X" );
        }

        [TestMethod]
        public void RemoveMode()
        {
            this._channel.RemoveMode( 'X' );
            AssertSent( "MODE #channel -X" );
        }

        [TestMethod]
        public void SetMode()
        {
            this._channel.SetMode( "+x-y +z" );
            AssertSent( "MODE #channel +x-y +z" );
        }

        [TestMethod]
        public void AddUserMode_Normal()
        {
            var user = this._network.GetUser( "user" );
            this._channel.AddUserMode( user, IrcChannelUserModes.Normal );
            AssertSent();
        }

        [TestMethod]
        public void AddUserMode()
        {
            var user = this._network.GetUser( "user" );
            this._channel.AddUserMode( user, IrcChannelUserModes.Op );
            AssertSent( "MODE #channel +o user" );
        }

        [TestMethod]
        public void AddUserMode_Unavailable()
        {
            this._network.Parameters.ChannelModes.UserModes = new ReadOnlyDictionary<char, IrcChannelUserModes>( new Dictionary<char, IrcChannelUserModes>() );
            var user = this._network.GetUser( "user" );
            AssertEx.Throws<ArgumentException>( () =>
            {
                this._channel.AddUserMode( user, IrcChannelUserModes.Admin );
            } );
        }

        [TestMethod]
        public void RemoveUserMode_Normal()
        {
            var user = this._network.GetUser( "user" );
            this._channel.RemoveUserMode( user, IrcChannelUserModes.Normal );
            AssertSent();
        }

        [TestMethod]
        public void RemoveUserMode()
        {
            var user = this._network.GetUser( "user" );
            this._channel.RemoveUserMode( user, IrcChannelUserModes.Op );
            AssertSent( "MODE #channel -o user" );
        }

        [TestMethod]
        public void RemoveUserMode_Unavailable()
        {
            this._network.Parameters.ChannelModes.UserModes = new ReadOnlyDictionary<char, IrcChannelUserModes>( new Dictionary<char, IrcChannelUserModes>() );
            var user = this._network.GetUser( "user" );
            AssertEx.Throws<ArgumentException>( () =>
            {
                this._channel.RemoveUserMode( user, IrcChannelUserModes.Admin );
            } );
        }

        [TestMethod]
        public void GetUserList()
        {
            this._channel.GetUserList();
            AssertSent( "NAMES #channel" );
        }

        private void AssertSent( params string[] lines )
        {
            Assert.AreEqual( lines.Length, this._wrapper.LinesSent.Count );
            for ( int n = 0; n < lines.Length; n++ )
            {
                Assert.AreEqual( lines[n], this._wrapper.LinesSent[n] );
            }
        }
    }
}