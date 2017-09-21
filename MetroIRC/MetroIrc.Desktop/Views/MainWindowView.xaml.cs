using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BasicMvvm;
using MetroControls;
using CM = System.ComponentModel;

namespace MetroIrc.Desktop.Views
{
    public sealed partial class MainWindowView : MetroWindow
    {
        public MainWindowView()
        {
            InitializeComponent();
            // Since you can't place DataTriggers directly in a Window...
            UpdateNetworksPanePosition();
            PropertyChangedEventManager.AddHandler( App.Current.Settings, Settings_PropertyChanged );
        }

        private void Settings_PropertyChanged( object sender, CM.PropertyChangedEventArgs e )
        {
            UpdateNetworksPanePosition();
        }

        private void UpdateNetworksPanePosition()
        {
            switch ( App.Current.Settings.NetworksListPosition )
            {
                case PaneDock.Left:
                    this.HorizontalNetworksPane.Visibility = Visibility.Collapsed;
                    this.VerticalNetworksPane.Visibility = Visibility.Visible;
                    Grid.SetColumn( this.VerticalNetworksPane, 0 );
                    this.LeftShadowBorder.Visibility = Visibility.Collapsed;
                    this.RightShadowBorder.Visibility = Visibility.Visible;
                    break;
                case PaneDock.Top:
                    this.VerticalNetworksPane.Visibility = Visibility.Collapsed;
                    this.HorizontalNetworksPane.Visibility = Visibility.Visible;
                    break;
                case PaneDock.Right:
                    this.HorizontalNetworksPane.Visibility = Visibility.Collapsed;
                    this.VerticalNetworksPane.Visibility = Visibility.Visible;
                    Grid.SetColumn( this.VerticalNetworksPane, 2 );
                    this.LeftShadowBorder.Visibility = Visibility.Visible;
                    this.RightShadowBorder.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        protected override void OnMouseLeftButtonDown( MouseButtonEventArgs e )
        {
            base.OnMouseLeftButtonDown( e );
            if ( !e.Handled )
            {
                FocusManager.SetFocusedElement( this, this.InputTextBox );
            }
        }
    }
}