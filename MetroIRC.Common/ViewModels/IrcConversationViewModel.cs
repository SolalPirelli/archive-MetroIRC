// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Collections.Generic;
using System.Linq;
using BasicMvvm;
using MetroIrc.Internals;
using IrcSharp;
using CM = System.ComponentModel;

namespace MetroIrc.ViewModels
{
    public abstract class IrcConversationViewModel : ObservableObject, IDisposable
    {
        #region Private members
        private IrcNetworkViewModel _networkVM;
        #endregion

        #region Property-backing fields
        private int _unreadMessagesCount;
        private bool _hasUnreadImportantMessage;
        private bool _isVisible;
        private bool _isSelected;
        #endregion

        #region Public properties
        public FixedSpaceCollection<Message> Messages { get; private set; }

        public int UnreadMessagesCount
        {
            get { return this._unreadMessagesCount; }
            set { this.SetProperty( ref this._unreadMessagesCount, value ); }
        }

        public bool HasUnreadImportantMessage
        {
            get { return this._hasUnreadImportantMessage; }
            set { this.SetProperty( ref this._hasUnreadImportantMessage, value ); }
        }

        public bool IsVisible
        {
            get { return this._isVisible; }
            set { this.SetProperty( ref this._isVisible, value ); }
        }

        public bool IsSelected
        {
            get { return this._isSelected; }
            set { this.SetProperty( ref this._isSelected, value ); }
        }

        // used for nick autocomplete
        public IEnumerable<string> UserNames
        {
            get { return this.Users.Select( u => u.Nickname ); }
        }
        #endregion

        #region Abstract/virtual properties
        public abstract string Title { get; }
        public abstract string TargetName { get; }
        public abstract Type ConversationType { get; }
        public virtual bool IsImportant
        {
            get { return false; }
        }
        public virtual bool CanBeClosed
        {
            get { return true; }
        }
        public virtual IEnumerable<IrcUser> Users
        {
            get { return Enumerable.Empty<IrcUser>(); }
        }
        #endregion

        protected IrcConversationViewModel( IrcNetworkViewModel networkVM )
        {
            int maxItemCount = Locator.Get<ISettings>().MaximumMessagesCount;
            this.Messages = new FixedSpaceCollection<Message>( maxItemCount, true );
            this._networkVM = networkVM;

            PropertyChangedEventManager.AddHandler( Locator.Get<ISettings>(), MaximumMessagesCountChanged, o => o.MaximumMessagesCount );
            PropertyChangedEventManager.AddHandler( this, This_IsSelectedChanged, o => o.IsSelected );
        }

        #region Public methods
        public void AddMessage( Message message )
        {
            if ( message.IsInformationMessage && !this.IsVisible )
            {
                return;
            }

            Locator.Get<IUIService>().Execute( () => this.Messages.Add( message ) );

            if ( !this.IsSelected )
            {
                if ( !message.IsInformationMessage )
                {
                    this.UnreadMessagesCount++;
                }
                if ( message.IsImportantMessage )
                {
                    this.HasUnreadImportantMessage = true;
                }
            }

            if ( !this.IsSelected || !this._networkVM.IsSelected || !Locator.Get<IUIService>().IsAppToForeground() )
            {
                Messenger.Send( new UnreadMessageReceivedMessage( this, message ) );
            }

            this.IsVisible = true;
        }
        #endregion

        #region Weak event handlers
        private void MaximumMessagesCountChanged( object sender, CM.PropertyChangedEventArgs e )
        {
            this.Messages.MaxItemCount = Locator.Get<ISettings>().MaximumMessagesCount;
        }

        private void This_IsSelectedChanged( object sender, CM.PropertyChangedEventArgs e )
        {
            this.HasUnreadImportantMessage = false;
            this.UnreadMessagesCount = 0;
        }
        #endregion

        #region IDisposable implementation
        public virtual void Dispose()
        {
            // nothing
        }
        #endregion
    }
}