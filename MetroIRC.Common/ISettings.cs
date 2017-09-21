// Copyright (C) 2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.using System.Text;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace MetroIrc
{
    public interface ISettings : INotifyPropertyChanged
    {
        ObservableCollection<IrcNetworkInfo> Networks { get; set; }
        string UserNickname { get; set; }
        string UserRealName { get; set; }
        bool ShowTimeStamps { get; set; }
        bool ShowTopic { get; set; }
        bool ShowUserList { get; set; }
        bool ShowSmileys { get; set; }
        int MaximumMessagesCount { get; set; }
        bool TransformLinks { get; set; }
        bool TransformChans { get; set; }
        bool NotifyAlways { get; set; }
        bool NotifyOnNickname { get; set; }
        ObservableCollection<string> NotifyWords { get; set; }
        bool EnableIdent { get; set; }

        bool HasErrors { get; }
        void Save();
    }
}