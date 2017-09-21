// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;

namespace IrcSharp.Internals
{
    /// <summary>
    /// Marks a method as a CTCP command interpreting method.
    /// </summary>
    [AttributeUsage( AttributeTargets.Method )]
    internal sealed class CtcpCommandAttribute : Attribute
    {
        /// <summary>
        /// Gets the command associated with the method the <see cref="CtcpCommandAttribute"/> marks.
        /// </summary>
        public string Command { get; private set; }

        public CtcpCommandAttribute( string command )
        {
            this.Command = command.ToLowerInvariant();
        }
    }
}