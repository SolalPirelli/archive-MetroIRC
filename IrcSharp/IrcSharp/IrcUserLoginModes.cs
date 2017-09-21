// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;

namespace IrcSharp
{
    /// <summary>
    /// The possible login modes for a client.
    /// </summary>
    [Flags]
    public enum IrcUserLoginModes
    {
        /// <summary>
        /// No mode is requested.
        /// Some servers still define modes depending on certain conditions (e.g. whether the connection is secured).
        /// </summary>
        None = 0,
        /// <summary>
        /// The <see cref="IrcUser"/> receives wall messages.
        /// Mostly obsolete.
        /// </summary>
        ReceivesWallMessages = 4,
        /// <summary>
        /// The <see cref="IrcUser"/> is invisible and unknown to people outside of the <see cref="IrcChannel"/>s they are on.
        /// </summary>
        Invisible = 8,
        /// <summary>
        /// Both <see cref="ReceivesWallMessages"/> and <see cref="Invisible"/>.
        /// </summary>
        Both = ReceivesWallMessages | Invisible
    }
}