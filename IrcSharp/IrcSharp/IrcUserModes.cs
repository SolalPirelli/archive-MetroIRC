// Copyright (C) 2011-2013, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Collections.Generic;
using System.Linq;
using IrcSharp.Internals;
using IrcSharp.Validation;

namespace IrcSharp
{
    /// <summary>
    /// The possible modes for an <see cref="IrcUser"/>, and related methods.
    /// </summary>
    public static class IrcUserModes
    {
        /// <summary>
        /// The <see cref="IrcUser"/> is away.
        /// </summary>
        public static readonly char Away = 'a';

        /// <summary>
        /// The <see cref="IrcUser"/> is invisible and unknown to people outside of the <see cref="IrcChannel"/>s they are on.
        /// </summary>
        public static readonly char Invisible = 'i';

        /// <summary>
        /// The <see cref="IrcUser"/> receives wall messages.
        /// Mostly obsolete.
        /// </summary>
        public static readonly char ReceivesWallMessages = 'w';

        /// <summary>
        /// The <see cref="IrcUser"/> is restricted.
        /// </summary>
        public static readonly char Restricted = 'r';

        /// <summary>
        /// The <see cref="IrcUser"/> is a global operator.
        /// </summary>
        public static readonly char GlobalOperator = 'o';

        /// <summary>
        /// The <see cref="IrcUser"/> is a local operator (on their server).
        /// </summary>
        public static readonly char ServerOperator = 'O';

        /// <summary>
        /// The <see cref="IrcUser"/> receives server notices.
        /// </summary>
        public static readonly char ReceivesServerNotices = 's';

        /// <summary>
        /// Splits a user mode string into a sequence of <see cref="IrcMode"/> objects.
        /// </summary>
        /// <param name="modeMessage">The user mode message.</param>
        /// <returns>A sequence of <see cref="IrcMode"/> objects.</returns>
        public static IEnumerable<IrcMode> SplitMode( string modeMessage )
        {
            Validate.IsNotNull( modeMessage, "modeMessage" );

            var modes = new List<string>();
            char currentModifier = IrcMode.PositiveModifier;

            foreach ( string part in modeMessage.Split( IrcUtils.MessagePartsSeparatorArray, StringSplitOptions.RemoveEmptyEntries ) )
            {
                foreach ( char c in part )
                {
                    if ( IrcMode.Modifiers.Contains( c ) )
                    {
                        currentModifier = c;
                    }
                    else
                    {
                        modes.Add( currentModifier.ToString() + c.ToString() );
                    }
                }
            }

            return modes.Select( s => new IrcMode( s, null ) );
        }
    }
}