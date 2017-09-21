// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System.Linq;
using System.Xml.Serialization;
using BasicMvvm;
using BasicMvvm.Validation;
using IrcSharp;
using MetroIrc.Services;

namespace MetroIrc
{
    /// <summary>
    /// A class which contains information about an IRC channel.
    /// </summary>
    public sealed class IrcChannelInfo : ModelBase
    {
        #region Property-backing fields
        private string _name;
        private string _key;
        #endregion

        #region Public properties
        [XmlAttribute]
        [Required]
        public string Name
        {
            get { return this._name; }
            set { this.SetProperty( ref this._name, value.Trim() ); }
        }

        [XmlAttribute]
        public string Key
        {
            get { return this._key; }
            set { this.SetProperty( ref this._key, value.Trim() ); }
        }

        [XmlIgnore]
        public bool JoinOnStartup { get; set; }
        #endregion

        public IrcChannelInfo( string name, string key = "" )
        {
            // TODO fix this, somehow
            if ( !new[] { '#', '+', '&', '!' }.Contains( name[0] ) )
            {
                name = "#" + name;
            }

            this.Name = name;
            this.Key = key;
        }

        // Needed for serialization
        public IrcChannelInfo() { }

        #region Validation
        protected override bool IsValid( string propertyName )
        {
            if ( propertyName == "Name" )
            {
                if ( this.Name == null || this.Name.Contains( " " ) )
                {
                    return false;
                }
            }
            return true;
        }

        protected override string GetErrorText( string propertyName )
        {
            return Locator.Get<ITranslationService>().Translate( "IrcChannelInfoErrors", propertyName );
        }
        #endregion
    }
}