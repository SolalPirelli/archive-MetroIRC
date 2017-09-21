// Copyright (C) 2012, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace IrcSharp.Internals
{
    /// <summary>
    /// An ObservableCollection with a SetItems method.
    /// This allows acceptable performance when something slow handles its CollectionChanged argument.
    /// </summary>
    internal sealed class ObservableCollectionEx<T> : ObservableCollection<T>
    {
        /// <summary>
        /// Clears the collection and fills it with new items.
        /// </summary>
        public void SetItems( IEnumerable<T> items )
        {
            this.Items.Clear();
            foreach ( var item in items )
            {
                this.Items.Add( item );
            }

            this.OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );
        }
    }
}