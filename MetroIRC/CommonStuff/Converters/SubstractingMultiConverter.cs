// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Globalization;
using System.Windows.Data;

namespace CommonStuff.Converters
{
    /// <summary>
    /// Returns the first value minus the second value.
    /// </summary>
    public sealed class SubstractingMultiConverter : SelfExtension<SubstractingMultiConverter>, IMultiValueConverter
    {
        public object Convert( object[] values, Type targetType, object parameter, CultureInfo culture )
        {
            double value1 = (double) values[0];
            double value2 = (double) values[1];
            return value1 - value2;
        }

        public object[] ConvertBack( object value, Type[] targetTypes, object parameter, CultureInfo culture )
        {
            throw new NotSupportedException();
        }
    }
}