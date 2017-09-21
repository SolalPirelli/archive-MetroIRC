// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace CommonStuff.Controls
{
    public sealed class TabPanelEx : TabControl
    {
        static TabPanelEx()
        {
            DefaultStyleKeyProperty.OverrideMetadata( typeof( TabPanelEx ), new FrameworkPropertyMetadata( typeof( TabPanelEx ) ) );
        }

        public object NewTabHeader
        {
            get { return (object) GetValue( NewTabHeaderProperty ); }
            set { SetValue( NewTabHeaderProperty, value ); }
        }

        public static readonly DependencyProperty NewTabHeaderProperty =
            DependencyProperty.Register( "NewTabHeader", typeof( object ), typeof( TabPanelEx ), new UIPropertyMetadata() );    
    }
}