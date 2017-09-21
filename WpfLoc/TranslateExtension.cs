// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using WpfLoc.Internals;

namespace WpfLoc
{
    /// <summary>
    /// The Translate Markup Extension returns a binding to a TranslationData that provides a translated resource from a specified key.
    /// </summary>
    public sealed class TranslateExtension : MarkupExtension
    {
        #region Public properties
        /// <summary>
        /// Gets or sets the group the resource belongs to.
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// Gets or sets the key of the resource.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the parameters to be used when translating the resource.
        /// </summary>
        /// <remarks>This is a list because otherwise it is not settable in XAML</remarks>
        public List<object> Parameters { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="TranslateExtension"/> class.
        /// </summary>
        /// <param name="group">The translation group</param>
        /// <param name="key">The translation key</param>
        public TranslateExtension( string group, string key )
        {
            this.Group = group;
            this.Key = key;
            this.Parameters = new List<object>();
        }
        #endregion

        /// <summary>
        /// See <see cref="MarkupExtension.ProvideValue" />
        /// </summary>
        public override object ProvideValue( IServiceProvider serviceProvider )
        {
            var rootProvider = (IProvideValueTarget) serviceProvider.GetService( typeof( IProvideValueTarget ) );
            var parent = rootProvider.TargetObject as DependencyObject;
            if ( parent == null )
            {
                return this; // parent is a SharedDp, an internal type. Somehow, returning "this" works.
            }

            return new Binding( "Value" )
            {
                Source = new TranslationData( parent, this.Group, this.Key, this.Parameters.ToArray() ),
                NotifyOnTargetUpdated = true,
                Mode = BindingMode.OneWay
            }.ProvideValue( serviceProvider );
        }
    }
}