// Copyright (C) 2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.using System.Text;

using System;
using System.Globalization;
using System.Windows.Data;
using BasicMvvm;
using CommonStuff;
using MetroIrc.Services;

namespace MetroIrc.Desktop.Converters
{
    public sealed class DialogButtonToStringConverter :SelfExtension<DialogButtonToStringConverter>, IValueConverter
    {
        private const string GroupName = "DialogButton";

        public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
        {
            return Locator.Get<ITranslationService>().Translate( GroupName, value.ToString() );
        }

        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
        {
            throw new NotSupportedException();
        }
    }
}