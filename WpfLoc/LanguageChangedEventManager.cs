// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Windows;

namespace WpfLoc
{
    /// <summary>
    /// A <see cref="System.Windows.WeakEventManager"/> that manages the static <see cref="TranslationManager.LanguageChanged"/> event.
    /// </summary>
    public sealed class LanguageChangedEventManager : WeakEventManager
    {
        /// <summary>
        /// Gets the current <see cref="LanguageChangedEventManager"/>.
        /// </summary>
        public static LanguageChangedEventManager CurrentManager
        {
            get
            {
                var managerType = typeof( LanguageChangedEventManager );
                var manager = (LanguageChangedEventManager) WeakEventManager.GetCurrentManager( managerType );

                if ( manager == null )
                {
                    manager = new LanguageChangedEventManager();
                    WeakEventManager.SetCurrentManager( managerType, manager );
                }

                return manager;
            }
        }

        /// <summary>
        /// Adds a listener to the <see cref="TranslationManager.LanguageChanged"/> event.
        /// </summary>
        public static void AddListener( IWeakEventListener listener )
        {
            CurrentManager.ProtectedAddListener( null, listener );
        }

        /// <summary>
        /// Removes a listener to the <see cref="TranslationManager.LanguageChanged"/> event.
        /// </summary>
        public static void RemoveListener( IWeakEventListener listener )
        {
            CurrentManager.ProtectedRemoveListener( null, listener );
        }

        /// <summary>
        /// Starts listening to the <see cref="TranslationManager"/>.
        /// </summary>
        protected override void StartListening( object source )
        {
            TranslationManager.LanguageChanged += DeliverStaticEvent;
        }

        /// <summary>
        /// Stops listening to the <see cref="TranslationManager"/>.
        /// </summary>
        protected override void StopListening( object source )
        {
            TranslationManager.LanguageChanged -= DeliverStaticEvent;
        }

        /// <summary>
        /// Delivers the static event.
        /// This is required to have a static weak event.
        /// </summary>
        private void DeliverStaticEvent( object source, EventArgs e )
        {
            this.DeliverEvent( null, e );
        }
    }
}