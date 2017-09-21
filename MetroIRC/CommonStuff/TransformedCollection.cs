// Copyright (C) 2013, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace CommonStuff
{
    /// <summary>
    /// A collection that encapsulates another collection and transforms it, taking care of collection and item changes.
    /// </summary>
    public sealed class TransformedCollection<T> : IEnumerable<object>, INotifyCollectionChanged
        where T : INotifyPropertyChanged
    {
        private const int TimerDelay = 50;

        private readonly Func<IEnumerable<T>, IEnumerable<object>> _transform;
        private readonly IEnumerable<T> _source;
        private readonly List<DependentProperty> _depProps;

        // to avoid massive performance problems when many items change at the same time
        private DelayTimer _transformTimer;
        private DelayTimer _setTimer;
        // previous items are kept to remove event handlers
        private List<T> _items;
        private IEnumerable<object> _lastResult;

        public TransformedCollection( INotifyCollectionChanged source, Func<IEnumerable<T>, IEnumerable<object>> transform )
        {
            if ( !( source is IEnumerable<T> ) )
            {
                throw new ArgumentException( "source must be an IEnumerable<T>." );
            }
            this._source = (IEnumerable<T>) source;
            this._transform = transform;

            this._transformTimer = new DelayTimer( this.TransformItems, TimerDelay );
            this._setTimer = new DelayTimer( this.SetItems, TimerDelay );
            this._items = new List<T>();
            this._depProps = new List<DependentProperty>();

            this.SetItems();
            this.TransformItems();

            CollectionChangedEventManager.AddHandler( source, Source_CollectionChanged );
        }

        /// <summary>
        /// Adds a dependent property; that is, a property whose changes will be monitored to refresh the <see cref="TransformedCollection"/>.
        /// </summary>
        public void AddDependentProperty( string propertyName )
        {
            this.AddDependentProperty( o => o, propertyName );
        }

        /// <summary>
        /// Adds a dependent property of a property; that is, a property whose changes will be monitored to refresh the <see cref="TransformedCollection"/>.
        /// </summary>
        public void AddDependentProperty( Func<T, INotifyPropertyChanged> selector, string propertyName )
        {
            var depProp = new DependentProperty( propertyName, selector, Item_PropertyChanged );
            this._depProps.Add( depProp );

            foreach ( var item in this._source )
            {
                depProp.Register( item );
            }
        }

        #region Private methods
        private void SetItems()
        {
            foreach ( var item in this._items.Except( this._source ) )
            {
                foreach ( var depProp in this._depProps )
                {
                    depProp.Unregister( item );
                }
            }
            foreach ( var item in this._source.Except( this._items ) )
            {
                foreach ( var depProp in this._depProps )
                {
                    depProp.Register( item );
                }
            }
            this._items = this._source.ToList();
        }

        private void TransformItems()
        {
            this._lastResult = this._transform( this._source );
            this.OnCollectionChanged();
        }
        #endregion

        #region Weak event handlers
        private void Source_CollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
        {
            this._setTimer.Delay();
            this._transformTimer.Delay();
        }

        private void Item_PropertyChanged( object sender, PropertyChangedEventArgs args )
        {
            this._transformTimer.Delay();
        }
        #endregion

        #region IEnumerable<T> implementation
        public IEnumerator<object> GetEnumerator()
        {
            return _lastResult.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
        #endregion

        #region INotifyCollectionChanged implementation
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        private void OnCollectionChanged()
        {
            var evt = this.CollectionChanged;
            if ( evt != null )
            {
                evt( this, new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );
            }
        }
        #endregion

        #region DependentProperty
        private sealed class DependentProperty
        {
            private readonly string _name;
            private readonly Func<T, INotifyPropertyChanged> _selector;
            private readonly EventHandler<PropertyChangedEventArgs> _handler;

            public DependentProperty( string name, Func<T, INotifyPropertyChanged> selector, EventHandler<PropertyChangedEventArgs> handler )
            {
                this._name = name;
                this._selector = selector;
                this._handler = handler;
            }

            public void Register( T item )
            {
                PropertyChangedEventManager.AddHandler( this._selector( item ), this._handler, this._name );
            }

            public void Unregister( T item )
            {
                PropertyChangedEventManager.RemoveHandler( this._selector( item ), this._handler, this._name );
            }
        }
        #endregion

        #region DelayTimer
        /// <summary>
        /// A timer that can delay an action any number of times, and will only fire when nobody delays it for a certain time.
        /// </summary>
        private sealed class DelayTimer
        {
            private readonly Action _action;
            private readonly int _delay;

            private int _delayCount;

            public DelayTimer( Action action, int delay )
            {
                this._action = action;
                this._delay = delay;
            }

            public async void Delay()
            {
                this._delayCount++;
                int oldCount = this._delayCount;

                await Task.Delay( this._delay );

                if ( this._delayCount == oldCount )
                {
                    this._action();
                }
            }
        }
        #endregion
    }
}