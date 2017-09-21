// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

namespace IrcSharp
{
    /// <summary>
    /// The IRC reply codes.
    /// </summary>
    public enum IrcReplyCode
    {
        /// <summary>
        /// Sent upon successful registration.
        /// Contains a welcome message.
        /// </summary>
        Welcome = 001,
        /// <summary>
        /// Sent upon successful registration.
        /// Contains information about the server.
        /// </summary>
        YourHost = 002,
        /// <summary>
        /// Sent upon successful registration.
        /// Contains the time at which the server was created.
        /// </summary>
        Created = 003,
        /// <summary>
        /// Sent upon successful registration.
        /// Contains information about the server.
        /// </summary>
        MyInfo = 004,
        /// <summary>
        /// This code has two separate meanings:
        /// Bounce: Sent by the server to a user to suggest an alternative server. This is often used when the connection is refused because the server is already full.
        /// ISupport: Sent by the server upon successful registration. Contains a list of the various things the server supports.
        /// </summary>
        ISupport = 005,

        /// <summary>
        /// Reply format used by USERHOST to list replies to the query list.
        /// </summary>
        /// <remarks>
        /// The reply string is composed as follows:
        /// &lt;nick&gt; [ "*" ] "=" ( "+" / "-" ) &lt;host&gt;
        /// The '*' indicates whether the client has registered as an Operator.  The '-' or '+' characters represent whether the client has set an AWAY message or not respectively.
        /// </remarks>
        UserHost = 302,
        /// <summary>
        /// Reply format used by ISON to list replies to the query list.
        /// </summary>
        /// <remarks>
        /// Reply string:
        /// :*1&lt;nick&gt; *( " " &lt;nick&gt; )
        /// </remarks>
        Ison = 303,

        /// <summary>
        /// Sent to any client sending a PRIVMSG to a client which is away.
        /// </summary>
        /// <remarks>&lt;nick&gt; :&lt;away message&gt;</remarks>
        AwayInfo = 301,
        /// <summary>
        /// Sent to a client as a confirmation that he is now no longer marked as away.
        /// </summary>
        AwayDisabled = 305,
        /// <summary>
        /// Sent to a client as a confirmation that he is now marked as away.
        /// </summary>
        AwayEnabled = 306,

        /// <summary>
        /// Reply to the WHOIS command on certain networks.
        /// Contains server-defined text about the user.
        /// </summary>
        WhoIsSpecial = 310,
        /// <summary>
        /// Reply to the WHOIS command. 
        /// Contains information about the nickname, username, host and real name of an user.
        /// </summary>
        WhoIsUser = 311,
        /// <summary>
        /// Reply to the WHOIS command.
        /// Contains information about a server.
        /// </summary>
        WhoIsServer = 312,
        /// <summary>
        /// Reply to the WHOIS command. 
        /// If present, indicates that the target user is an operator.
        /// </summary>
        WhoIsOperator = 313,
        /// <summary>
        /// Reply to the WHOIS command.
        /// Indicates the number of seconds the target user has been idle.
        /// </summary>
        WhoIsIdle = 317,
        /// <summary>
        /// Reply to the WHOIS command. 
        /// Indicates the end of the WHOIS replies.
        /// </summary>
        WhoIsEnd = 318,
        /// <summary>
        /// Reply to the WHOIS command.
        /// Indicates the channels the user is on.
        /// Multiple replies may be send if the user is on a very large number of channels.
        /// </summary>
        WhoIsChannels = 319,

        /// <summary>
        /// Reply to the WHOWAS command. 
        /// Contains information about the nickname, username, host and real name of an user
        /// </summary>
        WhoWasReply = 314,
        /// <summary>
        /// Reply to the WHOWAS command.
        /// Indicates the end of the WHOWAS replies.
        /// </summary>
        WhoWasEnd = 369,

        /// <summary>
        /// Reply to the LIST command.
        /// Contains server-defined text before the channels are listed.
        /// </summary>
        ListStart = 321,
        /// <summary>
        /// Reply to the LIST command.
        /// Contains information about a channel and its topic.
        /// </summary>
        ListChannel = 322,
        /// <summary>
        /// Reply to the LIST command.
        /// Indicates the end of the LIST replies.
        /// </summary>
        ListEnd = 323,

        /// <summary>
        /// Indicates who the creator of a channel is.
        /// </summary>
        ChannelCreatorIs = 325,

        /// <summary>
        /// Reply to a MODE command on a channel. 
        /// Contains information about the channel mode.
        /// </summary>
        /// <remarks>If the channel has no mode set, a single '+' will be returned.</remarks>
        ChannelMode = 324,
        /// <summary>
        /// Optional reply to a MODE command on a channel. Contains information about the channel creation date.
        /// </summary>
        ChannelCreationDate = 329,

        /// <summary>
        /// Reply to a TOPIC query. 
        /// Indicates that there is no topic.
        /// </summary>
        TopicNotSet = 331,
        /// <summary>
        /// Reply to a TOPIC query. 
        /// Indicates the topic.
        /// </summary>
        TopicContent = 332,
        /// <summary>
        /// Reply to a TOPIC query. 
        /// Contains information about the topic setter and the date it was set.
        /// </summary>
        TopicInfo = 333,

        /// <summary>
        /// Reply to an INVITE query. 
        /// Indicates that the invite has been successfully transmitted.
        /// </summary>
        InviteSent = 341,

        /// <summary>
        /// Reply to a SUMMON query.
        /// Indicates that the summon has been successfully transmitted.
        /// </summary>
        Summoning = 342,

        /// <summary>
        /// Reply to a MODE +I query.
        /// Contains information about one invite mask, and optionnally the user who set it as well as the time it was set.
        /// </summary>
        InviteExceptionList = 346,
        /// <summary>
        /// Reply to a MODE +I query. 
        /// Indicates the end of replies.
        /// </summary>
        EndOfInviteExceptionList = 347,

        /// <summary>
        /// Reply to a MODE +e query.
        /// Contains information about one ban mask (allowing users that match this mask to join even if they were banned), and optionnally the user who set it as well as the time it was set.
        /// </summary>
        BanExceptionList = 348,
        /// <summary>
        /// Reply to a MODE +e query.
        /// Indicates the end of replies.
        /// </summary>
        EndOfBanExceptionList = 349,

        /// <summary>
        /// Reply to a VERSION query.
        /// Contains various information about the version of the server.
        /// </summary>
        Version = 351,

        /// <summary>
        /// Reply to a WHO query. 
        /// Contains information about an user.
        /// </summary>
        WhoReply = 352,
        /// <summary>
        /// Reply to a WHO query.
        /// Indicates the end of WHO replies.
        /// </summary>
        WhoEnd = 315,

        /// <summary>
        /// Reply to a NAMES query.
        /// Contains information about the users on a channel.
        /// </summary>
        NamesReply = 353,
        /// <summary>
        /// Reply to a NAMES query.
        /// Indicates the end of NAMES replies.
        /// </summary>
        NamesEnd = 366,

        /// <summary>
        /// Reply to a LINKS query. 
        /// Contains information about a server.
        /// Depending on the servers, the information might not be available.
        /// </summary>
        LinksReply = 364,
        /// <summary>
        /// Reply to a LINKS query.
        /// Indicates the end of LINKS replies.
        /// </summary>
        LinksEnd = 365,

        /// <summary>
        /// Reply to a MODE +b query. 
        /// Contains information about one ban mask, and optionnally the user who set it as well as the time it was set.
        /// </summary>
        BanList = 367,
        /// <summary>
        /// Reply to a MODE +b query. 
        /// Indicates the end of replies.
        /// </summary>
        EndOfBanList = 368,

        /// <summary>
        /// Reply to an INFO query. 
        /// Usually contains information about the origins of IRC.
        /// </summary>
        InfoReply = 371,
        /// <summary>
        /// Reply to an INFO query. 
        /// Indicates the end of INFO replies.
        /// </summary>
        InfoEnd = 374,

        /// <summary>
        /// Optional reply to a MOTD query. 
        /// Indicates the start of the MOTD, as well as the server sending it.
        /// </summary>
        MessageOfTheDayStart = 375,
        /// <summary>
        /// Reply to a MOTD query. 
        /// The Message Of The Day.
        /// </summary>
        MessageOfTheDayContent = 372,
        /// <summary>
        /// Reply to a MOTD query. 
        /// Indicates the end of MOTD replies.
        /// </summary>
        MessageOfTheDayEnd = 376,
        /// <summary>
        /// Same as <see cref="IrcReplyCode.MessageOfTheDayContent"/>. 
        /// Sent by servers to bypass clients that block normal MOTD replies.
        /// </summary>
        MessageOfTheDayContent2 = 377,
        /// <summary>
        /// Same as <see cref="IrcReplyCode.MessageOfTheDayContent"/>. 
        /// Sent by servers to bypass clients that block normal MOTD replies.
        /// </summary>
        MessageOfTheDayContent3 = 378,

        /// <summary>
        /// Reply to an OPER command.
        /// Indicates that the command executed successfuly.
        /// </summary>
        OperatorAuthSuccessful = 381,

        /// <summary>
        /// Reply to a REHASH command. 
        /// Indicates that the command executed successfuly.
        /// </summary>
        RehashSuccessful = 382,

        /// <summary>
        /// Reply to a SERVICE command.
        /// Only sent to services.
        /// </summary>
        ServiceAuthSuccessful = 383,

        /// <summary>
        /// Reply to a TIME command. 
        /// Contains the time, in Unix format.
        /// </summary>
        Time = 391,

        /// <summary>
        /// Reply to a USERS command. 
        /// Indicates the beginning of the USERS list.
        /// </summary>
        UsersStart = 392,
        /// <summary>
        /// Reply to a USERS command.
        /// Contains the reply to the command.
        /// </summary>
        UsersContent = 393,
        /// <summary>
        /// Reply to a USERS command.
        /// Indicates the end of the USERS list.
        /// </summary>
        UsersEnd = 394,
        /// <summary>
        /// Reply to a USERS command.
        /// Indicates there are no users.
        /// </summary>
        UsersEmpty = 395,

        /// <summary>
        /// Part of a reply to a TRACE command. 
        /// The order in which it appears is not predictible.
        /// </summary>
        TraceLink = 200,
        /// <summary>
        /// Part of a reply to a TRACE command. 
        /// The order in which it appears is not predictible.
        /// </summary>
        TraceConnecting = 201,
        /// <summary>
        /// Part of a reply to a TRACE command. 
        /// The order in which it appears is not predictible.
        /// </summary>
        TraceHandshake = 202,
        /// <summary>
        /// Part of a reply to a TRACE command. 
        /// The order in which it appears is not predictible.
        /// </summary>
        TraceUnknown = 203,
        /// <summary>
        /// Part of a reply to a TRACE command. 
        /// The order in which it appears is not predictible.
        /// </summary>
        TraceOperator = 204,
        /// <summary>
        /// Part of a reply to a TRACE command. 
        /// The order in which it appears is not predictible.
        /// </summary>
        TraceUser = 205,
        /// <summary>
        /// Part of a reply to a TRACE command. 
        /// The order in which it appears is not predictible.
        /// </summary>
        TraceServer = 206,
        /// <summary>
        /// Part of a reply to a TRACE command. 
        /// The order in which it appears is not predictible.
        /// </summary>
        TraceService = 207,
        /// <summary>
        /// Part of a reply to a TRACE command. 
        /// The order in which it appears is not predictible.
        /// </summary>
        TraceNewType = 208,
        /// <summary>
        /// Part of a reply to a TRACE command. 
        /// The order in which it appears is not predictible.
        /// </summary>
        TraceClass = 209,
        /// <summary>
        /// Unused part of a TRACE command reply.
        /// </summary>
        TraceReconnect = 210,
        /// <summary>
        /// Part of a reply to a TRACE command. 
        /// The order in which it appears is not predictible.
        /// </summary>
        TraceLog = 261,
        /// <summary>
        /// Reply to a TRACE command. 
        /// Indicates the end of TRACE replies.
        /// </summary>
        TraceEnd = 262,

        /// <summary>
        /// Reply to a STATS l command. 
        /// Contains various information, including the preferred port number.
        /// </summary>
        StatsLinkInfo = 211,

        /// <summary>
        /// Reply to a STATS m command. 
        /// Contains information about a command.
        /// </summary>
        StatsCommands = 212,
        /// <summary>
        /// Reply to a STATS command. 
        /// Indicates the end of STATS replies.
        /// </summary>
        StatsEnd = 219,

        /// <summary>
        /// Reply to a STATS u command. 
        /// Indicates the server's uptime.
        /// </summary>
        StatsUptime = 242,
        /// <summary>
        /// Reply to a STATS o command. 
        /// Contains information about an "op line" ; that is, a line that describes a user which is eligible to be an IRC operator if s/he has the correct password.
        /// </summary>
        StatsOpLine = 243,
        /// <summary>
        /// Reply to a STATS u command. 
        /// Some servers send it after authentication.
        /// Indicates the current and highest Connection count.
        /// </summary>
        StatsConnectionCount = 250,

        /// <summary>
        /// Reply to a MODE command. 
        /// Contains the user's current mode.
        /// </summary>
        YourMode = 221,

        /// <summary>
        /// Reply to a SERVLIST command. 
        /// Contains information about a service.
        /// </summary>
        ServListContent = 234,
        /// <summary>
        /// Reply to a SERVLIST command.
        /// Indicates the end of SERVLIST replies.
        /// </summary>
        ServListEnd = 235,

        /// <summary>
        /// Reply to a LUSERS command.
        /// Also sent after authentication.
        /// Contains informations about the number of connected users, invisible users and servers.
        /// </summary>
        LUsersClients = 251,
        /// <summary>
        /// Reply to a LUSERS command.
        /// Also sent after authentication.
        /// Contains information about the number of connected operators.
        /// </summary>
        LUsersOperators = 252,
        /// <summary>
        /// Optional reply to a LUSERS command.
        /// Optionally sent after authentication.
        /// Contains information about the number of "unknown" connections ; that is, users who are registering.
        /// </summary>
        LUsersUnknown = 253,
        /// <summary>
        /// Reply to a LUSERS command.
        /// Also sent after authentication.
        /// Contains information about the number of channels.
        /// </summary>
        LUsersChannels = 254,
        /// <summary>
        /// Reply to a LUSERS command.
        /// Also sent after authentication.
        /// Contains information about the number of users and servers connected to the current servers.
        /// </summary>
        LUsersAboutMe = 255,

        /// <summary>
        /// Reply to an ADMIN command.
        /// </summary>
        AdminStart = 256,
        /// <summary>
        /// Optional reply to an ADMIN command.
        /// Can contain anything from e-mail addresses to postal codes.
        /// </summary>
        AdminContent1 = 257,
        /// <summary>
        /// Optional reply to an ADMIN command.
        /// Can contain anything from e-mail addresses to postal codes.
        /// </summary>
        AdminContent2 = 258,
        /// <summary>
        /// Optional reply to an ADMIN command.
        /// Can contain anything from e-mail addresses to postal codes.
        /// </summary>
        AdminContent3 = 259,

        /// <summary>
        /// Sent to a user when the server is too busy to respond.
        /// </summary>
        TryAgain = 263
    }
}