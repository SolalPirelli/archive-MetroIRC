// Copyright (C) 2012, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

namespace IrcSharp.Internals
{
    /// <summary>
    /// A CTCP message.
    /// </summary>
    internal sealed class CtcpMessage
    {
        /// <summary>
        /// Gets the <see cref="IrcChannel"/> to which the <see cref="CtcpMessage"/> was sent, if any.
        /// </summary>
        public IrcChannel Channel { get; private set; }

        /// <summary>
        /// Gets the <see cref="IrcUser"/> who sent the <see cref="CtcpMessage"/>.
        /// </summary>
        public IrcUser Sender { get; private set; }

        /// <summary>
        /// Gets the command of the <see cref="CtcpMessage"/>.
        /// </summary>
        public string Command { get; private set; }

        /// <summary>
        /// Gets the content of the <see cref="CtcpMessage"/>.
        /// </summary>
        public string Content { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="CtcpMessage"/> is a query.
        /// Messages that are not queries must not be answered.
        /// </summary>
        public bool IsQuery { get; private set; }


        public CtcpMessage( IrcChannel channel, IrcUser sender, string command, string content, bool isQuery )
        {
            this.Channel = channel;
            this.Sender = sender;
            this.Command = command;
            this.Content = content;
            this.IsQuery = isQuery;
        }
    }
}