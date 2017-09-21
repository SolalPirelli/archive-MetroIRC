// Copyright (C) 2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.using System.Text;

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MetroIrc.Desktop.Extensions
{
    public sealed class TextBoxExtensions : DependencyObject
    {
        #region Constants/readonly
        private const string FormattingPrefix = "^";
        private static readonly Dictionary<Key, char> FormattingChars = new Dictionary<Key, char>()
        {
            { Key.C, 'C' }, // color
            { Key.R, 'R' }, // reverse
            { Key.B, 'B' }, // bold
            { Key.U, 'U' }, // underline
            { Key.N, 'N' }  // normal
        };
        #endregion

        #region AllowFormattingChars AttachedProperty
        public static bool GetAllowFormattingChars( TextBox obj )
        {
            return (bool) obj.GetValue( AllowFormattingCharsProperty );
        }

        public static void SetAllowFormattingChars( TextBox obj, bool value )
        {
            obj.SetValue( AllowFormattingCharsProperty, value );
        }

        public static readonly DependencyProperty AllowFormattingCharsProperty =
            DependencyProperty.RegisterAttached( "AllowFormattingChars", typeof( bool ), typeof( TextBoxExtensions ), new UIPropertyMetadata( AllowFormattingCharsChanged ) );

        private static void AllowFormattingCharsChanged( DependencyObject obj, DependencyPropertyChangedEventArgs args )
        {
            var box = obj as TextBox;
            if ( box == null )
            {
                return;
            }

            if ( (bool) args.OldValue )
            {
                box.PreviewKeyDown -= TextBox_PreviewKeyDown;
            }

            if ( (bool) args.NewValue )
            {
                box.PreviewKeyDown += TextBox_PreviewKeyDown;
            }
        }
        #endregion

        private static void TextBox_PreviewKeyDown( object sender, KeyEventArgs e )
        {
            if ( ShouldInsertFormatting() )
            {
                var box = (TextBox) sender;

                if ( box.SelectionLength == 0 && FormattingChars.ContainsKey( e.Key ) )
                {
                    e.Handled = true;

                    int oldCaretIndex = box.CaretIndex;
                    string formatText = FormattingPrefix + FormattingChars[e.Key];
                    box.Text = box.Text.Insert( box.CaretIndex, formatText );
                    box.CaretIndex = oldCaretIndex + formatText.Length;
                }
            }
        }

        private static bool ShouldInsertFormatting()
        {
            return ( Keyboard.IsKeyDown( Key.LeftCtrl ) || Keyboard.IsKeyDown( Key.RightCtrl ) )
                && !( Keyboard.IsKeyDown( Key.LeftAlt ) || Keyboard.IsKeyDown( Key.RightAlt ) );
        }
    }
}