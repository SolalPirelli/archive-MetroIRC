// Copyright (C) 2012, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Diagnostics;

namespace IrcSharp.Validation
{
    /// <summary>
    /// An utility class to validate IRC-related parameters and states.
    /// </summary>
    internal static class IrcValidate
    {
        /// <summary>
        /// Ensures the specified status is connected.
        /// </summary>
        [DebuggerHidden]
        public static void IsConnected( bool isConnected )
        {
            if ( !isConnected )
            {
                throw new InvalidOperationException( ValidationErrors.NotConnected );
            }
        }

        /// <summary>
        /// Ensures the specified status is not connected.
        /// </summary>
        [DebuggerHidden]
        public static void IsNotConnected( bool isConnected )
        {
            if ( isConnected )
            {
                throw new InvalidOperationException( ValidationErrors.AlreadyConnected );
            }
        }

        /// <summary>
        /// Ensures the specified string is a valid channel name.
        /// </summary>
        [DebuggerHidden]
        public static void IsChannelName( string name, IrcNetworkParameters parameters, string paramName )
        {
            if ( !parameters.IsChannelName( name ) )
            {
                string text = string.Format( ValidationErrors.InvalidChannelName, name );
                throw new ArgumentException( text, paramName );
            }
        }

        private static class ValidationErrors
        {
            public const string NotConnected = "The client is not connected to the network.";
            public const string AlreadyConnected = "The client is already connected to the network.";
            public const string InvalidChannelName = "'{0}' is not a valid channel name.";
        }
    }
}