// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System.Windows;
using System.Windows.Controls;
using IrcSharp;

namespace MetroIrc.Desktop.Controls
{
    /// <summary>
    /// A simple control that represents an IRC user, including a context menu with various actions.
    /// </summary>
    public sealed class IrcUserControl : Control
    {
        #region User DependencyProperty
        public IrcUser User
        {
            get { return (IrcUser) GetValue( UserProperty ); }
            set { SetValue( UserProperty, value ); }
        }

        public static readonly DependencyProperty UserProperty =
            DependencyProperty.Register( "User", typeof( IrcUser ), typeof( IrcUserControl ), new PropertyMetadata( null ) );
        #endregion

        #region DisplayName DependencyProperty
        public string DisplayName
        {
            get { return (string) GetValue( DisplayNameProperty ); }
            set { SetValue( DisplayNameProperty, value ); }
        }

        public static readonly DependencyProperty DisplayNameProperty =
            DependencyProperty.Register( "DisplayName", typeof( string ), typeof( IrcUserControl ), new PropertyMetadata( null ) );
        #endregion

        #region TextAlignment DependencyProperty
        public TextAlignment TextAlignment
        {
            get { return (TextAlignment) GetValue( TextAlignmentProperty ); }
            set { SetValue( TextAlignmentProperty, value ); }
        }

        public static readonly DependencyProperty TextAlignmentProperty =
            DependencyProperty.Register( "TextAlignment", typeof( TextAlignment ), typeof( IrcUserControl ), new PropertyMetadata( TextAlignment.Left ) );
        #endregion
    }
}