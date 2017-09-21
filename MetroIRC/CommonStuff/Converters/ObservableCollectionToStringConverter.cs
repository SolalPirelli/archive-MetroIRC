// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Data;

namespace CommonStuff.Converters
{
    public sealed class ObservableCollectionToStringConverter : SelfExtension<ObservableCollectionToStringConverter>, IValueConverter
    {
        private const char Separator = ' ';
        private static readonly char[] SeparatorArray = new[] { Separator };

        public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
        {
            var collection = (IEnumerable<string>) value;
            return string.Join( Separator.ToString(), collection );
        }

        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
        {
            string s = (string) value;
            return new ObservableCollection<string>( s.Split( SeparatorArray, StringSplitOptions.RemoveEmptyEntries ) );
        }
    }
}