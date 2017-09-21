// Copyright (C) 2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using BasicMvvm;
using CommonStuff;
using IrcSharp;

namespace MetroIrc.Desktop
{
    public static partial class MessageFormatter
    {
        private static class WordProcessors
        {
            [WordProcessor( "SmileyCondition" )]
            public static object Smiley( string word, IrcMessage containingMessage )
            {
                var image = (ImageSource) MessageFormatter.SmileyService.GetSmiley( word );
                return new Image
                {
                    Source = image,
                    Style = ResourceService.GetResource<Style>( "BitmapImageStyle" ),
                    ToolTip = word
                };
            }

            public static bool SmileyCondition( string word, IrcMessage containingMessage )
            {
                return MessageFormatter.Settings.ShowSmileys && MessageFormatter.SmileyService.HasSmiley( word );
            }


            [WordProcessor( "LinkCondition" )]
            public static object Link( string word, IrcMessage containingMessage )
            {
                // The explicit style declaration is needed for some reason
                var link = new Hyperlink( new Run( word ) ) { Style = ResourceService.GetResource<Style>( typeof( Hyperlink ) ) };
                link.Click += ( s, e ) =>
                {
                    Process.Start( word );
                };

                return link;
            }

            public static bool LinkCondition( string word, IrcMessage containingMessage )
            {
                return MessageFormatter.Settings.TransformLinks && LinksPrefixes.Any( pref => word.BeginsWith( pref ) );
            }


            [WordProcessor( "ChannelCondition" )]
            public static object Channel( string word, IrcMessage containingMessage )
            {
                // The explicit style declaration is needed for some reason
                var link = new Hyperlink( new Run( word ) ) { Style = ResourceService.GetResource<Style>( typeof( Hyperlink ) ) };
                link.Click += ( s, e ) =>
                {
                    var channel = containingMessage.Network.GetChannel( word );
                    Messenger.Send( new JoinChannelMessage( channel, string.Empty ) );
                };

                return link;
            }

            public static bool ChannelCondition( string word, IrcMessage containingMessage )
            {
                return MessageFormatter.Settings.TransformChans
                    && ( ( containingMessage != null && containingMessage.Network.Parameters.IsCommonChannelName( word ) )
                      || ( word[0] == '#' ) );
            }


            [WordProcessor( "HLCondition" )]
            public static object HL( string word, IrcMessage containingMessage )
            {
                return new Run( word ) { Style = ResourceService.GetResource<Style>( "HighlightedRun" ) };
            }

            public static bool HLCondition( string word, IrcMessage containingMessage )
            {
                if ( containingMessage == null || containingMessage.Direction == MessageDirection.Sent )
                {
                    return false;
                }

                if ( MessageFormatter.Settings.NotifyOnNickname && word == containingMessage.Network.CurrentUser.Nickname )
                {
                    return true;
                }

                return MessageFormatter.Settings.NotifyWords.Contains( word );
            }
        }
    }
}