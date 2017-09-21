// Copyright (C) 2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using IrcSharp;

namespace MetroIrc.Log
{
    public abstract class IrcLogger : IDisposable
    {
        protected IrcClient Client { get; private set; }

        public abstract IEnumerable<string> LastMessages { get; }

        public IEnumerable<IrcChannel> Channels
        {
            get { return this.Client.Network.CurrentUser.Channels; }
        }

        #region Static properties
        private static Dictionary<string, IrcLogger> _currentLoggers = new Dictionary<string, IrcLogger>();
        public static ReadOnlyDictionary<string, IrcLogger> CurrentLoggers
        {
            get { return new ReadOnlyDictionary<string, IrcLogger>( _currentLoggers ); }
        }
        #endregion

        #region Abstract methods
        protected abstract void Log( string text, MessageType type );
        #endregion

        #region Public methods
        public virtual void Start( IrcClient client )
        {
            this.Client = client;
            this.Client.RawDataReceived += Client_RawDataReceived;
            this.Client.RawDataSent += Client_RawDataSent;
            this.Client.Network.PropertyChanged += Network_PropertyChanged;

            _currentLoggers.Add( this.Client.Network.HostName, this );
        }

        public void LogInfo( string text )
        {
            this.Log( text, MessageType.InfoMessage );
        }
        #endregion

        #region IDisposable implementation
        public virtual void Dispose()
        {
            this.Client.RawDataReceived -= Client_RawDataReceived;
            this.Client.RawDataSent -= Client_RawDataSent;

            _currentLoggers.Remove( this.Client.Network.HostName );
        }
        #endregion

        #region Event handlers
        private void Client_RawDataReceived( object sender, RawDataEventArgs e )
        {
            this.Log( e.Data, MessageType.Received );
        }

        private void Client_RawDataSent( object sender, RawDataEventArgs e )
        {
            this.Log( e.Data, MessageType.Sent );
        }

        private void Network_PropertyChanged( object sender, PropertyChangedEventArgs e )
        {
            if ( e.PropertyName == "ConnectionStatus" )
            {
                string text = "Connection status changed: " + this.Client.Network.ConnectionStatus.ToString();
                this.Log( text, MessageType.InfoMessage );
            }
        }
        #endregion

        protected enum MessageType
        {
            Sent,
            Received,
            InfoMessage
        }
    }
}