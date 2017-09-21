// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CommonStuff
{
    public static class GridExtensions
    {
        private static string[] Separators = new[] { " ", "," };

        private static GridLengthConverter LengthConverter = new GridLengthConverter();

        #region Columns AttachedProperty
        public static string GetColumns( DependencyObject obj )
        {
            return (string) obj.GetValue( ColumnsProperty );
        }

        public static void SetColumns( DependencyObject obj, string value )
        {
            obj.SetValue( ColumnsProperty, value );
        }

        public static readonly DependencyProperty ColumnsProperty =
            DependencyProperty.RegisterAttached( "Columns", typeof( string ), typeof( GridExtensions ), new UIPropertyMetadata( OnColumnsPropertyChanged ) );

        private static void OnColumnsPropertyChanged( DependencyObject obj, DependencyPropertyChangedEventArgs args )
        {
            var grid = obj as Grid;
            if ( grid == null )
            {
                return;
            }
            grid.ColumnDefinitions.Clear();
            foreach ( GridLength length in InterpretLengthsString( args.NewValue.ToString() ) )
            {
                grid.ColumnDefinitions.Add( new ColumnDefinition { Width = length } );
            }
        }
        #endregion

        #region Rows AttachedProperty
        public static string GetRows( DependencyObject obj )
        {
            return (string) obj.GetValue( RowsProperty );
        }

        public static void SetRows( DependencyObject obj, string value )
        {
            obj.SetValue( RowsProperty, value );
        }

        public static readonly DependencyProperty RowsProperty =
            DependencyProperty.RegisterAttached( "Rows", typeof( string ), typeof( GridExtensions ), new UIPropertyMetadata( OnRowsPropertyChanged ) );

        private static void OnRowsPropertyChanged( DependencyObject obj, DependencyPropertyChangedEventArgs args )
        {
            var grid = obj as Grid;
            if ( grid == null )
            {
                return;
            }
            List<GridLength> rowLengths = InterpretLengthsString( args.NewValue.ToString() );
            grid.RowDefinitions.Clear();
            foreach ( GridLength length in rowLengths )
            {
                grid.RowDefinitions.Add( new RowDefinition { Height = length } );
            }
        }
        #endregion

        private static List<GridLength> InterpretLengthsString( string lengths )
        {
            var lengthList = new List<GridLength>();
            string[] parts = lengths.Split( Separators, StringSplitOptions.RemoveEmptyEntries );
            for ( int n = 0; n < parts.Length; n++ )
            {
                string part = parts[n];
                int multiplier = 1;
                // allow for "20xAuto" syntax
                if ( ( part.Contains( "px" ) && part.Count( c => c == 'x' ) > 1 ) || ( !part.Contains( "px" ) && part.Contains( 'x' ) ) )
                {
                    if ( !int.TryParse( part.Substring( 0, part.IndexOf( 'x' ) ), out multiplier ) )
                    {
                        multiplier = 1;
                    }
                    part = part.Substring( part.IndexOf( 'x' ) + 1 );
                }

                try
                {
                    GridLength length = (GridLength) LengthConverter.ConvertFromString( part );
                    for ( int k = 0; k < multiplier; k++ )
                    {
                        lengthList.Add( length );
                    }
                }
                catch ( NotSupportedException )
                {
                    // nothing
                }
            }

            return lengthList;
        }
    }
}