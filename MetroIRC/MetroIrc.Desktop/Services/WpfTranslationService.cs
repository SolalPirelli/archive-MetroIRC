// Copyright (C) 2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.using System.Text;

using MetroIrc.Services;
using WpfLoc;

namespace MetroIrc.Desktop.Services
{
    public sealed class WpfTranslationService : ITranslationService
    {
        public bool CanTranslate( string group, string key )
        {
            return TranslationManager.CanTranslate( group, key );
        }

        public string Translate( string group, string key, params object[] args )
        {
            return TranslationManager.Translate( group, key, args );
        }
    }
}