// Copyright (C) 2012, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;

namespace IrcSharp.Ctcp
{
    /// <summary>
    /// An event handler for <see cref="CtcpClient"/> events.
    /// </summary>
    /// <typeparam name="T">The type of the event arguments.</typeparam>
    /// <param name="client">The <see cref="CtcpClient"/>.</param>
    /// <param name="args">The event arguments.</param>
    public delegate void CtcpClientEventHandler<T>( CtcpClient client, T args ) where T : EventArgs;
}