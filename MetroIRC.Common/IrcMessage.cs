// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System.Linq;
using BasicMvvm;
using IrcSharp;
using MetroIrc.Parsing;
using MetroIrc.Services;

namespace MetroIrc
{
    /// <summary>
    /// Represents an IRC message.
    /// </summary>
    public sealed class IrcMessage : Message
    {
        #region Property-backing fields
        private bool _isImportantMessage;
        #endregion

        #region Public properties
        /// <summary>
        /// Gets the target of the message.
        /// Optional, unless the message is a notice.
        /// </summary>
        public string Target { get; private set; }

        /// <summary>
        /// Gets the message type.
        /// </summary>
        public IrcMessageType Type { get; private set; }

        /// <summary>
        /// Gets the message content.
        /// </summary>
        public string Content { get; private set; }

        /// <summary>
        /// Gets the full name of the sender, including prefix (e.g. "to" or "from" for notices) if any.
        /// </summary>
        public override string DisplaySenderName
        {
            get
            {
                if ( this.Sender == null || this.IsInformationMessage || this.Type == IrcMessageType.Action )
                {
                    return string.Empty;
                }

                if ( this.Type == IrcMessageType.Notice )
                {
                    return this.Direction == MessageDirection.Sent ?
                           Locator.Get<ITranslationService>().Translate( "Conversation", "To" ) + " " + this.Target
                         : Locator.Get<ITranslationService>().Translate( "Conversation", "From" ) + " " + this.Sender.Nickname;
                }

                return this.Sender.Nickname;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this message is for information purposes
        /// </summary>
        public override bool IsInformationMessage
        {
            get { return this.Type > IrcMessageType.Action; }
        }

        public override bool IsImportantMessage
        {
            get { return this._isImportantMessage; }
        }
        #endregion

        /// <summary>
        /// Creates a new instance of the <see cref="IrcMessage"/> class.
        /// </summary>
        public IrcMessage( IrcNetwork network, MessageDirection direction, IrcUser sender, IrcMessageType type, string content, string target = null )
            : base( network, direction, sender )
        {
            this.Type = type;
            this.Content = content;
            this.Target = target;

            this._isImportantMessage = this.IsImportant();
        }

        public override Message Clone()
        {
            return (IrcMessage) base.Clone();
        }

        private bool IsImportant()
        {
            if ( this.Direction == MessageDirection.Sent || this.IsInformationMessage )
            {
                return false;
            }

            var settings = Locator.Get<ISettings>();

            if ( settings.NotifyAlways )
            {
                return true;
            }

            if ( settings.NotifyOnNickname && MessageParser.Contains( this.Content, this.Network.CurrentUser.Nickname ) )
            {
                return true;
            }

            return settings.NotifyWords.Any( s => MessageParser.Contains( this.Content, s ) );
        }
    }
}