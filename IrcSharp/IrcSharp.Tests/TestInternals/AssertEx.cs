// Copyright (C) 2013, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IrcSharp.Tests.TestInternals
{
    public static class AssertEx
    {
        public static void Throws<T>( Action a )
            where T : Exception
        {
            try
            {
                a();
                Assert.Fail( "An exception was expected." );
            }
            catch ( Exception e )
            {
                if ( !( e is AssertFailedException ) && !( e is T ) )
                {
                    Assert.Fail( "An exception of type {0} occured, but the expected type was {1}", e.GetType().Name, typeof( T ).Name );
                }
            }
        }
    }
}