using System.Collections.Generic;
using MetroIrc.Services;

namespace MetroIrc.Common.Tests.Internals
{
    internal sealed class TestTranslator : ITranslationService
    {
        private Dictionary<string, string> _values = new Dictionary<string, string>();

        public void Set( string group, string key, string value )
        {
            this._values.Add( GetKey( group, key ), value );
        }

        public void Clear()
        {
            this._values.Clear();
        }

        public bool CanTranslate( string group, string key )
        {
            return this._values.ContainsKey( GetKey( group, key ) );
        }

        public string Translate( string group, string key, params object[] args )
        {
            return string.Format( this._values[GetKey( group, key )], args );
        }

        private static string GetKey( string group, string key )
        {
            return group + "." + key;
        }
    }
}