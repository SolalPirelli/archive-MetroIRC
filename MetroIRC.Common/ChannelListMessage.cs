// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using IrcSharp;

namespace MetroIrc
{
    /// <summary>
    /// A message that contains a list of available channels.
    /// The list may be incomplete if the original query had a filter.
    /// </summary>
    public sealed class ChannelListMessage : Message
    {
        /// <summary>
        /// Gets a list of channels concerned by the <see cref="ChannelListMessage"/>.
        /// </summary>
        public ReadOnlyCollection<IrcChannel> Channels { get; private set; }

        /// <summary>
        /// This property always returns an empty string.
        /// </summary>
        public override string DisplaySenderName
        {
            get { return string.Empty; }
        }

        /// <summary>
        /// This message is an information message.
        /// </summary>
        public override bool IsInformationMessage
        {
            get { return true; }
        }

        /// <summary>
        /// This message is not very important.
        /// </summary>
        public override bool IsImportantMessage
        {
            get { return false; }
        }

        public ChannelListMessage( IrcNetwork network, List<IrcChannel> channels )
            : base( network, MessageDirection.FromServer, null )
        {
            this.Channels = new ReadOnlyCollection<IrcChannel>( channels );
        }
    }
}