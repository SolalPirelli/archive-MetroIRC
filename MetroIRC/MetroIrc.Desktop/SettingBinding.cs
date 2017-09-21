// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using BasicMvvm;
using System.Windows;
using System.Windows.Data;

namespace MetroIrc.Desktop
{
    /// <summary>
    /// A Binding subclass that binds to the settings in TwoWay mode by default.
    /// </summary>
    public sealed class SettingBinding : Binding
    {
        public SettingBinding() : this( string.Empty ) { }

        public SettingBinding( string path )
        {
            this.Path = new PropertyPath( path );
            this.Source = Locator.Get<ISettings>();
            this.Mode = BindingMode.TwoWay;
        }
    }
}