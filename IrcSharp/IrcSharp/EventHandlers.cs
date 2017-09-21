// Copyright (C) 2012, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;

namespace IrcSharp
{
    /// <summary>
    /// An event handler for <see cref="IrcUser"/> events.
    /// </summary>
    /// <typeparam name="T">The type of the event arguments.</typeparam>
    /// <param name="user">The user.</param>
    /// <param name="args">The event arguments.</param>
    public delegate void UserEventHandler<T>( IrcUser user, T args ) where T : EventArgs;

    /// <summary>
    /// An event handler for <see cref="IrcChannel"/> events.
    /// </summary>
    /// <typeparam name="T">The type of the event arguments.</typeparam>
    /// <param name="channel">The channel.</param>
    /// <param name="args">The event arguments.</param>
    public delegate void ChannelEventHandler<T>( IrcChannel channel, T args ) where T : EventArgs;

    /// <summary>
    /// An event handler for <see cref="IrcNetwork"/> events.
    /// </summary>
    /// <typeparam name="T">The type of the event arguments.</typeparam>
    /// <param name="network">The network.</param>
    /// <param name="args">The event arguments.</param>
    public delegate void NetworkEventHandler<T>( IrcNetwork network, T args ) where T : EventArgs;

    /// <summary>
    /// An event handler for <see cref="IrcClient"/> events.
    /// </summary>
    /// <typeparam name="T">The type of the event arguments.</typeparam>
    /// <param name="client">The client.</param>
    /// <param name="args">The event arguments.</param>
    public delegate void ClientEventHandler<T>( IrcClient client, T args ) where T : EventArgs;
}