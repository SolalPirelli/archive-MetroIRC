// Copyright (C) 2012, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Collections.Generic;
using System.Linq;

namespace IrcSharp.Internals
{
    /// <summary>
    /// An internal utility class for CTCP-related tasks such as encoding and decoding messages.
    /// </summary>
    /// <remarks>
    /// Allow me to digress from the nice docs for a while and talk about CTCP using more appropriate language.
    /// CTCP is horribly (and uselessly) complicated and badly documented.
    /// Some CTCP "commands" are not documented (and I mean no doc at all), which means they can't be implemented. One 
    /// "feature" of CTCP is that you can mix CTCP and IRC messages in one message. Annoying to support, useless in real life.
    /// Another "feature" is the quoting. There are two quoting levels that allow for arbitrary characters to be sent even 
    /// though nobody ever does that, so the lib has to support that, too. And by "support" I mean "let's hope nobody ever
    /// sends us a complicated quoted message because it's not gonna work since a proper implementation would be way too
    /// complicated".
    /// 
    /// There are a zillion ways to extend IRC; CTCP is probably the worst possible one.
    /// </remarks>
    internal static class CtcpUtils
    {
        #region Constants
        /// <summary>
        /// The separator used before and after CTCP messages.
        /// </summary>
        private const string CtcpDelimiter = "\x1";

        /// <summary>
        /// The escape character for low-level quoting.
        /// </summary>
        private const string LowQuoteCharacter = "\x10";

        /// <summary>
        /// The escape character for CTCP-level quoting.
        /// </summary>
        private const string CtcpQuoteCharacter = "\x5C";

        /// <summary>
        /// The low-level escaped characters.
        /// </summary>
        /// <remarks>
        /// Theoretically, these should be applied to both IRC and CTCP messages.
        /// IRC# does not do that, because I believe IRC and CTCP should remain separate.
        /// An IRC client should not need to understand the CTCP protocol to display messages properly.
        /// </remarks>
        private static readonly Dictionary<string, string> LowLevelEscapes = new Dictionary<string, string>
        {
            { LowQuoteCharacter + "0", "\0" },
            { LowQuoteCharacter + "r", "\r" },
            { LowQuoteCharacter + "n", "\n" },
        };

        /// <summary>
        /// The CTCP-level escaped characters.
        /// </summary>
        private static readonly Dictionary<string, string> CtcpLevelEscapes = new Dictionary<string, string>
        {
            { CtcpQuoteCharacter + "a", CtcpDelimiter },
        };

        /// <summary>
        /// The commands that can contain CTCP messages.
        /// Any other commands containing CTCP messages are silently ignored.
        /// </summary>
        private static readonly string[] EncapsulatingCommands = { "privmsg", "notice" };

        /// <summary>
        /// The commands that are CTCP queries (and not answers to queries).
        /// </summary>
        private static readonly string[] QueryCommands = { "privmsg" };

        /// <summary>
        /// The separators used between CTCP commands and their arguments.
        /// </summary>
        private static readonly char[] CommandSeparators = { ':', ' ' };
        #endregion

        #region Public methods
        /// <summary>
        /// Indicates whether the specified <see cref="IrcMessage"/> contains one or more <see cref="CtcpMessage"/>s.
        /// </summary>
        public static bool IsCtcpMessage( IrcMessage message )
        {
            return EncapsulatingCommands.Contains( message.Command )
                && message.Content.IndexOf( CtcpDelimiter ) != message.Content.LastIndexOf( CtcpDelimiter ); // clever way to check count(delimiter) > 2
        }

        /// <summary>
        /// Encodes the specified text using CTCP-level quoting, low-level quoting and wrapping between CTCP delimiters.
        /// </summary>
        public static string EncodeMessage( string text )
        {
            // CTCP-level quote
            text = text.Replace( CtcpQuoteCharacter, CtcpQuoteCharacter + CtcpQuoteCharacter );
            foreach ( var pair in CtcpLevelEscapes )
            {
                text = text.Replace( pair.Value, pair.Key );
            }
            // Low-level quote
            text = text.Replace( LowQuoteCharacter, LowQuoteCharacter + LowQuoteCharacter );
            foreach ( var pair in LowLevelEscapes )
            {
                text = text.Replace( pair.Value, pair.Key );
            }

            return CtcpDelimiter + text + CtcpDelimiter;
        }

        /// <summary>
        /// Decodes the specified text using low-level dequoting and then CTCP-level dequoting.
        /// </summary>
        public static string DecodeMessage( string text )
        {
            // Low-level dequote
            foreach ( var pair in LowLevelEscapes )
            {
                text = text.Replace( pair.Key, pair.Value );
            }
            text = text.Replace( LowQuoteCharacter + LowQuoteCharacter, LowQuoteCharacter );
            // CTCP-level dequote
            foreach ( var pair in CtcpLevelEscapes )
            {
                text = text.Replace( pair.Key, pair.Value );
            }
            text = text.Replace( CtcpQuoteCharacter + CtcpQuoteCharacter, CtcpQuoteCharacter );
            return text;
        }

        /// <summary>
        /// Filters the <see cref="IrcMessage"/>s and <see cref="CtcpMessage"/>s embedded in a single <see cref="IrcMessage"/>.
        /// </summary>
        public static MessagesInfo FilterMessage( IrcMessage message )
        {
            var commands = FilterCommands( message.Content );

            var ircMessages = commands.IrcTexts.Select( s => new IrcMessage( message.Sender, message.Command, message.CommandArguments, s ) ).ToArray();
            var ctcpMessages = commands.CtcpCommands.Select( s => ParseMessage( message, s ) ).ToArray();

            return new MessagesInfo( ircMessages, ctcpMessages );
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Indicates whether the specified <see cref="IrcMessage"/> is a CTCP query.
        /// </summary>
        private static bool IsQuery( IrcMessage message )
        {
            return QueryCommands.Contains( message.Command );
        }

        /// <summary>
        /// Extracts a <see cref="CtcpMessage"/> from an <see cref="IrcMessage"/> and the actual message text.
        /// </summary>
        private static CtcpMessage ParseMessage( IrcMessage message, string content )
        {
            IrcChannel channel = null;
            var network = message.Sender.Network;
            if ( network.Parameters.IsChannelName( message.CommandArguments[0] ) )
            {
                channel = network.GetChannel( message.CommandArguments[0] );
            }

            bool isQuery = IsQuery( message );
            var split = SplitMessage( content );
            string decodedContent = DecodeMessage( split.Content );
            return new CtcpMessage( channel, message.Sender, split.Command, decodedContent, isQuery );
        }

        /// <summary>
        /// Sort commands in one message ; there can be CTCP and IRC commands in one message.
        /// </summary>
        /// <remarks>
        /// The CTCP protocol says that CTCP messages have to be preceded and followed by the CTCP delimiter (char 1).
        /// Examples:
        /// \1 CTCP command \1
        /// \1 CTCP command \1 IRC command
        /// IRC message \1 CTCP command \1 CTCP command \1
        /// 
        /// We know that there is at least one CTCP message.
        /// </remarks>
        private static CommandsInfo FilterCommands( string message )
        {
            string[] parts = message.Split( new[] { CtcpDelimiter }, StringSplitOptions.RemoveEmptyEntries );
            var ircTexts = new List<string>();
            var ctcpCommands = new List<string>();

            if ( parts.Length == 1 ) // most common case.
            {
                ctcpCommands.Add( parts[0] );
            }
            else
            {
                bool isFirstCtcp = message.StartsWith( CtcpDelimiter );
                bool isLastCtcp = message.EndsWith( CtcpDelimiter );

                if ( !isFirstCtcp )
                {
                    ircTexts.Add( parts[0] );
                }
                if ( !isLastCtcp )
                {
                    ircTexts.Add( parts.Last() );
                }

                for ( int n = isFirstCtcp ? 0 : 1; n < parts.Length - ( isLastCtcp ? 0 : 1 ); n++ )
                {
                    ctcpCommands.Add( parts[n] );
                }
            }

            return new CommandsInfo( ircTexts, ctcpCommands );
        }

        /// <summary>
        /// Splits a message into a command and a message content.
        /// </summary>
        private static SplitInfo SplitMessage( string message )
        {
            message = message.TrimStart( CommandSeparators );
            string[] parts = message.Split( CommandSeparators );
            string command = parts[0].ToLowerInvariant();
            string content = message.Substring( command.Length ).TrimStart( CommandSeparators );

            return new SplitInfo( command, content );
        }
        #endregion

        #region Nested classes
        /// <summary>
        /// The result of the <see cref="FilterMessage"/> method.
        /// </summary>
        public sealed class MessagesInfo
        {
            /// <summary>
            /// Gets the IRC messages.
            /// </summary>
            public IList<IrcMessage> IrcMessages { get; private set; }

            /// <summary>
            /// Gets the CTCP messages.
            /// </summary>
            public IList<CtcpMessage> CtcpMessages { get; private set; }

            public MessagesInfo( IList<IrcMessage> ircMessages, IList<CtcpMessage> ctcpMessages )
            {
                this.IrcMessages = ircMessages;
                this.CtcpMessages = ctcpMessages;
            }
        }

        /// <summary>
        /// The result of the <see cref="FilterCommands"/> method.
        /// </summary>
        private sealed class CommandsInfo
        {
            /// <summary>
            /// Gets the IRC texts, at most two.
            /// </summary>
            public IList<string> IrcTexts { get; private set; }

            /// <summary>
            /// Gets the CTCP texts.
            /// </summary>
            public IList<string> CtcpCommands { get; private set; }

            public CommandsInfo( IList<string> ircTexts, IList<string> ctcpCommands )
            {
                this.IrcTexts = ircTexts;
                this.CtcpCommands = ctcpCommands;
            }
        }

        /// <summary>
        /// The result of the <see cref="SplitMessage"/> method.
        /// </summary>
        private sealed class SplitInfo
        {
            /// <summary>
            /// Gets the command.
            /// </summary>
            public string Command { get; private set; }

            /// <summary>
            /// Gets the content.
            /// </summary>
            public string Content { get; private set; }

            public SplitInfo( string command, string content )
            {
                this.Command = command;
                this.Content = content;
            }
        }
        #endregion
    }
}