// Copyright (C) 2011-2012 Solal Pirelli
//
//  This library is free software. It comes without any warranty, to
//  the extent permitted by applicable law. You can redistribute it
//  and/or modify it under the terms of the Do What The Fuck You Want
//  To Public License, Version 2, as published by Sam Hocevar. See
//  http://sam.zoy.org/wtfpl/COPYING for more details.

using System;

namespace CommonStuff
{
    /// <summary>
    /// A static class providing extensions method for the <see cref="System.String"/> class.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Checks whether the beginning of the <see cref="System.String"/> matches the specified string.
        /// </summary>
        /// <param name="s">The <see cref="System.String"/>.</param>
        /// <param name="value">The value that must be matched.</param>
        /// <param name="comparisonType">Optional. The comparison type to use. By default, <see cref="System.StringComparison.InvariantCultureIgnoreCase"/>.</param>
        /// <returns>A value indicating whether the beginning of the <see cref="System.String"/> matches the specified string.</returns>
        public static bool BeginsWith( this string s, string value, StringComparison comparisonType = StringComparison.InvariantCultureIgnoreCase )
        {
            if ( s.Length < value.Length )
            {
                return false;
            }
            return String.Equals( s.Substring( 0, value.Length ), value, comparisonType );
        }

        /// <summary>
        /// Checks whether the <see cref="System.String"/> is null, empty or consists only of white-space characters.
        /// </summary>
        /// <param name="s">The <see cref="System.String"/>.</param>
        /// <returns>A value indicating whether the <see cref="System.String"/> is null, empty or consists only of white-space characters.</returns>
        public static bool IsEmpty( this string s )
        {
            return string.IsNullOrWhiteSpace( s );
        }

        /// <summary>
        /// Checks whether the <see cref="System.String"/> contains any text.
        /// </summary>
        /// <param name="s">The <see cref="System.String"/>.</param>
        /// <returns>A value indicating whether the <see cref="System.String"/> contains any text.</returns>
        public static bool HasText( this string s )
        {
            return !string.IsNullOrWhiteSpace( s );
        }

        /// <summary>
        /// Replaces the first occurence of the specified pattern.
        /// </summary>
        /// <param name="s">The <see cref="System.String"/>.</param>
        /// <param name="oldValue">The string to be replaced.</param>
        /// <param name="newValue">The string to replace the first occurence of <paramref name="oldValue"/>.</param>
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