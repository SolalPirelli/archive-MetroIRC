// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace MetroControls.Internals
{
    internal sealed class ThumbDragHelper : DependencyObject
    {
        #region Command AttachedProperty
        public static ICommand GetCommand( DependencyObject obj )
        {
            return (ICommand) obj.GetValue( CommandProperty );
        }

        public static void SetCommand( DependencyObject obj, ICommand value )
        {
            obj.SetValue( CommandProperty, value );
        }

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached( "Command", typeof( ICommand ), typeof( ThumbDragHelper ), new UIPropertyMetadata( OnCommandPropertyChanged ) );

        private static void OnCommandPropertyChanged( DependencyObject obj, DependencyPropertyChangedEventArgs args )
        {
            var thumb = (Thumb) obj;
            var command = (ICommand) args.NewValue;

            thumb.DragDelta += ( s, e ) =>
            {
                var info = new DragInfo( GetIsTop( thumb ), GetIsLeft( thumb ), e.HorizontalChange, e.VerticalChange, GetDirection( thumb ) );
                if ( command.CanExecute( info ) )
                {
                    command.Execute( info );
                }
            };
        }
        #endregion

        #region IsTop AttachedProperty
        public static bool GetIsTop( DependencyObject obj )
        {
            return (bool) obj.GetValue( IsTopProperty );
        }

        public static void SetIsTop( DependencyObject obj, bool value )
        {
            obj.SetValue( IsTopProperty, value );
        }

        public static readonly DependencyProperty IsTopProperty =
            DependencyProperty.RegisterAttached( "IsTop", typeof( bool ), typeof( ThumbDragHelper ), new UIPropertyMetadata( false ) );
        #endregion

        #region IsLeft AttachedProperty
        public static bool GetIsLeft( DependencyObject obj )
        {
            return (bool) obj.GetValue( IsLeftProperty );
        }

        public static void SetIsLeft( DependencyObject obj, bool value )
        {
            obj.SetValue( IsLeftProperty, value );
        }

        public static readonly DependencyProperty IsLeftProperty =
            DependencyProperty.RegisterAttached( "IsLeft", typeof( bool ), typeof( ThumbDragHelper ), new UIPropertyMetadata( false ) );
        #endregion

        #region Direction AttachedProperty
        public static DragDirections GetDirection( DependencyObject obj )
        {
            return (DragDirections) obj.GetValue( DirectionProperty );
        }

        public static void SetDirection( DependencyObject obj, DragDirections value )
        {
            obj.SetValue( DirectionProperty, value );
        }

        public static readonly DependencyProperty DirectionProperty =
            DependencyProperty.RegisterAttached( "Direction", typeof( DragDirections ), typeof( ThumbDragHelper ), new UIPropertyMetadata() );
        #endregion
    }
}