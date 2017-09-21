// Copyright (C) 2012-2013, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

namespace IrcSharp
{
    /// <summary>
    /// Holds static readonlyants representing the common channel modes.
    /// </summary>
    public static class IrcChannelModes
    {
        #region Public modes
        // These modes should be used by clients

        /// <summary>
        /// Toggles the anonymous mode. 
        /// In this mode, all message authors are seen as "Anonymous" and all network quit messages are seen as channel leave messages.
        /// </summary>
        public static readonly char Anonymous = 'a';

        /// <summary>
        /// Access to the channel is restricted to people who got invited.
        /// Also, only operators may send invitations.
        /// </summary>
        public static readonly char InviteOnly = 'i';

        /// <summary>
        /// Normal users cannot speak in the channel; only users with voice privileges or higher may speak.
        /// </summary>
        public static readonly char Moderated = 'm';

        /// <summary>
        /// The channel may not receive messages from people outside it.
        /// </summary>
        public static readonly char NoOutsideMessages = 'n';

        /// <summary>
        /// Channel join and leave messages are not sent.
        /// From an user's point of view, the channel contains only one person: them.
        /// </summary>
        public static readonly char Quiet = 'q';

        /// <summary>
        /// If this mode is set and a channel loses all of its operators, the server will automatically give operator privileges to some members.
        /// </summary>
        public static readonly char NoOperatorLoss = 'r';

        /// <summary>
        /// The topic of the channel may only be set by operators.
        /// </summary>
        public static readonly char TopicSetByOps = 't';
        #endregion

        #region Internal modes
        // These modes are exposed via other API points and must not be public

        /// <summary>
        /// Sets the channel visibility as private.
        /// A private channel is not displayed when asking for one of its members' user information.
        /// </summary>
        internal const char Private = 'p';

        /// <summary>
        /// Sets the channel visibility to secret.
        /// Only users in the channel can know it exist; the channel is hidden from other messages, including channel lists.
        /// </summary>
        internal const char Secret = 's';

        /// <summary>
        /// The channel has a key.
        /// </summary>
        internal const char Key = 'k';

        /// <summary>
        /// The channel has an user limit.
        /// </summary>
        internal const char UserLimit = 'l';

        /// <summary>
        /// An user or a mask was banned from the channel.
        /// </summary>
        internal const char Ban = 'b';

        /// <summary>
        /// There are users who may enter the channel even if they are banned.
        /// </summary>
        internal const char BanException = 'e';

        /// <summary>
        /// There are users who may enter the channel even if they were not invited.
        /// </summary>
        internal const char InviteExceptions = 'I';
        #endregion
    }
}