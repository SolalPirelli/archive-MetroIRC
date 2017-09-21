// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Collections.Generic;
using System.IO;
using IrcSharp;
using MetroIrc.Log;

namespace MetroIrc.Desktop.Log
{
    /// <summary>
    /// A simple logger for IRC networks, logging the raw data that is received or sent by the client.
    /// </summary>
    public sealed class WpfIrcLogger : IrcLogger
    {
        #region Constants
#if DEBUG
        private static readonly string LogFolderPath = Path.Combine( App.Current.DataFolderPath, "Logs" );
#endif

        private static Dictionary<MessageType, string> Prefixes = new Dictionary<MessageType, string>
        {
            { MessageType.Received, "IN  : " },
            { MessageType.Sent, "OUT : " },
            { MessageType.InfoMessage, "INFO: " }
        };

        private const int InMemoryMessagesCount = 10;
        #endregion

        #region Private members
        private Queue<string> _lastMessages;

#if DEBUG
        private StreamWriter _writer;
#endif
        #endregion

        #region Public properties
        public override IEnumerable<string> LastMessages
        {
            get { return this._lastMessages; }
        }
        #endregion

        public WpfIrcLogger()
        {
            this._lastMessages = new Queue<string>();
        }

        #region Methods
        public override void Start( IrcClient client )
        {
            base.Start( client );
#if DEBUG
            this.InitializeWriter();
#endif
            this.LogInfo( "Log started." );
        }

#if DEBUG
        private void InitializeWriter()
        {
            if ( !Directory.Exists( LogFolderPath ) )
            {
                Directory.CreateDirectory( LogFolderPath );
            }

            string originalPath = Path.Combine( LogFolderPath, this.Client.Network.HostName );
            string path = originalPath + ".log";
            int retryCount = 1;

            do
            {
                try
                {
                    this._writer = File.AppendText( path );
                }
                catch ( IOException )
                {
                    path = string.Format( "{0} ({1}).log", originalPath, retryCount );
                    retryCount++;
                }
            } while ( this._writer == null );

            this._writer.AutoFlush = true;
        }
#endif

        protected override void Log( string text, MessageType type )
        {
            string prefix = Prefixes[type];
            string message = DateTime.Now.ToLongTimeString() + " " + prefix + " " + text;
#if DEBUG
            try
            {
                this._writer.WriteLine( message );
            }
            catch
            {
                // Some weird stuff happens when quitting networks...
                this.Dispose();
            }
#endif

            this._lastMessages.Enqueue( message );
            if ( this._lastMessages.Count > InMemoryMessagesCount )
            {
                this._lastMessages.Dequeue();
            }
        }
        #endregion

        #region IDisposable implementation
#if DEBUG
        public override void Dispose()
        {
            base.Dispose();
            try
            {
                this._writer.Dispose();
            }
            catch { }
        }
#endif
        #endregion
    }
}