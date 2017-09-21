// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

namespace IrcSharp.Internals
{
    /// <summary>
    /// An IRC message received from the network.
    /// </summary>
    internal sealed class IrcMessage
    {
        /// <summary>
        /// Gets the <see cref="IrcUser"/> who sent the <see cref="IrcMessage"/>.
        /// </summary>
        public IrcUser Sender { get; private set; }

        /// <summary>
        /// Gets the command of the <see cref="IrcMessage"/>.
        /// </summary>
        public string Command { get; private set; }

        /// <summary>
        /// Gets or sets the command arguments, if any, of the <see cref="IrcMessage"/>.
        /// </summary>
        public string[] CommandArguments { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IrcMessage"/>'s content.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="IrcMessage"/> is valid.
        /// </summary>
        public bool IsValid { get; set; }


        public IrcMessage( IrcUser sender, string command, string[] commandArguments, string content )
        {
            this.Sender = sender;
            this.Command = command;
            this.CommandArguments = commandArguments;
            this.Content = content;

            this.IsValid = true;
        }
    }
}