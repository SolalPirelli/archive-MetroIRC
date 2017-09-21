// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Windows.Markup;

namespace CommonStuff
{
    public class SelfExtension<T> : MarkupExtension
        where T : SelfExtension<T>, new()
    {
        private static Lazy<T> LazyInstance = new Lazy<T>( () => new T() );

        public override object ProvideValue( IServiceProvider serviceProvider )
        {
            return LazyInstance.Value;
        }
    }
}