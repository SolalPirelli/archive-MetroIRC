// Copyright (C) 2013, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;

namespace IrcSharp.Internals
{
    /// <summary>
    /// Marks a method as a capability handler.
    /// </summary>
    [AttributeUsage( AttributeTargets.Method, AllowMultiple = true )]
    internal sealed class CapabilityHandlerAttribute : Attribute
    {
        /// <summary>
        /// Gets the capability handled by the method the <see cref="CapabilityHandlerAttribute"/> marks.
        /// </summary>
        public string Capability { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the method the <see cref="CapabilityHandlerAttribute"/> marks needs a parameter.
        /// </summary>
        public bool NeedsParameter { get; set; }

        /// <summary>
        /// Gets or sets the default value of the parameter given to the method the <see cref="CapabilityHandlerAttribute"/> marks.
        /// </summary>
        public string DefaultValue { get; set; }

        public CapabilityHandlerAttribute( string capability )
        {
            this.Capability = capability;
            this.NeedsParameter = true;
        }
    }
}