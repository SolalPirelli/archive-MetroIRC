// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CommonStuff
{
    /// <summary>
    /// This class provides typography extensions such as all-caps text for non-OpenType fonts.
    /// Do NOT use more than one of these attached properties on an object. Weird things will happen.
    /// </summary>
    public static class TypographyExtensions
    {
        #region CapitalsText AttachedProperty
        public static string GetUppercaseText( DependencyObject obj )
        {
            return (string) obj.GetValue( UppercaseTextProperty );
        }

        public static void SetUppercaseText( DependencyObject obj, string value )
        {
            obj.SetValue( UppercaseTextProperty, value );
        }

        public static readonly DependencyProperty UppercaseTextProperty =
            DependencyProperty.RegisterAttached( "UppercaseText", typeof( string ), typeof( TypographyExtensions ), new UIPropertyMetadata( OnUppercaseTextChanged ) );

        private static void OnUppercaseTextChanged( DependencyObject obj, DependencyPropertyChangedEventArgs args )
        {
            var textBlock = obj as TextBlock;
            if ( textBlock == null )
            {
                return;
            }

            string newValue = ( (string) args.NewValue ?? string.Empty ).ToUpperInvariant();
            if ( textBlock.Text != newValue )
            {
                Application.Current.Dispatcher.BeginInvoke( (Action) ( () => textBlock.Text = newValue ), DispatcherPriority.Loaded );
            }
        }
        #endregion

        #region LowercaseText AttachedProperty
        public static string GetLowercaseText( DependencyObject obj )
        {
            return (string) obj.GetValue( UppercaseTextProperty );
        }

        public static void SetLowercaseText( DependencyObject obj, string value )
        {
            obj.SetValue( UppercaseTextProperty, value );
        }

        public static readonly DependencyProperty LowercaseTextProperty =
            DependencyProperty.RegisterAttached( "LowercaseText", typeof( string ), typeof( TypographyExtensions ), new UIPropertyMetadata( OnLowercaseTextChanged ) );

        private static void OnLowercaseTextChanged( DependencyObject obj, DependencyPropertyChangedEventArgs args )
        {
            var textBlock = obj as TextBlock;
            if ( textBlock == null )
            {
                return;
            }

            string newValue = ( (string) args.NewValue ?? string.Empty ).ToLowerInvariant();
            if ( textBlock.Text != newValue )
            {
                Application.Current.Dispatcher.BeginInvoke( (Action) ( () => textBlock.Text = newValue ), DispatcherPriority.Loaded );
            }
        }
        #endregion
    }
}