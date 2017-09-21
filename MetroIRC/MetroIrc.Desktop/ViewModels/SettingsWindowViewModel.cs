// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System.ComponentModel;
using System.Windows.Input;
using BasicMvvm;
using BasicMvvm.Dialogs;
using MetroIrc.Services;

namespace MetroIrc.Desktop.ViewModels
{
    public sealed class SettingsWindowViewModel : DialogViewModel
    {
        #region Public properties
        public SmileySettingsViewModel SmileySettings { get; private set; }
        #endregion

        public SettingsWindowViewModel()
        {
            this.SmileySettings = new SmileySettingsViewModel();
        }

        #region Commands
        public ICommand OkCommand
        {
            get
            {
                return new RelayCommand( _ => this.FireRequestClose( true ),
                                         _ => !App.Current.Settings.HasErrors );
            }
        }
        #endregion

        #region DialogViewModel overrides
        public override string Title
        {
            get { return Locator.Get<ITranslationService>().Translate( "SettingsWindow", "Title" ); }
        }

        public override object Icon
        {
            get { return Locator.Get<IResourceService>().GetResource( "SettingsWindowIcon" ); }
        }

        public override void BaseWindowClosing( object sender, CancelEventArgs e )
        {
            SettingsHelper.SaveSettings( App.Current.Settings );
            SmileyManager.Save();
        }
        #endregion
    }
}