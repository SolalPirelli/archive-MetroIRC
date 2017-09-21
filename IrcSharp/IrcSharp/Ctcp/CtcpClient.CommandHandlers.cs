// Copyright (C) 2011-2013, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Linq;
using IrcSharp.Internals;

namespace IrcSharp.Ctcp
{
    public sealed partial class CtcpClient
    {
        /// <summary>
        /// Contains method that handle CTCP commands.
        /// </summary>
        private static class CommandHandlers
        {
            // ACTION <message>
            // There is no query or reply form.
            [CtcpCommand( "action" )]
            public static void Action( CtcpClient client, CtcpMessage message )
            {
                if ( message.Channel == null )
                {
                    message.Sender.Ctcp.OnActionReceived( message.Content );
                }
                else
                {
                    message.Channel.Ctcp.OnActionReceived( message.Sender, message.Content );
                }
            }

            // Query :
            // FINGER
            // Reply :
            // FINGER :<message>
            // The message must contain information about the user such as their real name.
            [CtcpCommand( "finger" )]
            public static void Finger( CtcpClient client, CtcpMessage message )
            {
                if ( message.IsQuery )
                {
                    message.Sender.Ctcp.SendNotice( "FINGER :" + client.CurrentUserInformation );
                }
                else
                {
                    message.Sender.Ctcp.OnInformationReceived( message.Content );
                }
            }

            // Query :
            // VERSION
            // Reply :
            // VERSION <version>
            // The reply format defined in the CTCP RFC is <name>:<version>:<environment>, but nobody uses it.
            [CtcpCommand( "version" )]
            public static void Version( CtcpClient client, CtcpMessage message )
            {
                if ( message.IsQuery )
                {
                    message.Sender.Ctcp.SendNotice( "VERSION " + CtcpClient.ClientSoftware.ToString() );
                }
                else
                {
                    message.Sender.Ctcp.OnVersionReplyReceived( message.Content );
                }
            }

            // Query :
            // SOURCE
            // Reply :
            // SOURCE :<source>
            // The reply format defined in the CTCP RFC is <ftp address>:<directory name>:<file list>, but nobody uses it.
            [CtcpCommand( "source" )]
            public static void Source( CtcpClient client, CtcpMessage message )
            {
                if ( message.IsQuery )
                {
                    message.Sender.Ctcp.SendNotice( "SOURCE :" + CtcpClient.ClientSoftware.DownloadLocation );
                }
                else
                {
                    message.Sender.Ctcp.OnSourceReplyReceived( message.Content );
                }
            }

            // Query :
            // USERINFO
            // Reply :
            // USERINFO :<message>
            // The message can be anything.
            [CtcpCommand( "userinfo" )]
            public static void UserInfo( CtcpClient client, CtcpMessage message )
            {
                if ( message.IsQuery )
                {
                    message.Sender.Ctcp.SendNotice( "USERINFO :" + client.CurrentUserInformation );
                }
                else
                {
                    message.Sender.Ctcp.OnInformationReceived( message.Content );
                }
            }

            // Query :
            // CLIENTINFO [:command]
            // Reply to argumentless query :
            // CLIENTINFO :<command list>
            // Reply to argumented query :
            // CLIENTINFO :<command description>
            // IRC# sends the command in the command description to be more user-friendly.
            [CtcpCommand( "clientinfo" )]
            public static void ClientInfo( CtcpClient client, CtcpMessage message )
            {
                if ( message.IsQuery )
                {
                    string command = message.Content.GetFirstWord();
                    if ( command.HasText() )
                    {
                        string text;
                        if ( !CtcpClient.CommandDescriptions.TryGetValue( command, out text ) )
                        {
                            text = "N/A";
                        }
                        message.Sender.Ctcp.SendNotice( "CLIENTINFO :" + command + ": " + text );
                    }
                    else
                    {
                        message.Sender.Ctcp.SendNotice( "CLIENTINFO :" + string.Join( ", ", CtcpClient._commandHandlers.Keys ) );
                    }
                }
                else
                {
                    message.Sender.Ctcp.OnCapabilitiesReceived( message.Content );
                }
            }

            // ERRMSG <command> :<description>
            // The query form should not be used.
            [CtcpCommand( "errmsg" )]
            public static void ErrMsg( CtcpClient client, CtcpMessage message )
            {
                message.Sender.Ctcp.OnErrorMessageReceived( message.Content );
            }

            // Query :
            // PING <timestamp>
            // Reply :
            // PING <timestamp>
            // The timestamp is in "whatever format the client finds convenient". ChatZilla uses UNIX milliseconds, let's copy that.
            // Unlike the IRC ping, this ping is designed to measure lag ; the reply timestamp must be the user's, not the original one.
            [CtcpCommand( "ping" )]
            public static void Ping( CtcpClient client, CtcpMessage message )
            {
                if ( message.IsQuery )
                {
                    message.Sender.Ctcp.SendNotice( "PING " + message.Content );
                }
                else if ( message.Sender.Ctcp.PingTimes.Any() )
                {
                    long pingTime = TimeHelper.DateTimeToUnixMilliseconds( DateTime.UtcNow ) - message.Sender.Ctcp.PingTimes.Dequeue();
                    message.Sender.Ctcp.OnPingReplyReceived( TimeSpan.FromMilliseconds( pingTime ) );
                }
            }

            // Query :
            // TIME
            // Reply :
            // TIME :<time>
            // The time parameter is in a "human-readable time format".
            [CtcpCommand( "time" )]
            public static void Time( CtcpClient client, CtcpMessage message )
            {
                if ( message.IsQuery )
                {
                    message.Sender.Ctcp.SendNotice( "TIME :" + DateTime.Now.ToString( "F" ) );
                }
                else
                {
                    message.Sender.Ctcp.OnLocalTimeReceived( message.Content );
                }
            }
        }
    }
}