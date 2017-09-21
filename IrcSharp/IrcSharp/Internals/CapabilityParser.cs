using System;
using System.Collections.Generic;
using System.Linq;
// parameters, value
using CapabilityHandler = System.Action<IrcSharp.IrcNetworkParameters, string>;

namespace IrcSharp.Internals
{
    internal static partial class CapabilityParser
    {
        #region Constants
        /// <summary>
        /// The separator between the name and the value in a capability token.
        /// </summary>
        private const char ValueSeparator = '=';

        /// <summary>
        /// The separator for collections inside a parameter.
        /// </summary>
        private const char CollectionSeparator = ',';

        /// <summary>
        /// The separator for values inside values.
        /// </summary>
        private const char NestedValueSeparator = ':';
        #endregion

        #region Private members
        /// <summary>
        /// The handlers for capabilities.
        /// </summary>
        private static Tuple<CapabilityHandlerAttribute, CapabilityHandler>[] _handlers;
        #endregion

        static CapabilityParser()
        {
            InitializeHandlers();
        }

        #region Public methods
        /// <summary>
        /// Parses capability (ISUPPORT) tokens and sets the appropriate properties of the specified <see cref="IrcNetworkParameters"/>.
        /// </summary>
        public static void Parse( IrcNetworkParameters networkParams, IEnumerable<string> tokens )
        {
            foreach ( var part in tokens )
            {
                ParseSingle( networkParams, part );
            }
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Parses one ISUPPORT token and sets the appropriate properties of the specified <see cref="IrcNetworkParameters"/>.
        /// </summary>
        private static void ParseSingle( IrcNetworkParameters networkParams, string part )
        {
            string capability, param = null;
            if ( part.Contains( ValueSeparator.ToString() ) )
            {
                var split = part.Split( ValueSeparator );
                capability = split[0];
                param = split[1];
            }
            else
            {
                capability = part;
            }

            var match = _handlers.FirstOrDefault( p => capability.Equals( p.Item1.Capability, StringComparison.OrdinalIgnoreCase ) );
            if ( match != null && ( match.Item1.DefaultValue.HasText() || match.Item1.NeedsParameter == ( param != null ) ) )
            {
                if ( param == null )
                {
                    param = match.Item1.DefaultValue;
                }
                match.Item2( networkParams, param );
            }
        }

        /// <summary>
        /// Initializes the capability handlers.
        /// </summary>
        private static void InitializeHandlers()
        {
            _handlers = ReflectionHelper.GetAttributedMethods<CapabilityHandlerAttribute>( typeof( CapabilityParser.CapabilityHandlers ) )
                                        .Select( t => Tuple.Create( t.Item1,
                                                                    t.Item2.GetStaticDelegate<CapabilityHandler>() ) )
                                        .ToArray();
        }
        #endregion
    }
}