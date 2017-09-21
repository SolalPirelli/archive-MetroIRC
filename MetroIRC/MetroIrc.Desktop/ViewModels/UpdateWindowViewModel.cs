// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using BasicMvvm;
using BasicMvvm.Dialogs;
using MetroIrc.Services;

namespace MetroIrc.Desktop.ViewModels
{
    public sealed class UpdateWindowViewModel : DialogViewModel
    {
        #region Constants
        private const double MinimalShowTime = 5; // in seconds
        #endregion

        #region Property-backing fields
        private double _progressPercentage;
        private bool _isDownloadCompleted;
        #endregion

        #region Private members
        private WebClient _client;
        private string _downloadedFilePath;
        #endregion

        #region Public properties
        public double ProgressPercentage
        {
            get { return this._progressPercentage; }
            set { this.SetProperty( ref this._progressPercentage, value ); }
        }

        public bool IsDownloadCompleted
        {
            get { return this._isDownloadCompleted; }
            set { this.SetProperty( ref this._isDownloadCompleted, value ); }
        }
        #endregion

        public UpdateWindowViewModel()
        {
            this._client = new WebClient();

            string fileName = Path.GetFileName( UpdateChecker.FilePath );
            this._downloadedFilePath = Path.Combine( Path.GetTempPath(), fileName );
            this._client.DownloadFileAsync( new Uri( UpdateChecker.FilePath ), this._downloadedFilePath );

            this._client.DownloadProgressChanged += Client_DownloadProgressChanged;
            this._client.DownloadFileCompleted += Client_DownloadFileCompleted;
        }

        private void Client_DownloadProgressChanged( object sender, DownloadProgressChangedEventArgs e )
        {
            this.ProgressPercentage = e.ProgressPercentage;
        }

        private void Client_DownloadFileCompleted( object sender, EventArgs e )
        {
            this.ProgressPercentage = 100;
            this.IsDownloadCompleted = true;

            Process.Start( this._downloadedFilePath );

            Locator.Get<IUIService>().Execute( () =>
            {
                // works only on the main thread
                App.Current.Shutdown();
            } );
        }

        #region DialogViewModel overrides
        public override string Title
        {
            get { return Locator.Get<ITranslationService>().Translate( "UpdateWindow", "Title" ); }
        }

        public override void BaseWindowClosing( object sender, CancelEventArgs e )
        {
            e.Cancel = true;
        }
        #endregion
    }
}