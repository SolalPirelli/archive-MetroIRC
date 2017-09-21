// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using BasicMvvm;
using BasicMvvm.Validation;
using MetroIrc.Services;
using MetroIrc.Internals;

namespace MetroIrc
{
    /// <summary>
    /// A class which contains information about an IRC network.
    /// </summary>
    public sealed class IrcNetworkInfo : ModelBase
    {
        #region Property-backing fields
        private string _hostName;
        private int _portNumber = 6667;
        private string _password;
        private bool _useSsl;
        private bool _acceptInvalidCertificates;
        private bool _useGlobalInfo = true;
        private string _nickname;
        private string _realName;
        private bool _joinOnStartup;
        private string _joinCommand;
        private Encoding _encoding = Encoding.UTF8;
        private List<IrcChannelInfo> _favoriteChannels = new List<IrcChannelInfo>();
        #endregion

        #region Public properties
        [XmlAttribute]
        [Required]
        public string HostName
        {
            get { return this._hostName; }
            set { this.SetProperty( ref this._hostName, value.Trim() ); }
        }

        [XmlAttribute]
        public int PortNumber
        {
            get { return this._portNumber; }
            set { this.SetProperty( ref this._portNumber, value ); }
        }

        [XmlAttribute]
        public string Password
        {
            get { return this._password; }
            set { this.SetProperty( ref this._password, value.Trim() ); }
        }

        [XmlAttribute]
        public bool UseSsl
        {
            get { return this._useSsl; }
            set { this.SetProperty( ref this._useSsl, value ); }
        }

        [XmlAttribute]
        public bool AcceptInvalidCertificates
        {
            get { return this._acceptInvalidCertificates; }
            set { this.SetProperty( ref this._acceptInvalidCertificates, value ); }
        }

        [XmlAttribute]
        public bool UseGlobalInfo
        {
            get { return this._useGlobalInfo; }
            set { this.SetProperty( ref this._useGlobalInfo, value ); }
        }

        [XmlAttribute]
        [RequiredUnless( "UseGlobalInfo" )]
        public string Nickname
        {
            get { return this._nickname; }
            set { this.SetProperty( ref this._nickname, value.Trim() ); }
        }

        [XmlAttribute]
        [RequiredUnless( "UseGlobalInfo" )]
        public string RealName
        {
            get { return this._realName; }
            set { this.SetProperty( ref this._realName, value.Trim() ); }
        }

        [XmlAttribute]
        public bool JoinOnStartup
        {
            get { return this._joinOnStartup; }
            set { this.SetProperty( ref this._joinOnStartup, value ); }
        }

        [XmlAttribute]
        public string JoinCommand
        {
            get { return this._joinCommand; }
            set { this.SetProperty( ref this._joinCommand, value.Trim() ); }
        }

        [XmlAttribute] // needed for serialization
        public string EncodingName
        {
            get { return this._encoding.WebName; }
            set { this.Encoding = Encoding.GetEncoding( value ); }
        }

        [XmlElement( "FavoriteChannel" )]
        public List<IrcChannelInfo> FavoriteChannels
        {
            get { return this._favoriteChannels; }
            set { this.SetProperty( ref this._favoriteChannels, value ); }
        }


        [XmlIgnore]
        public Encoding Encoding
        {
            get { return this._encoding; }
            set { this.SetProperty( ref this._encoding, value ); }
        }

        [XmlIgnore]
        public string FriendlyName
        {
            get
            {
                // irc.epiknet.net => epiknet
                int dotsCount = this.HostName.ToCharArray().Count( c => c == '.' );
                if ( dotsCount == 0 || ( dotsCount == 2 && this.HostName.Contains( ".." ) ) )
                {
                    // The second condition prevents invalid names like "irc..net" from being transformed to empty strings.
                    return this.HostName;
                }

                // Remove the suffix (e.g. ".net") and the "irc." prefix if any
                string name = this.HostName.Substring( 0, this.HostName.LastIndexOf( '.' ) );
                return name.BeginsWith( "irc." ) ? name.Substring( "irc.".Length ) : name;
            }
        }
        #endregion

        public IrcNetworkInfo()
        {
            // Many servers have an "invalid" certificates because it ain't cheap.
            AcceptInvalidCertificates = true;
        }

        #region Public methods
        public IrcNetworkInfo Clone()
        {
            return (IrcNetworkInfo) this.MemberwiseClone();
        }

        public string GetActualNickname()
        {
            return this.UseGlobalInfo ? Locator.Get<ISettings>().UserNickname : this.Nickname;
        }

        public string GetActualRealName()
        {
            return this.UseGlobalInfo ? Locator.Get<ISettings>().UserRealName : this.RealName;
        }
        #endregion

        #region Validation
        protected override string GetErrorText( string propertyName )
        {
            return Locator.Get<ITranslationService>().Translate( "IrcNetworkInfoErrors", propertyName );
        }
        #endregion
    }
}