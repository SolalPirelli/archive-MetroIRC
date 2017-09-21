// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Globalization;
using System.Windows.Data;
using BasicMvvm;
using CommonStuff;
using MetroIrc.Services;

namespace MetroIrc.Desktop.Converters
{
    /// <summary>
    /// Converts from an enum value to its localizable equivalent, given by a <see cref="LocalizableEnumMemberAttribute"/>.
    /// </summary>
    public sealed class EnumTranslationConverter : SelfExtension<EnumTranslationConverter>, IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
        {
            string group = value.GetType().Name;
            string key = value.ToString();
            return Locator.Get<ITranslationService>().Translate( group, key );
        }

        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
        {
            throw new NotSupportedException();
        }
    }
}