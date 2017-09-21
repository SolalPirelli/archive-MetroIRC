// Copyright (C) 2012, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Text;
using System.Threading.Tasks;
using IrcSharp.Validation;

namespace IrcSharp
{
    /// <summary>
    /// A wrapper around a TCP client which sends and receives lines of text.
    /// </summary>
    public abstract class TcpWrapper : IDisposable
    {
        #region Public properties
        /// <summary>
        /// Gets the host name of the server the <see cref="TcpWrapper"/> is connected to.
        /// </summary>
        public string HostName { get; private set; }

        /// <summary>
        /// Gets the port of the server the <see cref="TcpWrapper"/> is connected to.
        /// </summary>
        public int Port { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the connection to the server is made via SSL.
        /// </summary>
        public bool UsesSsl { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether invalid SSL certificates are accepted.
        /// </summary>
        /// <remarks>This has no effect if <see cref="UsesSsl"/> is set to false.</remarks>
        public bool AcceptsInvalidCertificates { get; set; }

        /// <summary>
        /// Gets or sets the encoding used to decrypt received messages and encrypt sent ones.
        /// </summary>
        public Encoding Encoding { get; set; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="TcpWrapper"/> is connected.
        /// </summary>
        public bool IsConnected { get; protected set; }
        #endregion

        /// <summary>
        /// Creates a <see cref="TcpWrapper"/>.
        /// </summary>
        protected TcpWrapper()
        {
            this.Encoding = Encoding.UTF8;
        }

        /// <summary>
        /// Creates a <see cref="TcpWrapper"/> from connection parameters.
        /// The <see cref="TcpWrapper"/> does *not* connect after initialization.
        /// </summary>
        protected TcpWrapper( string hostName, int port, bool useSsl, bool acceptInvalidCertificates )
            : this()
        {
            Validate.HasText( hostName, "hostName" );
            Validate.IsPositive( port, "port" );

            this.HostName = hostName;
            this.Port = port;
            this.UsesSsl = useSsl;
            this.AcceptsInvalidCertificates = acceptInvalidCertificates;
        }

        #region Events
        /// <summary>
        /// Occurs when a line of text is received from the server
        /// </summary>
        public event EventHandler<RawDataEventArgs> LineReceived;
        /// <summary>
        /// Fires the <see cref="LineReceived"/> event.
        /// </summary>
        protected void OnLineReceived( string line )
        {
            if ( this.LineReceived != null )
            {
                this.LineReceived( this, new RawDataEventArgs( line ) );
            }
        }

        /// <summary>
        /// Occurs when the connection is closed, by the client or by the server.
        /// </summary>
        public event EventHandler ConnectionClosed;
        /// <summary>
        /// Fires the <see cref="ConnectionClosed"/> event.
        /// </summary>
        protected void OnConnectionClosed()
        {
            if ( this.ConnectionClosed != null )
            {
                this.ConnectionClosed( this, EventArgs.Empty );
            }
        }
        #endregion

        #region Abstract methods
        /// <summary>
        /// Attempts to connect to the server.
        /// </summary>
        public abstract Task<bool> ConnectAsync();

        /// <summary>
        /// Sends a line of text to the server.
        /// </summary>
        public abstract void SendLine( string line );
        #endregion

        #region IDisposable implementation
        /// <summary>
        /// Gets a value indicating whether this instance was disposed.
        /// </summary>
        protected bool Disposed { get; private set; }

        /// <summary>
        /// Finalizes this instance.
        /// </summary>
        ~TcpWrapper()
        {
            this.Dispose( false );
        }

        /// <summary>
        /// Disposes of the <see cref="TcpWrapper"/>'s resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose( true );
            GC.SuppressFinalize( this );
        }

        /// <summary>
        /// Disposes of the <see cref="TcpWrapper"/>'s managed and/or unmanaged resources.
        /// </summary>
        protected virtual void Dispose( bool isDisposing )
        {
            if ( !this.Disposed )
            {
                this.IsConnected = false;
                this.Disposed = true;
            }
        }
        #endregion
    }
}