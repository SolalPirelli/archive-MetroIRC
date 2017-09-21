// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.IO;
using System.Windows;
using System.Xml.Serialization;

namespace MetroIrc.Desktop
{
    /// <summary>
    /// A helper class to load and save settings.
    /// </summary>
    public static class SettingsHelper
    {
        private const string FileName = "settings.xml";
        private static readonly string SettingsPath = Path.Combine( App.Current.DataFolderPath, FileName );

        public static WpfSettings LoadSettings()
        {
            if ( !File.Exists( SettingsPath ) )
            {
                return new WpfSettings();
            }

            using ( StreamReader reader = new StreamReader( File.OpenRead( SettingsPath ) ) )
            {
                try
                {
                    var settings = (WpfSettings) new XmlSerializer( typeof( WpfSettings ) ).Deserialize( reader );
                    FixWindowPostion( settings );
                    return settings;
                }
                catch ( InvalidOperationException )
                {
                    return new WpfSettings(); // Happens if the file is corrupted.
                }
            }
        }

        public static void SaveSettings( WpfSettings settings )
        {
            using ( var stream = File.Open( SettingsPath, FileMode.Create, FileAccess.Write, FileShare.None ) )
            {
                new XmlSerializer( typeof( WpfSettings ) ).Serialize( stream, settings );
            }
        }

        /// <summary>
        /// Fixes weird behavior caused by multi-monitor setups losing a monitor.
        /// </summary>
        private static void FixWindowPostion( WpfSettings settings )
        {
            if ( settings.MainWindowLeft >= SystemParameters.WorkArea.Right )
            {
                settings.MainWindowLeft = double.NaN;
            }
            if ( settings.MainWindowTop <= 0 )
            {
                settings.MainWindowTop = 0;
            }
        }
    }
}