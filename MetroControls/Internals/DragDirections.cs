﻿// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;

namespace MetroControls.Internals
{
    [Flags]
    internal enum DragDirections
    {
        Horizontal = 1,
        Vertical = 2,
        Both = Horizontal | Vertical
    }
}