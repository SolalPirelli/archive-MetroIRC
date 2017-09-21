// Copyright (C) 2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.using System.Text;

namespace MetroIrc.Services
{
    public interface ITranslationService
    {
        bool CanTranslate( string group, string key );
        string Translate( string group, string key, params object[] args );
    }
}