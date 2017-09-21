// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Reflection;
using System.Xml.Linq;

namespace MetroIrc.Desktop
{
    /// <summary>
    /// A helper class that checks for new versions of the software.
    /// </summary>
    public static class UpdateChecker
    {
        #region Const/readonly
        private const string UpdateManifestLocation = "https://bitbucket.org/Aethec/metroirc/downloads/UpdateManifest.xml";
        private const double CheckFrequency = 1; // in days

        private static readonly XName VersionElementName = "Version";
        private static readonly XName FilePathElementName = "FilePath";
        private static readonly XName MinOSVersionElementName = "MinOSVersion";

        private const char OSVersionSeparator = '.';
        #endregion

        /// <summary>
        /// The path of the new version's installer.
        /// </summary>
        public static string FilePath { get; set; }

        /// <summary>
        /// Checks for an update and returns a value indicating whether an update is available.
        /// </summary>
        public static bool CheckForUpdate( bool forceCheck )
        {
            if ( App.Current.IsPortableInstall )
            {
                return false;
            }

            if ( !forceCheck && ( DateTime.Now - App.Current.Settings.LastUpdateCheck ).TotalDays < CheckFrequency )
            {
                return false;
            }

            try
            {
                var xmlManifest = XDocument.Load( UpdateManifestLocation );
                string version = xmlManifest.Root.Element( VersionElementName ).Value;
                FilePath = xmlManifest.Root.Element( FilePathElementName ).Value;

                // set only if the check succeeded
                App.Current.Settings.LastUpdateCheck = DateTime.Now;

                return Environment.OSVersion.Version >= Version.Parse( xmlManifest.Root.Element( MinOSVersionElementName ).Value )
                    && Version.Parse( version ) > Assembly.GetExecutingAssembly().GetName().Version;
            }
            catch
            {
                return false;
            }
        }
    }
}