// Copyright (C) 2012-2013, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace IrcSharp.Tests.TestInternals
{
    public sealed class TestTcpWrapper : TcpWrapper
    {
        /// <summary>
        /// Gets a list of lines that were "sent".
        /// </summary>
        public List<string> LinesSent { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="TestTcpWrapper"/> was disposed.
        /// </summary>
        public new bool Disposed { get; private set; }

        public TestTcpWrapper()
            : base( "example.org", 6667, false, false )
        {
            this.IsConnected = true;
            this.LinesSent = new List<string>();
        }

        /// <summary>
        /// Pretends to receive a line of text.
        /// </summary>
        public void ReceiveLine( string line )
        {
            this.OnLineReceived( line );
        }

        /// <summary>
        /// Pretends to be authenticated
        /// </summary>
        public void EndAuthenticate()
        {
            this.OnLineReceived( "002 you : Your host isn't even a server, since you're not connected." );
        }

        /// <summary>
        /// Pretends that the connection was closed.
        /// </summary>
        public void Disconnect()
        {
            this.OnConnectionClosed();
        }

        #region TcpWrapper implementation
        /// <summary>
        /// Does nothing.
        /// </summary>
        public override Task<bool> ConnectAsync()
        {
            return Task.FromResult( true );
        }

        /// <summary>
        /// Do not use this method.
        /// </summary>
        public override void SendLine( string line )
        {
            this.LinesSent.Add( line );
        }

        protected override void Dispose( bool isDisposing )
        {
            base.Dispose( isDisposing );
            this.Disposed = true;
        }
        #endregion
    }
}