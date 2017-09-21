// Copyright (C) 2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

namespace MetroIrc.Parsing
{
    /// <summary>
    /// The possible kinds of text parts.
    /// </summary>
    public enum TextPartKind
    {
        /// <summary>
        /// Not defined.
        /// </summary>
        None,
        /// <summary>
        /// A word.
        /// </summary>
        Word,
        /// <summary>
        /// One or more punctuation marks.
        /// </summary>
        Punctuation,
        /// <summary>
        /// One or more spacing characters.
        /// </summary>
        Space,

        /// <summary>
        /// A foreground color tag.
        /// </summary>
        ForegroundColorTag,
        /// <summary>
        /// A background color tag.
        /// </summary>
        BackgroundColorTag,
        /// <summary>
        /// A tag indicating the end of color.
        /// </summary>
        EndColorTag,
        /// <summary>
        /// A bold formatting tag.
        /// </summary>
        BoldTag,
        /// <summary>
        /// An underline formatting tag.
        /// </summary>
        UnderlineTag,
        /// <summary>
        /// A formatting tag which reverses the colors.
        /// </summary>
        ReverseColorsTag,
        /// <summary>
        /// A formatting tag which resets formatting.
        /// </summary>
        ResetFormattingTag,
    }
}