// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Windows;
using MetroControls.Internals;

namespace MetroControls
{
    /// <summary>
    /// A metro window.
    /// </summary>
    public class MetroWindow : Window
    {
        /// <summary>
        /// Gets or sets a value indicating whether the "Minimize" button should be shown.
        /// </summary>
        public bool ShowMinimizeButton
        {
            get { return (bool) GetValue( ShowMinimizeButtonProperty ); }
            set { SetValue( ShowMinimizeButtonProperty, value ); }
        }

        /// <summary>
        /// See the <see cref="MetroControls.MetroWindow.ShowMinimizeButton"/> property.
        /// </summary>
        public static readonly DependencyProperty ShowMinimizeButtonProperty =
            DependencyProperty.Register( "ShowMinimizeButton", typeof( bool ), typeof( MetroWindow ), new UIPropertyMetadata( true ) );


        /// <summary>
        /// Gets or sets a value indicating whether the "Maximize/Restore" button should be shown.
        /// </summary>
        public bool ShowMaximizeButton
        {
            get { return (bool) GetValue( ShowMaximizeButtonProperty ); }
            set { SetValue( ShowMaximizeButtonProperty, value ); }
        }

        /// <summary>
        /// See the <see cref="MetroControls.MetroWindow.ShowMaximizeButton"/> property.
        /// </summary>
        public static readonly DependencyProperty ShowMaximizeButtonProperty =
            DependencyProperty.Register( "ShowMaximizeButton", typeof( bool ), typeof( MetroWindow ), new UIPropertyMetadata( true ) );


        /// <summary>
        /// Gets or sets a value indicating whether the "Close" button should be shown.
        /// </summary>
        public bool ShowCloseButton
        {
            get { return (bool) GetValue( ShowCloseButtonProperty ); }
            set { SetValue( ShowCloseButtonProperty, value ); }
        }

        /// <summary>
        /// See the <see cref="MetroControls.MetroWindow.ShowCloseButton"/> property.
        /// </summary>
        public static readonly DependencyProperty ShowCloseButtonProperty =
            DependencyProperty.Register( "ShowCloseButton", typeof( bool ), typeof( MetroWindow ), new UIPropertyMetadata( true ) );


        /// <summary>
        /// Gets or sets a value indicating whether the icon in the window title bar should be shown.
        /// </summary>
        public bool ShowIcon
        {
            get { return (bool) GetValue( ShowIconProperty ); }
            set { SetValue( ShowIconProperty, value ); }
        }

        /// <summary>
        /// See the <see cref="MetroControls.MetroWindow.ShowIcon"/> property.
        /// </summary>
        public static readonly DependencyProperty ShowIconProperty =
            DependencyProperty.Register( "ShowIcon", typeof( bool ), typeof( MetroWindow ), new UIPropertyMetadata( true ) );


        /// <summary>
        /// Gets or sets a value indicating whether the window title should be shown.
        /// </summary>
        public bool ShowTitle
        {
            get { return (bool) GetValue( ShowTitleProperty ); }
            set { SetValue( ShowTitleProperty, value ); }
        }

        /// <summary>
        /// See the <see cref="MetroControls.MetroWindow.ShowTitle"/> property.
        /// </summary>
        public static readonly DependencyProperty ShowTitleProperty =
            DependencyProperty.Register( "ShowTitle", typeof( bool ), typeof( MetroWindow ), new UIPropertyMetadata( true ) );


        /// <summary>
        /// Gets or sets the content of the <see cref="MetroControls.MetroWindow"/>'s border, shown between the title and the window buttons.
        /// </summary>
        public UIElement BorderContent
        {
            get { return (UIElement) GetValue( BorderContentProperty ); }
            set { SetValue( BorderContentProperty, value ); }
        }

        /// <summary>
        /// See the <see cref="MetroControls.MetroWindow.BorderContent" /> property.
        /// </summary>
        public static readonly DependencyProperty BorderContentProperty =
            DependencyProperty.Register( "BorderContent", typeof( UIElement ), typeof( MetroWindow ), new UIPropertyMetadata() );


        /// <summary>
        /// Gets or sets a value indicating whether the window title bar should be shown.
        /// </summary>
        public bool ShowTitleBar
        {
            get { return (bool) GetValue( ShowTitleBarProperty ); }
            set { SetValue( ShowTitleBarProperty, value ); }
        }

        /// <summary>
        /// See the <see cref="MetroControls.MetroWindow.ShowTitleBar"/> property.
        /// </summary>
        public static readonly DependencyProperty ShowTitleBarProperty =
            DependencyProperty.Register( "ShowTitleBar", typeof( bool ), typeof( MetroWindow ), new UIPropertyMetadata( true ) );


        /// <summary>
        /// Creates a new <see cref="MetroControls.MetroWindow"/>.
        /// </summary>
        public MetroWindow()
        {
            ColorManager.SetDefaultColors(); // just in case
            this.Style = (Style) Application.Current.Resources[typeof( MetroWindow )]; // no other way to do that...
        }

        /// <summary>
        /// Raises the SourceInitialized event.
        /// </summary>
        protected override void OnSourceInitialized( EventArgs e )
        {
            base.OnSourceInitialized( e );
            NativeMethods.FixWindowMaxSize( this );
        }
    }
}