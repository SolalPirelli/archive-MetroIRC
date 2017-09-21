// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

namespace IrcSharp
{
    /// <summary>
    /// The IRC error codes.
    /// </summary>
    public enum IrcErrorCode
    {
        /// <summary>
        /// The given nickname does not exist.
        /// </summary>
        NoSuchNickname = 401,
        /// <summary>
        /// The given server does not exist.
        /// </summary>
        NoSuchServer = 402,
        /// <summary>
        /// The given channel does not exist.
        /// </summary>
        NoSuchChannel = 403,
        /// <summary>
        /// The message failed to reach the channel. 
        /// This can be caused by insufficent rights.
        /// </summary>
        CannotSendToChannel = 404,

        /// <summary>
        /// The user joined too many channels.
        /// </summary>
        TooManyChannels = 405,

        /// <summary>
        /// Error reply to a WHOWAS command.
        /// </summary>
        WasNoSuchNick = 406,

        /// <summary>
        /// Error reply to PRIVMSG and JOIN commands.
        /// Three possible causes: Two occurences of an user@host, too many people as target of a PRIVMSG, or JOINing a channel whose shortname is used more than once.
        /// </summary>
        TooManyTargets = 407,

        /// <summary>
        /// The given service does not exist.
        /// </summary>
        NoSuchService = 408,

        /// <summary>
        /// Error reply to a PING or PONG command.
        /// Indicates that the parameter was missing (e.g. "PING" instead of "PING :42").
        /// </summary>
        NoPingOrigin = 409,

        /// <summary>
        /// A recipient was expected but not present.
        /// </summary>
        NoRecipient = 411,
        /// <summary>
        /// Text was expected but not present.
        /// </summary>
        NoTextToSend = 412,
        /// <summary>
        /// A mask without top-level domain (TLD) was used.
        /// </summary>
        NoTopLevel = 413,
        /// <summary>
        /// A mask whose top-level domain (TLD) contains wildcards was used.
        /// </summary>
        WildTopLevel = 414,
        /// <summary>
        /// An invalid mask was used.
        /// </summary>
        BadMask = 415,

        /// <summary>
        /// An unknown or unsupported command was received.
        /// </summary>
        UnknownCommand = 421,

        /// <summary>
        /// There is no message of the day (MOTD).
        /// </summary>
        NoMessageOfTheDay = 422,

        /// <summary>
        /// That there is no information available about the server administrator.
        /// </summary>
        NoAdminInfo = 423,

        /// <summary>
        /// Something went wrong while manipulating a file.
        /// </summary>
        FileError = 424,

        /// <summary>
        /// A nickname was expected but not present.
        /// </summary>
        NoNicknameGiven = 431,

        /// <summary>
        /// An invalid nickname was used.
        /// </summary>
        InvalidNickname = 432,

        /// <summary>
        /// The nickname you wanted to use is already in use by someone else.
        /// </summary>
        NicknameInUse = 433,

        /// <summary>
        /// Somehow, two people have the same nickname at the same time.
        /// Usually followed by a disconnection.
        /// Not always sent.
        /// </summary>
        NicknameCollision = 436,

        /// <summary>
        /// You are trying to change your nickname while being on a channel you are banned from.
        /// You must leave the channel before changing your nickname.
        /// </summary>
        NicknameChangeWhileBanned = 437,

        /// <summary>
        /// You changed your nickname too quickly. Wait for a moment before changing your nickname again.
        /// </summary>
        NicknameChangeTooFast = 438,

        /// <summary>
        /// You are trying to perform a channel action on a user that is not in the channel.
        /// </summary>
        UserNotInChannel = 441,

        /// <summary>
        /// You are trying to perform a channel action but you aren't in the channel.
        /// </summary>
        ClientNotInChannel = 442,

        /// <summary>
        /// The user you invited to a channel is already in it.
        /// </summary>
        UserAlreadyOnChannel = 443,

        /// <summary>
        /// The user you wanted to summon (using the rarely-used SUMMON command) is not logged in.
        /// </summary>
        NotLoggedIn = 444,

        /// <summary>
        /// The SUMMON command is either disabled or not supported.
        /// </summary>
        NoSummonCommand = 445,

        /// <summary>
        /// The USERS command is either disabled or not supported.
        /// </summary>
        NoUsersCommand = 446,

        /// <summary>
        /// You are trying to perform an action without being authenticated.
        /// </summary>
        ClientNotAuthenticated = 451,

        /// <summary>
        /// The command you used needs more parameters.
        /// </summary>
        NeedMoreParameters = 461,

        /// <summary>
        /// It is impossible to change logon details using a PASS or USER command.
        /// </summary>
        AlreadyRegistered = 462,

        /// <summary>
        /// You attempted to authenticate using a server that has not been setup to allow connections from your host.
        /// </summary>
        CannotConnect = 463,

        /// <summary>
        /// Either you supplied the wrong password, or a password was expected but not given.
        /// </summary>
        IncorrectOrAbsentPassword = 464,

        /// <summary>
        /// You attempted to authenticate using a server that has been explicitly configured to deny connections from you.
        /// </summary>
        ClientBannedFromServer = 465,

        /// <summary>
        /// You will be banned soon.
        /// </summary>
        YouWillBeBanned = 466,

        /// <summary>
        /// You tried to change the key of a channel but there is already one.
        /// Remove the key then add the new one, in two separate messages.
        /// </summary>
        ChannelKeyAlreadySet = 467,

        /// <summary>
        /// You tried to join a channel whose user limit has been met.
        /// </summary>
        ChannelIsFull = 471,

        /// <summary>
        /// You tried to set a channel mode unknown to the server.
        /// </summary>
        UnknownChannelMode = 472,

        /// <summary>
        /// You tried to join an invite-only channel.
        /// </summary>
        ChannelIsInviteOnly = 473,

        /// <summary>
        /// You tried to join a channel you are banned from.
        /// </summary>
        ClientBannedFromChannel = 474,

        /// <summary>
        /// You tried to join a key-protected channel but the key you provided is incorrect.
        /// </summary>
        IncorrectChannelKey = 475,

        /// <summary>
        /// An invalid channel mask was sent.
        /// </summary>
        InvalidChannelMask = 476,

        /// <summary>
        /// You tried to set the mode of a channel that does not support modes.
        /// </summary>
        NoChannelMode = 477,

        /// <summary>
        /// You tried to ban an user from a channel but the ban list is already full.
        /// </summary>
        ChannelBanListIsFull = 478,

        /// <summary>
        /// You do not have the serverprivileges required.
        /// </summary>
        NotEnoughPrivileges = 481,

        /// <summary>
        /// You do not have the channel privileges required.
        /// </summary>
        NotEnoughChannelPrivileges = 482,

        /// <summary>
        /// You tried to disconnect a server as if it were an user.
        /// </summary>
        CannotKillServer = 483,

        /// <summary>
        /// You are restricted from performing certain actions, or you tried to perform an action on a service.
        /// </summary>
        ClientIsRestricted = 484,

        /// <summary>
        /// You tried to join a channel that has been blocked.
        /// </summary>
        CannotJoinBlockedChannel = 485,

        /// <summary>
        /// You tried to authenticate as an operator but the server does not list you as one.
        /// </summary>
        HostCannotBeOperator = 491,

        /// <summary>
        /// You tried to set an user mode unknown to the server.
        /// </summary>
        UnknownUserMode = 501,

        /// <summary>
        /// You tried to set another user's mode.
        /// </summary>
        UsersDoNotMatch = 502
    }
}