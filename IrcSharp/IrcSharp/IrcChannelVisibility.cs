// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

namespace IrcSharp
{
    /// <summary>
    /// The possible channel visibilities.
    /// </summary>
    public enum IrcChannelVisibility
    {
        /// <summary>
        /// The visibility is unknown.
        /// </summary>
        Unknown,
        /// <summary>
        /// The <see cref="IrcChannel"/> is visible by everyone.
        /// </summary>
        Normal, // =
        /// <summary>
        /// The <see cref="IrcChannel"/> cannot be seen by users not on it.
        /// </summary>
        Private, // *
        /// <summary>
        /// The <see cref="IrcChannel"/> is private and hidden from all commands; for example, a request for the <see cref="IrcChannel"/>'s topic will behave as if it didn't exist.
        /// </summary>
        Secret // @
    }
}