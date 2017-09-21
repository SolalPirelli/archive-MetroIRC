using System;
using IrcSharp.Tests.TestInternals;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IrcSharp.Tests.Ctcp
{
    [TestClass]
    public class CtcpClientCommandHandlersTests
    {
        private IrcNetwork _network;
        private TestTcpWrapper _wrapper;

        [TestInitialize]
        public void Initialize()
        {
            var tup = Initializer.GetAuthenticatedNetwork( "nick", "real" );
            this._network = tup.Item1;
            this._wrapper = tup.Item2;
        }

        [TestMethod]
        public void Action_User_Query()
        {
            bool fired = false;

            this._network.ServerUser.Ctcp.ActionReceived += ( s, e ) =>
            {
                fired = true;
                Assert.AreEqual( "hello", e.Message );
            };

            this._wrapper.ReceiveLine( "PRIVMSG nick :" + '\x01' + "ACTION hello\x01" );

            Assert.IsTrue( fired );
        }

        [TestMethod]
        public void Action_User_NotQuery()
        {
            bool fired = false;

            this._network.ServerUser.Ctcp.ActionReceived += ( s, e ) =>
            {
                fired = true;
                Assert.AreEqual( "hello", e.Message );
            };

            this._wrapper.ReceiveLine( "NOTICE nick :" + '\x01' + "ACTION hello\x01" );

            Assert.IsTrue( fired );
        }

        [TestMethod]
        public void Action_Channel_Query()
        {
            bool fired = false;

            this._network.GetChannel( "#channel" ).Ctcp.ActionReceived += ( s, e ) =>
            {
                fired = true;
                Assert.AreEqual( "hello", e.Message );
            };

            this._wrapper.ReceiveLine( "PRIVMSG #channel :" + '\x01' + "ACTION hello\x01" );

            Assert.IsTrue( fired );
        }
    }
}