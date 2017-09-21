// Copyright (C) 2013, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System.Diagnostics;
using IrcSharp.Internals;

namespace IrcSharp
{
    /// <summary>
    /// A simple <see cref="IrcUser"/>/<see cref="IrcChannelUserModes"/> pair.
    /// </summary>
    [DebuggerDisplay( "{User.Nickname}, {Mode}" )]
    public sealed class IrcUserChannelModePair : ObservableObject
    {
        /// <summary>
        /// Gets the <see cref="IrcUser"/>.
        /// </summary>
        public IrcUser User { get; private set; }

        private IrcChannelUserModes _mode;
        /// <summary>
        /// Gets the <see cref="IrcChannelUserModes"/>.
        /// </summary>
        public IrcChannelUserModes Mode
        {
            get { return this._mode; }
            set { this.SetProperty( ref this._mode, value ); }
        }

        internal IrcUserChannelModePair( IrcUser user, IrcChannelUserModes mode )
        {
            this.User = user;
            this.Mode = mode;
        }
    }
}