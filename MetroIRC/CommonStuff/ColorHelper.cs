// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace CommonStuff
{
    public static class ColorHelper
    {
        private static Dictionary<Color, string> _colors = new Dictionary<Color, string>();

        public static string GetColorName( Color color )
        {
            if ( _colors.Count == 0 )
            {
                foreach ( var prop in typeof( Colors ).GetProperties() )
                {
                    var c = (Color) prop.GetValue( null, null );
                    // Some colors have the same value but different names
                    if ( !_colors.ContainsKey( c ) )
                    {
                        _colors.Add( c, prop.Name );
                    }
                }
            }

            if ( _colors.ContainsKey( color ) )
            {
                return _colors[color];
            }

            throw new ArgumentException( "The provided Color is not named." );
        }
    }
}