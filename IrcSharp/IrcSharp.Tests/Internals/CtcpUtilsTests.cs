// Copyright (C) 2013, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System.Text;
using IrcSharp.Internals;
using IrcSharp.Tests.TestInternals;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IrcSharp.Tests.Internals
{
    [TestClass]
    public class CtcpUtilsTests
    {
        private const char CtcpDelimiter = (char) 1;
        private IrcUser _user;

        [TestInitialize]
        public void Initialize()
        {
            var tup = Initializer.GetAuthenticatedNetwork();
            this._user = tup.Item1.ServerUser;
        }


        [TestMethod]
        public void IsCtcpMessage_IrcMessage()
        {
            var message = new IrcMessage( this._user, "privmsg", new[] { "example" }, "Hi" );
            Assert.IsFalse( CtcpUtils.IsCtcpMessage( message ) );
        }

        [TestMethod]
        public void IsCtcpMessage_CtcpMessage()
        {
            var message = new IrcMessage( this._user, "privmsg", new[] { "example" }, CtcpDelimiter + "FINGER" + CtcpDelimiter );
            Assert.IsTrue( CtcpUtils.IsCtcpMessage( message ) );
        }

        [TestMethod]
        public void IsCtcpMessage_CtcpMessageInWrongCommand()
        {
            var message = new IrcMessage( this._user, "join", new[] { "example" }, CtcpDelimiter + "FINGER" + CtcpDelimiter );
            Assert.IsFalse( CtcpUtils.IsCtcpMessage( message ) );
        }

        [TestMethod]
        public void EncodeMessage_RfcExample1()
        {
            // directly from the CTCP RFC: http://www.irchelp.org/irchelp/rfc/ctcpspec.html
            string text = "Hi there!\n" + "How are you? " + @"\K?";
            string expected = CtcpDelimiter + @"Hi there!" + '\x10' + @"nHow are you? \\K?" + CtcpDelimiter;
            Assert.AreEqual( expected, CtcpUtils.EncodeMessage( text ) );
        }

        [TestMethod]
        public void EncodeMessage_RfcExample2()
        {
            // from the CTCP RFC: http://www.irchelp.org/irchelp/rfc/ctcpspec.html
            string text = "SED \n\t\big" + '\x10' + '\x1' + '\0' + @"\:";
            string expected = CtcpDelimiter + "SED " + '\x10' + "n\t\big" + '\x10' + '\x10' + @"\a" + '\x10' + @"0\\:" + CtcpDelimiter;
            Assert.AreEqual( expected, CtcpUtils.EncodeMessage( text ) );
        }

        [TestMethod]
        public void DecodeMessage_RfcExample1()
        {
            // from the CTCP RFC: http://www.irchelp.org/irchelp/rfc/ctcpspec.html
            string text = "Hi there!" + '\x10' + "nHow are you? " + @"\\K?";
            string expected = "Hi there!\nHow are you? " + @"\K?";
            Assert.AreEqual( expected, CtcpUtils.DecodeMessage( text ) );
        }

        [TestMethod]
        public void DecodeMessage_RfcExample2()
        {
            // from the CTCP RFC: http://www.irchelp.org/irchelp/rfc/ctcpspec.html
            string text = "SED " + '\x10' + "n\t\big" + '\x10' + '\x10' + @"\a" + '\x10' + @"0\\:";
            string expected = "SED \n\t\big" + '\x10' + '\x1' + '\0' + @"\:";
            Assert.AreEqual( expected, CtcpUtils.DecodeMessage( text ) );
        }

        [TestMethod]
        public void FilterMessage_OneCtcpMessage()
        {
            var message = new IrcMessage( this._user, "PRIVMSG", new string[] { "someone" }, CtcpDelimiter + "PING abc" + CtcpDelimiter );

            var res = CtcpUtils.FilterMessage( message );

            Assert.AreEqual( 0, res.IrcMessages.Count );
            Assert.AreEqual( 1, res.CtcpMessages.Count );
            Assert.AreEqual( message.Sender, res.CtcpMessages[0].Sender );
            Assert.AreEqual( "PING", res.CtcpMessages[0].Command, true );
            Assert.AreEqual( "abc", res.CtcpMessages[0].Content );
        }

        [TestMethod]
        public void FilterMessage_IrcThenCtcp()
        {
            var message = new IrcMessage( this._user, "PRIVMSG", new string[] { "someone" }, "Hi!" + CtcpDelimiter + "FINGER" + CtcpDelimiter );

            var res = CtcpUtils.FilterMessage( message );

            Assert.AreEqual( 1, res.IrcMessages.Count );
            Assert.AreEqual( message.Sender, res.IrcMessages[0].Sender );
            Assert.AreEqual( message.Command, res.IrcMessages[0].Command, true );
            Assert.AreEqual( message.CommandArguments, res.IrcMessages[0].CommandArguments );
            Assert.AreEqual( "Hi!", res.IrcMessages[0].Content );
            Assert.AreEqual( 1, res.CtcpMessages.Count );
            Assert.AreEqual( message.Sender, res.CtcpMessages[0].Sender );
            Assert.AreEqual( "FINGER", res.CtcpMessages[0].Command, true );
            Assert.AreEqual( string.Empty, res.CtcpMessages[0].Content );
        }

        [TestMethod]
        public void FilterMessage_CtcpThenIrc()
        {
            var message = new IrcMessage( this._user, "PRIVMSG", new string[] { "someone" }, CtcpDelimiter + "FINGER" + CtcpDelimiter + "Hi!" );

            var res = CtcpUtils.FilterMessage( message );

            Assert.AreEqual( 1, res.IrcMessages.Count );
            Assert.AreEqual( message.Sender, res.IrcMessages[0].Sender );
            Assert.AreEqual( message.Command, res.IrcMessages[0].Command, true );
            Assert.AreEqual( message.CommandArguments, res.IrcMessages[0].CommandArguments );
            Assert.AreEqual( "Hi!", res.IrcMessages[0].Content );
            Assert.AreEqual( 1, res.CtcpMessages.Count );
            Assert.AreEqual( message.Sender, res.CtcpMessages[0].Sender );
            Assert.AreEqual( "FINGER", res.CtcpMessages[0].Command, true );
            Assert.AreEqual( string.Empty, res.CtcpMessages[0].Content );
        }

        [TestMethod]
        public void FilterMessage_ManyCtcp()
        {
            int count = 10;
            var builder = new StringBuilder();
            for ( int n = 0; n < count; n++ )
            {
                builder.Append( CtcpDelimiter );
                builder.Append( "TEST" + n + " " + "content" + n );
            }
            builder.Append( CtcpDelimiter );

            var message = new IrcMessage( this._user, "PRIVMSG", new string[] { "someone" }, builder.ToString() );

            var res = CtcpUtils.FilterMessage( message );

            Assert.AreEqual( 0, res.IrcMessages.Count );
            Assert.AreEqual( count, res.CtcpMessages.Count );
            for ( int n = 0; n < count; n++ )
            {
                Assert.AreEqual( message.Sender, res.CtcpMessages[n].Sender );
                Assert.AreEqual( "TEST" + n, res.CtcpMessages[n].Command, true );
                Assert.AreEqual( "content" + n, res.CtcpMessages[n].Content );
            }
        }

        [TestMethod]
        public void FilterMessage_OnChannel()
        {
            var message = new IrcMessage( this._user, "PRIVMSG", new string[] { "#chan" }, CtcpDelimiter + "ACTION abc" + CtcpDelimiter );

            var res = CtcpUtils.FilterMessage( message );

            Assert.AreEqual( 0, res.IrcMessages.Count );
            Assert.AreEqual( 1, res.CtcpMessages.Count );
            Assert.AreEqual( message.Sender, res.CtcpMessages[0].Sender );
            Assert.AreEqual( "ACTION", res.CtcpMessages[0].Command, true );
            Assert.AreEqual( "abc", res.CtcpMessages[0].Content );
        }
    }
}