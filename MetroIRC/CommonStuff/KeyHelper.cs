// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System.Windows.Input;

namespace CommonStuff
{
    /// <summary>
    /// A helper class for keyboard key-related tasks.
    /// </summary>
    public static class KeyHelper
    {
        /// <summary>
        /// Indicates whether the specified key is a modifier one ; Ctrl, Alt or Shift.
        /// </summary>
        /// <param name="k">The key to test</param>
        public static bool IsModifierKey( Key k )
        {
            return k == Key.LeftCtrl || k == Key.RightCtrl ||
                   k == Key.LeftAlt || k == Key.RightAlt ||
                   k == Key.LeftShift || k == Key.RightShift;
        }
    }
}