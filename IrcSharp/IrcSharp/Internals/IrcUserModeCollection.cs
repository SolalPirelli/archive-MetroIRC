// Copyright (C) 2013, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace IrcSharp.Internals
{
    /// <summary>
    /// A collection of <see cref="IrcUser"/>s and their modes on a given <see cref="IrcChannel"/>.
    /// </summary>
    public sealed class IrcUserModeCollection : IReadOnlyList<IrcUserChannelModePair>, INotifyCollectionChanged
    {
        #region Private members
        private List<IrcUserChannelModePair> _items;
        private ObservableCollectionEx<IrcUser> _users;
        #endregion

        /// <summary>
        /// Gets the <see cref="IrcChannelUserModes"/> of the specified <see cref="IrcUser"/>.
        /// </summary>
        /// <param name="index">The <see cref="IrcUser"/>.</param>
        /// <returns>The <see cref="IrcChannelUserModes"/> of the specified <see cref="IrcUser"/>.</returns>
        public IrcChannelUserModes this[IrcUser index]
        {
            get
            {
                return this.GetPair( index ).Mode;
            }
            internal set
            {
                this.GetPair( index ).Mode = value;
            }
        }

        #region Internal properties
        /// <summary>
        /// Gets the users in the collection.
        /// </summary>
        internal ReadOnlyObservableCollection<IrcUser> Users { get; private set; }
        #endregion

        internal IrcUserModeCollection()
        {
            this._items = new List<IrcUserChannelModePair>();
            this._users = new ObservableCollectionEx<IrcUser>();
            this.Users = new ReadOnlyObservableCollection<IrcUser>( this._users );
        }

        #region Internal methods
        /// <summary>
        /// Adds an user to the <see cref="IrcUserModeCollection"/> with a normal mode.
        /// </summary>
        internal void Add( IrcUserChannelModePair item )
        {
            this._items.Add( item );
            this._users.Add( item.User );

            this.FireCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Add, item ) );
        }

        /// <summary>
        /// Removes an <see cref="IrcUserChannelModePair"/> from the <see cref="IrcUserModeCollection"/>.
        /// </summary>
        internal void Remove( IrcUserChannelModePair item )
        {
            this._items.Remove( item );
            this._users.Remove( item.User );

            this.FireCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Remove, item ) );
        }

        /// <summary>
        /// Removes an <see cref="IrcUser"/> from the <see cref="IrcUserModeCollection"/>.
        /// </summary>
        internal void RemoveUser( IrcUser user )
        {
            var pair = this._items.First( p => p.User == user );
            this.Remove( pair );
            this._users.Remove( user );
        }

        /// <summary>
        /// Sets the <see cref="IrcUserModeCollection"/>'s items.
        /// </summary>
        internal void SetItems( IEnumerable<IrcUserChannelModePair> items )
        {
            this._items.Clear();
            foreach ( var item in items )
            {
                this._items.Add( item );
            }
            this._users.SetItems( items.Select( p => p.User ) );
            this.FireCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );
        }

        /// <summary>
        /// Clears the <see cref="IrcUserModeCollection"/>'s items.
        /// </summary>
        internal void Clear()
        {
            this._items.Clear();
            this._users.Clear();
            this.FireCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );
        }
        #endregion

        #region Private methods
        private IrcUserChannelModePair GetPair( IrcUser user )
        {
            var pair = this._items.FirstOrDefault( p => p.User == user );
            if ( pair == null )
            {
                throw new KeyNotFoundException( "The specified IrcUser is not in the collection." );
            }
            return pair;
        }
        #endregion

        #region IReadOnlyList<IrcUserChannelModePair> implementation
        /// <summary>
        /// Gets the number of elements in the collection.
        /// </summary>
        public int Count
        {
            get { return this._items.Count; }
        }

        /// <summary>
        /// Gets the element at the specified index in the read-only list.
        /// </summary>
        /// <param name="index">The index</param>
        /// <returns>The element at the specified index.</returns>
        public IrcUserChannelModePair this[int index]
        {
            get { return this._items[index]; }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that iterates through the collection.</returns>
        public IEnumerator<IrcUserChannelModePair> GetEnumerator()
        {
            return this._items.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that iterates through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
        #endregion

        #region INotifyCollectionChanged implementation
        /// <summary>
        /// Occurs when the contents of the <see cref="IrcUserModeCollection"/> change.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        private void FireCollectionChanged( NotifyCollectionChangedEventArgs e )
        {
            var evt = this.CollectionChanged;
            if ( evt != null )
            {
                evt( this, e );
            }
        }
        #endregion
    }
}