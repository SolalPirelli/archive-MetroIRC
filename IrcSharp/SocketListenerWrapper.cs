// Copyright (C) 2012-2013, Solal Pirelli
// This code is licensed under a modified MIT License (see IRC#'s Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace IrcSharp.External
{
    /// <summary>
    /// A wrapper around a <see cref="TcpListener"/>.
    /// </summary>
    [ComVisible( false )]
    public sealed class SocketListenerWrapper : TcpListenerWrapper
    {
        private TcpListener _listener;

        public override void Listen( int port )
        {
            this._listener = new TcpListener( IPAddress.Any, port );
            this._listener.Start();
            this._listener.BeginAcceptSocket( AcceptSocketCallback, null );
        }

        private void AcceptSocketCallback( IAsyncResult result )
        {
            try
            {
                var socket = this._listener.EndAcceptSocket( result );
                var wrapper = new SocketWrapper( socket );
                this.OnConnectionReceived( wrapper );
            }
            catch
            {
                if ( !this.Disposed )
                {
                    this._listener.Start();
                }
            }

            if ( !this.Disposed )
            {
                this._listener.BeginAcceptSocket( AcceptSocketCallback, null );
            }
        }

        protected override void Dispose( bool isDisposing )
        {
            if ( !this.Disposed )
            {
                this._listener.Stop();
            }

            base.Dispose( isDisposing );
        }
    }
}