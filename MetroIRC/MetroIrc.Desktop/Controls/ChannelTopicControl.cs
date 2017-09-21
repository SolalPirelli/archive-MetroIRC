// Copyright (C) 2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BasicMvvm;
using CommonStuff;
using IrcSharp;
using MetroIrc.Services;
using CM = System.ComponentModel;

namespace MetroIrc.Desktop.Controls
{
    public sealed class ChannelTopicControl : Control, CM.INotifyPropertyChanged
    {
        #region Property-backing fields
        private string _oldTopic;
        private bool _showTopic;
        private string _topicInfo;
        private bool _showTopicInfo;
        private bool _isEditing;
        private string _temporaryTopic;
        #endregion

        #region Public properties
        public bool ShowTopic
        {
            get { return this._showTopic; }
            set
            {
                if ( this._showTopic != value )
                {
                    this._showTopic = value;
                    this.FirePropertyChanged();
                }
            }
        }

        public string TopicInfo
        {
            get { return this._topicInfo; }
            set
            {
                if ( this._topicInfo != value )
                {
                    this._topicInfo = value;
                    this.FirePropertyChanged();
                }
            }
        }

        public bool ShowTopicInfo
        {
            get { return this._showTopicInfo; }
            set
            {
                if ( this._showTopicInfo != value )
                {
                    this._showTopicInfo = value;
                    this.FirePropertyChanged();
                }
            }
        }

        public bool IsEditing
        {
            get { return this._isEditing; }
            set
            {
                if ( this._isEditing != value )
                {
                    this._isEditing = value;
                    this.FirePropertyChanged();
                }
            }
        }

        public string TemporaryTopic
        {
            get { return this._temporaryTopic; }
            set
            {
                if ( this._temporaryTopic != value )
                {
                    this._temporaryTopic = value;
                    this.FirePropertyChanged();
                }
            }
        }
        #endregion

        #region Channel DependencyProperty
        public IrcChannel Channel
        {
            get { return (IrcChannel) GetValue( ChannelProperty ); }
            set { SetValue( ChannelProperty, value ); }
        }

        public static readonly DependencyProperty ChannelProperty =
            DependencyProperty.Register( "Channel", typeof( IrcChannel ), typeof( ChannelTopicControl ), new PropertyMetadata( OnChannelPropertyChanged ) );

        private static void OnChannelPropertyChanged( DependencyObject obj, DependencyPropertyChangedEventArgs args )
        {
            var control = (ChannelTopicControl) obj;

            if ( args.OldValue != null )
            {
                var chan = (IrcChannel) args.OldValue;
                PropertyChangedEventManager.RemoveHandler( chan.Topic, control.Channel_TopicChanged, o => o.Text );
                PropertyChangedEventManager.RemoveHandler( chan.Topic, control.Channel_TopicInfoChanged, o => o.Setter );
                PropertyChangedEventManager.RemoveHandler( chan.Topic, control.Channel_TopicInfoChanged, o => o.SetDate );
            }
            if ( args.NewValue != null )
            {
                var chan = (IrcChannel) args.NewValue;

                PropertyChangedEventManager.AddHandler( chan.Topic, control.Channel_TopicChanged, o => o.Text );
                PropertyChangedEventManager.AddHandler( chan.Topic, control.Channel_TopicInfoChanged, o => o.Setter );
                PropertyChangedEventManager.AddHandler( chan.Topic, control.Channel_TopicInfoChanged, o => o.SetDate );

                control.ShowTopic = chan.Topic.Text.HasText();
                control.ShowTopicInfo = chan.Topic.SetDate != null;
                control.UpdateTopicInfo();
            }
        }
        #endregion

        #region CanEdit DependencyProperty
        public bool CanEdit
        {
            get { return (bool) GetValue( CanEditProperty ); }
            set { SetValue( CanEditProperty, value ); }
        }

        public static readonly DependencyProperty CanEditProperty =
            DependencyProperty.Register( "CanEdit", typeof( bool ), typeof( ChannelTopicControl ), new PropertyMetadata( false ) );
        #endregion

        #region Commands
        public ICommand EditCommand
        {
            get { return new RelayCommand( _ => this.BeginEdit() ); }
        }

        public ICommand FinishEditCommand
        {
            get { return new RelayCommand( _ => this.EndEdit() ); }
        }

        public ICommand CancelEditCommand
        {
            get { return new RelayCommand( CancelEditCommandExecuted ); }
        }

        private void CancelEditCommandExecuted( object parameter )
        {
            this.TemporaryTopic = this._oldTopic;
            this.EndEdit();
        }
        #endregion

        #region Private methods
        private void BeginEdit()
        {
            this._oldTopic = MessageSender.ReverseProcessing( this.Channel.Topic.Text );
            this.TemporaryTopic = this._oldTopic;
            this.IsEditing = true;
        }

        private void EndEdit()
        {
            if ( this.TemporaryTopic != this._oldTopic )
            {
                this.Channel.SetTopic( MessageSender.ProcessText( this.TemporaryTopic ) );
            }
            this.IsEditing = false;
        }

        private void UpdateTopicInfo()
        {
            if ( this.Channel.Topic.Setter == null || this.Channel.Topic.SetDate == null )
            {
                this.ShowTopicInfo = false;
                this.TopicInfo = null;
            }
            else
            {
                this.ShowTopicInfo = true;
                string date = this.Channel.Topic.SetDate.Value.ToString( "F" );
                this.TopicInfo = Locator.Get<ITranslationService>().Translate( "ChannelTopic", "TopicInfo", this.Channel.Topic.Setter.Nickname, date );
            }
        }
        #endregion

        #region Weak event handlers
        private void Channel_TopicChanged( object sender, CM.PropertyChangedEventArgs args )
        {
            // we're on a separate thread, so we have to use the dispatcher
            Locator.Get<IUIService>().Execute( () => this.ShowTopic = this.Channel.Topic.Text.HasText() );
        }

        private void Channel_TopicInfoChanged( object sender, CM.PropertyChangedEventArgs args )
        {
            // we're on a separate thread, blah blah blah
            Locator.Get<IUIService>().Execute( this.UpdateTopicInfo );
        }
        #endregion

        #region INotifyPropertyChanged implementation
        public event CM.PropertyChangedEventHandler PropertyChanged;
        private void FirePropertyChanged( [CallerMemberName] string propertyName = "" )
        {
            if ( this.PropertyChanged != null )
            {
                this.PropertyChanged( this, new CM.PropertyChangedEventArgs( propertyName ) );
            }
        }
        #endregion
    }
}