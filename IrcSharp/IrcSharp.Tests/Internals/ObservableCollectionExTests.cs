// Copyright (C) 2013, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using IrcSharp.Internals;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IrcSharp.Tests.Internals
{
    [TestClass]
    public sealed class ObservableCollectionExTests
    {
        [TestMethod]
        public void SetItems_FromEmpty()
        {
            var array = new[] { 1, 2, 3, 4, 5 };
            var coll = new ObservableCollectionEx<int>();

            coll.SetItems( array );
            Assert.AreEqual( array.Length, coll.Count );
            for ( int n = 0; n < array.Length; n++ )
            {
                Assert.AreEqual( array[n], coll[n] );
            }
        }

        [TestMethod]
        public void SetItems_SameItems()
        {
            var array = new[] { 1, 2, 3, 4, 5 };
            var coll = new ObservableCollectionEx<int>();
            coll.SetItems( array );

            coll.SetItems( array );
            Assert.AreEqual( array.Length, coll.Count );
            for ( int n = 0; n < array.Length; n++ )
            {
                Assert.AreEqual( array[n], coll[n] );
            }
        }

        [TestMethod]
        public void SetItems_DifferentItems()
        {
            var array1 = new[] { 1, 2, 3, 4, 5 };
            var array2 = new[] { 9, 8, 7 };
            var coll = new ObservableCollectionEx<int>();
            coll.SetItems( array1 );

            coll.SetItems( array2 );
            Assert.AreEqual( array2.Length, coll.Count );
            for ( int n = 0; n < array2.Length; n++ )
            {
                Assert.AreEqual( array2[n], coll[n] );
            }
        }

        [TestMethod]
        public void SetItems_ToEmpty()
        {
            var array = new[] { 1, 2, 3, 4, 5 };
            var coll = new ObservableCollectionEx<int>();
            coll.SetItems( array );

            coll.SetItems( new int[] { } );
            Assert.AreEqual( 0, coll.Count );
        }

        [TestMethod]
        public void SetItems_FiresCollectionChanged()
        {
            var coll = new ObservableCollectionEx<int>();
            bool fired = false;

            coll.CollectionChanged += ( s, e ) =>
            {
                fired = true;
            };

            coll.SetItems( new[] { 1, 2, 3 } );

            Assert.IsTrue( fired );
        }
    }
}