// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

namespace IrcSharp
{
    /// <summary>
    /// The possible <see cref="IrcChannel"/> kinds.
    /// </summary>
    public enum IrcChannelKind
    {
        /// <summary>
        /// Indicates a standard channel. Almost all channels are standard.
        /// </summary>
        Standard, // #
        /// <summary>
        /// Indicates a "safe" channel, which is a channel that cannot lose all of its operators. 
        /// If such a loss happened, the server would automatically give operator privileges to one or more users.
        /// </summary>
        Safe, // !
        /// <summary>
        /// Indicates a channel that supports anonymous conversations; everyone is seen as Anonymous!Anonymous@Anonymous.
        /// This does not mean that all channels of this kind have anonymous conversations enabled; that option is still a mode flag (<see cref="IrcChannelModes.Anonymous"/>).
        /// </summary>
        SupportsAnonymousConversations, // &
        /// <summary>
        /// Indicates a channel that does not support mode flags.
        /// The only mode flag set is <see cref="IrcChannelModes.TopicSetByOps"/>.
        /// </summary>
        NoModes // +
    }
}