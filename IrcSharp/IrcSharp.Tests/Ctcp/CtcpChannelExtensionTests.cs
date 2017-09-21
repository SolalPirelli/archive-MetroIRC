// Copyright (C) 2013, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using IrcSharp.Tests.TestInternals;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IrcSharp.Tests.Ctcp
{
    [TestClass]
    public class CtcpChannelExtensionTests
    {
        private const string Nickname = "nick";
        private const string RealName = "real";

        private IrcChannel _channel;
        private TestTcpWrapper _wrapper;

        [TestInitialize]
        public void Initialize()
        {
            var tup = Initializer.GetAuthenticatedNetwork( Nickname, RealName );
            this._wrapper = tup.Item2;
            this._channel = tup.Item1.GetChannel( "#channel" );
        }

        [TestMethod]
        public void SendAction_WorksCorrectly()
        {
            this._channel.Ctcp.SendAction( "leeroy jenkiiiins" );
            string expected = "PRIVMSG #channel :" + '\x01' + "ACTION leeroy jenkiiiins\x01";

            Assert.AreEqual( 1, this._wrapper.LinesSent.Count );
            Assert.AreEqual( expected, this._wrapper.LinesSent[0] );
        }

        [TestMethod]
        public void SendMessage_WorksCorrectly()
        {
            this._channel.Ctcp.SendMessage( "CUSTOMCOMMAND test" );
            string expected = "PRIVMSG #channel :" + '\x01' + "CUSTOMCOMMAND test\x01";

            Assert.AreEqual( 1, this._wrapper.LinesSent.Count );
            Assert.AreEqual( expected, this._wrapper.LinesSent[0] );
        }
    }
}