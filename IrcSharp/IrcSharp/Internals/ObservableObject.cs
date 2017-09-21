// Copyright (C) 2012-2013, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System.ComponentModel;
using System.Runtime.CompilerServices;
using IrcSharp.Validation;

namespace IrcSharp.Internals
{
    /// <summary>
    /// A helper base class to implement INotifyPropertyChanged in a nice way.
    /// </summary>
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        internal ObservableObject() { }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Triggers the <see cref="PropertyChanged"/> event.
        /// </summary>
        protected void OnPropertyChanged( [CallerMemberName] string propertyName = "" )
        {
            Validate.IsNotNull( propertyName, "propertyName" );

            var evt = this.PropertyChanged;
            if ( evt != null )
            {
                evt( this, new PropertyChangedEventArgs( propertyName ) );
            }
        }

        /// <summary>
        /// Sets the specified field to the specified value and raises <see cref="PropertyChanged"/> if needed.
        /// </summary>
        protected void SetProperty<T>( ref T field, T value, [CallerMemberName] string propertyName = "" )
        {
            if ( !object.Equals( field, value ) )
            {
                field = value;
                this.OnPropertyChanged( propertyName );
            }
        }
    }
}