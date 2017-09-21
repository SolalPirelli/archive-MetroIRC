// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace CommonStuff.Converters
{
    public sealed class EqualStringsMultiConverter:SelfExtension<EqualStringsMultiConverter>, IMultiValueConverter
    {
        public object Convert( object[] values, Type targetType, object parameter, CultureInfo culture )
        {
            var strings = values.Select( o => o.ToString() ).ToArray();
            for ( int n = 1; n < strings.Length; n++ )
            {
                if ( strings[n - 1] != strings[n] )
                {
                    return false;
                }
            }
            return true;
        }

        public object[] ConvertBack( object value, Type[] targetTypes, object parameter, CultureInfo culture )
        {
            throw new NotSupportedException();
        }
    }
}