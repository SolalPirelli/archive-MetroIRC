// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System.Windows.Input;
using BasicMvvm;
using BasicMvvm.Dialogs;
using IrcSharp;
using MetroIrc.Services;

namespace MetroIrc.ViewModels
{
    public sealed class JoinChannelWindowViewModel : DialogViewModel
    {
        #region Properties
        public IrcChannelInfo ChannelInfo { get; set; }
        public IrcNetwork Network { get; private set; }
        #endregion

        public JoinChannelWindowViewModel( IrcNetwork network )
        {
            this.ChannelInfo = new IrcChannelInfo();
            this.Network = network;
        }

        #region Command
        public ICommand CancelCommand
        {
            get { return new RelayCommand( _ => this.FireRequestClose( false ) ); }
        }

        #region OkCommand
        private RelayCommand _okCommand;
        public ICommand OkCommand
        {
            get
            {
                if ( this._okCommand == null )
                {
                    this._okCommand = new RelayCommand( OkCommandExecuted, CanOkCommandExecute );
                    this._okCommand.BindConditionToProperty( this.ChannelInfo, o => o.HasErrors );
                }
                return this._okCommand;
            }
        }

        private void OkCommandExecuted( object parameter )
        {
            if ( !this.Network.Parameters.IsChannelName( this.ChannelInfo.Name ) )
            {
                this.ChannelInfo.Name = '#' + this.ChannelInfo.Name;
            }
            this.FireRequestClose( true );
        }

        private bool CanOkCommandExecute( object parameter )
        {
            return !this.ChannelInfo.HasErrors;
        }
        #endregion
        #endregion

        #region DialogViewModel overrides
        public override string Title
        {
            get { return Locator.Get<ITranslationService>().Translate( "JoinChannelWindow", "Title" ); }
        }
        #endregion
    }
}