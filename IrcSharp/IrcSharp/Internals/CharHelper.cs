// Copyright (C) 2012, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

namespace IrcSharp.Internals
{
    /// <summary>
    /// A helper class for char-related tasks.
    /// </summary>
    internal static class CharHelper
    {
        /// <summary>
        /// Checks whether the specified <see cref="System.Char"/> is a 'basic' letter, i.e. a latin letter without accents.
        /// </summary>
        public static bool IsBasicLetter( char c )
        {
            return ( 'a' <= c && c <= 'z' ) || ( 'A' <= c && c <= 'Z' );
        }
    }
}