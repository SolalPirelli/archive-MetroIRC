// Copyright (C) 2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

namespace MetroIrc.Parsing
{
    /// <summary>
    /// A text part.
    /// </summary>
    public sealed class TextPart
    {
        public TextPartKind Kind { get; private set; }
        public string Content { get; private set; }

        public TextColor Color { get; private set; }

        public TextPart( TextPartKind kind, string content = null )
        {
            this.Kind = kind;
            this.Content = content;
        }

        public TextPart( TextPartKind kind, TextColor color )
        {
            this.Kind = kind;
            this.Color = color;
        }

        public int GetLength()
        {
            if ( this.Content != null )
            {
                return this.Content.Length;
            }

            if ( this.Color == TextColor.Default )
            {
                // normal tag
                return 1;
            }

            // one more, either for the char in the foreground color or the comma in the background color
            return ( (int) this.Color ).ToString().Length + 1;
        }
    }
}