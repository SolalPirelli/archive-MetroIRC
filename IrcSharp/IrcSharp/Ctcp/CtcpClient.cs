// Copyright (C) 2011-2013, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System.Collections.Generic;
using System.Linq;
using IrcSharp.Internals;

// ctcp client, message
using CommandHandler = System.Action<IrcSharp.Ctcp.CtcpClient, IrcSharp.Internals.CtcpMessage>;

namespace IrcSharp.Ctcp
{
    /// <summary>
    /// Implements the CTCP (Client-To-Client-Protocol) protocol.
    /// </summary>
    public sealed partial class CtcpClient
    {
        #region Private static members
        /// <summary>
        /// The CTCP commands such as FINGER, VERSION, PING...
        /// </summary>
        private static Dictionary<string, CommandHandler> _commandHandlers;
        #endregion

        #region Public static properties
        /// <summary>
        /// Gets the client software this <see cref="CtcpClient"/> is used by.
        /// This is used to answer queries from other users regarding the client software.
        /// </summary>
        public static IrcClientSoftware ClientSoftware { get; private set; }

        /// <summary>
        /// Gets or sets the command descriptions. By default, they are in English.
        /// </summary>
        public static Dictionary<string, string> CommandDescriptions { get; set; }

        /// <summary>
        /// Gets the default command descriptions.
        /// </summary>
        public static Dictionary<string, string> DefaultCommandDescriptions
        {
            get
            {
                return new Dictionary<string, string>
                {
                    { "action", "Syntax: action <text> | Used to simulate role-playing games" },
                    { "clientinfo", "Query: clientinfo [command] / Reply: clientinfo <info> | If no command is specified, the answer is a list of all commands ; otherwise, a description of the command's usage." },
                    { "errmsg", "Reply: errmsg <error message> | Indicates that an error happened. Cannot be used as a query." },
                    { "ping", "Query: ping <time> / Reply: ping <time> | Used to test the connection speed. The time is in Unix milliseconds format." },
                    { "source", "Query: source / Reply: source <source> | Gets the location at which my client can be downloaded." },
                    { "time", "Query: time / Reply: time <time> | Gets my local time." },
                    { "userinfo", "Query: userinfo / Reply: userinfo <info> | Gets information about me." },
                    { "version", "Query: version / Reply: version <Client> <Version> (<OS>) | Gets information about my client and operating system." },
                    { "finger", "Query: finger / reply: <finger information> | Gets extended information about me." }
                };
            }
        }

        /// <summary>
        /// Gets or sets the message sent when an unknown command is received. By default, it is in English.
        /// </summary>
        public static string UnknownCommandMessage { get; set; }

        /// <summary>
        /// Gets the default message sent when an unknown command is received.
        /// </summary>
        public static string DefaultUnknownCommandMessage
        {
            get { return "{0}: unknown command."; }
        }
        #endregion

        #region Public properties
        /// <summary>
        /// Gets or sets information about the current user.
        /// </summary>
        public string CurrentUserInformation { get; set; }
        #endregion

        static CtcpClient()
        {
            ClientSoftware = new IrcClientSoftware();
            CommandDescriptions = DefaultCommandDescriptions;
            UnknownCommandMessage = DefaultUnknownCommandMessage;
            InitializeHandlers();
        }

        internal CtcpClient()
        {
            this.CurrentUserInformation = "N/A";
        }

        #region Events
        /// <summary>
        /// Occurs when an unknown command is received.
        /// If this event is not handled, the <see cref="CtcpClient"/> will send an error message to the user that sent the command.
        /// </summary>
        public event CtcpClientEventHandler<UnknownCommandReceivedEventArgs> UnknownCommandReceived;
        private bool OnUnknownCommandReceived( CtcpMessage message )
        {
            if ( this.UnknownCommandReceived != null )
            {
                var e = new UnknownCommandReceivedEventArgs( message );
                this.UnknownCommandReceived( this, e );
                return e.Handled;
            }
            return false;
        }
        #endregion

        #region Internal methods
        /// <summary>
        /// Interprets an <see cref="IrcMessage"/> and returns the <see cref="IrcMessage"/>s that should be interpreted by an <see cref="IrcClient"/>.
        /// </summary>
        internal IEnumerable<IrcMessage> InterpretMessage( IrcMessage message )
        {
            if ( !CtcpUtils.IsCtcpMessage( message ) )
            {
                return new[] { message };
            }

            var info = CtcpUtils.FilterMessage( message );
            foreach ( var ctcpMessage in info.CtcpMessages )
            {
                this.HandleMessage( ctcpMessage );
            }

            return info.IrcMessages;
        }
        #endregion

        #region Private static methods
        /// <summary>
        /// Initializes the command handlers.
        /// </summary>
        private static void InitializeHandlers()
        {
            _commandHandlers = ReflectionHelper.GetAttributedMethods<CtcpCommandAttribute>( typeof( CtcpClient.CommandHandlers ) )
                                               .ToDictionary( tup => tup.Item1.Command,
                                                              tup => tup.Item2.GetStaticDelegate<CommandHandler>() );
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Handles a <see cref="CtcpMessage"/>.
        /// </summary>
        private void HandleMessage( CtcpMessage message )
        {
            if ( _commandHandlers.ContainsKey( message.Command ) )
            {
                _commandHandlers[message.Command].Invoke( this, message );
            }
            else
            {
                bool handled = this.OnUnknownCommandReceived( message );
                if ( !handled && message.IsQuery )
                {
                    message.Sender.Ctcp.SendErrorMessage( string.Format( UnknownCommandMessage, message.Command ) );
                }
            }
        }
        #endregion
    }
}