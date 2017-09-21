// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Globalization;

namespace IrcSharp.Internals
{
    /// <summary>
    /// Marks a method as an IRC command interpreting method.
    /// </summary>
    [AttributeUsage( AttributeTargets.Method, AllowMultiple = true )]
    internal sealed class IrcCommandAttribute : Attribute
    {
        /// <summary>
        /// Gets the command associated with the method the <see cref="IrcCommandAttribute"/> marks.
        /// </summary>
        public string Command { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the command interpreter should be run before the normal command interpreter as a compatibility fixer.
        /// </summary>
        public bool IsCompatibilityCommand { get; set; }


        public IrcCommandAttribute( string command )
        {
            this.Command = command.ToLowerInvariant();
        }

        public IrcCommandAttribute( int code )
        {
            this.Command = code.ToString( CultureInfo.InvariantCulture );
        }
    }
}