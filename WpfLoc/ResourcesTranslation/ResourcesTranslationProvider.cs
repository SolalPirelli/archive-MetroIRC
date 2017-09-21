// Copyright (C) 2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Windows;

namespace WpfLoc.ResourcesTranslation
{
    internal sealed class ResourcesTranslationProvider : ITranslationProvider
    {
        #region Constants/readonly
        private const string FolderName = "Languages";
        private static readonly string CurrentDirectory = Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location );
        private static readonly string LocalizationFolderPath = Path.Combine( CurrentDirectory, FolderName );

        private const string FilesFilter = "*.resources";
        private const string GroupKeySeparator = ".";

        private const char NamespaceSeparator = '.';

        private const char IgnoredEnd = '_';
        #endregion

        #region Private members
        private Dictionary<CultureInfo, Dictionary<string, string>> _dictionaries;
        #endregion

        internal ResourcesTranslationProvider()
        {
            this.LoadFiles();
        }

        #region Private methods
        private void LoadFiles()
        {
            this._dictionaries = new Dictionary<CultureInfo, Dictionary<string, string>>();

            var assembly = GetEntryAssembly();

            foreach ( var file in assembly.GetManifestResourceNames() )
            {
                string noExt = Path.GetFileNameWithoutExtension( file );
                string cultureString = noExt.Split( NamespaceSeparator ).Last();
                var culture = GetCulture( cultureString );

                if ( culture != null )
                {
                    var stream = assembly.GetManifestResourceStream( file );
                    var dic = Load( stream );
                    this._dictionaries.Add( culture, dic );
                }
            }
        }
        #endregion

        #region Private static methods
        /// <summary>
        /// Gets a CultureInfo from a string, or returns null if the string is not a valid CultureInfo.
        /// </summary>
        private static CultureInfo GetCulture( string text )
        {
            try
            {
                return CultureInfo.GetCultureInfo( text );
            }
            catch ( ArgumentException )
            {
                // not a valid translation file
                return null;
            }
        }

        /// <summary>
        /// Loads a dictionary of key and values from a file name.
        /// </summary>
        private static Dictionary<string, string> Load( Stream stream )
        {
            var dic = new Dictionary<string, string>();

            using ( var reader = new ResourceReader( stream ) )
            {
                var enumerator = reader.GetEnumerator();

                while ( enumerator.MoveNext() )
                {
                    string actualKey = ProcessKey( enumerator.Key.ToString() );
                    dic.Add( actualKey, enumerator.Value.ToString() );
                }
            }

            return dic;
        }

        /// <summary>
        /// Gets the actual key from a group/key pair.
        /// </summary>
        private static string GetActualKey( string group, string key )
        {
            return group + GroupKeySeparator + key;
        }

        /// <summary>
        /// Transforms keys. This is needed because .resources files are case-insensitive.
        /// </summary>
        /// <remarks>
        /// Very simple: add _ at the end of a key and it will be ignored.
        /// </remarks>
        private static string ProcessKey( string key )
        {
            if ( key[key.Length - 1] == IgnoredEnd )
            {
                return key.Substring( 0, key.Length - 1 );
            }
            return key;
        }

        /// <summary>
        /// Gets the assembly that called WpfLoc.
        /// </summary>
        private static Assembly GetEntryAssembly()
        {
            if ( DesignTime.IsActive )
            {
                return AppDomain.CurrentDomain.GetAssemblies()
                                .First( a => a.EntryPoint != null && a.ExportedTypes.Any( t => t.IsSubclassOf( typeof( Application ) ) ) );
            }
            return Assembly.GetEntryAssembly();
        }
        #endregion

        #region ITranslationProvider implementation
        /// <summary>
        /// Gets a value indicating whether the specified key in the specified group can be translated in the specified culture.
        /// </summary>
        public bool CanTranslate( string group, string key, CultureInfo culture )
        {
            string actualKey = GetActualKey( group, key );
            return this._dictionaries.ContainsKey( culture ) && this._dictionaries[culture].ContainsKey( actualKey );
        }

        /// <summary>
        /// Translates the specified key in the specified culture.
        /// </summary>
        public string Translate( string group, string key, CultureInfo culture )
        {
            return this._dictionaries[culture][GetActualKey( group, key )];
        }

        /// <summary>
        /// Gets the available languages.
        /// </summary>
        public ReadOnlyCollection<CultureInfo> AvailableLanguages
        {
            get { return new ReadOnlyCollection<CultureInfo>( this._dictionaries.Keys.ToArray() ); }
        }
        #endregion
    }
}