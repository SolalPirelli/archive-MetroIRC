// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System.Collections.ObjectModel;
using BasicMvvm;

namespace MetroIrc.Desktop
{
    public sealed class SmileyPack : ObservableObject
    {
        public string Name { get; private set; }

        public bool IsDefault { get; private set; }

        public ObservableCollection<Smiley> Smileys { get; private set; }

        public SmileyPack( string name, bool isDefault )
        {
            this.Name = name;
            this.IsDefault = isDefault;
            this.Smileys = new ObservableCollection<Smiley>();
        }
    }
}