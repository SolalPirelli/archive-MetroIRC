// Copyright (C) 2011-2013, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Collections.Generic;
using System.Linq;
using IrcSharp.Internals;

namespace IrcSharp
{
    public sealed partial class IrcClient
    {
        /// <summary>
        /// Contains method that handle IRC commands.
        /// </summary>
        private static class CommandHandlers
        {
            #region Compatibility
            //:<user> JOIN <channel>
            // Sometimes the channel is in the message content
            [IrcCommand( "join", IsCompatibilityCommand = true )]
            public static void Compatibility_Join( IrcClient client, IrcMessage message )
            {
                if ( message.CommandArguments.Length == 0 )
                {
                    message.CommandArguments = new string[] { message.Content };
                }
            }

            //:<user> PART <channel> :<reason>
            // Sometimes the channel is in the message content (and there's no part reason)
            [IrcCommand( "part", IsCompatibilityCommand = true )]
            public static void Compatibility_Part( IrcClient client, IrcMessage message )
            {
                if ( message.CommandArguments.Length == 0 )
                {
                    message.CommandArguments = new string[] { message.Content };
                    message.Content = string.Empty; // otherwise the nick is understood as a part reason
                }
            }

            //:<user> NICK <nick>
            // Sometimes the new nick is in the message content
            [IrcCommand( "nick", IsCompatibilityCommand = true )]
            public static void Compatibility_Nick( IrcClient client, IrcMessage message )
            {
                if ( message.CommandArguments.Length == 0 )
                {
                    message.CommandArguments = new string[] { message.Content };
                }
            }

            //:<user> MODE <user> <mode> [mode]*
            // Sometimes the message content contains the mode
            [IrcCommand( "mode", IsCompatibilityCommand = true )]
            public static void Compatibility_Mode( IrcClient client, IrcMessage message )
            {
                if ( message.CommandArguments.Length == 1 )
                {
                    message.CommandArguments = message.CommandArguments.Concat( message.Content.Split( new[] { " " }, StringSplitOptions.RemoveEmptyEntries ) ).ToArray();
                }
            }

            //:<user> INVITE <user> <channel>
            // Sometimes the message contains the channel
            [IrcCommand( "invite", IsCompatibilityCommand = true )]
            public static void Compatibility_Invite( IrcClient client, IrcMessage message )
            {
                if ( message.CommandArguments.Length == 1 )
                {
                    message.CommandArguments = message.CommandArguments.Concat( new[] { message.Content } ).ToArray();
                }
            }

            //:<user> KICK <channel> <nick> :<reason>
            // Sometimes the reason is the sender's nickname
            [IrcCommand( "kick", IsCompatibilityCommand = true )]
            public static void Compatibility_Kick( IrcClient client, IrcMessage message )
            {
                if ( message.Content == message.Sender.Nickname )
                {
                    message.Content = string.Empty;
                }
            }

            //:<server> 322 <channel> <user count> :<topic>
            // Sometimes the channel name is the '*' char. I have no idea why...
            [IrcCommand( (int) IrcReplyCode.ListChannel, IsCompatibilityCommand = true )]
            public static void Compatibility_ListChannel( IrcClient client, IrcMessage message )
            {
                message.IsValid = client.Network.Parameters.IsChannelName( message.CommandArguments[1] );
            }
            #endregion

            //PING :<data>
            [IrcCommand( "ping" )]
            public static void Ping( IrcClient client, IrcMessage message )
            {
                client.SendPong( message.Content );
            }

            //PONG :<ping data>
            [IrcCommand( "pong" )]
            public static void Pong( IrcClient client, IrcMessage message )
            {
                if ( client._pingData.Contains( message.Content ) )
                {
                    client._pingData.Remove( message.Content );
                }
            }

            //:<user> NICK <nick>
            [IrcCommand( "nick" )]
            public static void Nick( IrcClient client, IrcMessage message )
            {
                string oldNick = message.Sender.Nickname;
                string newNick = message.CommandArguments[0];

                if ( oldNick == newNick )
                {
                    // Since some networks do not send back a NICK message when sending a NICK command
                    // we have to change it in SendNick, thus here it might be wrong for the current user
                    oldNick = client.Network.PreviousNickname;
                }

                message.Sender.Nickname = newNick;
                message.Sender.OnNicknameChanged( oldNick );
            }

            //:<user> MODE <target> <mode> [mode]*
            [IrcCommand( "mode" )]
            public static void Mode( IrcClient client, IrcMessage message )
            {
                string target = message.CommandArguments[0];
                string mode = string.Join( " ", message.CommandArguments.Skip( 1 ) );

                if ( client.Network.Parameters.IsChannelName( target ) )
                {
                    var channel = client.Network.GetChannel( target );
                    channel.SetMode( mode, false );
                    channel.OnModeChanged( mode, message.Sender );
                }
                else
                {
                    var user = client.Network.GetUser( target );
                    user.SetModeInternal( mode );
                    user.OnModeChanged( mode, message.Sender );
                }
            }

            //:<user> QUIT :<reason>
            [IrcCommand( "quit" )]
            public static void Quit( IrcClient client, IrcMessage message )
            {
                message.Sender.OnQuit( message.Content );

                foreach ( var channel in message.Sender.Channels.ToArray() ) // HACK: ToArray() allows us to modify the collection
                {
                    channel.RemoveUser( message.Sender );
                }

                client.Network.KnownUsersInternal.Remove( message.Sender );
            }

            //:<user> JOIN <channel>
            [IrcCommand( "join" )]
            public static void Join( IrcClient client, IrcMessage message )
            {
                var channel = client.Network.GetChannel( message.CommandArguments[0] );
                channel.AddUser( message.Sender );

                if ( message.Sender == client.Network.CurrentUser )
                {
                    // We have joined a channel; if the user list isn't received shortly afterwards, send a request for it
                    client.DelaySendNames( channel );
                    // Also, we need the modes (e.g. to know whether the topic can be set by non-ops)
                    channel.UpdateInformation();
                }

                channel.OnUserJoined( message.Sender );
            }

            //:<user> PART <channel> :<reason>
            [IrcCommand( "part" )]
            public static void Part( IrcClient client, IrcMessage message )
            {
                var channel = client.Network.GetChannel( message.CommandArguments[0] );
                channel.RemoveUser( message.Sender );
                channel.OnUserLeft( message.Sender, message.Content );
            }

            //:<user> TOPIC <channel> :<topic>
            [IrcCommand( "topic" )]
            public static void Topic( IrcClient client, IrcMessage message )
            {
                var channel = client.Network.GetChannel( message.CommandArguments[0] );
                channel.Topic.Text = message.Content;
                channel.Topic.Setter = message.Sender;
                channel.Topic.SetDate = DateTime.Now;
            }

            //:<user> INVITE <nick> <channel>
            [IrcCommand( "invite" )]
            public static void Invite( IrcClient client, IrcMessage message )
            {
                client.Network.GetChannel( message.CommandArguments[1] ).OnInviteReceived( message.Sender );
            }

            //:<user> KICK <channel> <nick> :<reason>
            [IrcCommand( "kick" )]
            public static void Kick( IrcClient client, IrcMessage message )
            {
                var channel = client.Network.GetChannel( message.CommandArguments[0] );
                var kickedUser = client.Network.GetUser( message.CommandArguments[1] );
                channel.RemoveUser( kickedUser );
                channel.OnUserKicked( kickedUser, message.Sender, message.Content );

                if ( IrcClient.RejoinOnKick && kickedUser == client.Network.CurrentUser )
                {
                    channel.Join();
                }
            }

            //:<user> PRIVMSG <target> :<message>
            [IrcCommand( "privmsg" )]
            public static void PrivMsg( IrcClient client, IrcMessage message )
            {
                string target = message.CommandArguments[0];

                if ( client.Network.Parameters.IsChannelName( target ) )
                {
                    client.Network.GetChannel( target ).OnMessageReceived( message.Sender, message.Content );
                }
                else
                {
                    message.Sender.OnMessageReceived( message.Content );
                }
            }

            //:<user> NOTICE <nick> :<message>
            [IrcCommand( "notice" )]
            public static void Notice( IrcClient client, IrcMessage message )
            {
                message.Sender.OnNoticeReceived( message.Content );
            }

            //:<user> KILL <nick> :<server list> <message>
            [IrcCommand( "kill" )]
            public static void Kill( IrcClient client, IrcMessage message )
            {
                var user = client.Network.GetUser( message.CommandArguments[0] );
                if ( user == client.Network.CurrentUser )
                {
                    client.Stop( true );
                }
            }

            // :<server> 002 <nick> :Your host is <servername>, running version <version>
            [IrcCommand( (int) IrcReplyCode.YourHost )]
            public static void YourHost( IrcClient client, IrcMessage message )
            {
                // This message only gets sent once during any session
                client.HandleAuthentication();
            }

            // :<server> 005 <nick> <tokens> :are supported by this server
            [IrcCommand( (int) IrcReplyCode.ISupport )]
            public static void ISupport( IrcClient client, IrcMessage message )
            {
                CapabilityParser.Parse( client.Network.Parameters, message.CommandArguments.Skip( 1 ) );
            }

            #region Names
            // :<server> 353 <you> ( "=" / "@" / "*" ) <channel>: *<user>
            [IrcCommand( (int) IrcReplyCode.NamesReply )]
            public static void NamesReply( IrcClient client, IrcMessage message )
            {
                var channel = client.Network.GetChannel( message.CommandArguments[2] );
                channel.SetVisibility( message.CommandArguments[1][0] );

                if ( message.Content.IsEmpty() )
                {
                    // This happens when asking NAMES for a channel you aren't a member of
                    return;
                }

                if ( client._delayedNamesChannels.Contains( channel ) )
                {
                    // we received a NAMES reply, it's ok, don't send one
                    client._delayedNamesChannels.Remove( channel );
                }

                if ( !client._tempInfo.ChannelUsers.ContainsKey( channel ) )
                {
                    client._tempInfo.ChannelUsers.Add( channel, new List<string>() );
                }

                var userNicknames = message.Content.Split( IrcUtils.MessagePartsSeparatorArray, StringSplitOptions.RemoveEmptyEntries );
                client._tempInfo.ChannelUsers[channel].AddRange( userNicknames );
            }

            //:<server> 366 <you> <channel> :End of NAMES list
            [IrcCommand( (int) IrcReplyCode.NamesEnd )]
            public static void EndOfNames( IrcClient client, IrcMessage message )
            {
                var channel = client.Network.GetChannel( message.CommandArguments[1] );

                if ( client._tempInfo.ChannelUsers.ContainsKey( channel ) )
                {
                    channel.SetUsers( client._tempInfo.ChannelUsers[channel] );
                    client._tempInfo.ChannelUsers.Remove( channel );
                }
                else
                {
                    channel.SetUsers( Enumerable.Empty<string>() );
                }
            }
            #endregion

            #region Topic
            //:<server> 331 <you> <channel> :No topic is set
            [IrcCommand( (int) IrcReplyCode.TopicNotSet )]
            public static void TopicNotSet( IrcClient client, IrcMessage message )
            {
                var channel = client.Network.GetChannel( message.CommandArguments[1] );
                channel.Topic.Text = string.Empty;
                channel.Topic.Setter = null;
                channel.Topic.SetDate = null;
            }

            //:<server> 332 <you> <channel> :<topic>
            [IrcCommand( (int) IrcReplyCode.TopicContent )]
            public static void TopicContent( IrcClient client, IrcMessage message )
            {
                var channel = client.Network.GetChannel( message.CommandArguments[1] );
                channel.Topic.Text = message.Content;
            }

            //:<server> 333 <you> <channel> <topicSetter> <time>
            [IrcCommand( (int) IrcReplyCode.TopicInfo )]
            public static void TopicInfo( IrcClient client, IrcMessage message )
            {
                var channel = client.Network.GetChannel( message.CommandArguments[1] );
                channel.Topic.Setter = client.Network.GetUserFromFullName( message.CommandArguments[2] );
                channel.Topic.SetDate = TimeHelper.DateTimeFromUnixSeconds( int.Parse( message.CommandArguments[3] ) );
            }
            #endregion

            //:<server> 221 <you> <mode>
            [IrcCommand( (int) IrcReplyCode.YourMode )]
            public static void YourMode( IrcClient client, IrcMessage message )
            {
                client.Network.CurrentUser.SetMode( message.CommandArguments[1] );
            }

            //:<server> 324 <you> <channel> <mode>
            [IrcCommand( (int) IrcReplyCode.ChannelMode )]
            public static void ChannelMode( IrcClient client, IrcMessage message )
            {
                var channel = client.Network.GetChannel( message.CommandArguments[1] );

                if ( message.CommandArguments[2] == "+" )
                {
                    channel.SetMode( string.Empty, true );
                }
                else
                {
                    channel.SetMode( string.Join( " ", message.CommandArguments.Skip( 2 ) ), true );
                }
            }

            //:<server> 329 <you> <channel> <date in unix seconds>
            [IrcCommand( (int) IrcReplyCode.ChannelCreationDate )]
            public static void ChannelCreationDate( IrcClient client, IrcMessage message )
            {
                var channel = client.Network.GetChannel( message.CommandArguments[1] );
                channel.CreationDate = TimeHelper.DateTimeFromUnixSeconds( int.Parse( message.CommandArguments[2] ) );
            }

            #region Nickname errors
            //:<server> 433 <you> <nick> :Nickname is already in use
            [IrcCommand( (int) IrcErrorCode.NicknameInUse )]
            public static void NicknameInUse( IrcClient client, IrcMessage message )
            {
                client.Network.OnNicknameCollision( message.CommandArguments[1] );

                if ( client.Network.IsAuthenticated )
                {
                    client.Network.CurrentUser.Nickname = message.CommandArguments[0];
                }
                else if ( IrcClient.RetryOnNicknameCollision )
                {
                    client.Network.CurrentUser.Nickname = IrcClient.NicknameCollisionTransform( client.Network.CurrentUser.Nickname );
                    client.Network.ChangeNickname( client.Network.CurrentUser.Nickname );
                }
            }

            //:<server> 432 <you> <nick> :Erroneous nickname.
            [IrcCommand( (int) IrcErrorCode.InvalidNickname )]
            //:<server> 437 <you> <nick> :Cannot change nickname while banned on channel.
            [IrcCommand( (int) IrcErrorCode.NicknameChangeWhileBanned )]
            //:<server> 438 <you> <nick> :Nick change too fast. Please wait X seconds.
            [IrcCommand( (int) IrcErrorCode.NicknameChangeTooFast )]
            public static void BadNickname( IrcClient client, IrcMessage message )
            {
                client.Network.CurrentUser.Nickname = message.CommandArguments[0];
                client.Network.OnErrorReceived( message.Content, message.Command );
            }
            #endregion

            #region WHOIS replies
            //:<server> 311 <you> <target> <username> <host> * :<realname>
            [IrcCommand( (int) IrcReplyCode.WhoIsUser )]
            public static void WhoIsUser( IrcClient client, IrcMessage message )
            {
                var user = client.Network.GetUser( message.CommandArguments[1] );
                user.UserName = message.CommandArguments[2];
                user.Host = message.CommandArguments[3];
                user.RealName = message.Content;
                client._tempInfo.WhoIsReplies[user] = new UserInformationReceivedEventArgs();
            }

            //:<server> 312 <you> <target> <server> :<server info>
            [IrcCommand( (int) IrcReplyCode.WhoIsServer )]
            public static void WhoIsServer( IrcClient client, IrcMessage message )
            {
                var user = client.Network.GetUser( message.CommandArguments[1] );

                if ( client._tempInfo.WhoIsReplies.ContainsKey( user ) )
                {
                    client._tempInfo.WhoIsReplies[user].ConnectionServer = message.CommandArguments[2];
                    client._tempInfo.WhoIsReplies[user].AdditionalMessagesInternal.Add( message.Content );
                }
                // TODO handle WHOWAS requests, which also use the 312 numeric
            }

            //:<server> 313 <you> <target> :<useless message>
            [IrcCommand( (int) IrcReplyCode.WhoIsOperator )]
            public static void WhoIsOperator( IrcClient client, IrcMessage message )
            {
                var user = client.Network.GetUser( message.CommandArguments[1] );
                user.AddModeInternal( IrcUserModes.GlobalOperator );
            }

            //:<server> 317 <you> <target> <idletime> <signontime> :<useless description>
            [IrcCommand( (int) IrcReplyCode.WhoIsIdle )]
            public static void WhoIsIdle( IrcClient client, IrcMessage message )
            {
                var user = client.Network.GetUser( message.CommandArguments[1] );
                client._tempInfo.WhoIsReplies[user].IdleTime = int.Parse( message.CommandArguments[2] );
                client._tempInfo.WhoIsReplies[user].LoginDate = TimeHelper.DateTimeFromUnixSeconds( int.Parse( message.CommandArguments[3] ) );
            }

            //:<server> 319 <you> <target> :[channel]*
            // May not be sent if the target isn't on any channel.
            [IrcCommand( (int) IrcReplyCode.WhoIsChannels )]
            public static void WhoIsChannel( IrcClient client, IrcMessage message )
            {
                var user = client.Network.GetUser( message.CommandArguments[1] );
                foreach ( string chanName in message.Content.Split( IrcUtils.MessagePartsSeparatorArray, StringSplitOptions.RemoveEmptyEntries ) )
                {
                    var pair = IrcChannel.GuessChannelName( chanName, client.Network.Parameters );

                    var channel = client.Network.GetChannel( pair.Name );
                    if ( !channel.Users.Contains( user ) )
                    {
                        channel.AddUser( user );
                    }
                    channel.UserModes[user] = IrcChannel.GetUserMode( pair.Modifier, client.Network.Parameters );

                    client._tempInfo.WhoIsReplies[user].ChannelsInternal.Add( channel );
                }
            }

            //:<server> 318 <you> <target> :<useless message>
            [IrcCommand( (int) IrcReplyCode.WhoIsEnd )]
            public static void WhoIsEnd( IrcClient client, IrcMessage message )
            {
                var user = client.Network.GetUser( message.CommandArguments[1] );
                // It seems that sometimes, an end-of-whois is sent before the rest? received bug reports about that.
                if ( client._tempInfo.WhoIsReplies.ContainsKey( user ) )
                {
                    user.OnInformationReceived( client._tempInfo.WhoIsReplies[user] );
                    client._tempInfo.WhoIsReplies.Remove( user );
                }
            }
            #endregion

            #region Message Of The Day
            [IrcCommand( (int) IrcReplyCode.MessageOfTheDayStart )]
            public static void MessageOfTheDayStart( IrcClient client, IrcMessage message )
            {
                client._tempInfo.MotdBuilder.Clear();
            }

            [IrcCommand( (int) IrcReplyCode.MessageOfTheDayContent )]
            [IrcCommand( (int) IrcReplyCode.MessageOfTheDayContent2 )]
            [IrcCommand( (int) IrcReplyCode.MessageOfTheDayContent3 )]
            public static void MotdContent( IrcClient client, IrcMessage message )
            {
                client._tempInfo.MotdBuilder.AppendLine( message.Content );
            }

            [IrcCommand( (int) IrcReplyCode.MessageOfTheDayEnd )]
            public static void MotdEnd( IrcClient client, IrcMessage message )
            {
                client.Network.MessageOfTheDay = client._tempInfo.MotdBuilder.ToString();
            }
            #endregion

            #region LIST replies
            [IrcCommand( (int) IrcReplyCode.ListStart )]
            public static void ListStart( IrcClient client, IrcMessage message )
            {
                client._tempInfo.ListedChannels = new List<IrcChannel>();
            }

            //:<server> 322 <user> <channel> <member count> : <channel description>
            [IrcCommand( (int) IrcReplyCode.ListChannel )]
            public static void ListContent( IrcClient client, IrcMessage message )
            {
                var channel = client.Network.GetChannel( message.CommandArguments[1] );
                channel.Topic.Text = message.Content;

                client._tempInfo.ListedChannels.Add( channel );
            }

            [IrcCommand( (int) IrcReplyCode.ListEnd )]
            public static void ListEnd( IrcClient client, IrcMessage message )
            {
                client.Network.OnChannelListReceived( client._tempInfo.ListedChannels );
            }
            #endregion

            //:<server> 367 <you> <channel> <ban> [banner nick] [time]
            [IrcCommand( (int) IrcReplyCode.BanList )]
            public static void BanList( IrcClient client, IrcMessage message )
            {
                var channel = client.Network.GetChannel( message.CommandArguments[1] );

                if ( !client._tempInfo.ChannelBans.ContainsKey( channel ) )
                {
                    client._tempInfo.ChannelBans.Add( channel, new List<string>() );
                }
                client._tempInfo.ChannelBans[channel].Add( message.CommandArguments[2] );
            }

            //:<server> 368 <you> <channel> :End of Channel Ban list
            [IrcCommand( (int) IrcReplyCode.EndOfBanList )]
            public static void EndOfBanList( IrcClient client, IrcMessage message )
            {
                var channel = client.Network.GetChannel( message.CommandArguments[1] );

                if ( client._tempInfo.ChannelBans.ContainsKey( channel ) )
                {
                    channel.BanMasksInternal.SetItems( client._tempInfo.ChannelBans[channel] );
                    client._tempInfo.ChannelBans.Remove( channel );
                }
            }

            //:<server> 348 <you> <channel> <exception> [setter nick] [time]
            [IrcCommand( (int) IrcReplyCode.BanExceptionList )]
            public static void BanExceptionList( IrcClient client, IrcMessage message )
            {
                var channel = client.Network.GetChannel( message.CommandArguments[1] );

                if ( !client._tempInfo.ChannelBanExceptions.ContainsKey( channel ) )
                {
                    client._tempInfo.ChannelBanExceptions.Add( channel, new List<string>() );
                }
                client._tempInfo.ChannelBanExceptions[channel].Add( message.CommandArguments[2] );
            }

            //:<server> 349 <you> <channel> :End of Channel Exception list
            [IrcCommand( (int) IrcReplyCode.EndOfBanExceptionList )]
            public static void EndOfBanExceptionList( IrcClient client, IrcMessage message )
            {
                var channel = client.Network.GetChannel( message.CommandArguments[1] );

                if ( client._tempInfo.ChannelBanExceptions.ContainsKey( channel ) )
                {
                    channel.BanExceptionsInternal.SetItems( client._tempInfo.ChannelBanExceptions[channel] );
                    client._tempInfo.ChannelBanExceptions.Remove( channel );
                }
            }

            //:<server> 346 <you> <channel> <exception> [setter nick] [time]
            [IrcCommand( (int) IrcReplyCode.InviteExceptionList )]
            public static void InviteExceptionList( IrcClient client, IrcMessage message )
            {
                var channel = client.Network.GetChannel( message.CommandArguments[1] );

                if ( !client._tempInfo.ChannelInviteExceptions.ContainsKey( channel ) )
                {
                    client._tempInfo.ChannelInviteExceptions.Add( channel, new List<string>() );
                }
                client._tempInfo.ChannelInviteExceptions[channel].Add( message.CommandArguments[2] );
            }

            //:<server> 347 <you> <channel> :End of Channel Invite list
            [IrcCommand( (int) IrcReplyCode.EndOfInviteExceptionList )]
            public static void EndOfInviteExceptionList( IrcClient client, IrcMessage message )
            {
                var channel = client.Network.GetChannel( message.CommandArguments[1] );

                if ( client._tempInfo.ChannelInviteExceptions.ContainsKey( channel ) )
                {
                    channel.InviteExceptionsInternal.SetItems( client._tempInfo.ChannelInviteExceptions[channel] );
                    client._tempInfo.ChannelInviteExceptions.Remove( channel );
                }
            }
        }
    }
}