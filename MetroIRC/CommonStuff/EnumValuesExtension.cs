// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Windows.Markup;

namespace CommonStuff
{
    // from http://stackoverflow.com/questions/878356/wpf-combobox-binding-to-enum-what-did-i-do-wrong
    public sealed class EnumValuesExtension : MarkupExtension
    {
        public EnumValuesExtension( Type enumType )
        {
            this.EnumType = enumType;
        }

        public Type EnumType { get; set; }

        public override object ProvideValue( IServiceProvider serviceProvider )
        {
            if ( this.EnumType == null )
            {
                throw new ArgumentException( "The enum type is not set" );
            }

            return Enum.GetValues( this.EnumType );
        }
    }
}