// Copyright (C) 2012-2013, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

namespace IrcSharp.Tests.TestInternals
{
    public sealed class TestTcpListenerWrapper : TcpListenerWrapper
    {
        /// <summary>
        /// Gets a value indicating whether the <see cref="TestTcpListenerWrapper"/> was disposed.
        /// </summary>
        public new bool Disposed { get; set; }

        /// <summary>
        /// Pretends to add a client.
        /// </summary>
        public void AddClient( TcpWrapper wrapper )
        {
            this.OnConnectionReceived( wrapper );
        }

        #region TcpListenerWrapper implementation
        /// <summary>
        /// Do not use this method.
        /// </summary>
        public override void Listen( int port )
        {
            return;
        }

        protected override void Dispose( bool isDisposing )
        {
            base.Dispose( isDisposing );
            this.Disposed = true;
        }
        #endregion
    }
}