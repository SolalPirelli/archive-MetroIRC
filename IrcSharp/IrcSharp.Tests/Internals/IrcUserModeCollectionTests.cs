using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using IrcSharp.Internals;
using IrcSharp.Tests.TestInternals;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IrcSharp.Tests.Internals
{
    [TestClass]
    public class IrcUserModeCollectionTests
    {
        private IrcNetwork _network;

        [TestInitialize]
        public void Initialize()
        {
            var tup = Initializer.GetAuthenticatedNetwork();
            this._network = tup.Item1;
        }

        [TestMethod]
        public void Add()
        {
            var user = this._network.GetUser( "alice" );
            var pair = new IrcUserChannelModePair( user, IrcChannelUserModes.Op );
            var coll = new IrcUserModeCollection();
            bool fired = false, firedUsers = false;
            ( (INotifyCollectionChanged) coll ).CollectionChanged += ( s, e ) =>
            {
                fired = true;
            };
            ( (INotifyCollectionChanged) coll.Users ).CollectionChanged += ( s, e ) =>
            {
                firedUsers = true;
            };
            coll.Add( pair );

            Assert.AreEqual( 1, coll.Count );
            Assert.IsTrue( coll.Contains( pair ) );
            Assert.IsTrue( fired );
            Assert.IsTrue( firedUsers );
        }

        [TestMethod]
        public void Remove_Empty()
        {
            var user = this._network.GetUser( "alice" );
            var pair = new IrcUserChannelModePair( user, IrcChannelUserModes.Op );
            var coll = new IrcUserModeCollection();
            int firedCount = 0, firedUsersCount = 0;
            ( (INotifyCollectionChanged) coll ).CollectionChanged += ( s, e ) =>
            {
                firedCount++;
            };
            ( (INotifyCollectionChanged) coll.Users ).CollectionChanged += ( s, e ) =>
            {
                firedUsersCount++;
            };
            coll.Add( pair );
            coll.Remove( pair );

            Assert.AreEqual( 0, coll.Count );
            Assert.AreEqual( 2, firedCount );
            Assert.AreEqual( 2, firedUsersCount );
        }

        [TestMethod]
        public void Remove_NotEmpty()
        {
            var user1 = this._network.GetUser( "alice" );
            var pair1 = new IrcUserChannelModePair( user1, IrcChannelUserModes.Op );
            var user2 = this._network.GetUser( "bob" );
            var pair2 = new IrcUserChannelModePair( user2, IrcChannelUserModes.Admin );
            var coll = new IrcUserModeCollection();
            int firedCount = 0, firedUsersCount = 0;
            ( (INotifyCollectionChanged) coll ).CollectionChanged += ( s, e ) =>
            {
                firedCount++;
            };
            ( (INotifyCollectionChanged) coll.Users ).CollectionChanged += ( s, e ) =>
            {
                firedUsersCount++;
            };
            coll.Add( pair1 );
            coll.Add( pair2 );
            coll.Remove( pair1 );

            Assert.AreEqual( 1, coll.Count );
            Assert.IsTrue( coll.Contains( pair2 ) );
            Assert.AreEqual( 3, firedCount );
            Assert.AreEqual( 3, firedUsersCount );
        }

        [TestMethod]
        public void RemoveUser()
        {
            var user1 = this._network.GetUser( "alice" );
            var pair1 = new IrcUserChannelModePair( user1, IrcChannelUserModes.Op );
            var user2 = this._network.GetUser( "bob" );
            var pair2 = new IrcUserChannelModePair( user2, IrcChannelUserModes.Admin );
            var coll = new IrcUserModeCollection();
            int firedCount = 0, firedUsersCount = 0;
            ( (INotifyCollectionChanged) coll ).CollectionChanged += ( s, e ) =>
            {
                firedCount++;
            };
            ( (INotifyCollectionChanged) coll.Users ).CollectionChanged += ( s, e ) =>
            {
                firedUsersCount++;
            };
            coll.Add( pair1 );
            coll.Add( pair2 );
            coll.RemoveUser( user1 );

            Assert.AreEqual( 1, coll.Count );
            Assert.IsTrue( coll.Contains( pair2 ) );
            Assert.AreEqual( 3, firedCount );
            Assert.AreEqual( 3, firedUsersCount );
        }

        [TestMethod]
        public void SetItems_Empty()
        {
            var coll = new IrcUserModeCollection();
            var pairs = new[] 
            { 
                new IrcUserChannelModePair( this._network.GetUser( "alice" ), IrcChannelUserModes.Admin ), 
                new IrcUserChannelModePair( this._network.GetUser( "bob" ), IrcChannelUserModes.Op ) 
            };
            int firedCount = 0, firedUsersCount = 0;
            ( (INotifyCollectionChanged) coll ).CollectionChanged += ( s, e ) =>
            {
                firedCount++;
            };
            ( (INotifyCollectionChanged) coll.Users ).CollectionChanged += ( s, e ) =>
            {
                firedUsersCount++;
            };
            coll.SetItems( pairs );

            Assert.AreEqual( pairs.Length, coll.Count );
            foreach ( var pair in pairs )
            {
                Assert.IsTrue( coll.Contains( pair ) );
            }
            Assert.AreEqual( 1, firedCount );
            Assert.AreEqual( 1, firedUsersCount );
        }

        [TestMethod]
        public void SetItems_NotEmpty()
        {
            var coll = new IrcUserModeCollection();
            var firstPair = new IrcUserChannelModePair( this._network.GetUser( "charles" ), IrcChannelUserModes.Normal );
            var pairs = new[] 
            { 
                new IrcUserChannelModePair( this._network.GetUser( "alice" ), IrcChannelUserModes.Admin ), 
                new IrcUserChannelModePair( this._network.GetUser( "bob" ), IrcChannelUserModes.Op ) 
            };
            int firedCount = 0, firedUsersCount = 0;
            ( (INotifyCollectionChanged) coll ).CollectionChanged += ( s, e ) =>
            {
                firedCount++;
            };
            ( (INotifyCollectionChanged) coll.Users ).CollectionChanged += ( s, e ) =>
            {
                firedUsersCount++;
            };
            coll.Add( firstPair );
            coll.SetItems( pairs );

            Assert.AreEqual( pairs.Length, coll.Count );
            foreach ( var pair in pairs )
            {
                Assert.IsTrue( coll.Contains( pair ) );
            }
            Assert.IsFalse( coll.Contains( firstPair ) );
            Assert.AreEqual( 2, firedCount );
            Assert.AreEqual( 2, firedUsersCount );
        }

        [TestMethod]
        public void Clear()
        {
            var coll = new IrcUserModeCollection();
            var pair = new IrcUserChannelModePair( this._network.GetUser( "alice" ), IrcChannelUserModes.Admin );
            int firedCount = 0, firedUsersCount = 0;
            ( (INotifyCollectionChanged) coll ).CollectionChanged += ( s, e ) =>
            {
                firedCount++;
            };
            ( (INotifyCollectionChanged) coll.Users ).CollectionChanged += ( s, e ) =>
            {
                firedUsersCount++;
            };
            coll.Add( pair );
            coll.Clear();

            Assert.AreEqual( 0, coll.Count );
            Assert.AreEqual( 2, firedCount );
            Assert.AreEqual( 2, firedUsersCount );
        }

        [TestMethod]
        public void Index()
        {
            var coll = new IrcUserModeCollection();
            var pair = new IrcUserChannelModePair( this._network.GetUser( "alice" ), IrcChannelUserModes.Admin );
            coll.Add( pair );

            Assert.AreEqual( pair, coll[0] );
        }

        [TestMethod]
        public void UserIndex()
        {
            var user = this._network.GetUser( "alice" );
            var pair = new IrcUserChannelModePair( user, IrcChannelUserModes.Op );
            var coll = new IrcUserModeCollection();
            coll.Add( pair );
            Assert.AreEqual( pair.Mode, coll[pair.User] );
        }

        [TestMethod]
        public void UserIndex_Get_DoesNotContain()
        {
            var user = this._network.GetUser( "alice" );
            var coll = new IrcUserModeCollection();
            AssertEx.Throws<KeyNotFoundException>( () => { var v = coll[user]; } );
        }

        [TestMethod]
        public void UserIndex_Set_DoesNotContain()
        {
            var user = this._network.GetUser( "alice" );
            var coll = new IrcUserModeCollection();
            AssertEx.Throws<KeyNotFoundException>( () => coll[user] = IrcChannelUserModes.Op );
        }

        [TestMethod]
        public void NonGenericEnumerator_IsSameAsGenericOne()
        {
            var coll = new IrcUserModeCollection();
            var genEnum = coll.GetEnumerator();
            var nonGenEnum = ( (IEnumerable) coll ).GetEnumerator();
            Assert.AreEqual( genEnum.GetType(), nonGenEnum.GetType() );
        }
    }
}