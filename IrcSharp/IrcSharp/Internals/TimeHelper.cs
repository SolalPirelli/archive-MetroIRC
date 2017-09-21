// Copyright (C) 2012, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;

namespace IrcSharp.Internals
{
    /// <summary>
    /// A helper class for time-related tasks.
    /// </summary>
    internal static class TimeHelper
    {
        private static readonly DateTime UnixTimeStart = new DateTime( 1970, 1, 1 );

        /// <summary>
        /// Converts an Unix timestamp to a <see cref="DateTime"/>.
        /// </summary>
        public static DateTime DateTimeFromUnixSeconds( int seconds )
        {
            return UnixTimeStart.AddSeconds( seconds );
        }

        /// <summary>
        /// Converts the specified <see cref="DateTime"/> into its Unix representation in milliseconds.
        /// </summary>
        public static long DateTimeToUnixMilliseconds( DateTime date )
        {
            return (long) Math.Round( ( date - UnixTimeStart ).TotalMilliseconds );
        }
    }
}