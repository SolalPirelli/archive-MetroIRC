// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace CommonStuff
{
    public sealed class RichTextBoxExtensions : DependencyObject
    {
        public static FlowDocument GetBoundDocument( DependencyObject obj )
        {
            return (FlowDocument) obj.GetValue( BoundDocumentProperty );
        }

        public static void SetBoundDocument( DependencyObject obj, FlowDocument value )
        {
            obj.SetValue( BoundDocumentProperty, value );
        }

        public static readonly DependencyProperty BoundDocumentProperty =
            DependencyProperty.RegisterAttached( "BoundDocument", typeof( FlowDocument ), typeof( RichTextBoxExtensions ), new UIPropertyMetadata( OnBoundDocumentChanged ) );

        private static void OnBoundDocumentChanged( DependencyObject obj, DependencyPropertyChangedEventArgs args )
        {
            var box = obj as RichTextBox;
            if ( box == null )
            {
                return;
            }

            if ( args.NewValue == null )
            {
                box.Document = new FlowDocument();
            }
            else
            {
                box.Document = (FlowDocument) args.NewValue;
            }
        }
    }
}