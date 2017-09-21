// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;

namespace WpfLoc.Internals
{
    /// <summary>
    /// The TranslationData class is a wrapper around the TranslationManager for one key in one group.
    /// It provides dynamic language changing using INotifyPropertyChanged.
    /// </summary>
    internal sealed class TranslationData : INotifyPropertyChanged, IWeakEventListener
    {
        #region Private members
        private DependencyObject _parent;
        private string _group;
        private string _key;
        private object[] _parameters;
        #endregion

        #region Public properties
        /// <summary>
        /// Gets the translated value.
        /// </summary>
        public object Value
        {
            get
            {
                if ( DesignTime.IsActive )
                {
                    return this.GetDesignModeValue();
                }
                return TranslationManager.Translate( this._group, this._key, this._parameters );
            }
        }
        #endregion

        internal TranslationData( DependencyObject parent, string group, string key, params object[] parameters )
        {
            this._parent = parent;
            this._group = group;
            this._key = key;
            this._parameters = parameters;
            LanguageChangedEventManager.AddListener( this );

            if ( DesignTime.IsActive )
            {
                // May cause memory leaks, but we're in design mode so we don't care
                DependencyPropertyDescriptor.FromProperty( DesignTime.LanguageProperty, typeof( DesignTime ) )
                                            .AddValueChanged( this._parent, ( s, e ) => this.FireValueChanged() );
                DependencyPropertyDescriptor.FromProperty( DesignTime.TranslationProviderProperty, typeof( DesignTime ) )
                                            .AddValueChanged( this._parent, ( s, e ) => this.FireValueChanged() );
            }
        }

        private string GetDesignModeValue()
        {
            var provider = DesignTime.GetTranslationProvider( this._parent ) ?? TranslationManager.TranslationProvider;
            string language = DesignTime.GetLanguage( this._parent );
            var culture = language == null ? TranslationManager.CurrentLanguage : new CultureInfo( language );

            if ( provider == null || culture == null )
            {
                return string.Format( "!!{0}.{1}!!", this._group, this._key );
            }

            return provider.Translate( this._group, this._key, culture );
        }

        #region INotifyPropertyChanged implementation
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        private void FireValueChanged()
        {
            if ( this.PropertyChanged != null )
            {
                this.PropertyChanged( this, new PropertyChangedEventArgs( "Value" ) );
            }
        }
        #endregion

        #region IWeakEventListener implementation
        /// <summary>
        /// Receives events from the centralized event manager.
        /// </summary>
        public bool ReceiveWeakEvent( Type managerType, object sender, EventArgs e )
        {
            if ( managerType == typeof( LanguageChangedEventManager ) )
            {
                this.FireValueChanged();
                return true;
            }
            return false;
        }
        #endregion
    }
}