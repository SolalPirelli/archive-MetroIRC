// Copyright (C) 2011-2013, Solal Pirelli
// This code is licensed under a modified MIT License (see IRC#'s Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace IrcSharp.External
{
    /// <summary>
    /// A wrapper around a <see cref="Socket"/> that provides event for connection to a server and sending/receiving messages using a specified encoding.
    /// </summary>
    [ComVisible( false )]
    public sealed class SocketWrapper : TcpWrapper
    {
        #region Constants
        private const int BufferSize = short.MaxValue;
        private const int ConnectionTimeout = 2000; // ms
        // I can't believe string.Split() still needs an array in .NET 4...
        private static readonly string[] NewLineArray = { Environment.NewLine };
        #endregion

        #region Private members
        private Socket _socket;
        private Stream _stream;
        private string _unfinishedMessage = string.Empty;
        #endregion

        /// <summary>
        /// Creates a SocketWrapper from connection parameters.
        /// The SocketWrapper does *not* connect after initialization.
        /// </summary>
        public SocketWrapper( string hostName, int port, bool useSsl = false, bool acceptInvalidCertificates = false )
            : base( hostName, port, useSsl, acceptInvalidCertificates )
        {
        }

        /// <summary>
        /// Creates a SocketWrapper from an existing socket.
        /// </summary>
        public SocketWrapper( Socket socket )
        {
            this._socket = socket;
            this._stream = new NetworkStream( socket );
            this.IsConnected = true;
            this.Read();
        }

        #region Public methods
        /// <summary>
        /// Connects to the server.
        /// </summary>
        public override async Task<bool> ConnectAsync()
        {
            this._socket = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
            this._socket.ReceiveBufferSize = BufferSize;

            try
            {
                var task = Task.Factory.FromAsync( this._socket.BeginConnect, this._socket.EndConnect, this.HostName, this.Port, null );

                // Elegant way to wait for a task with a timeout
                if ( await Task.WhenAny( task, Task.Delay( ConnectionTimeout ) ) == task )
                {
                    this._stream = new NetworkStream( this._socket );

                    if ( this.UsesSsl )
                    {
                        var sslStream = new SslStream( this._stream, false, ValidateCertificateCallback );
                        sslStream.AuthenticateAsClient( this.HostName );
                        this._stream = sslStream;
                    }

                    this.IsConnected = true;
                    this.Read();
                    return true;
                }
            }
            catch
            {
                // many weird errors can happen
            }

            this.Stop( false );
            return false;
        }

        /// <summary>
        /// Sends string data to the server.
        /// </summary>
        /// <param name="line">A string that should be encoded using this SocketWrapper's Encoding.</param>
        public override void SendLine( string line )
        {
            this.TryDo( () =>
            {
                byte[] bytes = this.Encoding.GetBytes( line + Environment.NewLine );
                this._stream.Write( bytes, 0, bytes.Length );
                this._stream.Flush();
            } );
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Fired when a SSL certificate needs to be validated.
        /// </summary>
        private bool ValidateCertificateCallback( object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors )
        {
            return this.AcceptsInvalidCertificates || sslPolicyErrors == SslPolicyErrors.None;
        }

        /// <summary>
        /// Asynchronously reads data from the stream, forever.
        /// </summary>
        private async void Read()
        {
            while ( this.IsConnected )
            {
                byte[] buffer = new byte[BufferSize];
                int bytesCount;

                try
                {
                    bytesCount = await this._stream.ReadAsync( buffer, 0, BufferSize );
                }
                catch ( Exception e )
                {
                    if ( e is IOException || e is SocketException )
                    {
                        this.Stop( true );
                        return;
                    }
                    if ( e is ObjectDisposedException )
                    {
                        return;
                    }
                    throw;
                }

                foreach ( string line in this.GetLines( buffer, bytesCount ) )
                {
                    this.OnLineReceived( line );
                }
            }
        }

        /// <summary>
        /// Gets lines of text from received bytes, updating the "unfinished" string if needed.
        /// </summary>
        private IEnumerable<string> GetLines( byte[] receivedBytes, int count )
        {
            if ( count == 0 )
            {
                return Enumerable.Empty<string>();
            }

            string text = this._unfinishedMessage + this.Encoding.GetString( receivedBytes, 0, count );
            string[] lines = text.Split( NewLineArray, StringSplitOptions.RemoveEmptyEntries );
            if ( text.EndsWith( Environment.NewLine ) )
            {
                this._unfinishedMessage = string.Empty;
                return lines;
            }

            this._unfinishedMessage = lines.Last();
            return lines.Take( lines.Length - 1 );
        }

        /// <summary>
        /// Surrounds an action with a try..catch that matches IOExceptions, SocketExceptions and ObjectDisposedExceptions.
        /// </summary>
        private bool TryDo( Action action )
        {
            try
            {
                action();
                return true;
            }
            catch ( Exception e )
            {
                if ( e is IOException || e is SocketException || e is AuthenticationException )
                {
                    this.Stop( true );
                    return false;
                }
                if ( e is ObjectDisposedException )
                {
                    return false;
                }
                throw;
            }
        }

        /// <summary>
        /// Stop! Hammer time.
        /// </summary>
        private void Stop( bool fireEvent )
        {
            this.IsConnected = false;
            this._unfinishedMessage = string.Empty;

            if ( this._stream != null )
            {
                this._stream.Close();
            }
            if ( this._socket != null )
            {
                this._socket.Close();
            }

            if ( fireEvent )
            {
                this.OnConnectionClosed();
            }
        }
        #endregion

        #region IDisposable implementation
        /// <summary>
        /// Disposes of this instance.
        /// </summary>
        protected override void Dispose( bool isDisposing )
        {
            if ( !this.Disposed )
            {
                if ( this._stream != null )
                {
                    this._stream.Dispose();
                }

                if ( this._socket != null )
                {
                    if ( this._socket.Connected )
                    {
                        this._socket.Disconnect( false );
                    }
                    this._socket.Dispose();
                }
            }

            base.Dispose( isDisposing );
        }
        #endregion
    }
}