// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Globalization;
using System.Windows.Data;

namespace CommonStuff.Converters
{
    /// <summary>
    /// Returns true if both dates passed as parameters are equal when formatted using the format passed as third parameter.
    /// </summary>
    public sealed class EqualDatesMultiConverter : SelfExtension<EqualDatesMultiConverter>, IMultiValueConverter
    {
        public object Convert( object[] values, Type targetType, object parameter, CultureInfo culture )
        {
            DateTime? date1 = values[0] as DateTime?;
            DateTime? date2 = values[1] as DateTime?;
            string format = values[2] as string;

            if ( date1.HasValue && date2.HasValue && format != null )
            {
                return date1.Value.ToString( format ) == date2.Value.ToString( format );
            }
            return false;
        }

        public object[] ConvertBack( object value, Type[] targetTypes, object parameter, CultureInfo culture )
        {
            throw new NotSupportedException();
        }
    }
}