// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System.Collections.ObjectModel;
using System.Globalization;

namespace WpfLoc
{
    /// <summary>
    /// An interface that represents a translation provider.
    /// </summary>
    public interface ITranslationProvider
    {
        /// <summary>
        /// Gets a value indicating whether the group/key pair. can be translated.
        /// </summary>
        /// <param name="group">The group the key belongs to.</param>
        /// <param name="key">The key.</param>
        /// <param name="language">The language.</param>
        /// <returns>A value indicating whether the group/key pair. can be translated.</returns>
        bool CanTranslate( string group, string key, CultureInfo language );

        /// <summary>
        /// Translates a group/key pair and formats the resulting string using the specified arguments.
        /// </summary>
        /// <param name="group">The group the key belongs to.</param>
        /// <param name="key">The key.</param>
        /// <param name="language">The language.</param>
        /// <returns>The translated value.</returns>
        string Translate( string group, string key, CultureInfo language );

        /// <summary>
        /// Gets the available languages.
        /// </summary>
        ReadOnlyCollection<CultureInfo> AvailableLanguages { get; }
    }
}