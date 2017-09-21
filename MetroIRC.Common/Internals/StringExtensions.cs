// Copyright (C) 2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;

namespace MetroIrc.Internals
{
    /// <summary>
    /// A static class providing extensions method for the <see cref="String"/> class.
    /// </summary>
    internal static class StringExtensions
    {
        /// <summary>
        /// A cached array containing one space.
        /// </summary>
        private static readonly string[] SpaceArray = { " " };

        /// <summary>
        /// Checks whether the beginning of the <see cref="String"/> matches the specified <see cref="String"/>.
        /// </summary>
        public static bool BeginsWith( this string s, string value, StringComparison comparisonType = StringComparison.OrdinalIgnoreCase )
        {
            if ( s.Length < value.Length )
            {
                return false;
            }
            return String.Equals( s.Substring( 0, value.Length ), value, comparisonType );
        }

        /// <summary>
        /// Checks whether the <see cref="String"/> is null, empty or consists only of white-space characters.
        /// </summary>
        public static bool IsEmpty( this string s )
        {
            return string.IsNullOrWhiteSpace( s );
        }

        /// <summary>
        /// Checks whether the <see cref="String"/> contains any text.
        /// </summary>
        public static bool HasText( this string s )
        {
            return !string.IsNullOrWhiteSpace( s );
        }

        /// <summary>
        /// Gets the <see cref="String"/>'s first word.
        /// </summary>
        public static string GetFirstWord( this string s )
        {
            if ( s == null )
            {
                return null;
            }
            else if ( s.IsEmpty() )
            {
                return string.Empty;
            }

            return s.Trim().Split( SpaceArray, StringSplitOptions.RemoveEmptyEntries )[0];
        }

        /// <summary>
        /// Returns a new <see cref="String"/> with the first occurence of the specified pattern replaced by the specified value.
        /// </summary>
        public static string ReplaceFirst( this string s, string oldValue, string newValue )
        {
            int position = s.IndexOf( oldValue );
            if ( position < 0 )
            {
                return s;
            }
            return s.Substring( 0, position ) + newValue + s.Substring( position + oldValue.Length );
        }
    }
}