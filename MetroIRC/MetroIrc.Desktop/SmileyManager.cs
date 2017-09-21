// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace MetroIrc.Desktop
{
    public static class SmileyManager
    {
        #region Constants
        private const string SmileysFolderName = "Smileys";
        public static readonly string CustomSmileysFolder = Path.Combine( App.Current.DataFolderPath, SmileysFolderName );
        public static readonly string DefaultSmileysFolder = Path.Combine( App.Current.CurrentDirectory, SmileysFolderName );

        private const string ManifestFileName = "manifest.xml";
        private static readonly XName DocumentRootName = "SmileyPack";
        private static readonly XName IsDefaultAttributeName = "IsDefault";
        private static readonly XName SmileyElementName = "Smiley";
        private static readonly XName FileNameAttributeName = "FileName";
        private static readonly XName ShortcutsAttributeName = "TextShortcuts";

        private const char SmileyShortcutsSeparator = ' ';
        private const char SmileyNamePrefix = '_'; // used when two smileys share the image name
        #endregion

        #region Property-backing fields
        private static SmileyPack _currentPack;
        #endregion

        #region Public properties
        public static ObservableCollection<SmileyPack> Packs { get; private set; }

        public static SmileyPack CurrentPack
        {
            get { return _currentPack; }
            set
            {
                if ( _currentPack != value )
                {
                    _currentPack = value;
                    FirePropertyChanged();
                }
            }
        }
        #endregion

        static SmileyManager()
        {
            if ( !Directory.Exists( DefaultSmileysFolder ) )
            {
                Directory.CreateDirectory( DefaultSmileysFolder );
            }

            if ( !Directory.Exists( CustomSmileysFolder ) )
            {
                Directory.CreateDirectory( CustomSmileysFolder );
            }

            Packs = new ObservableCollection<SmileyPack>();

            Load();
        }

        #region Public methods
        public static void Load()
        {
            string currentPackName = CurrentPack == null ? null : CurrentPack.Name;

            Packs.Clear();

            var defaultDirs = new DirectoryInfo( DefaultSmileysFolder ).EnumerateDirectories();
            var customDirs = new DirectoryInfo( CustomSmileysFolder ).EnumerateDirectories();

            foreach ( var dir in defaultDirs.Concat( customDirs ) )
            {
                var doc = XDocument.Load( Path.Combine( dir.FullName, ManifestFileName ) );
                bool isDefault = bool.Parse( doc.Root.Attribute( IsDefaultAttributeName ).Value );
                var pack = new SmileyPack( dir.Name, isDefault );

                foreach ( var elem in doc.Root.Elements( SmileyElementName ) )
                {
                    string fileName = elem.Attribute( FileNameAttributeName ).Value;
                    string[] shortcuts = elem.Attribute( ShortcutsAttributeName ).Value
                                             .Split( SmileyShortcutsSeparator );

                    var smiley = new Smiley();
                    smiley.FileName = fileName;
                    smiley.Image = GetImage( Path.Combine( GetPath( pack ), fileName ) );

                    foreach ( string s in shortcuts )
                    {
                        smiley.Shortcuts.Add( s );
                    }

                    pack.Smileys.Add( smiley );
                }

                Packs.Add( pack );
            }

            if ( currentPackName == null )
            {
                CurrentPack = Packs.First();
            }
            else
            {
                SetCurrentPack( currentPackName );
            }
        }

        public static void Save()
        {
            foreach ( var pack in Packs.Where( p => !p.IsDefault ) )
            {
                var root = new XElement( DocumentRootName );
                root.SetAttributeValue( IsDefaultAttributeName, pack.IsDefault );

                foreach ( var smiley in pack.Smileys )
                {
                    var elem = new XElement( SmileyElementName );
                    elem.SetAttributeValue( FileNameAttributeName, smiley.FileName );
                    elem.SetAttributeValue( ShortcutsAttributeName, string.Join( SmileyShortcutsSeparator.ToString(), smiley.Shortcuts ) );

                    root.Add( elem );
                }

                string manifestPath = Path.Combine( GetPath( pack ), ManifestFileName );
                new XDocument( root ).Save( File.Open( manifestPath, FileMode.Create ) );
            }
        }


        public static Smiley AddSmiley( string imagePath, IEnumerable<string> shortcuts, SmileyPack pack )
        {
            if ( pack.IsDefault )
            {
                throw new ArgumentException( "Cannot change a default pack." );
            }

            string fileName = Path.GetFileName( imagePath );
            string newPath = GetUniquePath( pack, fileName );
            File.Copy( imagePath, newPath );

            var smiley = new Smiley();
            smiley.FileName = Path.GetFileName( newPath );
            smiley.Image = GetImage( newPath );

            foreach ( string s in shortcuts )
            {
                smiley.Shortcuts.Add( s );
            }

            pack.Smileys.Add( smiley );

            return smiley;
        }

        public static void RemoveSmiley( Smiley smiley, SmileyPack pack )
        {
            if ( pack.IsDefault )
            {
                throw new ArgumentException( "Cannot change a default pack." );
            }

            pack.Smileys.Remove( smiley );
            File.Delete( Path.Combine( CustomSmileysFolder, pack.Name, smiley.FileName ) );
        }

        public static void SetSmileyImage( SmileyPack containingPack, Smiley smiley, string imagePath )
        {
            string oldFile = Path.Combine( GetPath( containingPack ), smiley.FileName );
            string newFile = GetUniquePath( containingPack, Path.GetFileName( imagePath ) );

            File.Delete( oldFile );
            File.Copy( imagePath, newFile );
            smiley.FileName = Path.GetFileName( newFile );
            smiley.Image = GetImage( newFile );
        }


        public static void SetCurrentPack( string packName )
        {
            CurrentPack = Packs.FirstOrDefault( p => p.Name == packName );

            if ( CurrentPack == null )
            {
                CurrentPack = Packs.First();
            }
        }

        public static SmileyPack AddPack( string name )
        {
            var pack = new SmileyPack( name, false );
            Directory.CreateDirectory( GetPath( pack ) );
            Packs.Add( pack );
            return pack;
        }

        public static SmileyPack CopyPack( SmileyPack pack, string newName )
        {
            var newPack = AddPack( newName );

            foreach ( var smiley in pack.Smileys )
            {
                string oldPath = Path.Combine( GetPath( pack ), smiley.FileName );
                string newPath = Path.Combine( GetPath( newPack ), smiley.FileName );

                File.Copy( oldPath, newPath );

                var newSmiley = new Smiley();
                newSmiley.FileName = smiley.FileName;
                newSmiley.Image = GetImage( newPath );

                foreach ( string s in smiley.Shortcuts )
                {
                    newSmiley.Shortcuts.Add( s );
                }

                newPack.Smileys.Add( smiley );
            }

            return newPack;
        }

        public static void DeletePack( SmileyPack pack )
        {
            if ( pack.IsDefault )
            {
                throw new ArgumentException( "Cannot delete a default pack." );
            }

            Packs.Remove( pack );
            Directory.Delete( GetPath( pack ), true );

            CurrentPack = Packs.First();
        }


        public static bool HasSmiley( string text )
        {
            return GetSmileyPrivate( text ) != null;
        }

        public static ImageSource GetSmiley( string text )
        {
            return GetSmileyPrivate( text ).Image;
        }
        #endregion

        #region Events
        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;
        private static void FirePropertyChanged( [CallerMemberName] string propertyName = "" )
        {
            if ( StaticPropertyChanged != null )
            {
                StaticPropertyChanged( null, new PropertyChangedEventArgs( propertyName ) );
            }
        }
        #endregion

        #region Private methods
        private static string GetPath( SmileyPack pack )
        {
            return Path.Combine( pack.IsDefault ? DefaultSmileysFolder : CustomSmileysFolder, pack.Name );
        }

        private static string GetUniquePath( SmileyPack pack, string fileName )
        {
            string pathPrefix = string.Empty;
            while ( File.Exists( Path.Combine( GetPath( pack ), pathPrefix + fileName ) ) )
            {
                pathPrefix += SmileyNamePrefix;
            }
            return Path.Combine( GetPath( pack ), pathPrefix + fileName );
        }

        private static ImageSource GetImage( string imagePath )
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            using ( var stream = File.OpenRead( imagePath ) )
            {
                image.StreamSource = stream;
                image.EndInit();
            }
            image.Freeze();

            return image;
        }

        private static Smiley GetSmileyPrivate( string text )
        {
            return CurrentPack.Smileys.FirstOrDefault( s => s.Shortcuts.Any( sh => string.Compare( text, sh, true ) == 0 ) );
        }
        #endregion
    }
}