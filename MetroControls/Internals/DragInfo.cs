// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

namespace MetroControls.Internals
{
    internal sealed class DragInfo
    {
        public bool IsTop { get; private set; }
        public bool IsLeft { get; private set; }

        public double HorizontalDelta { get; private set; }
        public double VerticalDelta { get; private set; }

        public DragDirections Direction { get; private set; }

        public DragInfo( bool isTop, bool isLeft, double horizontalDelta, double verticalDelta, DragDirections direction )
        {
            this.IsTop = isTop;
            this.IsLeft = isLeft;
            this.HorizontalDelta = horizontalDelta;
            this.VerticalDelta = verticalDelta;
            this.Direction = direction;
        }
    }
}