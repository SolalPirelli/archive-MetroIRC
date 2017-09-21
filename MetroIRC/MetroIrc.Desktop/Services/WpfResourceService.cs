// Copyright (C) 2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.using System.Text;

using MetroIrc.Services;

namespace MetroIrc.Desktop.Services
{
    public sealed class WpfResourceService : IResourceService
    {
        public object GetResource( object key )
        {
            return App.Current.Resources[key];
        }

        public T GetResource<T>( object key )
        {
            return (T) App.Current.Resources[key];
        }
    }
}