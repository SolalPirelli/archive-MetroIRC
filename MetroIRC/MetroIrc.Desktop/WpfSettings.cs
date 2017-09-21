// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Media;
using System.Xml.Serialization;
using BasicMvvm;
using BasicMvvm.Validation;
using CommonStuff;
using IrcSharp;
using IrcSharp.External;
using MetroControls;
using MetroIrc.Services;
using WpfLoc;

namespace MetroIrc.Desktop
{
    /// <summary>
    /// The class responsible for storing settings.
    /// </summary>
    public sealed class WpfSettings : ModelBase, ISettings
    {
        #region Property-backing fields
        private string _userNickname = Environment.UserName;
        private string _userRealName = Environment.UserName;

        private bool _showTimeStamps = false;
        private string _timeStampFormat = "HH:mm";
        private bool _showTopic = true;
        private bool _showUserList = true;
        private bool _showSmileys = true;
        private bool _showRepeats = false;
        private int _maximumMessagesCount = 200;

        private bool _transformLinks = true;
        private bool _transformChans = true;

        private bool _notifyAlways = false;
        private bool _notifyOnNickname = true;
        private ObservableCollection<string> _notifyWords = new ObservableCollection<string>();
        private bool _notifyWithSound = true;

        private bool _enableIdent = false;

        private PaneDock _networksListPosition = PaneDock.Top;

        private DateTime _lastUpdateCheck = DateTime.MinValue;

        private double _mainWindowTop = 150;
        private double _mainWindowLeft = 200;
        private double _mainWindowHeight = 700;
        private double _mainWindowWidth = 600;

        private bool _hasCultureChanged;
        private CultureInfo _currentCulture;
        private CultureInfo _originalCulture;
        #endregion

        #region Public properties
        public ObservableCollection<IrcNetworkInfo> Networks { get; set; }

        [XmlAttribute]
        [Required]
        public string UserNickname
        {
            get { return this._userNickname; }
            set { this.SetProperty( ref this._userNickname, value.Trim() ); }
        }

        [XmlAttribute]
        [Required]
        public string UserRealName
        {
            get { return this._userRealName; }
            set { this.SetProperty( ref this._userRealName, value.Trim() ); }
        }

        [XmlAttribute]
        public bool ShowTimeStamps
        {
            get { return this._showTimeStamps; }
            set { this.SetProperty( ref this._showTimeStamps, value ); }
        }

        [XmlAttribute]
        public string TimeStampFormat
        {
            get { return this._timeStampFormat; }
            set { this.SetProperty( ref this._timeStampFormat, value ); }
        }

        [XmlAttribute]
        public bool ShowTopic
        {
            get { return this._showTopic; }
            set { this.SetProperty( ref this._showTopic, value ); }
        }

        [XmlAttribute]
        public bool ShowUserList
        {
            get { return this._showUserList; }
            set { this.SetProperty( ref this._showUserList, value ); }
        }

        [XmlAttribute]
        public bool ShowSmileys
        {
            get { return this._showSmileys; }
            set { this.SetProperty( ref this._showSmileys, value ); }
        }

        [XmlAttribute]
        public string SmileyPackName
        {
            get { return SmileyManager.CurrentPack.Name; }
            set
            {
                SmileyManager.SetCurrentPack( value );
                this.FirePropertyChanged();
            }
        }

        [XmlAttribute]
        public bool ShowRepeats
        {
            get { return this._showRepeats; }
            set { this.SetProperty( ref this._showRepeats, value ); }
        }

        [XmlAttribute]
        public int MaximumMessagesCount
        {
            get { return this._maximumMessagesCount; }
            set { this.SetProperty( ref this._maximumMessagesCount, value ); }
        }

        [XmlAttribute]
        public bool TransformLinks
        {
            get { return _transformLinks; }
            set { this.SetProperty( ref this._transformLinks, value ); }
        }

        [XmlAttribute]
        public bool TransformChans
        {
            get { return _transformChans; }
            set { this.SetProperty( ref this._transformChans, value ); }
        }

        [XmlAttribute]
        public bool NotifyAlways
        {
            get { return this._notifyAlways; }
            set { this.SetProperty( ref this._notifyAlways, value ); }
        }

        [XmlAttribute]
        public bool NotifyOnNickname
        {
            get { return this._notifyOnNickname; }
            set { this.SetProperty( ref this._notifyOnNickname, value ); }
        }

        [XmlElement( "NotifyWord" )]
        public ObservableCollection<string> NotifyWords
        {
            get { return this._notifyWords; }
            set { this.SetProperty( ref this._notifyWords, value ); }
        }

        [XmlAttribute]
        public bool NotifyWithSound
        {
            get { return this._notifyWithSound; }
            set { this.SetProperty( ref this._notifyWithSound, value ); }
        }

        [XmlAttribute]
        public PaneDock NetworksListPosition
        {
            get { return this._networksListPosition; }
            set { this.SetProperty( ref this._networksListPosition, value ); }
        }

        [XmlAttribute]
        public bool EnableIdent
        {
            get { return this._enableIdent; }
            set
            {
                if ( this._enableIdent != value )
                {
                    if ( value )
                    {
                        IdentServer.Start( new SocketListenerWrapper() );
                    }
                    else
                    {
                        IdentServer.Stop();
                    }

                    this.SetProperty( ref this._enableIdent, value );
                }
            }
        }


        [XmlAttribute]
        public DateTime LastUpdateCheck
        {
            get { return this._lastUpdateCheck; }
            set { this.SetProperty( ref this._lastUpdateCheck, value ); }
        }

        [XmlAttribute]
        public double MainWindowTop
        {
            get { return this._mainWindowTop; }
            set { this.SetProperty( ref this._mainWindowTop, value ); }
        }

        [XmlAttribute]
        public double MainWindowLeft
        {
            get { return this._mainWindowLeft; }
            set { this.SetProperty( ref this._mainWindowLeft, value ); }
        }

        [XmlAttribute]
        public double MainWindowHeight
        {
            get { return this._mainWindowHeight; }
            set { this.SetProperty( ref this._mainWindowHeight, value ); }
        }

        [XmlAttribute]
        public double MainWindowWidth
        {
            get { return this._mainWindowWidth; }
            set { this.SetProperty( ref this._mainWindowWidth, value ); }
        }
        #endregion

        #region Color-related properties
        private Dictionary<Color, List<Color>> _accentColors = new Dictionary<Color, List<Color>>()
        {
            { Colors.Black, BlackAccentColors },
            { Colors.White, WhiteAccentColors }
        };

        [XmlIgnore]
        public Color MainColor
        {
            get { return ColorManager.MainColor; }
            set
            {
                if ( ColorManager.MainColor != value )
                {
                    ColorManager.MainColor = value;
                    this.FirePropertyChanged();
                    this.SetAccentColor();
                }
            }
        }

        // HACK: Color can't be serialized...
        [XmlAttribute]
        public string MainColorName
        {
            get { return ColorHelper.GetColorName( this.MainColor ); }
            set { this.MainColor = (Color) ColorConverter.ConvertFromString( value ); }
        }

        [XmlIgnore]
        public Color AccentColor
        {
            get { return ColorManager.AccentColor; }
            set { ColorManager.AccentColor = value; }
        }

        private int _accentColorIndex = 1; // orange is the default
        [XmlAttribute]
        public int AccentColorIndex
        {
            get { return this._accentColorIndex; }
            set
            {
                if ( this._accentColorIndex != value )
                {
                    this._accentColorIndex = value;
                    this.FirePropertyChanged();
                    this.SetAccentColor();
                }
            }
        }

        private void SetAccentColor()
        {
            this.AccentColor = this._accentColors[ColorManager.MainColor][this._accentColorIndex];
        }

        // NOT the actual accent colors, just the ones shown in the settings dialog
        [XmlIgnore]
        public List<Color> AvailableAccentColors
        {
            get
            {
                return new List<Color>
                {
                    Color.FromRgb( 0xC0, 0x00, 0x00 ), // Red
                    Color.FromRgb( 0xC0, 0x60, 0x00 ), // Orange
                    Color.FromRgb( 0xC0, 0xA0, 0x00 ), // Yellow
                    Color.FromRgb( 0x00, 0xC0, 0x00 ), // Green
                    Color.FromRgb( 0x00, 0x60, 0xC0 ), // Blue
                    Color.FromRgb( 0x8B, 0x00, 0xCC ), // Violet
                    Color.FromRgb( 0x90, 0x90, 0x90 ), // Gray
                };
            }
        }

        private static List<Color> BlackAccentColors = new List<Color>
        {
            Color.FromRgb( 0xD0, 0x00, 0x00 ), // Red
            Color.FromRgb( 0xFF, 0xA5, 0x00 ), // Orange
            Color.FromRgb( 0xFA, 0xD3, 0x00 ), // Yellow
            Color.FromRgb( 0x1C, 0xA0, 0x16 ), // Green
            Color.FromRgb( 0x06, 0x80, 0xE0 ), // Blue
            Color.FromRgb( 0xB6, 0x0C, 0xEF ), // Violet
            Color.FromRgb( 0x97, 0x97, 0x97 ), // Gray
        };

        private static List<Color> WhiteAccentColors = new List<Color>
        {
            Color.FromRgb( 0xDE, 0x0E, 0x0E ), // Red
            Color.FromRgb( 0xF5, 0x65, 0x00 ), // Orange
            Color.FromRgb( 0xC0, 0xA0, 0x00 ), // Yellow
            Color.FromRgb( 0x26, 0x7F, 0x00 ), // Green
            Color.FromRgb( 0x00, 0x94, 0xFF ), // Blue
            Color.FromRgb( 0xD8, 0x51, 0xFF ), // Violet
            Color.FromRgb( 0x80, 0x80, 0x80 ), // Gray
        };
        #endregion

        #region Culture-related properties
        [XmlAttribute]
        public string CurrentCultureName
        {
            get { return this.CurrentCulture.Name; }
            set { this.CurrentCulture = CultureInfo.GetCultureInfo( value ); }
        }

        [XmlIgnore]
        public CultureInfo CurrentCulture
        {
            get { return this._currentCulture ?? TranslationManager.CurrentLanguage; }
            set
            {
                if ( this._currentCulture == null )
                {
                    this._originalCulture = value;
                    TranslationManager.CurrentLanguage = value;
                }
                this._currentCulture = value;
                this.HasCultureChanged = this._currentCulture != this._originalCulture;
            }
        }

        [XmlIgnore]
        public bool HasCultureChanged
        {
            get { return this._hasCultureChanged; }
            private set { this.SetProperty( ref this._hasCultureChanged, value ); }
        }
        #endregion

        public WpfSettings()
        {
            this.Networks = new ObservableCollection<IrcNetworkInfo>();
            this.SmileyPackName = "Fugue";
        }

        public void Save()
        {
            SettingsHelper.SaveSettings( this );
        }

        protected override string GetErrorText( string propertyName )
        {
            return Locator.Get<ITranslationService>().Translate( "SettingsErrors", propertyName );
        }
    }
}