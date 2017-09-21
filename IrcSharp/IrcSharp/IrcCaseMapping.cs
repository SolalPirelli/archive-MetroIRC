// Copyright (C) 2013, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

namespace IrcSharp
{
    /// <summary>
    /// The possible case mappings applied by IRC networks.
    /// </summary>
    public abstract class IrcCaseMapping
    {
        /// <summary>
        /// Gets the default case mapping.
        /// </summary>
        /// <remarks>IRC# will default to <see cref="Rfc1459"/>.</remarks>
        public static IrcCaseMapping Default { get; private set; }

        /// <summary>
        /// Gets the ASCII case mapping.
        /// </summary>
        public static IrcCaseMapping Ascii { get; private set; }

        /// <summary>
        /// Gets the case mapping defined by RFC 1459; 
        /// like ASCII, but the { } | ~ characters. are considered to be the lowercase versions of [ ] \ ^ respectively.
        /// </summary>
        public static IrcCaseMapping Rfc1459 { get; private set; }

        /// <summary>
        /// Gets the case mapping actually defined by RFC 1459;
        /// like ASCII, but the { } | characters. are considered to be the lowercase versions of [ ] \ respectively.
        /// RFC 1459 does not say anything about the ~ and ^ characters, but almost all IRC networks consider them to be equal.
        /// </summary>
        public static IrcCaseMapping StrictRfc1459 { get; private set; }

        static IrcCaseMapping()
        {
            Default = new AsciiCaseMapping();
            Ascii = new AsciiCaseMapping();
            Rfc1459 = new Rfc1459CaseMapping();
            StrictRfc1459 = new StrictRfc1459CaseMapping();
        }

        /// <summary>
        /// Indicates whether the two specified strings are considered to be equal on an <see cref="IrcNetwork"/> using the <see cref="IrcCaseMapping"/>.
        /// </summary>
        /// <param name="s1">The first string.</param>
        /// <param name="s2">The second string.</param>
        /// <returns>A value indicating whether the two specified strings are considered to be equal on an <see cref="IrcNetwork"/> using the <see cref="IrcCaseMapping"/>.</returns>
        public bool AreEqual( string s1, string s2 )
        {
            return this.ToLower( s1 ) == this.ToLower( s2 );
        }

        /// <summary>
        /// Compares the two given strings, ignoring the case, using the <see cref="IrcCaseMapping"/>.
        /// </summary>
        /// <param name="strA">The first string.</param>
        /// <param name="strB">The second string.</param>
        /// <returns>An integer that is less than 0 if strA is less than strB, equal to 0 if strA is equal to strB and greater than 0 if strA is greater than strB.</returns>
        public int Compare( string strA, string strB )
        {
            return this.ToLower( strA ).CompareTo( this.ToLower( strB ) );
        }

        /// <summary>
        /// Gets the lowercase version of the specified string.
        /// </summary>
        internal abstract string ToLower( string s );

        /// <summary>
        /// The ASCII case mapping. It actually maps everything, including non-ASCII characters.
        /// </summary>
        private sealed class AsciiCaseMapping : IrcCaseMapping
        {
            internal override string ToLower( string s )
            {
                return s.ToLowerInvariant();
            }
        }

        /// <summary>
        /// The RFC 1459's suggested case mapping.
        /// </summary>
        private sealed class Rfc1459CaseMapping : IrcCaseMapping
        {
            internal override string ToLower( string s )
            {
                return s.ToLowerInvariant()
                        .Replace( '[', '{' )
                        .Replace( ']', '}' )
                        .Replace( '\\', '|' )
                        .Replace( '^', '~' );
            }
        }

        /// <summary>
        /// The case mapping actually defined by RFC 1459, even though there are no known implementations.
        /// </summary>
        private sealed class StrictRfc1459CaseMapping : IrcCaseMapping
        {
            internal override string ToLower( string s )
            {
                return s.ToLowerInvariant()
                        .Replace( '[', '{' )
                        .Replace( ']', '}' )
                        .Replace( '\\', '|' );
            }
        }
    }
}