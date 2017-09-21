// Copyright (C) 2013, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using IrcSharp.Validation;

namespace IrcSharp
{
    /// <summary>
    /// Contains parameters related to an <see cref="IrcNetwork"/>.
    /// All are optional and may return null; some are more commonly used than others.
    /// </summary>
    public sealed class IrcNetworkParameters
    {
        #region Internal static properties
        /// <summary>
        /// The possible channel prefixes and their meanings.
        /// </summary>
        internal static readonly Dictionary<char, IrcChannelKind> DefaultChannelKinds = new Dictionary<char, IrcChannelKind>
        {
            { '#', IrcChannelKind.Standard },
            { '!', IrcChannelKind.Safe },
            { '&', IrcChannelKind.SupportsAnonymousConversations },
            { '+', IrcChannelKind.NoModes }
        };
        #endregion

        #region Public static properties
        /// <summary>
        /// Gets the default network parameters.
        /// </summary>
        public static IrcNetworkParameters Default
        {
            get { return new IrcNetworkParameters(); }
        }
        #endregion

        #region Public properties
        /// <summary>
        /// Gets the name of the <see cref="IrcNetwork"/>.
        /// </summary>
        public string NetworkName { get; internal set; }

        /// <summary>
        /// Gets the maximum length of nicknames on the <see cref="IrcNetwork"/>.
        /// </summary>
        public int MaxNicknameLength { get; internal set; }

        /// <summary>
        /// Gets the maximum length of channel names on the <see cref="IrcNetwork"/>.
        /// </summary>
        public int MaxChannelNameLength { get; internal set; }

        /// <summary>
        /// Gets the maximum length of channel topics on the <see cref="IrcNetwork"/>.
        /// </summary>
        public int MaxTopicLength { get; internal set; }

        /// <summary>
        /// Gets the maximum length of the reason message for channel expulsions on the <see cref="IrcNetwork"/>.
        /// </summary>
        public int MaxKickReasonLength { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether ban exceptions are enabled on the <see cref="IrcNetwork"/>.
        /// </summary>
        public bool AreBanExceptionsEnabled { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether invite exceptions are enabled on the <see cref="IrcNetwork"/>.
        /// </summary>
        public bool AreInviteExceptionsEnabled { get; internal set; }

        /// <summary>
        /// Gets the available channel modes on the <see cref="IrcNetwork"/>.
        /// </summary>
        public IrcChannelModeCollection ChannelModes { get; internal set; }

        /// <summary>
        /// Gets the case mapping used by the <see cref="IrcNetwork"/>.
        /// </summary>
        public IrcCaseMapping CaseMapping { get; internal set; }

        /// <summary>
        /// Gets the available channel prefixes associated with their <see cref="IrcChannelKind"/> on the <see cref="IrcNetwork"/>.
        /// </summary>
        public ReadOnlyDictionary<char, IrcChannelKind> ChannelKinds { get; internal set; }

        /// <summary>
        /// Gets the limits of <see cref="IrcChannel"/> that can be joined simultaneously, per <see cref="IrcChannelKind"/> on the <see cref="IrcNetwork"/>.
        /// </summary>
        public ReadOnlyDictionary<IrcChannelKind, int> ChannelCountLimits { get; internal set; }
        #endregion

        internal IrcNetworkParameters()
        {
            this.MaxNicknameLength = int.MaxValue;
            this.MaxChannelNameLength = int.MaxValue;
            this.MaxTopicLength = int.MaxValue;
            this.MaxKickReasonLength = int.MaxValue;
            this.CaseMapping = IrcCaseMapping.Default;
            this.ChannelModes = IrcChannelModeCollection.GetDefault();
            this.ChannelKinds = new ReadOnlyDictionary<char, IrcChannelKind>( DefaultChannelKinds );
            this.ChannelCountLimits = new ReadOnlyDictionary<IrcChannelKind, int>( this.ChannelKinds.ToDictionary( p => p.Value, p => int.MaxValue ) );
        }

        #region Public methods
        /// <summary>
        /// Checks whether the specified text is a valid channel name.
        /// </summary>
        /// <param name="name">The potential name of a channel, including the prefix.</param>
        /// <returns>A value indicating whether the specified string is a valid channel name.</returns>
        public bool IsChannelName( string name )
        {
            Validate.HasText( name, "name" );

            return name.Length >= 2
                && ChannelKinds.ContainsKey( name[0] ) && !IrcChannel.ForbiddenNameChars.Any( c => name.Contains( c.ToString() ) );
        }

        /// <summary>
        /// Checks whether the specified text is a valid channel name with a common prefix.
        /// </summary>
        /// <param name="fullName">The potential name of a channel, including the prefix.</param>
        /// <returns>A value indicating whether the specified string is a valid channel name with a common prefix.</returns>
        public bool IsCommonChannelName( string fullName )
        {
            // validation is done by IsChannelName
            return IsChannelName( fullName ) && fullName[0] == IrcChannel.StandardChannelPrefix;
        }

        /// <summary>
        /// Checks whether the specified character is a valid channel prefix.
        /// </summary>
        /// <param name="c">The <see cref="System.Char"/> to test.</param>
        /// <returns>A value indicating whether the specified character is a valid channel prefix.</returns>
        public bool IsChannelPrefix( char c )
        {
            return ChannelKinds.ContainsKey( c );
        }
        #endregion
    }
}