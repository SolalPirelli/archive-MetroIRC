// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CommonStuff.Converters
{
    /// <summary>
    /// Unlike the BooleanToVisibilityConverter, this returns Collapsed if value is true and Visible otherwise.
    /// </summary>
    public sealed class BooleanToOppositeVisibilityConverter : SelfExtension<BooleanToOppositeVisibilityConverter>, IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
        {
            return (bool) value ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
        {
            return (Visibility) value != Visibility.Visible;
        }
    }
}