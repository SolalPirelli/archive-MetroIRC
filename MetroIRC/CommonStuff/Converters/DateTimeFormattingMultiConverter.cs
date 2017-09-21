// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Globalization;
using System.Windows.Data;

namespace CommonStuff.Converters
{
    public sealed class DateTimeFormattingMultiConverter : SelfExtension<DateTimeFormattingMultiConverter>, IMultiValueConverter
    {
        public object Convert( object[] values, Type targetType, object parameter, CultureInfo culture )
        {
            var date = (DateTime) values[0];
            string format = (string) values[1];
            return date.ToString( format );
        }

        public object[] ConvertBack( object value, Type[] targetTypes, object parameter, CultureInfo culture )
        {
            throw new NotSupportedException();
        }
    }
}