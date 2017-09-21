// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CommonStuff
{
    /// <summary>
    /// A helper class for reflection-based tasks.
    /// </summary>
    public static class ReflectionHelper
    {
        /// <summary>
        /// Gets all methods of the specified class that are marked by one or more instances of the specified attribute type.
        /// </summary>
        /// <returns>Pairs of attributes of the <typeparamref name="TAttribute"/> type and methods marked by them.</returns>
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
        /// Gets a delegate from <see cref="System.Reflection.MethodInfo"/>, pointing to a static method.
        /// </summary>
        public static TDelegate GetStaticDelegate<TDelegate>( this MethodInfo info )
            where TDelegate : class
        {
            return (TDelegate) (object) info.CreateDelegate( typeof( TDelegate ) );
        }
    }
}