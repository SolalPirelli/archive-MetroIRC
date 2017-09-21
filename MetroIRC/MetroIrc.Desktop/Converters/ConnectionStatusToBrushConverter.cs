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
    /// A converter that associates a connection status to a brush.
    /// </summary>
    public sealed class ConnectionStatusToBrushConverter : SelfExtension<ConnectionStatusToBrushConverter>, IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
        {
            return Locator.Get<IResourceService>().GetResource( "Status" + value + "Brush" );
        }

        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
        {
            throw new NotSupportedException();
        }
    }
}