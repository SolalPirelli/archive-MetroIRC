// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Text;
using System.Threading.Tasks;
using BasicMvvm;
using IrcSharp;
using MetroIrc.Services;
using MetroIrc.Internals;

namespace MetroIrc
{
    public static partial class MessageSender
    {
        private static class CommandProcessors
        {
            [CommandProcessor( "part", NeedsChannel = ParameterStatus.Needed, NeedsText = ParameterStatus.Optional )]
            public static void Part( IrcNetwork network, CommandMessage command )
            {
                command.Channel.Leave( command.Text );
                Messenger.Send( new LeaveChannelMessage( command.Channel ) );
            }

            [CommandProcessor( "me", NeedsText = ParameterStatus.Needed )]
            public static void Me( IrcNetwork network, CommandMessage command )
            {
                string text = network.CurrentUser.Nickname + " " + command.Text;
                var ircMessage = new IrcMessage( network, MessageDirection.Sent, network.CurrentUser, IrcMessageType.Action, text );

                if ( network.Parameters.IsChannelName( command.TargetName ) )
                {
                    var channel = network.GetChannel( command.TargetName );
                    channel.Ctcp.SendAction( command.Text );
                    Messenger.Send( new ChannelMessageSentMessage( channel, ircMessage ) );
                }
                else
                {
                    var user = network.GetUser( command.TargetName );
                    user.Ctcp.SendAction( command.Text );
                    Messenger.Send( new UserMessageSentMessage( user, ircMessage ) );
                }
            }

            // Do not open a conversation or write the text we sent.
            [CommandProcessor( "msg", NeedsUser = ParameterStatus.Needed, NeedsText = ParameterStatus.Needed )]
            public static void Msg( IrcNetwork network, CommandMessage message )
            {
                message.User.SendMessage( message.Text );
            }


            // Open a conversation and write the text we sent.
            [CommandProcessor( "query", NeedsUser = ParameterStatus.Needed, NeedsText = ParameterStatus.Optional )]
            public static void Query( IrcNetwork network, CommandMessage message )
            {
                if ( message.Text.HasText() )
                {
                    message.User.SendMessage( message.Text );
                    var ircMessage = new IrcMessage( network, MessageDirection.Sent, network.CurrentUser, IrcMessageType.Normal, message.Text );
                    Messenger.Send( new UserMessageSentMessage( message.User, ircMessage ) );
                }
                else
                {
                    Messenger.Send( new OpenPrivateConversationMessage( message.User ) );
                }
            }

            [CommandProcessor( "quit", NeedsText = ParameterStatus.Optional )]
            public static void Quit( IrcNetwork network, CommandMessage message )
            {
                network.Quit( message.Text );

                var msg = new QuitNetworkMessage( network );
                Messenger.Send( msg );
            }

            [CommandProcessor( "kick", NeedsUser = ParameterStatus.Needed, NeedsChannel = ParameterStatus.Needed, NeedsText = ParameterStatus.Optional )]
            public static void Kick( IrcNetwork network, CommandMessage message )
            {
                message.Channel.KickUser( message.User, message.Text );
            }

            [CommandProcessor( "invite", NeedsUser = ParameterStatus.Needed, NeedsChannel = ParameterStatus.Needed )]
            public static void Invite( IrcNetwork network, CommandMessage message )
            {
                message.Channel.InviteUser( message.User );
            }

            [CommandProcessor( "topic", NeedsChannel = ParameterStatus.Needed, NeedsText = ParameterStatus.Needed )]
            public static void Topic( IrcNetwork network, CommandMessage message )
            {
                message.Channel.SetTopic( message.Text );
            }

            [CommandProcessor( "cleartopic", NeedsChannel = ParameterStatus.Needed )]
            public static void ClearTopic( IrcNetwork network, CommandMessage message )
            {
                message.Channel.ClearTopic();
            }

            [CommandProcessor( "ban", NeedsChannel = ParameterStatus.Needed, NeedsUser = ParameterStatus.Needed, NeedsText = ParameterStatus.Optional )]
            public static void Ban( IrcNetwork network, CommandMessage message )
            {
                message.Channel.BanUser( message.User, true, message.Text );
            }

            [CommandProcessor( "banlist", NeedsChannel = ParameterStatus.Needed )]
            public static void BanList( IrcNetwork network, CommandMessage message )
            {
                message.Channel.SetMode( "+b" );
            }

            [CommandProcessor( "mode", NeedsChannel = ParameterStatus.Optional, NeedsText = ParameterStatus.Needed )]
            public static void Mode( IrcNetwork network, CommandMessage message )
            {
                if ( message.Channel == null )
                {
                    network.Client.SendRawData( message.Text );
                }
                else
                {
                    message.Channel.SetMode( message.Text );
                }
            }

            // server <hostname> <port> [password]
            [CommandProcessor( "server", NeedsText = ParameterStatus.Needed )]
            public static void Server( IrcNetwork network, CommandMessage message )
            {
                string[] parts = message.Text.Split( new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries );
                int port;

                if ( parts.Length > 1 && int.TryParse( parts[1], out port ) )
                {
                    string password = RemoveFirstWords( message.Text, 2 );

                    var msg = new AddNetworkMessage( parts[0], port, password );
                    Messenger.Send( msg );
                }
            }

            [CommandProcessor( "charset", NeedsText = ParameterStatus.Optional )]
            public static void Charset( IrcNetwork network, CommandMessage message )
            {
                string text;
                if ( message.Text.HasText() )
                {
                    try
                    {
                        Encoding encoding = Encoding.GetEncoding( message.Text );
                        text = Locator.Get<ITranslationService>().Translate( "CharsetInformation", "CharsetChanged", encoding.WebName );

                        var encodingMsg = new ChangeEncodingMessage( network, encoding );
                        Messenger.Send( encodingMsg );
                    }
                    catch
                    {
                        text = Locator.Get<ITranslationService>().Translate( "CharsetInformation", "InvalidCharset" );
                    }
                }
                else
                {
                    text = Locator.Get<ITranslationService>().Translate( "CharsetInformation", "CurrentCharsetIs", network.Client.Encoding.WebName );
                }

                var ircMessage = new IrcMessage( network, MessageDirection.Internal, null, IrcMessageType.Info, text );
                Messenger.Send( new GlobalMessageSentMessage( ircMessage ) );
            }

            // delay <time in seconds> <command>
            [CommandProcessor( "delay", NeedsText = ParameterStatus.Needed )]
            public async static void Delay( IrcNetwork network, CommandMessage message )
            {
                var parts = message.Text.Split( new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries );
                int delay;
                if ( parts.Length > 1 && int.TryParse( parts[0], out delay ) )
                {
                    delay *= 1000; // convert to milliseconds
                    string newText = RemoveFirstWords( message.Text, 1 );

                    await Task.Delay( delay );
                    MessageSender.SendMessage( network, message.TargetName, newText );
                }
            }

            #region Modes
            [CommandProcessor( "op", NeedsChannel = ParameterStatus.Needed, NeedsUser = ParameterStatus.Needed )]
            public static void Op( IrcNetwork network, CommandMessage message )
            {
                SetMode( message, "+o" );
            }

            // deop [channel] <user>
            // channel is the current one if not specified
            [CommandProcessor( "deop" )]
            public static void Deop( IrcNetwork network, CommandMessage message )
            {
                SetMode( message, "-o" );
            }

            // halfop [channel] <user>
            // channel is the current one if not specified
            [CommandProcessor( "halfop" )]
            public static void Halfop( IrcNetwork network, CommandMessage message )
            {
                SetMode( message, "+h" );
            }

            // dehalfop [channel] <user>
            // channel is the current one if not specified
            [CommandProcessor( "dehalfop" )]
            public static void Dehalfop( IrcNetwork network, CommandMessage message )
            {
                SetMode( message, "-h" );
            }

            // voice [channel] <user>
            // channel is the current one if not specified
            [CommandProcessor( "voice" )]
            public static void Voice( IrcNetwork network, CommandMessage message )
            {
                SetMode( message, "+v" );
            }

            // devoice [channel] <user>
            // channel is the current one if not specified
            [CommandProcessor( "devoice" )]
            public static void Devoice( IrcNetwork network, CommandMessage message )
            {
                SetMode( message, "-v" );
            }
            #endregion

            #region CTCP
            // ping <user>
            [CommandProcessor( "ctcp-ping", NeedsUser = ParameterStatus.Needed )]
            public static void CtcpPing( IrcNetwork network, CommandMessage message )
            {
                message.User.Ctcp.Ping();
            }

            // userinfo <user>
            [CommandProcessor( "ctcp-userinfo", NeedsUser = ParameterStatus.Needed )]
            public static void CtcpUserInfo( IrcNetwork network, CommandMessage message )
            {
                message.User.Ctcp.GetInformation();
            }

            // source <user>
            [CommandProcessor( "ctcp-source", NeedsUser = ParameterStatus.Needed )]
            public static void CtcpSource( IrcNetwork network, CommandMessage message )
            {
                message.User.Ctcp.GetClientLocation();
            }

            // clientinfo <user> [command]
            [CommandProcessor( "ctcp-clientinfo", NeedsUser = ParameterStatus.Needed, NeedsText = ParameterStatus.Optional )]
            public static void CtcpClientInfo( IrcNetwork network, CommandMessage message )
            {
                message.User.Ctcp.GetClientCapabilities( message.Text );
            }

            // version <user>
            [CommandProcessor( "ctcp-version", NeedsUser = ParameterStatus.Needed )]
            public static void CtcpVersion( IrcNetwork network, CommandMessage message )
            {
                message.User.Ctcp.GetClientInformation();
            }

            // time <user>
            [CommandProcessor( "ctcp-time", NeedsUser = ParameterStatus.Needed )]
            public static void CtcpTime( IrcNetwork network, CommandMessage message )
            {
                message.User.Ctcp.GetLocalTime();
            }
            #endregion

            // Generic method to set the mode.
            private static void SetMode( CommandMessage message, string mode )
            {
                message.Channel.SetMode( mode + " " + message.User.Nickname );
            }
        }
    }
}