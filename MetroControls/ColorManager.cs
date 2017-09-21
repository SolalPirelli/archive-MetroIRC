// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace MetroControls
{
    /// <summary>
    /// The manager class for colors. MetroControls' main component.
    /// </summary>
    public static class ColorManager
    {
        #region Property-backing fields
        private static Color _mainColor = Colors.Transparent;
        private static Color _accentColor = Colors.Transparent;
        #endregion

        #region Private members
        private static Dictionary<Color, List<ResourceDictionary>> _mainColorDictionaries;
        #endregion

        #region Public properties
        /// <summary>
        /// Gets the available main colors.
        /// </summary>
        public static ReadOnlyCollection<Color> AvailableMainColors
        {
            get { return new ReadOnlyCollection<Color>( _mainColorDictionaries.Keys.ToArray() ); }
        }

        /// <summary>
        /// Gets or sets the main color.
        /// </summary>
        public static Color MainColor
        {
            get { return _mainColor; }
            set
            {
                SetMainColor( value );
                _mainColor = value;
            }
        }

        /// <summary>
        /// Gets or sets the accent color.
        /// </summary>
        public static Color AccentColor
        {
            get { return _accentColor; }
            set
            {
                SetAccentColor( value );
                _accentColor = value;
            }
        }
        #endregion

        static ColorManager()
        {
            _mainColorDictionaries = new Dictionary<Color, List<ResourceDictionary>>();

            AddMainColorDictionary( Colors.Black, new Uri( "MetroControls;component/Colors/Black.xaml", UriKind.Relative ) );
            AddMainColorDictionary( Colors.White, new Uri( "MetroControls;component/Colors/White.xaml", UriKind.Relative ) );
        }

        #region Public methods
        /// <summary>
        /// Adds a ResourceDictionary to a specified main color's dictionaries.
        /// When that color is set as the current color, the dictionary will be loaded.
        /// </summary>
        /// <param name="color">The color</param>
        /// <param name="dictionaryUri">The Uri of the ResourceDictionary</param>
        public static void AddMainColorDictionary( Color color, Uri dictionaryUri )
        {
            var dic = (ResourceDictionary) Application.LoadComponent( dictionaryUri );

            if ( !_mainColorDictionaries.ContainsKey( color ) )
            {
                _mainColorDictionaries.Add( color, new List<ResourceDictionary> { dic } );
            }
            else
            {
                _mainColorDictionaries[color].Add( dic );
            }

            if ( color == MainColor )
            {
                Application.Current.Resources.MergedDictionaries.Add( dic );
            }
        }
        #endregion

        #region Internal methods
        internal static void SetDefaultColors()
        {
            // Can't put that in the static constructor, since it's not called if the user doesn't set any color...
            if ( MainColor == Colors.Transparent )
            {
                MainColor = Colors.Black;
            }
            if ( AccentColor == Colors.Transparent )
            {
                AccentColor = Colors.Orange;
            }
        }
        #endregion

        #region Private methods
        private static void SetMainColor( Color newColor )
        {
            foreach ( var dic in _mainColorDictionaries[newColor] )
            {
                Application.Current.Resources.MergedDictionaries.Add( dic );
            }

            if ( _mainColorDictionaries.ContainsKey( MainColor ) )
            {
                foreach ( var dic in _mainColorDictionaries[MainColor].Where( d => Application.Current.Resources.MergedDictionaries.Contains( d ) ) )
                {
                    Application.Current.Resources.MergedDictionaries.Remove( dic );
                }
            }
        }

        private static void SetAccentColor( Color color )
        {
            var brush = new SolidColorBrush( color );

            var lightColor = TransformColor( color, 1.1 );
            lightColor.A = 90;
            var lightBrush = new SolidColorBrush( lightColor );
            var darkBrush = new SolidColorBrush( TransformColor( color, 0.75 ) );
            var veryDarkBrush = new SolidColorBrush( TransformColor( color, 0.6 ) );

            brush.Freeze();
            lightBrush.Freeze();
            darkBrush.Freeze();
            veryDarkBrush.Freeze();

            Application.Current.Resources["SelectedItemForegroundBrush"] = brush;
            Application.Current.Resources["TabHoverForegroundBrush"] = darkBrush;

            Application.Current.Resources["AccentBrush"] = brush;
            Application.Current.Resources["LightAccentBrush"] = lightBrush;
            Application.Current.Resources["DarkAccentBrush"] = darkBrush;
            Application.Current.Resources["VeryDarkAccentBrush"] = veryDarkBrush;
        }

        private static Color TransformColor( Color color, double multiplier )
        {
            return Color.FromRgb( (byte) Math.Min( 255, color.R * multiplier ),
                                  (byte) Math.Min( 255, color.G * multiplier ),
                                  (byte) Math.Min( 255, color.B * multiplier ) );
        }
        #endregion
    }
}