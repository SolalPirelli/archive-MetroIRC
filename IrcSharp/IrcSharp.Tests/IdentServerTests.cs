// Copyright (C) 2013, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using IrcSharp.Tests.TestInternals;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IrcSharp.Tests
{
    [TestClass]
    public class IdentServerTests
    {
        [TestMethod]
        public void Start()
        {
            IdentServer.UserNameNeeded += ( s, e ) => e.UserName = "alice";

            var client = new TestTcpWrapper();
            var listener = new TestTcpListenerWrapper();
            IdentServer.Start( listener );
            listener.AddClient( client );
            client.ReceiveLine( "0,0" );
            Assert.AreEqual( 1, client.LinesSent.Count );
            Assert.AreEqual( "0,0 : USERID : UNIX : alice", client.LinesSent[0] );
            Assert.IsTrue( client.Disposed );
        }

        [TestMethod]
        public void Stop_DisposeListener()
        {
            IdentServer.UserNameNeeded += ( s, e ) => e.UserName = "alice";

            var client = new TestTcpWrapper();
            var listener = new TestTcpListenerWrapper();
            IdentServer.Start( listener );
            IdentServer.Stop();
            listener.AddClient( client );
            client.ReceiveLine( "0,0" );
            Assert.AreEqual( 0, client.LinesSent.Count );
            Assert.IsTrue( listener.Disposed );
        }

        [TestMethod]
        public void Stop_DoNotDisposeListener()
        {
            IdentServer.UserNameNeeded += ( s, e ) => e.UserName = "alice";

            var client = new TestTcpWrapper();
            var listener = new TestTcpListenerWrapper();
            IdentServer.Start( listener );
            IdentServer.Stop( false );
            Assert.IsFalse( listener.Disposed );
        }
    }
}