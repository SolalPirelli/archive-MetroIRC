// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace MetroControls.Internals
{
    internal static class TypographyExtensions
    {
        #region TextCasing AttachedProperty
        public static TextCasing GetTextCasing( DependencyObject obj )
        {
            return (TextCasing) obj.GetValue( TextCasingProperty );
        }

        public static void SetTextCasing( DependencyObject obj, TextCasing value )
        {
            obj.SetValue( TextCasingProperty, value );
        }

        public static readonly DependencyProperty TextCasingProperty =
            DependencyProperty.RegisterAttached( "TextCasing",
                                                 typeof( TextCasing ),
                                                 typeof( TypographyExtensions ),
                                                 new UIPropertyMetadata( TextCasing.Normal, OnTextCasingChanged ) );

        private static void OnTextCasingChanged( DependencyObject obj, DependencyPropertyChangedEventArgs args )
        {
            var textBlock = obj as TextBlock;
            if ( textBlock == null )
            {
                return;
            }

            SetText( textBlock, (TextCasing) args.NewValue );
            Binding.AddTargetUpdatedHandler( textBlock, ( s, e ) => SetText( textBlock, (TextCasing) args.NewValue ) );
        }
        #endregion

        private static void SetText( TextBlock textBlock, TextCasing textCasing )
        {
            switch ( textCasing )
            {
                case TextCasing.UpperCase:
                    textBlock.Text = textBlock.Text.ToUpper();
                    break;
                case TextCasing.LowerCase:
                    textBlock.Text = textBlock.Text.ToLower();
                    break;
            }
        }
    }
}