// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CommonStuff
{
    public static class TextHistory
    {
        #region Constants
        private const int HistorySize = 40;
        #endregion

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
            DependencyProperty.RegisterAttached( "IsEnabled", typeof( bool ), typeof( TextHistory ), new PropertyMetadata( IsEnabledPropertyChanged ) );

        private static void IsEnabledPropertyChanged( DependencyObject obj, DependencyPropertyChangedEventArgs args )
        {
            var box = obj as TextBox;
            if ( box == null )
            {
                return;
            }

            if ( (bool) args.NewValue )
            {
                box.PreviewKeyDown += TextBox_PreviewKeyDown;
                box.PreviewKeyUp += TextBox_PreviewKeyUp;
                SetTextHistory( box, new FixedSpaceCollection<string>( HistorySize, false ) );
            }
            else
            {
                box.PreviewKeyDown -= TextBox_PreviewKeyDown;
                box.PreviewKeyUp -= TextBox_PreviewKeyUp;
                SetTemporaryText( box, null );
                SetTextHistory( box, null );
            }
        }
        #endregion

        #region (private) TextHistory AttachedProperty
        private static FixedSpaceCollection<string> GetTextHistory( DependencyObject obj )
        {
            return (FixedSpaceCollection<string>) obj.GetValue( TextHistoryProperty );
        }

        private static void SetTextHistory( DependencyObject obj, FixedSpaceCollection<string> value )
        {
            obj.SetValue( TextHistoryProperty, value );
        }

        private static readonly DependencyProperty TextHistoryProperty =
            DependencyProperty.RegisterAttached( "TextHistory", typeof( FixedSpaceCollection<string> ), typeof( TextHistory ), new PropertyMetadata( null ) );
        #endregion

        #region (private) HistoryIndex AttachedProperty
        public static int GetHistoryIndex( DependencyObject obj )
        {
            return (int) obj.GetValue( HistoryIndexProperty );
        }

        public static void SetHistoryIndex( DependencyObject obj, int value )
        {
            obj.SetValue( HistoryIndexProperty, value );
        }

        public static readonly DependencyProperty HistoryIndexProperty =
            DependencyProperty.RegisterAttached( "HistoryIndex", typeof( int ), typeof( TextHistory ), new PropertyMetadata( 0 ) );
        #endregion

        #region (private) TemporaryText AttachedProperty
        public static string GetTemporaryText( DependencyObject obj )
        {
            return (string) obj.GetValue( TemporaryTextProperty );
        }

        public static void SetTemporaryText( DependencyObject obj, string value )
        {
            obj.SetValue( TemporaryTextProperty, value );
        }

        public static readonly DependencyProperty TemporaryTextProperty =
            DependencyProperty.RegisterAttached( "TemporaryText", typeof( string ), typeof( TextHistory ), new PropertyMetadata( null ) );
        #endregion

        private static void TextBox_PreviewKeyDown( object sender, KeyEventArgs e )
        {
            var box = (TextBox) sender;

            if ( e.Key == Key.Enter )
            {
                var history = GetTextHistory( box );
                if ( box.Text.HasText() )
                {
                    history.Insert( 0, box.Text );
                }

                SetTemporaryText( box, string.Empty );
            }
        }

        private static void TextBox_PreviewKeyUp( object sender, KeyEventArgs e )
        {
            var box = (TextBox) sender;
            string oldText = box.Text;

            if ( e.Key == Key.Up )
            {
                GoUp( box );
            }
            else if ( e.Key == Key.Down )
            {
                GoDown( box );
            }
            else if ( !KeyHelper.IsModifierKey( e.Key ) )
            {
                SetTemporaryText( box, box.Text );
                SetHistoryIndex( box, -1 );
            }

            if ( box.Text != oldText && box.Text.HasText() )
            {
                box.CaretIndex = box.Text.Length - 1;
            }
        }

        private static void GoUp( TextBox box )
        {
            int index = GetHistoryIndex( box );
            var history = GetTextHistory( box );

            if ( index < history.Count - 1 )
            {
                index++;
                SetHistoryIndex( box, index );
                box.Text = history[index];
            }
        }

        private static void GoDown( TextBox box )
        {
            int index = GetHistoryIndex( box );
            var history = GetTextHistory( box );

            if ( index >= 0 )
            {
                index--;
                SetHistoryIndex( box, index );

                string text = index >= 0 ? history[index] : GetTemporaryText( box );
                box.Text = text;          
            }
        }
    }
}