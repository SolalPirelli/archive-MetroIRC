using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using BasicMvvm;
using CommonStuff;
using IrcSharp;
using MetroIrc.Services;

namespace MetroIrc.Desktop.Controls
{
    public sealed class ChannelUsersControl : Control, INotifyPropertyChanged
    {
        #region Property-backing fields
        private TransformedCollection<IrcUserChannelModePair> _users;
        #endregion

        #region Channel DependencyProperty
        public IrcChannel Channel
        {
            get { return (IrcChannel) GetValue( ChannelProperty ); }
            set { SetValue( ChannelProperty, value ); }
        }

        public static readonly DependencyProperty ChannelProperty =
            DependencyProperty.Register( "Channel", typeof( IrcChannel ), typeof( ChannelUsersControl ), new PropertyMetadata( OnChannelPropertyChanged ) );

        private static void OnChannelPropertyChanged( DependencyObject obj, DependencyPropertyChangedEventArgs args )
        {
            var control = (ChannelUsersControl) obj;

            control.Users = new TransformedCollection<IrcUserChannelModePair>( control.Channel.UserModes, UsersGrouper.SortUsers );
            control.Users.AddDependentProperty( "Mode" );
            control.Users.AddDependentProperty( u => u.User, "Nickname" );
        }
        #endregion

        #region Public properties
        public TransformedCollection<IrcUserChannelModePair> Users
        {
            get { return this._users; }
            set
            {
                if ( this._users != value )
                {
                    this._users = value;
                    this.FirePropertyChanged();
                }
            }
        }
        #endregion

        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void FirePropertyChanged( [CallerMemberName] string propertyName = "" )
        {
            if ( this.PropertyChanged != null )
            {
                this.PropertyChanged( this, new PropertyChangedEventArgs( propertyName ) );
            }
        }
        #endregion

        private static class UsersGrouper
        {
            private static readonly ITranslationService Service = Locator.Get<ITranslationService>();
            private static readonly string OwnerDescription = Service.Translate( "Channel", "Owner" );
            private static readonly string AdminUsersDescription = Service.Translate( "Channel", "AdminUsers" );
            private static readonly string OpUsersDescription = Service.Translate( "Channel", "OperatorUsers" );
            private static readonly string HalfOpUsersDescription = Service.Translate( "Channel", "HalfOpUsers" );
            private static readonly string VoicedUsersDescription = Service.Translate( "Channel", "VoicedUsers" );
            private static readonly string NormalUsersDescription = Service.Translate( "Channel", "NormalUsers" );

            private static Dictionary<IrcChannelUserModes, string> Groups = new Dictionary<IrcChannelUserModes, string>
            {
                { IrcChannelUserModes.Owner, OwnerDescription },
                { IrcChannelUserModes.Admin, AdminUsersDescription },
                { IrcChannelUserModes.Op, OpUsersDescription },
                { IrcChannelUserModes.HalfOp, HalfOpUsersDescription },
                { IrcChannelUserModes.Voiced, VoicedUsersDescription },
                { IrcChannelUserModes.Normal, NormalUsersDescription }
            };

            private static string GetModeString( IrcChannelUserModes userMode )
            {
                return Groups[Groups.Max( pair => pair.Key & userMode )];
            }

            public static IEnumerable<object> SortUsers( IEnumerable<IrcUserChannelModePair> users )
            {
                return users.OrderByDescending( p => p.Mode )
                            .GroupBy( p => GetModeString( p.Mode ) )
                            .SelectMany( g => new object[] { g.Key }.Concat( g.OrderBy( c => c.User.Nickname ) ) );
            }
        }
    }
}