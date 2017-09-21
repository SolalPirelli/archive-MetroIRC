// Copyright (C) 2013, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

namespace IrcSharp
{
    /// <summary>
    /// An IRC mode, for a channel or an user.
    /// </summary>
    public sealed class IrcMode
    {
        #region Constants
        /// <summary>
        /// The positive mode modifier.
        /// </summary>
        internal const char PositiveModifier = '+';
        /// <summary>
        /// The negative mode modifier.
        /// </summary>
        internal const char NegativeModifier = '-';

        /// <summary>
        /// The possible mode modifiers.
        /// </summary>
        internal static readonly char[] Modifiers = { PositiveModifier, NegativeModifier };
        #endregion

        /// <summary>
        /// Gets a value indicating whether the mode was added or removed.
        /// </summary>
        public bool IsAdded { get; private set; }

        /// <summary>
        /// Gets the mode flag.
        /// </summary>
        public char Flag { get; private set; }

        /// <summary>
        /// Gets the mode argument, if any.
        /// </summary>
        public string Argument { get; private set; }

        internal IrcMode( string modeWithModifier, string argument )
        {
            this.IsAdded = modeWithModifier[0] == PositiveModifier;
            this.Flag = modeWithModifier[1];
            this.Argument = argument;
        }
    }
}