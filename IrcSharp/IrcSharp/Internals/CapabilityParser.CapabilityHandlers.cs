using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace IrcSharp.Internals
{
    internal static partial class CapabilityParser
    {
        private static class CapabilityHandlers
        {
            /// <summary>
            /// The possible CASEMAPPING values and their meanings.
            /// </summary>
            private static readonly Tuple<string, IrcCaseMapping>[] CaseMappings = 
            {
                Tuple.Create( "ascii", IrcCaseMapping.Ascii ),
                Tuple.Create( "rfc1459", IrcCaseMapping.Rfc1459 ),
                Tuple.Create( "strict-rfc1459", IrcCaseMapping.StrictRfc1459 )
            };

            /// <summary>
            /// The available channel user mode chars and their meanings.
            /// </summary>
            private static readonly Dictionary<char, IrcChannelUserModes> ChannelUserModes = new Dictionary<char, IrcChannelUserModes>
            {
                { 'O', IrcChannelUserModes.Creator },
                { 'q', IrcChannelUserModes.Owner },
                { 'a', IrcChannelUserModes.Admin },
                { 'o', IrcChannelUserModes.Op },
                { 'h', IrcChannelUserModes.HalfOp },
                { 'v', IrcChannelUserModes.Voiced },
                { 'd', IrcChannelUserModes.Deaf }
            };

            // CASEMAPPING=(ascii | rfc1459 | strict-rfc1459)
            [CapabilityHandler( "casemapping" )]
            public static void CaseMapping( IrcNetworkParameters parameters, string value )
            {
                var match = CaseMappings.FirstOrDefault( t => value.Equals( t.Item1, StringComparison.OrdinalIgnoreCase ) );
                if ( match != null )
                {
                    parameters.CaseMapping = match.Item2;
                }
            }

            // CHANLIMIT=(<prefix>+:<limit>,)+
            // e.g. #:10,&:15 or &#:10
            [CapabilityHandler( "chanlimit" )]
            public static void ChanLimit( IrcNetworkParameters parameters, string value )
            {
                var limits = new Dictionary<IrcChannelKind, int>();
                var parts = value.Split( CapabilityParser.CollectionSeparator );
                foreach ( string part in parts )
                {
                    var split = part.Split( CapabilityParser.NestedValueSeparator );
                    int limit = int.Parse( split[1] );

                    foreach ( char c in split[0] )
                    {
                        if ( IrcNetworkParameters.DefaultChannelKinds.ContainsKey( c ) )
                        {
                            limits.Add( IrcNetworkParameters.DefaultChannelKinds[c], limit );
                        }
                    }
                }

                parameters.ChannelCountLimits = new ReadOnlyDictionary<IrcChannelKind, int>( limits );
            }

            // CHANMODES=<group A>,<group B>,<group C>,<group D>
            // A = "list" modes, with parameter to add or remove and without to list
            // B = modes with parameters
            // C = modes with parameters when adding, not when removing
            // D = modes without parameters
            [CapabilityHandler( "chanmodes" )]
            public static void ChanModes( IrcNetworkParameters parameters, string value )
            {
                var parts = value.Split( CapabilityParser.CollectionSeparator );
                var lists = new List<char>[4];
                for ( int n = 0; n < lists.Length; n++ )
                {
                    lists[n] = new List<char>();
                    foreach ( char c in parts[n] )
                    {
                        lists[n].Add( c );
                    }
                }

                parameters.ChannelModes.ListModes = new ReadOnlyCollection<char>( lists[0] );
                parameters.ChannelModes.ParameterizedModes = new ReadOnlyCollection<char>( lists[1] );
                parameters.ChannelModes.ParameterizedOnSetModes = new ReadOnlyCollection<char>( lists[2] );
                parameters.ChannelModes.ParameterlessModes = new ReadOnlyCollection<char>( lists[3] );
            }

            // CHANNELLEN=<length>
            [CapabilityHandler( "channellen" )]
            public static void ChannelLen( IrcNetworkParameters parameters, string value )
            {
                parameters.MaxChannelNameLength = int.Parse( value );
            }

            // CHANTYPES=<prefix>+
            // e.g. CHANTYPES=# or CHANTYPES=#&!+
            [CapabilityHandler( "chantypes" )]
            public static void ChanTypes( IrcNetworkParameters parameters, string value )
            {
                var kinds = new Dictionary<char, IrcChannelKind>();
                foreach ( char c in value )
                {
                    if ( IrcNetworkParameters.DefaultChannelKinds.ContainsKey( c ) )
                    {
                        kinds.Add( c, IrcNetworkParameters.DefaultChannelKinds[c] );
                    }
                    else
                    {
                        // yes, this happens...
                        kinds.Add( c, IrcChannelKind.Standard );
                    }
                }
                parameters.ChannelKinds = new ReadOnlyDictionary<char, IrcChannelKind>( kinds );
            }

            // EXCEPTS[=<char>]
            // default is 'e'
            [CapabilityHandler( "excepts", DefaultValue = "e" )]
            public static void Excepts( IrcNetworkParameters parameters, string value )
            {
                parameters.ChannelModes.BanExceptionMode = value[0];
                parameters.AreBanExceptionsEnabled = true;
            }

            // INVEX[=<char>]
            // default is 'I'
            [CapabilityHandler( "invex", DefaultValue = "I" )]
            public static void InvEx( IrcNetworkParameters parameters, string value )
            {
                parameters.ChannelModes.InviteExceptionMode = value[0];
                parameters.AreInviteExceptionsEnabled = true;
            }

            // KICKLEN=<num>
            [CapabilityHandler( "kicklen" )]
            public static void KickLen( IrcNetworkParameters parameters, string value )
            {
                parameters.MaxKickReasonLength = int.Parse( value );
            }

            // MAXCHANNELS=<num>
            // Superseded by CHANLIMIT
            [CapabilityHandler( "maxchannels" )]
            public static void MaxChannels( IrcNetworkParameters parameters, string value )
            {
                if ( parameters.ChannelCountLimits.Any( p => p.Value != int.MaxValue ) )
                {
                    return;
                }

                var dic = new Dictionary<IrcChannelKind, int>();
                int val = int.Parse( value );
                foreach ( var kind in parameters.ChannelKinds.Values )
                {
                    dic.Add( kind, val );
                }
                parameters.ChannelCountLimits = new ReadOnlyDictionary<IrcChannelKind, int>( dic );
            }

            // NETWORK=<name>
            [CapabilityHandler( "network" )]
            public static void Network( IrcNetworkParameters parameters, string value )
            {
                parameters.NetworkName = value;
            }

            // NICKLEN=<num>
            [CapabilityHandler( "nicklen" )]
            public static void NickLen( IrcNetworkParameters parameters, string value )
            {
                parameters.MaxNicknameLength = int.Parse( value );
            }

            // PREFIX=(<char>+)<prefix>+
            // e.g. PREFIX=(ov)@+ or PREFIX=(qaohv)~&@%+
            [CapabilityHandler( "prefix" )]
            public static void Prefix( IrcNetworkParameters parameters, string value )
            {
                var modes = new Dictionary<char, IrcChannelUserModes>();
                var prefixes = new Dictionary<char, IrcChannelUserModes>();
                var split = value.Substring( 1 ).Split( ')' );
                for ( int n = 0; n < split[0].Length; n++ )
                {
                    char modeChar = split[0][n];
                    char prefixChar = split[1][n];
                    if ( ChannelUserModes.ContainsKey( modeChar ) )
                    {
                        var mode = ChannelUserModes[modeChar];
                        modes.Add( modeChar, mode );
                        prefixes.Add( prefixChar, mode );
                    }
                    else
                    {
                        modes.Add( modeChar, IrcChannelUserModes.Unknown );
                        prefixes.Add( prefixChar, IrcChannelUserModes.Unknown );
                    }
                }

                parameters.ChannelModes.UserModes = new ReadOnlyDictionary<char, IrcChannelUserModes>( modes );
                parameters.ChannelModes.UserPrefixes = new ReadOnlyDictionary<char, IrcChannelUserModes>( prefixes );
            }

            // TOPICLEN=<int>
            [CapabilityHandler( "topiclen" )]
            public static void TopicLen( IrcNetworkParameters parameters, string value )
            {
                parameters.MaxTopicLength = int.Parse( value );
            }
        }
    }
}