// Copyright (C) 2011-2013, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Collections.Generic;
using System.Linq;
using BasicMvvm;
using IrcSharp;
using MetroIrc.Internals;
// network, command
using CommandProcessor = System.Action<IrcSharp.IrcNetwork, MetroIrc.CommandMessage>;

namespace MetroIrc
{
    public static partial class MessageSender
    {
        #region Constants
        private static readonly char[] CommandSeparators = { ' ' };
        private const string CommandIndicator = "/";

        private const string CtcpPrefix = CommandIndicator + "ctcp-";
        private const string AlternateCtcpPrefix = CommandIndicator + "ctcp ";

        private static readonly Dictionary<string, string> CommandAliases = new Dictionary<string, string>
        {
            { "j", "join" },
        };

        private static readonly Dictionary<string, string> ReplaceableStrings = new Dictionary<string, string>
        {
            { "^C", "\x03" },
            { "^R", "\x16" },
            { "^B", "\x02" },
            { "^U", "\x1f" },
            { "^N", "\x0f" }
        };
        #endregion

        #region Private static members
        private static Dictionary<CommandProcessorAttribute, CommandProcessor> Processors = new Dictionary<CommandProcessorAttribute, CommandProcessor>();
        #endregion

        static MessageSender()
        {
            InitializeProcessors();
        }

        #region Public methods
        public static void SendMessage( IrcNetwork network, string targetName, string text )
        {
            var split = SplitCommand( ProcessText( text ) );
            if ( split.Item1.HasText() )
            {
                HandleCommandMessage( network, targetName, split.Item1, split.Item2 );
            }
            else
            {
                HandleNormalMessage( network, targetName, text );
            }
        }

        public static string ProcessText( string text )
        {
            if ( text.IsEmpty() )
            {
                return text;
            }

            foreach ( var pair in ReplaceableStrings )
            {
                text = text.Replace( pair.Key, pair.Value );
            }

            return text;
        }

        public static string ReverseProcessing( string text )
        {
            if ( text.IsEmpty() )
            {
                return text;
            }

            foreach ( var pair in ReplaceableStrings )
            {
                text = text.Replace( pair.Value, pair.Key );
            }

            return text;
        }
        #endregion

        #region Private methods
        private static void InitializeProcessors()
        {
            Processors = ReflectionHelper.GetAttributedMethods<CommandProcessorAttribute>( typeof( MessageSender.CommandProcessors ) )
                                         .ToDictionary( t => t.Item1,
                                                        t => t.Item2.GetStaticDelegate<CommandProcessor>() );
        }

        private static Tuple<string, string> SplitCommand( string text )
        {
            if ( text.BeginsWith( CommandIndicator ) )
            {
                if ( text.BeginsWith( AlternateCtcpPrefix ) )
                {
                    text = text.ReplaceFirst( AlternateCtcpPrefix, CtcpPrefix );
                }
                string command = text.Split( CommandSeparators )[0].Substring( 1 ).ToLowerInvariant();
                return Tuple.Create( command, RemoveFirstWords( text, 1 ) );
            }
            return Tuple.Create( string.Empty, text );
        }

        private static void HandleCommandMessage( IrcNetwork network, string targetName, string command, string text )
        {
            if ( CommandAliases.ContainsKey( command ) )
            {
                command = CommandAliases[command];
            }

            var attrib = Processors.Keys.FirstOrDefault( p => p.Command == command );

            if ( attrib == null )
            {
                network.Client.SendRawData( command + CommandSeparators[0] + text );
            }
            else
            {
                var message = GetMessage( network, text, targetName, attrib );
                if ( CheckCommandMessage( message, attrib ) )
                {
                    Processors[attrib]( network, message );
                }
            }
        }

        private static void HandleNormalMessage( IrcNetwork network, string targetName, string text )
        {
            var ircMessage = new IrcMessage( network, MessageDirection.Sent, network.CurrentUser, IrcMessageType.Normal, text );

            if ( targetName == null )
            {
                // don't show the sent message
                network.Client.SendRawData( text );
            }
            else if ( network.Parameters.IsChannelName( targetName ) )
            {
                var channel = network.GetChannel( targetName );
                channel.SendMessage( text );
                Messenger.Send( new ChannelMessageSentMessage( channel, ircMessage ) );
            }
            else
            {
                var user = network.GetUser( targetName );
                user.SendMessage( text );
                Messenger.Send( new UserMessageSentMessage( user, ircMessage ) );
            }
        }

        private static CommandMessage GetMessage( IrcNetwork network, string text, string targetName, CommandProcessorAttribute attrib )
        {
            var split = ParseCommand( network, text, attrib );
            split.SetChannelFallback( network, targetName );

            var chan = split.ChannelName.HasText() ? network.GetChannel( split.ChannelName ) : null;
            var user = split.UserName.HasText() ? network.GetUser( split.UserName ) : null;

            return new CommandMessage( split.Text.Trim(), chan, user, targetName );
        }

        private static SplitInfo ParseCommand( IrcNetwork network, string text, CommandProcessorAttribute attrib )
        {
            if ( text.IsEmpty() )
            {
                return new SplitInfo( string.Empty, string.Empty, string.Empty );
            }

            string[] twoFirst = text.Split( CommandSeparators, 2, StringSplitOptions.RemoveEmptyEntries );

            if ( network.Parameters.IsChannelName( twoFirst[0] ) )
            {
                if ( attrib.NeedsChannel == ParameterStatus.NotNeeded )
                {
                    return new SplitInfo( text, string.Empty, string.Empty );
                }

                if ( twoFirst.Length > 1 && attrib.NeedsUser != ParameterStatus.NotNeeded )
                {
                    string content = RemoveFirstWords( text, 2 );
                    return new SplitInfo( content, twoFirst[0], twoFirst[1] );
                }
                else
                {
                    string content = RemoveFirstWords( text, 1 );
                    return new SplitInfo( content, twoFirst[0], string.Empty );
                }
            }
            else
            {
                if ( attrib.NeedsUser == ParameterStatus.NotNeeded )
                {
                    return new SplitInfo( text, string.Empty, string.Empty );
                }

                if ( twoFirst.Length > 1 && network.Parameters.IsChannelName( twoFirst[1] ) && attrib.NeedsChannel != ParameterStatus.NotNeeded )
                {
                    string content = RemoveFirstWords( text, 2 );
                    return new SplitInfo( content, twoFirst[1], twoFirst[0] );
                }
                else
                {
                    string content = RemoveFirstWords( text, 1 );
                    return new SplitInfo( content, string.Empty, twoFirst[0] );
                }
            }
        }

        private static string RemoveFirstWords( string s, int count )
        {
            int found = 0;
            int n = 0;
            for ( ; n < s.Length && found < count; n++ )
            {
                if ( char.IsWhiteSpace( s[n] ) )
                {
                    found++;
                }
            }

            return s.Substring( n );
        }

        private static bool CheckCommandMessage( CommandMessage message, CommandProcessorAttribute attrib )
        {
            if ( message.Channel == null && attrib.NeedsChannel == ParameterStatus.Needed )
            {
                return false;
            }
            if ( message.User == null && attrib.NeedsUser == ParameterStatus.Needed )
            {
                return false;
            }
            if ( message.Text.IsEmpty() && attrib.NeedsText == ParameterStatus.Needed )
            {
                return false;
            }
            return true;
        }
        #endregion

        private sealed class SplitInfo
        {
            public string Text { get; private set; }
            public string ChannelName { get; private set; }
            public string UserName { get; private set; }

            public SplitInfo( string text, string channelName, string userName )
            {
                this.Text = text;
                this.ChannelName = channelName;
                this.UserName = userName;
            }

            public void SetChannelFallback( IrcNetwork network, string name )
            {
                if ( this.ChannelName.IsEmpty() && name.HasText() && network.Parameters.IsChannelName( name ) )
                {
                    this.ChannelName = name;
                }
            }
        }

        private sealed class CommandProcessorAttribute : Attribute
        {
            public string Command { get; private set; }
            public ParameterStatus NeedsText { get; set; }
            public ParameterStatus NeedsChannel { get; set; }
            public ParameterStatus NeedsUser { get; set; }

            public CommandProcessorAttribute( string command )
            {
                this.Command = command;
                this.NeedsText = ParameterStatus.NotNeeded;
                this.NeedsChannel = ParameterStatus.NotNeeded;
                this.NeedsUser = ParameterStatus.NotNeeded;
            }
        }

        private enum ParameterStatus
        {
            Needed,
            Optional,
            NotNeeded
        }
    }
}