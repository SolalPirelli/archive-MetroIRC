// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MetroControls
{
    /// <summary>
    /// A button containing a glyph.
    /// </summary>
    public sealed class GlyphButton : Button
    {
        /// <summary>
        /// Gets or sets the stroke of the <see cref="MetroControls.GlyphButton"/>.
        /// </summary>
        public Brush Stroke
        {
            get { return (Brush) GetValue( StrokeProperty ); }
            set { SetValue( StrokeProperty, value ); }
        }

        /// <summary>
        /// See the <see cref="MetroControls.GlyphButton.Stroke"/> property.
        /// </summary>
        public static readonly DependencyProperty StrokeProperty =
            DependencyProperty.Register( "Stroke", typeof( Brush ), typeof( GlyphButton ), new UIPropertyMetadata( Brushes.White ) );

        /// <summary>
        /// Gets or sets the stroke thickness of the <see cref="MetroControls.GlyphButton"/>.
        /// </summary>
        public double StrokeThickness
        {
            get { return (double) GetValue( StrokeThicknessProperty ); }
            set { SetValue( StrokeThicknessProperty, value ); }
        }

        /// <summary>
        /// See the <see cref="MetroControls.GlyphButton.StrokeThickness"/> property.
        /// </summary>
        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyProperty.Register( "StrokeThickness", typeof( double ), typeof( GlyphButton ), new UIPropertyMetadata( 1.0 ) );

        /// <summary>
        /// Gets or sets the content of the <see cref="MetroControls.GlyphButton"/>.
        /// </summary>
        public Geometry Data
        {
            get { return (Geometry) GetValue( DataProperty ); }
            set { SetValue( DataProperty, value ); }
        }

        /// <summary>
        /// See the <see cref="MetroControls.GlyphButton.Data"/> property.
        /// </summary>
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register( "Data", typeof( Geometry ), typeof( GlyphButton ), new UIPropertyMetadata( null ) );
    }
}