// Copyright (C) 2011-2013, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System.Collections.Generic;
using System.Linq;
using BasicMvvm;
using IrcSharp;
using MetroIrc.Internals;
using MetroIrc.Services;

/* Translations used:
 *      ChannelMode.Default
 *      ChannelMode.SetBy
 *      
 *      PositiveChannelModes.<mode>   
 *      NegativeChannelModes.<mode>
 *      
 *      ChannelUserModes.+
 *      ChannelUserModes.-
 *      ChannelUserModes.<mode>
 *      
 *      UserMode.Default
 *      UserMode.SetBy
 *      
 *      PositiveUserModes.<mode>   
 *      NegativeUserModes.<mode>
 */

namespace MetroIrc
{
    /// <summary>
    /// A helper class to translate IRC mode messages into human-readable modes.
    /// </summary>
    internal static class IrcModeTranslator
    {
        private static ITranslationService _translator = Locator.Get<ITranslationService>();

        /// <summary>
        /// Translates a channel mode message.
        /// </summary>
        public static IEnumerable<string> TranslateChannelMode( IrcUser setter, string modeString )
        {
            var split = setter.Network.Parameters.ChannelModes.SplitMode( modeString );
            if ( split.Any() )
            {
                return split.Select( m => TranslateOneChannelMode( setter, m ) );
            }

            string defaultVal = _translator.Translate( "ChannelMode", "Default", modeString );
            string suffix = _translator.Translate( "ChannelMode", "SetBy", setter.Nickname );
            return new[] { defaultVal + suffix };
        }

        /// <summary>
        /// Translates an user mode message.
        /// </summary>
        public static IEnumerable<string> TranslateUserMode( IrcUser user, IrcUser setter, string modeString )
        {
            var split = IrcUserModes.SplitMode( modeString );
            if ( split.Any() )
            {
                return split.Select( m => TranslateOneUserMode( user, setter, m ) );
            }

            string defaultVal = _translator.Translate( "UserMode", "Default", user.Nickname, modeString );
            string suffix = GetUserModeSuffix( user, setter );
            return new[] { defaultVal + suffix };
        }

        private static string TranslateOneChannelMode( IrcUser setter, IrcMode mode )
        {
            if ( setter.Network.Parameters.ChannelModes.UserModes.ContainsKey( mode.Flag ) && mode.Argument.HasText() )
            {
                string formatKey = mode.IsAdded ? "+" : "-";
                string arg = _translator.Translate( "ChannelUserModes", mode.Flag.ToString() );
                return _translator.Translate( "ChannelUserModes", formatKey, setter.Nickname, arg, mode.Argument );
            }

            string key = mode.Flag.ToString();
            string suffix = _translator.Translate( "ChannelMode", "SetBy", setter.Nickname );
            string group = mode.IsAdded ? "PositiveChannelModes" : "NegativeChannelModes";

            if ( _translator.CanTranslate( group, key ) )
            {
                return _translator.Translate( group, key, mode.Argument ) + suffix;
            }

            string modeString = ( mode.IsAdded ? "+" : "-" ) + mode.Flag + " " + mode.Argument;
            return _translator.Translate( "ChannelMode", "Default", modeString ) + suffix;
        }

        private static string TranslateOneUserMode( IrcUser user, IrcUser setter, IrcMode mode )
        {
            string suffix = GetUserModeSuffix( user, setter );
            string group = mode.IsAdded ? "PositiveUserModes" : "NegativeUserModes";
            string key = mode.Flag.ToString();

            if ( _translator.CanTranslate( group, key ) )
            {
                return _translator.Translate( group, key, user.Nickname ) + suffix;
            }
            else
            {
                string text = ( mode.IsAdded ? "+" : "-" ) + mode.Flag;
                return _translator.Translate( "UserMode", "Default", user.Nickname, text ) + suffix;
            }
        }

        private static string GetUserModeSuffix( IrcUser user, IrcUser setter )
        {
            if ( user == setter )
            {
                return string.Empty;
            }
            return _translator.Translate( "UserMode", "SetBy", setter.Nickname );
        }
    }
}