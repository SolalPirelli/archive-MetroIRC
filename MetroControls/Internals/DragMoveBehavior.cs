// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Windows;

namespace MetroControls.Internals
{
    internal sealed class DragMoveBehavior : DependencyObject
    {
        #region IsEnabled AttachedProperty
        public static bool GetIsEnabled( DependencyObject obj )
        {
            return (bool) obj.GetValue( IsEnabledProperty );
        }

        public static void SetIsEnabled( DependencyObject obj, bool value )
        {
            obj.SetValue( IsEnabledProperty, value );
        }

        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached( "IsEnabled", typeof( bool ), typeof( DragMoveBehavior ), new UIPropertyMetadata( false, OnIsEnabledPropertyChanged ) );

        private static void OnIsEnabledPropertyChanged( DependencyObject obj, DependencyPropertyChangedEventArgs args )
        {
            var element = obj as FrameworkElement;
            if ( element == null )
            {
                return;
            }

            if ( GetParentWindow( element ) == null )
            {
                var window = element.TemplatedParent as Window ?? GetParent<Window>( element );
                if ( window == null )
                {
                    throw new ArgumentException( "The element does not have a window as parent." );
                }
                SetParentWindow( element, window );
            }

            if ( (bool) args.NewValue )
            {
                element.MouseLeftButtonDown += Element_MouseLeftButtonDown;
            }
            else
            {
                element.MouseLeftButtonDown -= Element_MouseLeftButtonDown;
            }
        }
        #endregion

        #region (private) ParentWindow AttachedProperty
        private static Window GetParentWindow( DependencyObject obj )
        {
            return (Window) obj.GetValue( ParentWindowProperty );
        }

        private static void SetParentWindow( DependencyObject obj, Window value )
        {
            obj.SetValue( ParentWindowProperty, value );
        }

        private static readonly DependencyProperty ParentWindowProperty =
            DependencyProperty.RegisterAttached( "ParentWindow", typeof( Window ), typeof( DragMoveBehavior ), new UIPropertyMetadata() );
        #endregion

        private static void Element_MouseLeftButtonDown( object sender, EventArgs e )
        {
            NativeMethods.DragMove( GetParentWindow( (DependencyObject) sender ) );
        }

        private static T GetParent<T>( DependencyObject obj )
            where T : DependencyObject
        {
            var currentObj = obj;
            while ( currentObj != null && !( currentObj is T ) )
            {
                currentObj = LogicalTreeHelper.GetParent( currentObj );
            }
            return (T) currentObj;
        }
    }
}