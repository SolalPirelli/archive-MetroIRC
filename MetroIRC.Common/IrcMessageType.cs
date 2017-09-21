// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

namespace MetroIrc
{
    /// <summary>
    /// The different messages types.
    /// </summary>
    public enum IrcMessageType
    {
        Normal,
        Notice,
        Action,
        Join,
        Part,
        Kick,
        Quit,
        UserMode,
        ChannelMode,
        NickChange,
        Invite,
        Info,
        Error
    }
}