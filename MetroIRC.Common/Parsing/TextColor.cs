// Copyright (C) 2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

namespace MetroIrc.Parsing
{
    /// <summary>
    /// The possible text colors.
    /// They are one-off from mIRC's color table since 0 must be the default color.
    /// </summary>
    public enum TextColor
    {
        /// <summary>
        /// The default color from the foreground when talking about the background, and vice-versa.
        /// </summary>
        ReversedDefault = -1,
        /// <summary>
        /// The default color.
        /// </summary>
        Default = 0,
        /// <summary>
        /// White (mIRC uses #FFFFFF).
        /// </summary>
        White = 1,
        /// <summary>
        /// Black (mIRC uses #000000).
        /// </summary>
        Black = 2,
        /// <summary>
        /// Navy (mIRC uses #00007F).
        /// </summary>
        Navy = 3,
        /// <summary>
        /// Green (mIRC uses #009300).
        /// </summary>
        Green = 4,
        /// <summary>
        /// Red (mIRC uses #FF0000).
        /// </summary>
        Red = 5,
        /// <summary>
        /// Maroon (mIRC uses #7F0000).
        /// </summary>
        Maroon = 6,
        /// <summary>
        /// Purple (mIRC uses #9C009C).
        /// </summary>
        Purple = 7,
        /// <summary>
        /// Orange (mIRC uses #FC7F00).
        /// </summary>
        Orange = 8,
        /// <summary>
        /// Yellow (mIRC uses #FFFF00).
        /// </summary>
        Yellow = 9,
        /// <summary>
        /// Lime (mIRC uses #00FC00).
        /// </summary>
        Lime = 10,
        /// <summary>
        /// Teal (mIRC uses #009393).
        /// </summary>
        Teal = 11,
        /// <summary>
        /// Aqua (mIRC uses #00FFFF).
        /// </summary>
        Aqua = 12,
        /// <summary>
        /// Royal Blue (mIRC uses #0000FC).
        /// </summary>
        RoyalBlue = 13,
        /// <summary>
        /// Fuchsia (mIRC uses #FF00FF).
        /// </summary>
        Fuchsia = 14,
        /// <summary>
        /// Gray (mIRC uses #7F7F7F).
        /// </summary>
        Gray = 15,
        /// <summary>
        /// Silver (mIRC uses #D2D2D2).
        /// </summary>
        Silver = 16,
        /// <summary>
        /// Transparent (alpha = 0).
        /// </summary>
        Transparent = 100
    }
}