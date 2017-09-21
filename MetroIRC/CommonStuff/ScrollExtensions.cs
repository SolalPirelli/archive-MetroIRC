// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using WeakPair = System.Tuple<System.WeakReference<System.Collections.Specialized.INotifyCollectionChanged>, System.WeakReference<System.Windows.Controls.ScrollViewer>>;

namespace CommonStuff
{
    public sealed class ScrollExtensions : DependencyObject
    {
        private static List<WeakPair> _pairs = new List<WeakPair>();

        #region AutoScroll AttachedProperty
        public static AutoScrollMode GetAutoScroll( DependencyObject obj )
        {
            return (AutoScrollMode) obj.GetValue( AutoScrollProperty );
        }

        public static void SetAutoScroll( DependencyObject obj, AutoScrollMode value )
        {
            obj.SetValue( AutoScrollProperty, value );
        }

        public static readonly DependencyProperty AutoScrollProperty =
            DependencyProperty.RegisterAttached( "AutoScroll", typeof( AutoScrollMode ), typeof( ScrollExtensions ), new PropertyMetadata( AutoScrollMode.BottomOnly ) );
        #endregion

        #region AutoScrollSource AttachedProperty
        public static INotifyCollectionChanged GetAutoScrollSource( DependencyObject obj )
        {
            return (INotifyCollectionChanged) obj.GetValue( AutoScrollSourceProperty );
        }

        public static void SetAutoScrollSource( DependencyObject obj, INotifyCollectionChanged value )
        {
            obj.SetValue( AutoScrollSourceProperty, value );
        }

        public static readonly DependencyProperty AutoScrollSourceProperty =
            DependencyProperty.RegisterAttached( "AutoScrollSource", typeof( INotifyCollectionChanged ), typeof( ScrollExtensions ), new PropertyMetadata( AutoScrollPropertyChanged ) );

        private static void AutoScrollPropertyChanged( DependencyObject obj, DependencyPropertyChangedEventArgs args )
        {
            var scroller = obj as ScrollViewer;
            if ( scroller == null || args.OldValue == args.NewValue )
            {
                return;
            }

            if ( args.OldValue != null )
            {
                var coll = (INotifyCollectionChanged) args.OldValue;
                CollectionChangedEventManager.RemoveHandler( coll, AutoScrollSource_CollectionChanged );
                WeakEventManager<ScrollViewer, SizeChangedEventArgs>.RemoveHandler( scroller, "SizeChanged", ScrollViewer_SizeChanged );

                WeakPair pair = Get( coll, ( p, _, __ ) => p );
                if ( pair != null )
                {
                    _pairs.Remove( pair );
                }
            }

            if ( args.NewValue != null )
            {
                var coll = (INotifyCollectionChanged) args.NewValue;
                CollectionChangedEventManager.AddHandler( coll, AutoScrollSource_CollectionChanged );
                WeakEventManager<ScrollViewer, SizeChangedEventArgs>.AddHandler( scroller, "SizeChanged", ScrollViewer_SizeChanged );
                AddPair( coll, scroller );
            }
        }
        #endregion

        #region Weak event handlers
        private static void AutoScrollSource_CollectionChanged( object source, NotifyCollectionChangedEventArgs e )
        {
            ScrollViewer scroller = Get( (INotifyCollectionChanged) source, ( _, __, scroll ) => scroll );
            if ( scroller == null )
            {
                return;
            }

            ScrollIfNeeded( scroller );
        }

        private static void ScrollViewer_SizeChanged( object source, SizeChangedEventArgs e )
        {
            ScrollIfNeeded( (ScrollViewer) source );
        } 
        #endregion

        #region Private methods
        private static void ScrollIfNeeded( ScrollViewer scroller )
        {
            Application.Current.Dispatcher.Invoke( () =>
            {
                var mode = GetAutoScroll( scroller );

                if ( ShouldScroll( scroller, mode ) )
                {
                    scroller.ScrollToBottom();
                }
            } );
        }

        private static void AddPair( INotifyCollectionChanged collection, ScrollViewer scroller )
        {
            _pairs.Add( new WeakPair( new WeakReference<INotifyCollectionChanged>( collection ), new WeakReference<ScrollViewer>( scroller ) ) );
        }

        private static T Get<T>( INotifyCollectionChanged collection, Func<WeakPair, INotifyCollectionChanged, ScrollViewer, T> retFunc )
            where T : class
        {
            foreach ( var pair in _pairs.ToArray() )
            {
                INotifyCollectionChanged coll;
                ScrollViewer scroller;
                if ( pair.Item1.TryGetTarget( out coll ) && pair.Item2.TryGetTarget( out scroller ) )
                {
                    if ( coll == collection )
                    {
                        return retFunc( pair, coll, scroller );
                    }
                }
                else
                {
                    _pairs.Remove( pair );
                }
            }

            return null;
        }

        private static bool ShouldScroll( ScrollViewer viewer, AutoScrollMode mode )
        {
            return mode == AutoScrollMode.Disabled ? false
                 : mode == AutoScrollMode.Enabled ? true
                 : IsVerticalScrollbarDown( viewer );
        }

        private static bool IsVerticalScrollbarDown( ScrollViewer scrollViewer )
        {
            return scrollViewer.ScrollableHeight < 0
                || scrollViewer.VerticalOffset + scrollViewer.ViewportHeight >= scrollViewer.ExtentHeight;
        } 
        #endregion
    }

    public enum AutoScrollMode
    {
        Disabled,
        Enabled,
        BottomOnly
    }
}