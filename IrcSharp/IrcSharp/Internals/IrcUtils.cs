// Copyright (C) 2012, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Text.RegularExpressions;

namespace IrcSharp.Internals
{
    /// <summary>
    /// An internal utility class for IRC-related tasks.
    /// </summary>
    internal static class IrcUtils
    {
        #region Constants
        /// <summary>
        /// The separators between message parts.
        /// </summary>
        internal const string MessagePartsSeparator = " ";

        /// <summary>
        /// A cached array containing the separator between message parts.
        /// </summary>
        internal static readonly string[] MessagePartsSeparatorArray = { MessagePartsSeparator };

        /// <summary>
        /// The presence of this char at the beginning of a message indicates that there is a sender.
        /// </summary>
        internal const string SenderIndicator = ":";

        /// <summary>
        /// The separator between the message command/args and the actual message body.
        /// </summary>
        internal const string MessageCommandSeparator = " :";

        /// <summary>
        /// The separator between a nickname and an username.
        /// </summary>
        internal const string UserNameSeparator = "!";

        /// <summary>
        /// The separator between a use name and a host.
        /// </summary>
        internal const string UserHostSeparator = "@";

        /// <summary>
        /// The separators in an user's full name.
        /// </summary>
        internal static readonly string[] FullNameSeparators = { UserHostSeparator, UserNameSeparator };

        /// <summary>
        /// A regex that matches all of mIRC's non-standard codes for text formatting.
        /// </summary>
        private static readonly Lazy<Regex> FormattingRegex = new Lazy<Regex>( () => new Regex( "\x1f|\x0f|\x16|\x02|\x03(?:[0-9]{1,2}(?:,[0-9]{1,2})?)?" ) );
        #endregion

        /// <summary>
        /// Interprets raw text from the server as an IRC message.
        /// </summary>
        public static IrcMessage ParseMessage( IrcNetwork network, string text )
        {
            IrcUser sender = network.ServerUser;
            string content = string.Empty;

            if ( text.StartsWith( SenderIndicator ) )
            {
                int prefixEnd = text.IndexOf( MessagePartsSeparator );
                string prefix = text.Substring( SenderIndicator.Length, prefixEnd - SenderIndicator.Length );
                sender = network.GetUserFromFullName( prefix );
                text = text.Substring( prefixEnd + SenderIndicator.Length );
            }

            int suffixIndex = text.IndexOf( MessageCommandSeparator );
            if ( suffixIndex != -1 )
            {
                content = text.Substring( suffixIndex + MessageCommandSeparator.Length );
                text = text.Substring( 0, suffixIndex );
            }

            if ( IrcClient.StripFormatting )
            {
                content = FormattingRegex.Value.Replace( content, string.Empty );
            }

            string[] split = text.Split( MessagePartsSeparatorArray, StringSplitOptions.RemoveEmptyEntries );
            string command = split[0].Trim( '0' ).ToLower();
            string[] args = split.Slice( 1 );
            return new IrcMessage( sender, command, args, content );
        }
    }
}