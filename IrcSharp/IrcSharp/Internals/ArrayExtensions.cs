// Copyright (C) 2012, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;

namespace IrcSharp.Internals
{
    /// <summary>
    /// A static class providing extension methods for the <see cref="Array"/> class.
    /// </summary>
    internal static class ArrayExtensions
    {
        /// <summary>
        /// Returns a slice of the <see cref="Array"/>, from the specified index.
        /// </summary>
        public static T[] Slice<T>( this T[] array, int startIndex )
        {
            int length = array.Length - startIndex;
            if ( length < 0 )
            {
                return new T[0];
            }
            var newArray = new T[length];
            Array.Copy( array, startIndex, newArray, 0, length );
            return newArray;
        }
    }
}