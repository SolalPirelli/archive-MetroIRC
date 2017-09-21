// Copyright (C) 2012, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Text;
using IrcSharp.Internals;
using IrcSharp.Tests.TestInternals;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IrcSharp.Tests
{
    [TestClass]
    public class IrcUtilsTests
    {
        private static readonly string[] FormattingStrings = { "\x1f", "\x0f", "\x16", "\x02", "\x03" + "1", "\x03" + "11", "\x03" + "11,1", "\x03" + "11,11" };

        private IrcNetwork _network;

        [TestInitialize]
        public void Initialize()
        {
            IrcClient.StripFormatting = false;
            var tup = Initializer.GetAuthenticatedNetwork( "xxx", "yyy" );
            this._network = tup.Item1;
        }

        [TestMethod]
        public void MessageParsing_RealLife_Auth()
        {
            string text = ":pinkiepie.smoothirc.net NOTICE Auth :*** Looking up your hostname...";

            var message = IrcUtils.ParseMessage( this._network, text );

            Assert.AreEqual( "pinkiepie.smoothirc.net", message.Sender.Nickname );
            Assert.AreEqual( "notice", message.Command, true );
            Assert.AreEqual( 1, message.CommandArguments.Length );
            Assert.AreEqual( "Auth", message.CommandArguments[0] );
            Assert.AreEqual( "*** Looking up your hostname...", message.Content );
        }

        [TestMethod]
        public void MessageParsing_RealLife_Motd()
        {
            string text = ":pinkiepie.smoothirc.net 372 Aethec :-   ~ SSL : 6669";

            var message = IrcUtils.ParseMessage( this._network, text );

            Assert.AreEqual( "pinkiepie.smoothirc.net", message.Sender.Nickname );
            Assert.AreEqual( 372, int.Parse( message.Command ) );
            Assert.AreEqual( 1, message.CommandArguments.Length );
            Assert.AreEqual( "Aethec", message.CommandArguments[0] );
            Assert.AreEqual( "-   ~ SSL : 6669", message.Content );
        }

        [TestMethod]
        public void MessageParsing_RealLife_UserMode()
        {
            string text = ":Aethec!Aethec@Smooth-uq4imh.epfl.ch MODE Aethec +x";

            var message = IrcUtils.ParseMessage( this._network, text );

            Assert.AreEqual( "Aethec", message.Sender.Nickname );
            Assert.AreEqual( "Aethec", message.Sender.UserName );
            Assert.AreEqual( "Smooth-uq4imh.epfl.ch", message.Sender.Host );
            Assert.AreEqual( "mode", message.Command, true );
            Assert.AreEqual( 2, message.CommandArguments.Length );
            Assert.AreEqual( "Aethec", message.CommandArguments[0] );
            Assert.AreEqual( "+x", message.CommandArguments[1] );
        }

        [TestMethod]
        public void MessageParsing_RealLife_Pong()
        {
            string text = ":strasbourg.fr.epiknet.org PONG strasbourg.fr.epiknet.org :1353594267701";

            var message = IrcUtils.ParseMessage( this._network, text );

            Assert.AreEqual( "strasbourg.fr.epiknet.org", message.Sender.Nickname );
            Assert.AreEqual( "pong", message.Command, true );
            Assert.AreEqual( 1, message.CommandArguments.Length );
            Assert.AreEqual( "strasbourg.fr.epiknet.org", message.CommandArguments[0] );
            Assert.AreEqual( "1353594267701", message.Content );
        }

        [TestMethod]
        public void MessageParsing_RealLife_MyInfo()
        {
            string text = ":strasbourg.fr.epiknet.org 004 Aethec strasbourg.fr.epiknet.org Unreal3.2.7+EpiKnet iowghraAsORTVSxNCWqBzvdHtGp lvhopsmntikrRcaqOALQbSeIKVfMCuzNTGj";

            var message = IrcUtils.ParseMessage( this._network, text );

            Assert.AreEqual( "strasbourg.fr.epiknet.org", message.Sender.Nickname );
            Assert.AreEqual( 4, int.Parse( message.Command ) );
            Assert.AreEqual( 5, message.CommandArguments.Length );
            Assert.AreEqual( "Aethec", message.CommandArguments[0] );
            Assert.AreEqual( "strasbourg.fr.epiknet.org", message.CommandArguments[1] );
            Assert.AreEqual( "Unreal3.2.7+EpiKnet", message.CommandArguments[2] );
            Assert.AreEqual( "iowghraAsORTVSxNCWqBzvdHtGp", message.CommandArguments[3] );
            Assert.AreEqual( "lvhopsmntikrRcaqOALQbSeIKVfMCuzNTGj", message.CommandArguments[4] );
            Assert.AreEqual( string.Empty, message.Content );
        }

        [TestMethod]
        public void MessageParsing_NoSender()
        {
            TestMessage
            (
                command: "command",
                arguments: new[] { "arg" },
                content: "the quick brown fox jumps over the lazy dog"
            );
        }

        [TestMethod]
        public void MessageParsing_NoArguments()
        {
            TestMessage
            (
                nick: "nick", user: "user", host: "host",
                command: "command",
                content: "the quick brown fox jumps over the lazy dog"
            );
        }

        [TestMethod]
        public void MessageParsing_NoContent()
        {
            TestMessage
            (
                nick: "nick", user: "user", host: "host",
                command: "command",
                arguments: new[] { "arg" }
            );
        }

        [TestMethod]
        public void MessageParsing_NoSender_NoArguments()
        {
            TestMessage
            (
                command: "command",
                content: "the quick brown fox jumps over the lazy dog"
            );
        }

        [TestMethod]
        public void MessageParsing_NoSender_NoContent()
        {
            TestMessage
            (
                command: "command",
                arguments: new[] { "arg" }
            );
        }

        [TestMethod]
        public void MessageParsing_NoArguments_NoContent()
        {
            TestMessage
            (
                nick: "nick", user: "user", host: "host",
                command: "command"
            );
        }

        [TestMethod]
        public void MessageParsing_NoSender_NoArguments_NoContent()
        {
            TestMessage
            (
                command: "command"
            );
        }

        [TestMethod]
        public void MessageParsing_FullMessage()
        {
            TestMessage
            (
                nick: "nick", user: "user", host: "host",
                command: "command",
                arguments: new[] { "arg" },
                content: "the quick brown fox jumps over the lazy dog"
            );
        }

        [TestMethod]
        public void MessageParsing_ManyArguments_NoContent()
        {
            TestMessage
            (
                nick: "nick", user: "user", host: "host",
                command: "command",
                arguments: new[] { "a", "bb", "ccc", "ddd", "eeee", "fff", "gg", "h" }
            );
        }

        [TestMethod]
        public void MessageParsing_FullMessage_ManyArguments()
        {
            TestMessage
            (
                nick: "nick", user: "user", host: "host",
                command: "command",
                arguments: new[] { "a", "bb", "ccc", "ddd", "eeee", "fff", "gg", "h" },
                content: "the quick brown fox jumps over the lazy dog"
            );
        }

        [TestMethod]
        public void MessageParsing_ContentWithSpecialChars()
        {
            TestMessage
            (
                nick: "nick", user: "user", host: "host",
                command: "command",
                arguments: new[] { "arg" },
                content: IrcUtils.MessagePartsSeparator + IrcUtils.MessageCommandSeparator + IrcUtils.UserHostSeparator + IrcUtils.UserNameSeparator
            );
        }

        [TestMethod]
        public void MessageParsing_ContentWithSpecialChars_NoSender()
        {
            TestMessage
            (
                command: "command",
                arguments: new[] { "arg" },
                content: IrcUtils.MessagePartsSeparator + IrcUtils.MessageCommandSeparator + IrcUtils.UserHostSeparator + IrcUtils.UserNameSeparator
            );
        }

        [TestMethod]
        public void MessageParsing_FormatStrippingRequired_FormatStripped()
        {
            IrcClient.StripFormatting = true;

            var message = TestMessage
            (
                nick: "nick", user: "user", host: "host",
                command: "command",
                arguments: new[] { "arg" },
                content: string.Join( "word", FormattingStrings )
            );

            foreach ( string format in FormattingStrings )
            {
                Assert.IsFalse( message.Content.Contains( format ) );
            }
        }

        [TestMethod]
        public void MessageParsing_FormatStrippingDisabled_FormatNotStripped()
        {
            IrcClient.StripFormatting = false;

            var message = TestMessage
            (
                nick: "nick", user: "user", host: "host",
                command: "command",
                arguments: new[] { "arg" },
                content: string.Join( "word", FormattingStrings )
            );

            foreach ( string format in FormattingStrings )
            {
                Assert.IsTrue( message.Content.Contains( format ) );
            }
        }

        [TestMethod]
        [ExpectedException( typeof( Exception ), AllowDerivedTypes = true )]
        public void MessageParsing_NoSenderHost()
        {
            IrcUtils.ParseMessage( this._network, ":nick!user_butnohost blah blah blah" );
        }

        [TestMethod]
        [ExpectedException( typeof( Exception ), AllowDerivedTypes = true )]
        public void MessageParsing_NoSenderUsername()
        {
            IrcUtils.ParseMessage( this._network, ":nick_butnouser@host blah blah blah" );
        }

        [TestMethod]
        [ExpectedException( typeof( Exception ), AllowDerivedTypes = true )]
        public void MessageParsing_NoCommand()
        {
            IrcUtils.ParseMessage( this._network, ":nick!user@host" );
        }


        private IrcMessage TestMessage( string nick = null, string user = null, string host = null,
                                        string command = null, string[] arguments = null,
                                        string content = null )
        {
            string sender = string.Empty;
            if ( nick != null )
            {
                sender = IrcUtils.SenderIndicator + nick + IrcUtils.UserNameSeparator + user + IrcUtils.UserHostSeparator + host;
            }

            var builder = new StringBuilder();
            builder.Append( sender );
            builder.Append( IrcUtils.MessagePartsSeparator );
            builder.Append( command );
            builder.Append( IrcUtils.MessagePartsSeparator );

            if ( arguments != null )
            {
                builder.Append( string.Join( IrcUtils.MessagePartsSeparator, arguments ) );
            }
            if ( content != null )
            {
                builder.Append( IrcUtils.MessageCommandSeparator );
                builder.Append( content );
            }

            var message = IrcUtils.ParseMessage( this._network, builder.ToString() );

            if ( sender == string.Empty )
            {
                Assert.AreEqual( this._network.ServerUser, message.Sender );
            }
            else
            {
                Assert.AreEqual( nick, message.Sender.Nickname );
                Assert.AreEqual( user, message.Sender.UserName );
                Assert.AreEqual( host, message.Sender.Host );
            }

            Assert.AreEqual( command, message.Command, true );

            if ( arguments != null )
            {
                Assert.AreEqual( message.CommandArguments.Length, arguments.Length );
                for ( int n = 0; n < arguments.Length; n++ )
                {
                    Assert.AreEqual( arguments[n], message.CommandArguments[n] );
                }
            }

            if ( content == null )
            {
                Assert.AreEqual( string.Empty, message.Content );
            }
            else if ( IrcClient.StripFormatting == false )
            {
                Assert.AreEqual( content, message.Content );
            }

            return message;
        }
    }
}