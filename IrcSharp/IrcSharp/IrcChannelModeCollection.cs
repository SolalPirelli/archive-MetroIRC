// Copyright (C) 2013, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using IrcSharp.Internals;
using IrcSharp.Validation;

namespace IrcSharp
{
    /// <summary>
    /// Contains all channel modes declared by a network, sorted by kind.
    /// </summary>
    /// <remarks>
    /// While the available modes are part of the <see cref="IrcNetworkParameters"/>, their meaning is not.
    /// IRC# associates channel user modes with their meanings because all meanings are unique across all known networks,
    /// but almost every channel mode has multiple meanings.
    /// 
    /// This class attempts to parse any non-ambiguous mode message; if there is ambiguity, it doesn't return anything.
    /// </remarks>
    public sealed class IrcChannelModeCollection
    {
        #region Public properties
        /// <summary>
        /// Gets the modes that correspond to lists;
        /// they may be used with a parameter to add or remove an item,
        /// or without a parameter to view the list.
        /// </summary>
        public ReadOnlyCollection<char> ListModes { get; internal set; }

        /// <summary>
        /// Gets the modes that always take a parameter.
        /// This list does not include the <see cref="UserModes"/>.
        /// </summary>
        public ReadOnlyCollection<char> ParameterizedModes { get; internal set; }

        /// <summary>
        /// Gets the modes that take a parameter when they are added, but not when they are removed.
        /// </summary>
        public ReadOnlyCollection<char> ParameterizedOnSetModes { get; internal set; }

        /// <summary>
        /// Gets the modes that never take a parameter.
        /// Modes that are not defined in any other list belong here.
        /// </summary>
        public ReadOnlyCollection<char> ParameterlessModes { get; internal set; }

        /// <summary>
        /// Gets the channel user modes and their meaning.
        /// </summary>
        public ReadOnlyDictionary<char, IrcChannelUserModes> UserModes { get; internal set; }

        /// <summary>
        /// Gets the channel user modes and their prefixes.
        /// </summary>
        public ReadOnlyDictionary<char, IrcChannelUserModes> UserPrefixes { get; internal set; }

        /// <summary>
        /// Gets the char used for ban exceptions.
        /// If set, it's included in the <see cref="ListModes"/>.
        /// </summary>
        public char BanExceptionMode { get; internal set; }

        /// <summary>
        /// Gets the char used for invite exceptions.
        /// If set, it's included in the <see cref="ListModes"/>.
        /// </summary>
        public char InviteExceptionMode { get; internal set; }
        #endregion

        internal IrcChannelModeCollection()
        {
            this.ListModes = this.ParameterizedModes = this.ParameterizedOnSetModes = this.ParameterlessModes
                = new ReadOnlyCollection<char>( new List<char>() );
            this.UserModes = this.UserPrefixes
                = new ReadOnlyDictionary<char, IrcChannelUserModes>( new Dictionary<char, IrcChannelUserModes>() );
        }

        #region Public methods
        /// <summary>
        /// Splits the given channel mode message.
        /// </summary>
        /// <param name="modeMessage">The channel mode message.</param>
        /// <returns>A sequence of <see cref="IrcMode"/> objects, or null if the message is corrupt or ambiguous.</returns>
        public IEnumerable<IrcMode> SplitMode( string modeMessage )
        {
            Validate.IsNotNull( modeMessage, "modeMessage" );

            var result = new List<IrcMode>();

            foreach ( var tup in ParseMode( modeMessage ) )
            {
                var use = this.GetArgumentUse( tup.Item1, tup.Item2 );
                if ( use == ArgumentUse.Error )
                {
                    // returning null is better than an empty collection here
                    // since an empty collection is a valid return value
                    // this point will only be reached if the message is corrupt or ambiguous
                    return null;
                }

                int argIndex = 0;
                foreach ( var mode in tup.Item1 )
                {
                    string argument = null;
                    if ( this.NeedsArgument( mode ) || ( this.MayNeedArgument( mode ) && use == ArgumentUse.AllModes ) )
                    {
                        argument = tup.Item2[argIndex];
                        argIndex++;
                    }
                    result.Add( new IrcMode( mode, argument ) );
                }
            }

            return result;
        }
        #endregion

        #region Public static methods
        /// <summary>
        /// Gets the default <see cref="IrcChannelModeCollection"/>, as defined in the IRC RFCs.
        /// </summary>
        public static IrcChannelModeCollection GetDefault()
        {
            return new IrcChannelModeCollection
            {
                ListModes = new ReadOnlyCollection<char>( new[] { 'b', 'e', 'I' } ),
                ParameterizedModes = new ReadOnlyCollection<char>( new[] { 'O', 'o', 'v', 'h' } ),
                ParameterizedOnSetModes = new ReadOnlyCollection<char>( new[] { 'k', 'l' } ),
                ParameterlessModes = new ReadOnlyCollection<char>( new[] { 'a', 'i', 'm', 'n', 'p', 'q', 'r', 's', 't' } ),
                UserModes = new ReadOnlyDictionary<char, IrcChannelUserModes>( new Dictionary<char, IrcChannelUserModes>
                {
                    { 'O', IrcChannelUserModes.Creator },
                    { 'o', IrcChannelUserModes.Op },
                    { 'h', IrcChannelUserModes.HalfOp },
                    { 'v', IrcChannelUserModes.Voiced }
                } ),
                UserPrefixes = new ReadOnlyDictionary<char, IrcChannelUserModes>( new Dictionary<char, IrcChannelUserModes>
                {
                    { '@', IrcChannelUserModes.Op },
                    { '%', IrcChannelUserModes.HalfOp },
                    { '+', IrcChannelUserModes.Voiced }
                } ),
                BanExceptionMode = 'e',
                InviteExceptionMode = 'I'
            };
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Indicates whether the specified modes and arguments are valid, that is, 
        /// all modes needing arguments have arguments and there are no unmatched arguments.
        /// </summary>
        private ArgumentUse GetArgumentUse( IList<string> modes, IList<string> arguments )
        {
            int neededCount = modes.Count( this.NeedsArgument );
            // If there are both list modes with arguments and list modes without, it's impossible to know what is what.
            return neededCount == arguments.Count ? ArgumentUse.OnlyWhenNeeded
                 : neededCount + modes.Count( this.MayNeedArgument ) == arguments.Count ? ArgumentUse.AllModes
                 : ArgumentUse.Error;
        }

        /// <summary>
        /// Indicates whether the specified modifier/mode string needs an argument.
        /// </summary>
        private bool NeedsArgument( string modeWithModifier )
        {
            return this.ParameterizedModes.Contains( modeWithModifier[1] )
                || this.UserModes.ContainsKey( modeWithModifier[1] )
                || ( modeWithModifier[0] == IrcMode.PositiveModifier && this.ParameterizedOnSetModes.Contains( modeWithModifier[1] ) )
                || ( this.ListModes.Contains( modeWithModifier[1] ) && modeWithModifier[0] == IrcMode.NegativeModifier );

        }

        /// <summary>
        /// Indicates whether the specified modifier/mode string may need an argument.
        /// </summary>
        private bool MayNeedArgument( string modeWithModifier )
        {
            return this.ListModes.Contains( modeWithModifier[1] )
                && modeWithModifier[0] == IrcMode.PositiveModifier;
        }
        #endregion

        #region Private static methods
        /// <summary>
        /// Splits the specified mode message into a sequence of modes/arguments tuples.
        /// </summary>
        /// <remarks>
        /// The reason for the odd return type is that some mode messages are ambiguous if you do not include the order,
        /// e.g. "+b Alice +e". Without order, it's impossible to guess. With order, it's obvious "Bob" is "+b"'s argument
        /// while "+e" doesn't have an argument.
        /// </remarks>
        private static IEnumerable<Tuple<List<string>, List<string>>> ParseMode( string modeMessage )
        {
            char currentModifier = IrcMode.PositiveModifier;
            var modes = new List<string>();
            var arguments = new List<string>();

            foreach ( string part in modeMessage.Split( IrcUtils.MessagePartsSeparatorArray, StringSplitOptions.RemoveEmptyEntries ) )
            {
                if ( IrcMode.Modifiers.Contains( part[0] ) )
                {
                    if ( modes.Count != 0 )
                    {
                        yield return Tuple.Create( modes.ToList(), arguments.ToList() ); // ToList is an easy way to copy them
                        modes.Clear();
                        arguments.Clear();
                    }
                    foreach ( char c in part )
                    {
                        if ( IrcMode.Modifiers.Contains( c ) )
                        {
                            currentModifier = c;
                        }
                        else
                        {
                            modes.Add( currentModifier + c.ToString() );
                        }
                    }
                }
                else
                {
                    arguments.Add( part );
                }
            }

            if ( modes.Count != 0 )
            {
                yield return Tuple.Create( modes, arguments );
            }
        }
        #endregion

        #region Nested enum
        /// <summary>
        /// How argument should be used.
        /// </summary>
        private enum ArgumentUse
        {
            AllModes,
            OnlyWhenNeeded,
            Error
        }
        #endregion
    }
}