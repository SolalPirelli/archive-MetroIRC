// Copyright (C) 2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System.Collections.Generic;
using System.Linq;

namespace MetroIrc.Parsing
{
    /// <summary>
    /// A text container, containing text parts and information about formatting.
    /// </summary>
    public sealed class TextContainer
    {
        public TextPart[] Parts { get; private set; }

        public bool IsBold { get; private set; }
        public bool IsUnderlined { get; private set; }

        public TextColor ForegroundColor { get; private set; }
        public TextColor BackgroundColor { get; private set; }

        public TextContainer( IEnumerable<TextPart> parts, bool isBold, bool isUnderlined, TextColor foreground, TextColor background )
        {
            this.Parts = parts.ToArray();
            this.IsBold = isBold;
            this.IsUnderlined = isUnderlined;
            this.ForegroundColor = foreground;
            this.BackgroundColor = background;
        }
    }
}