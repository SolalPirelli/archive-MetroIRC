// Copyright (C) 2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.using System.Text;

using MetroIrc.Services;

namespace MetroIrc.Desktop.Services
{
    public sealed class WpfSmileyService : ISmileyService
    {
        public bool HasSmiley( string word )
        {
            return SmileyManager.HasSmiley( word );
        }

        public object GetSmiley( string word )
        {
            return SmileyManager.GetSmiley( word );
        }
    }
}