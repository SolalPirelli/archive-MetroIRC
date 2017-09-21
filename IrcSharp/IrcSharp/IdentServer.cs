// Copyright (C) 2012, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Linq;
using IrcSharp.Internals;
using IrcSharp.Validation;

namespace IrcSharp
{
    /// <summary>
    /// A simple ident server.
    /// Useless for identification, but most servers query ident and timeout after a few seconds.
    /// Enabling this (which may require port forwarding on the client) dramatically reduces the connection time.
    /// </summary>
    public static class IdentServer
    {
        #region Constants
        private const int IdentPort = 113;
        // Let's pretend we're running on UNIX to simplify things.
        private const string ReplyString = "{0} : USERID : UNIX : {1}";
        #endregion

        #region Private members
        private static TcpListenerWrapper _listener;
        #endregion

        #region Internal event
        /// <summary>
        /// Occurs when an user name is needed.
        /// </summary>
        internal static event EventHandler<UserNameNeededEventArgs> UserNameNeeded;
        private static string OnUserNameNeeded()
        {
            if ( UserNameNeeded != null )
            {
                var e = new UserNameNeededEventArgs();
                UserNameNeeded( null, e );
                return e.UserName;
            }
            return null;
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Starts listening for incoming connections.
        /// </summary>
        /// <param name="wrapper">The <see cref="TcpListenerWrapper"/> used to listen for incoming connections.</param>
        public static void Start( TcpListenerWrapper wrapper )
        {
            Validate.IsNotNull( wrapper, "wrapper" );

            _listener = wrapper;
            _listener.Listen( IdentPort );
            _listener.ConnectionReceived += TcpListenerWrapper_ConnectionReceived;
        }

        /// <summary>
        /// Stops listening for incoming connections.
        /// </summary>
        /// <param name="disposeListener">Optional. Whether to dispose of the <see cref="TcpListenerWrapper"/> that was used in the <see cref="Start"/> method. True by default.</param>
        public static void Stop( bool disposeListener = true )
        {
            _listener.ConnectionReceived -= TcpListenerWrapper_ConnectionReceived;

            if ( disposeListener )
            {
                _listener.Dispose();
            }
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Gets a name to be sent back to the server.
        /// </summary>
        private static string GetName()
        {
            string name = OnUserNameNeeded();
            name = GetUserName( name );

            if ( name.HasText() )
            {
                return name;
            }
            return 'i' + new Random().Next( 0, int.MaxValue ).ToString();
        }

        /// <summary>
        /// Gets a valid username from a requested username.
        /// The returned value may be empty if the requested username contains no alphanumeric characters.
        /// </summary>
        private static string GetUserName( string requestedName )
        {
            if ( requestedName == null )
            {
                return null;
            }
            return new string( requestedName.ToCharArray().Where( CharHelper.IsBasicLetter ).ToArray() );
        }
        #endregion

        #region Event handlers
        private static void TcpListenerWrapper_ConnectionReceived( object sender, ConnectionReceivedEventArgs e )
        {
            e.ConnectionWrapper.LineReceived += TcpWrapper_LineReceived;
        }

        private static void TcpWrapper_LineReceived( object sender, RawDataEventArgs e )
        {
            var wrapper = (TcpWrapper) sender;
            // let's hope the message is well-formed
            string reply = string.Format( ReplyString, e.Data, GetName() );
            wrapper.SendLine( reply );

            wrapper.LineReceived -= TcpWrapper_LineReceived;
            wrapper.Dispose();
        }
        #endregion
    }
}