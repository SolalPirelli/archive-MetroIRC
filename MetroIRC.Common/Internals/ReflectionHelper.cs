// Copyright (C) 2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MetroIrc.Internals
{
    /// <summary>
    /// A helper class for reflection-based tasks.
    /// </summary>
    internal static class ReflectionHelper
    {
        /// <summary>
        /// Gets all methods of the specified type that are marked by one or more instances of the specified attribute type.
        /// </summary>
        /// <returns>Pairs of attributes of the <typeparamref name="TAttribute"/> type and methods they mark.</returns>
        public static IEnumerable<Tuple<TAttribute, MethodInfo>> GetAttributedMethods<TAttribute>( Type type )
            where TAttribute : Attribute
        {
            return type.GetTypeInfo()
                       .DeclaredMethods
                       .SelectMany( m => m.GetCustomAttributes<TAttribute>()
                       .Cast<TAttribute>()
                       .Select( a => Tuple.Create( a, m ) ) );

        }

        /// <summary>
        /// Gets a delegate from a <see cref="System.Reflection.MethodInfo"/>, pointing to a static method.
        /// </summary>
        public static TDelegate GetStaticDelegate<TDelegate>( this MethodInfo info )
            where TDelegate : class
        {
            return info.CreateDelegate( typeof( TDelegate ) ) as TDelegate;
        }
    }
}