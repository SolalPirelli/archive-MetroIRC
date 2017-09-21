// Copyright (C) 2013, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using IrcSharp.Internals;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IrcSharp.Tests.Internals
{
    [TestClass]
    public class ArrayExtensionsTests
    {
        [TestMethod]
        public void Slice_WhenEmpty()
        {
            Assert.AreEqual( 0, new int[0].Slice( 1 ).Length );
        }

        [TestMethod]
        public void Slice_EntireArray()
        {
            Assert.AreEqual( 10, new int[10].Slice( 0 ).Length );
        }

        [TestMethod]
        public void Slice_FromEnd()
        {
            Assert.AreEqual( 0, new int[10].Slice( 10 ).Length );
        }

        [TestMethod]
        public void Slice()
        {
            int from = 30;
            int[] array = new int[100];
            for ( int n = 0; n < array.Length; n++ )
            {
                array[n] = n;
            }

            array = array.Slice( from );

            for ( int n = 0; n < array.Length; n++ )
            {
                Assert.AreEqual( n + from, array[n] );
            }
        }
    }
}