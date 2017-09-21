// Copyright (C) 2011-2013, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using CommonStuff;
using IrcSharp;

namespace MetroIrc.Desktop.Converters
{
    /// <summary>
    /// A converter that converts a list of IrcChannelInfos to a string, and back.
    /// </summary>
    public sealed class ChannelInfoListToStringConverter : SelfExtension<ChannelInfoListToStringConverter>, IValueConverter
    {
        // This converter uses the ":" char as a key/chan separator because it's forbidden in chan names for obvious reasons
        private static char[] ChanSeparators = new char[] { ',', ' ' };
        private static string InsertedChanSeparator = ", ";
        private static char KeyChanSeparator = ':';

        public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
        {
            var list = value as List<IrcChannelInfo>;
            if ( list == null )
            {
                return null;
            }

            return string.Join( InsertedChanSeparator, list.Select( chan => chan.Name + ( chan.Key.HasText() ? KeyChanSeparator + chan.Key : string.Empty ) ) );
        }

        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
        {
            var s = value as string;
            if ( s == null )
            {
                return null;
            }

            var chans = new List<IrcChannelInfo>();

            foreach ( var part in s.Split( ChanSeparators, StringSplitOptions.RemoveEmptyEntries ) )
            {
                if ( part.Contains( KeyChanSeparator ) )
                {
                    var tab = part.Split( KeyChanSeparator );
                    chans.Add( new IrcChannelInfo( tab[0], tab[1] ) );
                }
                else
                {
                    chans.Add( new IrcChannelInfo( part ) );
                }
            }

            return chans;
        }
    }
}