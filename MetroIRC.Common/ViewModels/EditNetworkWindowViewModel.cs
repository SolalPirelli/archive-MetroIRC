// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using BasicMvvm;
using BasicMvvm.Dialogs;
using IrcSharp;
using MetroIrc.Services;

namespace MetroIrc.ViewModels
{
    public sealed class EditNetworkWindowViewModel : DialogViewModel
    {
        #region Constants
        /// <summary>
        /// The most used encodings, plus default Windows encodings.
        /// </summary>
        private static readonly string[] _encodings = 
        { "windows-1250", "windows-1251", "windows-1252", "windows-1253", 
          "windows-1254", "windows-1255", "windows-1256", "windows-1257", 
          "windows-1258", "big5", "ascii", "utf-8", "gb2312", "shift_jis", 
          "iso-8859-1", "euc-jp", "euc-cn", "euc-kr" };
        #endregion

        #region Private members
        private string _title;
        #endregion

        #region Properties
        public IrcNetworkInfo NetworkInfo { get; private set; }
        public IrcNetwork Network { get; private set; }

        public List<Encoding> Encodings { get; private set; }
        #endregion

        public EditNetworkWindowViewModel()
        {
            this.NetworkInfo = new IrcNetworkInfo();
            this._title = Locator.Get<ITranslationService>().Translate( "EditNetworkWindow", "AddNetworkTitle" );
            this.SetEncodings();
        }

        public EditNetworkWindowViewModel( IrcNetworkInfo info )
        {
            this.NetworkInfo = info;
            this._title = Locator.Get<ITranslationService>().Translate( "EditNetworkWindow", "EditNetworkTitle", this.NetworkInfo.FriendlyName );
            this.SetEncodings();
        }

        #region Commands
        public ICommand CancelCommand
        {
            get { return new RelayCommand( _ => this.FireRequestClose( false ) ); }
        }

        private RelayCommand _okCommand;
        public ICommand OkCommand
        {
            get
            {
                if ( this._okCommand == null )
                {
                    this._okCommand = new RelayCommand( _ => this.FireRequestClose( true ),
                                                        _ => !this.NetworkInfo.HasErrors );
                    this._okCommand.BindConditionToProperty( this.NetworkInfo, o => o.HasErrors );
                }
                return this._okCommand;
            }
        }
        #endregion

        private void SetEncodings()
        {
            this.Encodings = _encodings.Select( s => Encoding.GetEncoding( s ) ).OrderBy( e => e.WebName ).ToList();

            // in case the user set an encoding using /charset
            if ( !this.Encodings.Contains( this.NetworkInfo.Encoding ) )
            {
                this.Encodings.Add( this.NetworkInfo.Encoding );
            }
        }

        #region DialogViewModel overrides
        public override string Title
        {
            get { return this._title; }
        }

        public override object Icon
        {
            get { return Locator.Get<IResourceService>().GetResource( "EditNetworkWindowIcon" ); }
        }
        #endregion
    }
}