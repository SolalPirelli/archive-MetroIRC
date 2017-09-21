// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System.Collections.ObjectModel;
using System.Windows.Media;
using BasicMvvm;

namespace MetroIrc.Desktop
{
    public sealed class Smiley : ObservableObject
    {
        #region Property-backing fields
        private ImageSource _image;
        #endregion

        #region Public properties
        public string FileName { get; set; }

        public ImageSource Image
        {
            get { return this._image; }
            set { this.SetProperty( ref this._image, value ); }
        }

        public ObservableCollection<string> Shortcuts { get; private set; }
        #endregion

        public Smiley()
        {
            this.Shortcuts = new ObservableCollection<string>();
        }
    }
}