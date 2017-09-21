// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading;
using WpfLoc.ResourcesTranslation;

namespace WpfLoc
{
    /// <summary>
    /// The TranslationManager class is the main part of this library.
    /// </summary>
    public static class TranslationManager
    {
        #region Public properties
        /// <summary>
        /// Gets or sets the current thread language.
        /// </summary>
        public static CultureInfo CurrentLanguage
        {
            get
            {
                return Thread.CurrentThread.CurrentUICulture;
            }
            set
            {
                if ( !Thread.CurrentThread.CurrentUICulture.Equals( value ) ) // compare by value, not by ref
                {
                    Thread.CurrentThread.CurrentUICulture = value;
                    OnLanguageChanged();
                }
            }
        }

        /// <summary>
        /// Gets the available languages.
        /// </summary>
        public static ReadOnlyCollection<CultureInfo> AvailableLanguages
        {
            get
            {
                if ( TranslationProvider != null )
                {
                    return TranslationProvider.AvailableLanguages;
                }
                return new ReadOnlyCollection<CultureInfo>( new List<CultureInfo>() );
            }
        }

        /// <summary>
        /// Gets or sets the current translation provider.
        /// </summary>
        public static ITranslationProvider TranslationProvider { get; set; }

        /// <summary>
        /// Gets a translation provider that uses .resource (compiled ResX) files.
        /// </summary>
        public static ITranslationProvider ResourcesTranslationProvider { get; private set; }

        /// <summary>
        /// Gets or sets the default culture, used when a localized translation is not available.
        /// English (US) by default.
        /// </summary>
        public static CultureInfo DefaultLanguage { get; set; }
        #endregion

        static TranslationManager()
        {
            ResourcesTranslationProvider = new ResourcesTranslationProvider();
            DefaultLanguage = new CultureInfo( "en-US" );
        }

        #region Events
        /// <summary>
        /// Occurs when the <see cref="TranslationManager.CurrentLanguage"/> changes.
        /// </summary>
        public static event EventHandler<EventArgs> LanguageChanged;
        private static void OnLanguageChanged()
        {
            if ( LanguageChanged != null )
            {
                LanguageChanged( null, EventArgs.Empty );
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Gets a value indicating whether the group/key pair. can be translated.
        /// </summary>
        /// <param name="group">The group the key belongs to.</param>
        /// <param name="key">The key.</param>
        /// <returns>A value indicating whether the group/key pair. can be translated.</returns>
        public static bool CanTranslate( string group, string key )
        {
            if ( string.IsNullOrWhiteSpace( group ) )
            {
                throw new ArgumentException( "group must not be null or consist only of white-space characters." );
            }
            if ( string.IsNullOrWhiteSpace( key ) )
            {
                throw new ArgumentException( "key must not be null or consist only of white-space characters." );
            }

            return CanTranslate( group, key, CurrentLanguage );
        }

        /// <summary>
        /// Translates a group/key pair and formats the resulting string using the specified arguments.
        /// </summary>
        /// <param name="group">The group the key belongs to.</param>
        /// <param name="key">The key.</param>
        /// <param name="args">The arguments to be used when formatting the string.</param>
        /// <returns>The translated value.</returns>
        public static string Translate( string group, string key, params object[] args )
        {
            if ( string.IsNullOrWhiteSpace( group ) )
            {
                throw new ArgumentException( "group must not be null or consist only of white-space characters." );
            }
            if ( string.IsNullOrWhiteSpace( key ) )
            {
                throw new ArgumentException( "key must not be null or consist only of white-space characters." );
            }
            if ( TranslationProvider == null )
            {
                throw new Exception( "TranslationProvider must not be null" );
            }

            string translatedValue = Translate( group, key, CurrentLanguage );
            return string.Format( translatedValue, args );
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Gets a value indicating whether the specified group/key pair can be translated in the specified language.
        /// </summary>
        private static bool CanTranslate( string group, string key, CultureInfo language )
        {
            if ( TranslationProvider == null )
            {
                return false;
            }

            return TranslationProvider.AvailableLanguages.Contains( language ) && TranslationProvider.CanTranslate( group, key, language );
        }

        /// <summary>
        /// Gets the translation of the specified group/key pair in the specified language.
        /// </summary>
        private static string Translate( string group, string key, CultureInfo language )
        {
            if ( CanTranslate( group, key, language ) )
            {
                return TranslationProvider.Translate( group, key, language );
            }
            if ( language == DefaultLanguage )
            {
                string text = string.Format( "Group {0}, key {1} was not found in the default culture dictionary.", group, key );
                throw new ArgumentException( text );
            }
            return Translate( group, key, DefaultLanguage );
        }
        #endregion
    }
}