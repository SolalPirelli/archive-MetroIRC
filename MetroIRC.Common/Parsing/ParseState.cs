// Copyright (C) 2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System.Collections.Generic;

namespace MetroIrc.Parsing
{
    /// <summary>
    /// A class representing a parsing state for the MessageParser.
    /// </summary>
    internal sealed class ParseState
    {
        public List<TextPart> CurrentParts { get; private set; }

        public bool IsBold { get; set; }
        public bool IsUnderlined { get; set; }

        public TextColor ForegroundColor { get; set; }
        public TextColor BackgroundColor { get; set; }
        public bool AreColorsReversed { get; set; }

        public ParseState()
        {
            this.CurrentParts = new List<TextPart>();
        }

        public void ResetFormatting()
        {
            this.IsBold = false;
            this.IsUnderlined = false;
            this.ForegroundColor = TextColor.Default;
            this.BackgroundColor = TextColor.Default;
            this.AreColorsReversed = false;
        }
    }
}