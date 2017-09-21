using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace MetroIrc.Internals
{
    /// <summary>
    /// An observable collection with a maximum number of elements.
    /// </summary>
    public sealed class FixedSpaceCollection<T> : ObservableCollection<T>
    {
        private int _maxItemCount;
        public int MaxItemCount
        {
            get { return this._maxItemCount; }
            set
            {
                this._maxItemCount = value;
                int currentCount = this.Count;
                this.RemoveOverflow();

                if ( currentCount != this.Count )
                {
                    this.OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );
                }
            }
        }

        public bool RemoveFirstOnOverflow { get; set; }

        public FixedSpaceCollection( int maxItemCount, bool removeFirst )
        {
            this.MaxItemCount = maxItemCount;
            this.RemoveFirstOnOverflow = removeFirst;
        }

        protected override void InsertItem( int index, T item )
        {
            this.RemoveOverflow();

            if ( index > this.Count )
            {
                index = this.Count;
            }

            base.InsertItem( index, item );
        }

        private void RemoveOverflow()
        {
            while ( this.Count == this.MaxItemCount )
            {
                this.RemoveItem( this.RemoveFirstOnOverflow ? 0 : this.Count - 1 );
            }
        }
    }
}