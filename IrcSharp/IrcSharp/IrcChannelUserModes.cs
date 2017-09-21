// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;

namespace IrcSharp
{
    /// <summary>
    /// The possible channel modes used on users.
    /// </summary>
    [Flags]
    public enum IrcChannelUserModes
    {
        /// <summary>
        /// The normal user mode. No special privileges are given.
        /// </summary>
        Normal = 0,
        /// <summary>
        /// An unknown user mode. <see cref="IrcUser"/>s with an unknown mode may or may not have special privileges.
        /// </summary>
        Unknown = 1,
        /// <summary>
        /// The nonstandard deaf user mode prevents the <see cref="IrcUser"/> from seeing messages.
        /// </summary>
        Deaf = 2,
        /// <summary>
        /// The voiced user mode gives an <see cref="IrcUser"/> the right to talk when the <see cref="IrcChannel"/> is moderated (+m).
        /// </summary>
        Voiced = 4,
        /// <summary>
        /// The half-operator user mode gives most privileges such as ban, unban, setting channel mode...
        /// </summary>
        HalfOp = 8,
        /// <summary>
        /// The operator user mode gives all privileges on a channel, apart from performing actions on a channel admin if the server supports channel admins.
        /// </summary>
        Op = 16,
        /// <summary>
        /// The nonstandard admin user mode protects from kicks and gives all privileges, apart from performing actions on a channel owner.
        /// </summary>
        Admin = 32,
        /// <summary>
        /// The nonstandard owner user mode gives all privileges, including unbanning oneself.
        /// </summary>
        Owner = 64,
        /// <summary>
        /// The creator user mode marks an user as the one who originally created the channel and gives all privileges to them.
        /// </summary>
        Creator = 128
    }
}