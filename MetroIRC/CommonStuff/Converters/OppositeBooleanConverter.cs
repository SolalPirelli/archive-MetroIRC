﻿// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Globalization;
using System.Windows.Data;

namespace CommonStuff.Converters
{
    public sealed class OppositeBooleanConverter : SelfExtension<OppositeBooleanConverter>, IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
        {
            return !(bool) value;
        }

        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
        {
            return !(bool) value;
        }
    }
}