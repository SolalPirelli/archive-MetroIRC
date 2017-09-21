// Copyright (C) 2012, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Diagnostics;
using System.Linq;

namespace IrcSharp.Validation
{
    /// <summary>
    /// An utility class to validate parameters.
    /// </summary>
    internal static class Validate
    {
        /// <summary>
        /// Ensures the specified object is not null.
        /// </summary>
        [DebuggerHidden]
        public static void IsNotNull<T>( T value, string paramName )
            where T : class
        {
            if ( value == null )
            {
                throw new ArgumentNullException( paramName );
            }
        }

        /// <summary>
        /// Ensures the specified string is not null nor empty nor whitespace.
        /// </summary>
        [DebuggerHidden]
        public static void HasText( string value, string paramName )
        {
            if ( string.IsNullOrWhiteSpace( value ) )
            {
                string text = string.Format( ValidationErrors.NullOrBlankString, paramName, value ?? ValidationErrors.Null );
                throw new ArgumentException( text, paramName );
            }
        }

        /// <summary>
        /// Ensures the specified number is positive.
        /// </summary>
        [DebuggerHidden]
        public static void IsPositive( long value, string paramName )
        {
            if ( value < 0 )
            {
                string text = string.Format( ValidationErrors.NegativeNumber, paramName, value );
                throw new ArgumentOutOfRangeException( paramName, text );
            }
        }

        /// <summary>
        /// Ensures the specified integer is a declared value of the given enum type.
        /// </summary>
        [DebuggerHidden]
        public static void IsEnumValue<T>( T value, string paramName )
        {
            if ( !Enum.GetValues( typeof( T ) ).Cast<T>().Contains( value ) )
            {
                string text = string.Format( ValidationErrors.InvalidEnumValue, paramName, typeof( T ).FullName, value );
                throw new ArgumentException( text, paramName );
            }
        }

        /// <summary>
        /// Ensures the specified bool is true; if it's true, assume the caller has been disposed.
        /// </summary>
        [DebuggerHidden]
        public static void IsNotDisposed( bool disposed )
        {
            if ( disposed )
            {
                throw new ObjectDisposedException( null, ValidationErrors.DisposedObject );
            }
        }

        private static class ValidationErrors
        {
            public const string NullOrBlankString = "{0} must not be null, empty or consist fully of white-space characters. (value: {1}).";
            public const string NegativeNumber = "{0} must be positive. (value: {1}).";
            public const string InvalidEnumValue = "{0} must be a valid {1} value. (value: {2}).";
            public const string DisposedObject = "The object has been disposed and cannot be used any more.";

            public const string Null = "null";
        }
    }
}