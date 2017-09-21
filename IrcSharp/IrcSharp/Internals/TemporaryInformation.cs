// Copyright (C) 2012, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System.Collections.Generic;
using System.Text;

namespace IrcSharp.Internals
{
    /// <summary>
    /// Holds temporary information used to handle IRC messages.
    /// </summary>
    internal sealed class TemporaryInformation
    {
        /// <summary>
        /// Gets the "Message Of The Day" (MOTD) being built.
        /// </summary>
        public StringBuilder MotdBuilder { get; private set; }

        /// <summary>
        /// Gets or sets the channels returned by a LIST command.
        /// </summary>
        public List<IrcChannel> ListedChannels { get; set; }
        
        /// <summary>
        /// Gets WHOIS replies per user.
        /// </summary>
        public Dictionary<IrcUser, UserInformationReceivedEventArgs> WhoIsReplies { get; private set; }

        /// <summary>
        /// Gets the aggregated NAMES replies per channel.
        /// </summary>
        public Dictionary<IrcChannel, List<string>> ChannelUsers { get; private set; }
        /// <summary>
        /// Gets the aggregated MODE +b replies per channel.
        /// </summary>
        public Dictionary<IrcChannel, List<string>> ChannelBans { get; private set; }
        /// <summary>
        /// Gets the aggregated MODE e replies per channel.
        /// </summary>
        public Dictionary<IrcChannel, List<string>> ChannelBanExceptions { get; private set; }
        /// <summary>
        /// Gets the aggregated MODE I replies per channel.
        /// </summary>
        public Dictionary<IrcChannel, List<string>> ChannelInviteExceptions { get; private set; }

        public TemporaryInformation()
        {
            this.MotdBuilder = new StringBuilder();
            this.WhoIsReplies = new Dictionary<IrcUser, UserInformationReceivedEventArgs>();
            this.ChannelUsers = new Dictionary<IrcChannel, List<string>>();
            this.ChannelBans = new Dictionary<IrcChannel, List<string>>();
            this.ChannelBanExceptions = new Dictionary<IrcChannel, List<string>>();
            this.ChannelInviteExceptions = new Dictionary<IrcChannel, List<string>>();
        }
    }
}