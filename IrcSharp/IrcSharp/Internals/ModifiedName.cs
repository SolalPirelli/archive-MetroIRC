// Copyright (C) 2012-2013, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

namespace IrcSharp.Internals
{
    /// <summary>
    /// A modified name.
    /// </summary>
    internal sealed class ModifiedName
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the modifier.
        /// </summary>
        public string Modifier { get; private set; }

        public ModifiedName( string name, string modifier )
        {
            this.Name = name;
            this.Modifier = modifier;
        }
    }
}