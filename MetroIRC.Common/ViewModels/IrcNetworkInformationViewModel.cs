// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Collections.Generic;
using System.Linq;
using BasicMvvm;
using IrcSharp;
using MetroIrc.Services;
using CM = System.ComponentModel;

namespace MetroIrc.ViewModels
{
    public sealed class IrcNetworkInformationViewModel : IrcConversationViewModel
    {
        #region Properties
        public IrcNetwork Network { get; private set; }
        #endregion

        #region IrcConversationViewModel overrides
        public override string Title
        {
            get { return Locator.Get<ITranslationService>().Translate( "NetworkInformation", "Title" ); }
        }

        public override string TargetName
        {
            get { return null; }
        }

        public override Type ConversationType
        {
            get { return typeof( IrcNetworkInfo ); }
        }

        public override bool CanBeClosed
        {
            get { return false; }
        }
        #endregion

        public IrcNetworkInformationViewModel( IrcNetwork network, IrcNetworkViewModel networkVM )
            : base( networkVM )
        {
            this.Network = network;
            this.IsVisible = true;
            PropertyChangedEventManager.AddHandler( this.Network, Network_ConnectionStatusChanged, o => o.ConnectionStatus );
        }

        #region Weak event handlers
        private void Network_ConnectionStatusChanged( object sender, CM.PropertyChangedEventArgs e )
        {
            string text = Locator.Get<ITranslationService>().Translate( "ConnectionStatusChanged", this.Network.ConnectionStatus.ToString() );
            var type = this.Network.ConnectionStatus >= ConnectionStatus.Connected ? IrcMessageType.Info
                     : IrcMessageType.Error;

            var message = new IrcMessage( this.Network, MessageDirection.Internal, null, type, text );
            this.AddMessage( message );
        }
        #endregion
    }
}