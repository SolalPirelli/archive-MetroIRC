// Copyright (C) 2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.using System.Text;

using CommonStuff;
using MetroIrc.Services;

namespace MetroIrc.Desktop.Services
{
    public sealed class WpfNotificationService : INotificationService
    {
        public void Notify( string message )
        {
            App.Current.Dispatcher.Invoke( () =>
            {
                FlashHelper.Flash( App.Current.MainWindow );

                if ( App.Current.Settings.NotifyWithSound )
                {
                    App.Current.PlaySound( App.NewHighlightedMessageSound );
                }
            } );
        }
    }
}