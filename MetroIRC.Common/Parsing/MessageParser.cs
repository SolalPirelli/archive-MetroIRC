// Copyright (C) 2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MetroIrc.Parsing
{
    public static class MessageParser
    {
        #region Constants/readonly
        private const char ColorTag = '\x03';
        private static Dictionary<char, TextPartKind> SimpleTags = new Dictionary<char, TextPartKind>
            {
                { '\x16', TextPartKind.ReverseColorsTag },
                { '\x02', TextPartKind.BoldTag },
                { '\x1f', TextPartKind.UnderlineTag },
                { '\x0f', TextPartKind.ResetFormattingTag }
            };

        private const char ColorsSeparator = ',';
        private const char ChannelPrefix = '#';

        private const int ColorsMaxDigitCount = 2;
        private const int TagMaxLength = 6; // ColorTag + "XX,YY"
        #endregion

        /// <summary>
        /// Parses the specified message, using an exclude filter for words whose punctuation must not be removed if necessary.
        /// </summary>
        public static IEnumerable<TextContainer> Parse( string text, Func<string, bool> excludeFilter = null )
        {
            var state = new ParseState();

            foreach ( var part in SplitParts( text, excludeFilter ) )
            {
                if ( part.Kind == TextPartKind.Word || part.Kind == TextPartKind.Punctuation || part.Kind == TextPartKind.Space )
                {
                    state.CurrentParts.Add( part );
                }
                else
                {
                    if ( state.CurrentParts.Any() )
                    {
                        yield return CreateContainer( state );
                        state.CurrentParts.Clear();
                    }

                    ChangeState( state, part );
                }
            }

            if ( state.CurrentParts.Any() )
            {
                yield return CreateContainer( state );
            }
        }

        /// <summary>
        /// Checks whether the specified text contains the specified word.
        /// </summary>
        public static bool Contains( string text, string word )
        {
            return SplitParts( text ).Any( p => p.Content == word );
        }

        #region Private methods
        /// <summary>
        /// Parses text and returns the text parts.
        /// </summary>
        private static IEnumerable<TextPart> SplitParts( string text, Func<string, bool> excludeFilter = null )
        {
            var parts = new List<TextPart>();
            var builder = new StringBuilder();
            var kind = TextPartKind.None;

            Action addLastWord = () =>
            {
                if ( builder.Length > 0 )
                {
                    string word = builder.ToString();
                    if ( kind != TextPartKind.Word || ( excludeFilter != null && excludeFilter( word ) ) )
                    {
                        parts.Add( new TextPart( kind, word ) );
                    }
                    else
                    {
                        parts.AddRange( SeparatePunctuation( word ) );
                    }
                    builder.Clear();
                }
            };

            for ( int n = 0; n < text.Length; n++ )
            {
                if ( IsFormatTagBeginning( text[n] ) )
                {
                    int minLength = Math.Min( text.Length - n, TagMaxLength );
                    string possibleTag = text.Substring( n, minLength );
                    var tags = GetFormatTag( possibleTag );
                    // skip the rest of the tags
                    n += tags.Sum( t => t.GetLength() ) - 1;

                    addLastWord();

                    parts.AddRange( tags );
                }
                else
                {
                    if ( ( char.IsWhiteSpace( text[n] ) && kind != TextPartKind.Space ) || kind != TextPartKind.Word )
                    {
                        addLastWord();
                        kind = char.IsWhiteSpace( text[n] ) ? TextPartKind.Space : TextPartKind.Word;
                    }
                    builder.Append( text[n] );
                }
            }

            addLastWord();

            return parts;
        }


        /// <summary>
        /// Indicates whether the given char is considered to be punctuation.
        /// </summary>
        private static bool IsPunctuation( char c )
        {
            return char.IsPunctuation( c ) && c != ChannelPrefix;
        }

        /// <summary>
        /// Indicates whether the char is the beginning of a format tag.
        /// </summary>
        private static bool IsFormatTagBeginning( char c )
        {
            return c == ColorTag || SimpleTags.ContainsKey( c );
        }

        /// <summary>
        /// Gets a tag from a text beginning with it.
        /// </summary>
        private static IEnumerable<TextPart> GetFormatTag( string text )
        {
            if ( text[0] == ColorTag )
            {
                string tagText = SeparateColorTag( text );
                foreach ( var tag in GetColorTags( tagText ) )
                {
                    yield return tag;
                }
                yield break;
            }

            yield return new TextPart( SimpleTags[text[0]] );
        }

        /// <summary>
        /// Gets a color tag from a text beginning with it.
        /// </summary>
        private static IEnumerable<TextPart> GetColorTags( string tagText )
        {
            if ( tagText.Length == 1 )
            {
                yield return new TextPart( TextPartKind.EndColorTag );
                yield break;
            }

            int[] nums = tagText.Substring( 1 )
                                .Split( ColorsSeparator )
                                .Select( s => int.Parse( s ) + 1 ) // +1 since (TextColor) 0 is Default
                                .ToArray();

            if ( Enum.IsDefined( typeof( TextColor ), nums[0] ) )
            {
                yield return new TextPart( TextPartKind.ForegroundColorTag, (TextColor) nums[0] );
            }

            if ( nums.Length > 1 && Enum.IsDefined( typeof( TextColor ), nums[1] ) )
            {
                yield return new TextPart( TextPartKind.BackgroundColorTag, (TextColor) nums[1] );
            }
        }

        /// <summary>
        /// Gets the color tag and its parameters as a string from a text beginning with it.
        /// </summary>
        private static string SeparateColorTag( string text )
        {
            bool foundSeparator = false;
            int digitCount = 0;

            int length = 1;
            for ( length = 1; length < text.Length; length++ )
            {
                if ( text[length] == ColorsSeparator )
                {
                    if ( foundSeparator || digitCount == 0 ) // only one allowed / there must be digits
                    {
                        break;
                    }
                    foundSeparator = true;
                    digitCount = 0;
                }
                else if ( char.IsDigit( text[length] ) )
                {
                    if ( digitCount == ColorsMaxDigitCount ) // color space is 0-99
                    {
                        break;
                    }
                    digitCount++;
                }
                else
                {
                    break;
                }
            }

            if ( text[length - 1] == ColorsSeparator )
            {
                length--; // in case a tag is followed by a comma not part of it, e.g. TAG22,lalala
            }

            return text.Substring( 0, length );
        }

        /// <summary>
        /// Returns {punctuation before, word, punctuation after}.
        /// </summary>
        private static IEnumerable<TextPart> SeparatePunctuation( string word )
        {
            int beforeIndex = 0, afterIndex = 0, n = 0;

            while ( n < word.Length && IsPunctuation( word[n] ) )
            {
                n++;
            }
            beforeIndex = n;

            n = word.Length - 1;
            while ( n > beforeIndex && IsPunctuation( word[n] ) )
            {
                n--;
            }
            afterIndex = n + 1;

            if ( beforeIndex > 0 )
            {
                yield return new TextPart( TextPartKind.Punctuation, word.Substring( 0, beforeIndex ) );
            }

            int length = afterIndex - beforeIndex;
            if ( length > 0 )
            {
                yield return new TextPart( TextPartKind.Word, word.Substring( beforeIndex, length ) );
            }

            if ( afterIndex < word.Length )
            {
                yield return new TextPart( TextPartKind.Punctuation, word.Substring( afterIndex ) );
            }
        }

        /// <summary>
        /// Creates a container using state information
        /// </summary>
        private static TextContainer CreateContainer( ParseState state )
        {
            TextColor foreground = state.ForegroundColor,
                      background = state.BackgroundColor;
            if ( state.AreColorsReversed )
            {
                if ( foreground == TextColor.Default && background == TextColor.Default )
                {
                    foreground = background = TextColor.ReversedDefault;
                }
                else if ( background == TextColor.Default )
                {
                    background = foreground;
                    foreground = TextColor.ReversedDefault;
                }
                else
                {
                    var temp = foreground;
                    foreground = background;
                    background = temp;
                }
                // no case for foreground unset & background set since this cannot happen, the color tag requires a foreground color
            }

            return new TextContainer( state.CurrentParts, state.IsBold, state.IsUnderlined, foreground, background );
        }

        /// <summary>
        /// Changes the state based on a text part.
        /// </summary>
        private static void ChangeState( ParseState state, TextPart part )
        {
            switch ( part.Kind )
            {
                case TextPartKind.ForegroundColorTag:
                    state.ForegroundColor = part.Color;
                    break;
                case TextPartKind.BackgroundColorTag:
                    state.BackgroundColor = part.Color;
                    break;
                case TextPartKind.EndColorTag:
                    state.ForegroundColor = state.BackgroundColor = TextColor.Default;
                    break;
                case TextPartKind.ReverseColorsTag:
                    state.AreColorsReversed = !state.AreColorsReversed;
                    break;
                case TextPartKind.BoldTag:
                    state.IsBold = !state.IsBold;
                    break;
                case TextPartKind.UnderlineTag:
                    state.IsUnderlined = !state.IsUnderlined;
                    break;
                case TextPartKind.ResetFormattingTag:
                    state.ResetFormatting();
                    break;
            }
        }
        #endregion
    }
}