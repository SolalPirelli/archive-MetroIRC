using System.Collections.Generic;
using IrcSharp;

namespace MetroIrc.Common.Tests.Internals
{
    public sealed class TestTcpWrapper : TcpWrapper
    {
        /// <summary>
        /// Gets a list of lines that were "sent".
        /// </summary>
        public List<string> LinesSent { get; private set; }

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
        /// Pretends to be connected.
        /// </summary>
        public void EndConnect()
        {
            this.OnConnected();
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
        public override void Connect()
        {
            return;
        }

        /// <summary>
        /// Do not use this method.
        /// </summary>
        public override void SendLine( string line )
        {
            this.LinesSent.Add( line );
        }
        #endregion
    }
}