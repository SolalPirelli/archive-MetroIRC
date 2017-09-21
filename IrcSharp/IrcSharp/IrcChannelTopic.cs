// Copyright (C) 2013, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using IrcSharp.Internals;

namespace IrcSharp
{
    /// <summary>
    /// An <see cref="IrcChannel"/>'s topic.
    /// </summary>
    public sealed class IrcChannelTopic : ObservableObject
    {
        #region Property-backing fields
        private string _text;
        private IrcUser _setter;
        private DateTime? _setDate;
        #endregion

        #region Public properties
        /// <summary>
        /// Gets the topic text.
        /// </summary>
        public string Text
        {
            get { return this._text; }
            internal set { this.SetProperty( ref this._text, value ); }
        }

        /// <summary>
        /// Gets the <see cref="IrcUser"/> who set the topic.
        /// </summary>
        public IrcUser Setter
        {
            get { return this._setter; }
            internal set { this.SetProperty( ref this._setter, value ); }
        }

        /// <summary>
        /// Gets the date at which the topic was set, if it is known.
        /// </summary>
        public DateTime? SetDate
        {
            get { return this._setDate; }
            internal set { this.SetProperty( ref this._setDate, value ); }
        }
        #endregion

        internal IrcChannelTopic() { }
    }
}