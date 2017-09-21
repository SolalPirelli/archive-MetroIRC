// Copyright (C) 2013, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using IrcSharp.Tests.TestInternals;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IrcSharp.Tests
{
    [TestClass]
    public sealed class IrcNetworkTests
    {
        private const string Nickname = "nick";
        private const string RealName = "real";

        private IrcNetwork _network;
        private TestTcpWrapper _wrapper;

        [TestInitialize]
        public void Initialize()
        {
            var tup = Initializer.GetAuthenticatedNetwork( Nickname, RealName );
            this._network = tup.Item1;
            this._wrapper = tup.Item2;
        }

        [TestMethod]
        public void Properties_AreCorrect()
        {
            Assert.IsNotNull( this._network.Client );
            Assert.AreEqual( this._wrapper.HostName, this._network.HostName );
            Assert.AreEqual( this._wrapper.Port, this._network.Port );
            Assert.AreEqual( this._wrapper.UsesSsl, this._network.UsesSsl );
            Assert.IsNotNull( this._network.Parameters );
            Assert.IsNotNull( this._network.CurrentUser );
            Assert.AreEqual( Nickname, this._network.CurrentUser.Nickname );
            Assert.AreSame( RealName, this._network.CurrentUser.RealName );
            Assert.IsNotNull( this._network.ServerUser );
            Assert.IsNotNull( this._network.KnownChannels );
            Assert.IsNotNull( this._network.KnownUsers );
        }

        [TestMethod]
        public void GetUser_SameReturnValue()
        {
            var user = this._network.GetUser( "nickname" );
            Assert.AreSame( user, this._network.GetUser( user.Nickname ) );
        }

        [TestMethod]
        public void GetChannel_SameReturnValue()
        {
            var channel = this._network.GetChannel( "#chan" );
            Assert.AreSame( channel, this._network.GetChannel( channel.FullName ) );
        }

        [TestMethod]
        public void GetUserFromFullName()
        {
            var user = this._network.GetUserFromFullName( "nickname!user@host" );

            Assert.AreEqual( "nickname", user.Nickname );
            Assert.AreEqual( "user", user.UserName );
            Assert.AreEqual( "host", user.Host );
        }

        [TestMethod]
        public void GetUserFromFullName_SameReturnValue()
        {
            var user1 = this._network.GetUserFromFullName( "nickname!user@host" );
            var user2 = this._network.GetUserFromFullName( "nickname!user@host" );

            Assert.AreSame( user1, user2 );
        }

        [TestMethod]
        public void GetUserFromFullName_SameHostButDifferentUser()
        {
            var user1 = this._network.GetUserFromFullName( "nick1!user1@host" );
            var user2 = this._network.GetUserFromFullName( "nick2!user2@host" );

            Assert.AreNotSame( user1, user2 );
        }

        [TestMethod]
        public void GetUserFromFullName_SameUsernameButDifferentUser()
        {
            var user1 = this._network.GetUserFromFullName( "nick1!user@host1" );
            var user2 = this._network.GetUserFromFullName( "nick2!user@host2" );

            Assert.AreNotSame( user1, user2 );
        }

        [TestMethod]
        public void GetUserFromFullName_SameUserAndHostButDifferentUser()
        {
            var user1 = this._network.GetUserFromFullName( "nick1!user@host" );
            var user2 = this._network.GetUserFromFullName( "nick2!user@host" );

            Assert.AreNotSame( user1, user2 );
        }
    }
}