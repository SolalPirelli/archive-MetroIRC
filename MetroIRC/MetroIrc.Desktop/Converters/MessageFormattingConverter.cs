// Copyright (C) 2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.using System.Text;

using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Documents;
using CommonStuff;

namespace MetroIrc.Desktop.Converters
{
    public sealed class MessageFormattingConverter : SelfExtension<MessageFormattingConverter>, IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
        {
            var ircMessage = value as IrcMessage;
            if ( ircMessage == null )
            {
                if ( value == null )
                {
                    return new FlowDocument();
                }

                return MessageFormatter.FormatMessage( value.ToString() );
            }
            else
            {
                return MessageFormatter.FormatIrcMessage( ircMessage );
            }
        }

        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
        {
            throw new NotSupportedException();
        }
    }
}