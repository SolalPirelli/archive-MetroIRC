// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using IrcSharp;

namespace MetroIrc
{
    /// <summary>
    /// Represents a message.
    /// </summary>
    public abstract class Message
    {
        #region Public properties
        /// <summary>
        /// Gets the network on which the <see cref="Message"/> was sent.
        /// </summary>
        public IrcNetwork Network { get; private set; }

        /// <summary>
        /// Gets the direction of the <see cref="Message"/>.
        /// </summary>
        public MessageDirection Direction { get; private set; }

        /// <summary>
        /// Gets the user who sent the <see cref="Message"/>.
        /// </summary>
        public IrcUser Sender { get; private set; }

        /// <summary>
        /// Gets the date at which the <see cref="Message"/> was sent.
        /// </summary>
        public DateTime Date { get; private set; }
        #endregion

        #region Abstract properties
        /// <summary>
        /// Gets the display name of the <see cref="Message"/>'s sender.
        /// </summary>
        public abstract string DisplaySenderName { get; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="Message"/> exists for information purposes.
        /// </summary>
        public abstract bool IsInformationMessage { get; }

        /// <summary>
        /// Gets a value indicating whether the user should be notified about the <see cref="Message"/>.
        /// </summary>
        public abstract bool IsImportantMessage { get; }
        #endregion

        public Message( IrcNetwork network, MessageDirection direction, IrcUser sender )
        {
            this.Network = network;
            this.Direction = direction;
            this.Sender = sender;
            this.Date = DateTime.Now;
        }

        #region Public methods
        /// <summary>
        /// Clones the <see cref="Message"/>.
        /// This is used when a message must be displayed in more than one conversation.
        /// </summary>
        public virtual Message Clone()
        {
            return (Message) this.MemberwiseClone();
        }
        #endregion
    }
}