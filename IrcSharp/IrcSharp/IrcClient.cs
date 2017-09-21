// Copyright (C) 2011-2013, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IrcSharp.Ctcp;
using IrcSharp.Internals;
using IrcSharp.Validation;
using CommandHandler = System.Action<IrcSharp.IrcClient, IrcSharp.Internals.IrcMessage>;

namespace IrcSharp
{
    /// <summary>
    /// A class that communicates with an IRC server.
    /// </summary>
    public sealed partial class IrcClient
    {
        #region Constants
        /// <summary>
        /// All numeric codes above and including this are errors.
        /// </summary>
        private const int ErrorCodesBeginning = 400;

        // Config:

        /// <summary>
        /// The maximum time, in milliseconds, to wait between a JOIN command is sent and a NAMES message is received for a channel.
        /// If no NAMES message is received, the IrcClient will send a NAMES query.
        /// </summary>
        private const int JoinNamesDelay = 200;

        /// <summary>
        /// The time, in milliseconds, to wait between two PINGs sent to the server.
        /// </summary>
        private const int PingDelay = 30000;

        /// <summary>
        /// The maximum time, in milliseconds, to wait between a sent PING and its PONG response.
        /// </summary>
        private const int PongDelay = 10000;
        #endregion

        #region Private static members
        /// <summary>
        /// The IRC commands such as JOIN, NICK, 123...
        /// </summary>
        private static Dictionary<string, CommandHandler> _commandHandlers;

        /// <summary>
        /// These commands are invoked prior to invoking IrcCommandHandlers.
        /// Their purpose is to fix the parameters, because some servers have bugs (e.g. omitting separators).
        /// </summary>
        private static Dictionary<string, CommandHandler> _compatibilityCommandHandlers;
        #endregion

        #region Private members
        /// <summary>
        /// The <see cref="TcpWrapper"/> used to send and receive messages from the server.
        /// </summary>
        private readonly TcpWrapper _client;

        /// <summary>
        /// The data sent with the pings to the server as key, the date they were sent as value.
        /// </summary>
        private readonly List<string> _pingData = new List<string>();

        /// <summary>
        /// The list of channels JOINed without a NAMES reply from the server afterwards.
        /// </summary>
        private readonly List<IrcChannel> _delayedNamesChannels = new List<IrcChannel>();

        /// <summary>
        /// The temporary information related to this client.
        /// </summary>
        private readonly TemporaryInformation _tempInfo = new TemporaryInformation();

        /// <summary>
        /// Whether this instance has been disposed.
        /// </summary>
        private bool _disposed;
        #endregion

        #region Public properties
        /// <summary>
        /// Gets the <see cref="IrcNetwork"/> the <see cref="IrcClient"/> is connected to.
        /// </summary>
        public IrcNetwork Network { get; private set; }

        /// <summary>
        /// Gets the <see cref="CtcpClient"/> associated with the <see cref="IrcClient"/>.
        /// </summary>
        public CtcpClient Ctcp { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="Encoding"/> used by the <see cref="IrcClient"/>.
        /// </summary>
        public Encoding Encoding
        {
            get { return this._client.Encoding; }
            set { this._client.Encoding = value; }
        }

        /// <summary>
        /// Gets or sets the user name the <see cref="IdentServer"/> will send if needed.
        /// This should be set if the <see cref="IdentServer"/> is started.
        /// Non-ASCII characters should be avoided and will be stripped from the name.
        /// If this property is not set, a random username will be sent instead.
        /// </summary>
        public string IdentUserName { get; set; }
        #endregion

        #region Public static properties
        /// <summary>
        /// Gets or sets a value that indicates whether <see cref="IrcClient"/>s will retry another nickname in case of a nickname collision during authentication.
        /// True by default.
        /// </summary>
        public static bool RetryOnNicknameCollision { get; set; }

        /// <summary>
        /// Gets or sets a function that is used when a nickname collision is detected during authentication.
        /// The input parameter is the current nickname, the output value is the nickname used to retry.
        /// By default, an underline '_' char is appended.
        /// </summary>
        public static Func<string, string> NicknameCollisionTransform { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether <see cref="IrcClient"/>s will attempt to rejoin an <see cref="IrcChannel"/> in case the current <see cref="IrcUser"/> gets kicked from it.
        /// True by default.
        /// </summary>
        public static bool RejoinOnKick { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to strip the non-standard formatting codes used by mIRC and other clients.
        /// The default is false; set this to true if your client does not support them.
        /// </summary>
        public static bool StripFormatting { get; set; }
        #endregion

        static IrcClient()
        {
            RetryOnNicknameCollision = true;
            NicknameCollisionTransform = nick => nick + "_";
            RejoinOnKick = true;
            StripFormatting = false;

            InitializeHandlers();
        }

        internal IrcClient( IrcNetwork network, TcpWrapper wrapper )
        {
            this.Network = network;
            this._client = wrapper;
            this._client.LineReceived += Client_DataReceived;
            this._client.ConnectionClosed += Client_ConnectionClosed;

            this.Ctcp = new CtcpClient();

            IdentServer.UserNameNeeded += IdentServer_UserNameNeeded;
        }

        #region Events
        /// <summary>
        /// Occurs when raw data is received from the server.
        /// </summary>
        public event ClientEventHandler<RawDataEventArgs> RawDataReceived;
        private void OnRawDataReceived( string rawData )
        {
            this.Raise( this.RawDataReceived, new RawDataEventArgs( rawData ) );
        }

        /// <summary>
        /// Occurs when raw data is sent to the server.
        /// </summary>
        public event ClientEventHandler<RawDataEventArgs> RawDataSent;
        private void OnRawDataSent( string rawData )
        {
            this.Raise( this.RawDataSent, new RawDataEventArgs( rawData ) );
        }

        /// <summary>
        /// Occurs when an unknown command is received.
        /// If this event is not handled, the <see cref="IrcClient"/> will handle it as an information message or an error depending on the command.
        /// </summary>
        public event ClientEventHandler<UnknownCommandReceivedEventArgs> UnknownCommandReceived;
        private bool OnUnknownCommandReceived( IrcMessage message )
        {
            var e = new UnknownCommandReceivedEventArgs( message );
            this.Raise( this.UnknownCommandReceived, e );
            return e.Handled;
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Sends raw data. Be careful.
        /// </summary>
        /// <param name="data">The data to be sent. Newlines are not allowed.</param>
        public void SendRawData( string data )
        {
            Validate.HasText( data, "data" );
            Validate.IsNotDisposed( this._disposed );
            IrcValidate.IsConnected( this.Network.IsConnected );

            data = data.TrimEnd().Replace( Environment.NewLine, string.Empty );
            this._client.SendLine( data );
            this.OnRawDataSent( data );
        }
        #endregion

        #region Internal methods
        /// <summary>
        /// Asynchronously connects to the server.
        /// </summary>
        internal Task<bool> ConnectAsync()
        {
            Validate.IsNotDisposed( this._disposed );

            return this._client.ConnectAsync();
        }

        /// <summary>
        /// Stop! Hammer time.
        /// </summary>
        internal void Stop( bool withError )
        {
            Validate.IsNotDisposed( this._disposed );

            this.Network.IsConnected = false;
            this.Network.IsAuthenticated = false;

            if ( withError )
            {
                this.Network.OnConnectionLost();
            }

            this._pingData.Clear();
            this._delayedNamesChannels.Clear();
        }
        #endregion

        #region Private static methods
        /// <summary>
        /// Initializes IrcCommands and CompatibilityCommands.
        /// </summary>
        private static void InitializeHandlers()
        {
            var allMethods = ReflectionHelper.GetAttributedMethods<IrcCommandAttribute>( typeof( IrcClient.CommandHandlers ) ).ToArray();

            _commandHandlers = allMethods.Where( tup => !tup.Item1.IsCompatibilityCommand )
                                         .ToDictionary( tup => tup.Item1.Command,
                                                        tup => tup.Item2.GetStaticDelegate<CommandHandler>() );

            _compatibilityCommandHandlers = allMethods.Where( tup => tup.Item1.IsCompatibilityCommand )
                                                      .ToDictionary( tup => tup.Item1.Command,
                                                                     tup => tup.Item2.GetStaticDelegate<CommandHandler>() );
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Sends a PING message.
        /// </summary>
        private void SendPing( string data )
        {
            // PING :<ping data>
            this.SendRawData( "PING :" + data );
            this.HandlePing( data );
        }

        /// <summary>
        /// Sends a PONG, reply to a PING message.
        /// </summary>
        private void SendPong( string data )
        {
            // PONG :<ping data>
            this.SendRawData( "PONG :" + data );
        }

        /// <summary>
        /// Calls the correct command (CTCP or IRC).
        /// </summary>
        /// <param name="message"></param>
        private void HandleMessage( IrcMessage message )
        {
            var ircMessages = this.Ctcp.InterpretMessage( message );
            foreach ( var newMessage in ircMessages )
            {
                this.HandleIrcMessage( newMessage );
            }
        }

        /// <summary>
        /// Calls the appropriate IRC command, depending on the message's command.
        /// If a compatibility command is available, run it as well.
        /// </summary>
        private void HandleIrcMessage( IrcMessage message )
        {
            if ( _commandHandlers.ContainsKey( message.Command ) )
            {
                if ( _compatibilityCommandHandlers.ContainsKey( message.Command ) )
                {
                    _compatibilityCommandHandlers[message.Command]( this, message );
                }
                if ( message.IsValid )
                {
                    _commandHandlers[message.Command]( this, message );
                }
            }
            else
            {
                bool handled = this.OnUnknownCommandReceived( message );
                if ( handled )
                {
                    return;
                }

                // This works for most unknown replies
                // Skip the first argument, it's the user's nickname
                // Add a space only if there are at least two arguments to avoid putting a blank space before some info messages
                string messageContent = string.Join( IrcUtils.MessagePartsSeparator, message.CommandArguments.Skip( 1 ) )
                                                 + ( message.CommandArguments.Length > 1 ? IrcUtils.MessagePartsSeparator : string.Empty )
                                                 + message.Content;

                int code;
                if ( int.TryParse( message.Command, out code ) && code >= ErrorCodesBeginning )
                {
                    this.Network.OnErrorReceived( messageContent, message.Command );
                }
                else
                {
                    this.Network.OnInformationReceived( messageContent, message.Command );
                }
            }
        }

        /// <summary>
        /// Pings the server at the interval specified by PingDelay.
        /// </summary>
        private async void PingServer()
        {
            while ( this.Network.IsConnected )
            {
                string data = TimeHelper.DateTimeToUnixMilliseconds( DateTime.UtcNow ).ToString();
                this.SendPing( data );

                await Task.Delay( PingDelay );
            }
        }

        /// <summary>
        /// Handles a sent ping request.
        /// </summary>
        private async void HandlePing( string data )
        {
            this._pingData.Add( data );
            await Task.Delay( PongDelay );

            if ( this._pingData.Contains( data ) )
            {
                this.Stop( true );
            }
        }

        /// <summary>
        /// Delays the sending of a NAMES query
        /// </summary>
        private async void DelaySendNames( IrcChannel channel )
        {
            if ( this._delayedNamesChannels.Contains( channel ) )
            {
                return;
            }

            this._delayedNamesChannels.Add( channel );

            await Task.Delay( JoinNamesDelay );

            if ( this._delayedNamesChannels.Contains( channel ) )
            {
                channel.GetUserList();
                this._delayedNamesChannels.Remove( channel );
            }
        }

        /// <summary>
        /// Sets the client in "authenticated" mode.
        /// </summary>
        private void HandleAuthentication()
        {
            this.Network.IsAuthenticated = true;
            // Some servers do not require ident
            IdentServer.UserNameNeeded -= IdentServer_UserNameNeeded;
            this.PingServer();
        }

        /// <summary>
        /// Raises an event.
        /// </summary>
        private void Raise<T>( ClientEventHandler<T> handler, T args )
            where T : EventArgs
        {
            if ( handler != null )
            {
                handler( this, args );
            }
        }
        #endregion

        #region Event handlers
        private void Client_DataReceived( object sender, RawDataEventArgs e )
        {
            this.OnRawDataReceived( e.Data );

            IrcMessage message;
            try
            {
                message = IrcUtils.ParseMessage( this.Network, e.Data );
            }
            catch // bad data from server? all kinds of weird exceptions.
            {
                this.Stop( true );
                return;
            }

            this.HandleMessage( message );
        }

        private void Client_ConnectionClosed( object sender, EventArgs e )
        {
            this.Stop( true );
        }

        private void IdentServer_UserNameNeeded( object sender, UserNameNeededEventArgs e )
        {
            e.UserName = this.IdentUserName;
            IdentServer.UserNameNeeded -= IdentServer_UserNameNeeded;
        }
        #endregion

        #region Pseudo-IDisposable implementation
        // IrcClient does not implement IDisposable because it's disposed internally.

        /// <summary>
        /// Finalizes this instance.
        /// </summary>
        ~IrcClient()
        {
            this.Dispose( false );
        }

        /// <summary>
        /// Disposes of the <see cref="IrcClient"/>'s resources.
        /// </summary>
        internal void Dispose()
        {
            this.Dispose( true );
            GC.SuppressFinalize( this );
        }

        /// <summary>
        /// Disposes of the <see cref="IrcClient"/>'s managed and unmanaged resources.
        /// </summary>
        private void Dispose( bool isDisposing )
        {
            if ( !this._disposed )
            {
                this._disposed = true;
                this._client.Dispose();

                IdentServer.UserNameNeeded -= IdentServer_UserNameNeeded;
            }
        }
        #endregion
    }
}