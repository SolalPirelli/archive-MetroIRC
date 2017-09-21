// Copyright (C) 2012-2013, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
namespace IrcSharp.Internals
{
    /// <summary>
    /// An object associated with an <see cref="IrcClient"/>.
    /// </summary>
    public abstract class ConnectedObject : ObservableObject
    {
        #region Private members
        /// <summary>
        /// The <see cref="IrcClient"/> associated with this object.
        /// </summary>
        private IrcClient _client;
        #endregion

        internal ConnectedObject() { }

        internal ConnectedObject( IrcClient client )
        {
            this._client = client;
        }

        #region Protected methods
        /// <summary>
        /// Gets the IrcClient.
        /// </summary>
        protected void SetClient( IrcClient client )
        {
            if ( _client != null )
            {
                throw new InvalidOperationException( "The client was already set." );
            }
            this._client = client;
        }

        /// <summary>
        /// Sends text parts.
        /// </summary>
        protected void Send( params string[] parts )
        {
            // that Replace method is there to avoid sending spaces before messages
            string message = string.Join( IrcUtils.MessagePartsSeparator, parts ).ReplaceFirst( ": ", ":" );
            this._client.SendRawData( message );
        }
        #endregion
    }
}