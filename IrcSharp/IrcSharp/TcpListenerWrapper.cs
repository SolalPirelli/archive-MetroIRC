// Copyright (C) 2012, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;

namespace IrcSharp
{
    /// <summary>
    /// A wrapper around a TCP listener.
    /// </summary>
    public abstract class TcpListenerWrapper : IDisposable
    {
        /// <summary>
        /// Starts listening for incoming connections on the specified port.
        /// </summary>
        public abstract void Listen( int port );

        /// <summary>
        /// Occurs when an incoming connection is received.
        /// </summary>
        public event EventHandler<ConnectionReceivedEventArgs> ConnectionReceived;
        /// <summary>
        /// Fires the <see cref="TcpListenerWrapper.ConnectionReceived"/> event.
        /// </summary>
        protected void OnConnectionReceived( TcpWrapper wrapper )
        {
            if ( this.ConnectionReceived != null )
            {
                this.ConnectionReceived( this, new ConnectionReceivedEventArgs( wrapper ) );
            }
        }

        #region IDisposable implementation
        /// <summary>
        /// Gets a value indicating whether this instance was disposed.
        /// </summary>
        protected bool Disposed { get; private set; }

        /// <summary>
        /// Finalizes this instance.
        /// </summary>
        ~TcpListenerWrapper()
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
                this.Disposed = true;
            }
        }
        #endregion
    }
}