// Copyright (C) 2012-2013, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System.ComponentModel;
using System.Windows;

namespace WpfLoc
{
    /// <summary>
    /// A class containing attached properties for design-time localization.
    /// </summary>
    public sealed class DesignTime : DependencyObject
    {
        private static DependencyObject _depObj = new DependencyObject();
        internal static bool IsActive
        {
            get
            {
                return DesignerProperties.GetIsInDesignMode( _depObj );
            }
        }

        private DesignTime()
        {
        }

        #region TranslationProvider AttachedProperty
        /// <summary>
        /// See the <see cref="TranslationProviderProperty"/> dependency property.
        /// </summary>
        public static ITranslationProvider GetTranslationProvider( DependencyObject obj )
        {
            return (ITranslationProvider) obj.GetValue( TranslationProviderProperty );
        }

        /// <summary>
        /// See the <see cref="TranslationProviderProperty"/> dependency property.
        /// </summary>
        public static void SetTranslationProvider( DependencyObject obj, ITranslationProvider value )
        {
            obj.SetValue( TranslationProviderProperty, value );
        }

        /// <summary>
        /// Gets or sets the design-time translation provider.
        /// </summary>
        [DesignOnly( true )]
        public static readonly DependencyProperty TranslationProviderProperty =
            DependencyProperty.RegisterAttached( "TranslationProvider", typeof( ITranslationProvider ), typeof( DesignTime ),
            new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.Inherits ) );
        #endregion

        #region Language AttachedProperty
        /// <summary>
        /// See the <see cref="LanguageProperty"/> dependency property.
        /// </summary>
        public static string GetLanguage( DependencyObject obj )
        {
            return (string) obj.GetValue( LanguageProperty );
        }

        /// <summary>
        /// See the <see cref="LanguageProperty"/> dependency property.
        /// </summary>
        public static void SetLanguage( DependencyObject obj, string value )
        {
            obj.SetValue( LanguageProperty, value );
        }

        /// <summary>
        /// Gets or sets the design-time language.
        /// </summary>
        [DesignOnly( true )]
        public static readonly DependencyProperty LanguageProperty =
            DependencyProperty.RegisterAttached( "Language", typeof( string ), typeof( DesignTime ),
            new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.Inherits ) );
        #endregion
    }
}