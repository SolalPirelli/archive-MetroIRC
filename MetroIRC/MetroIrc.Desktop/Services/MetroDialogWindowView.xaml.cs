using System.ComponentModel;
using BasicMvvm.Dialogs;
using MetroControls;

namespace MetroIrc.Desktop.Views
{
    // ZOMG! It's code-behind! OH NOES!!1!one!1!
    internal sealed partial class MetroDialogWindowView : MetroWindow
    {
        private DialogViewModel _content;

        public MetroDialogWindowView( DialogViewModel content )
        {
            this._content = content;
            this.DataContext = this._content;
            InitializeComponent();

            this._content.RequestClose += Content_RequestClose;
            this.Closing += This_Closing;
        }

        private void Content_RequestClose( object sender, DialogResultEventArgs e )
        {
            this.DialogResult = e.DialogResult;
            this.Close();
        }

        private void This_Closing( object sender, CancelEventArgs e )
        {
            this._content.BaseWindowClosing( sender, e );

            if ( !e.Cancel )
            {
                this._content.RequestClose -= Content_RequestClose;
            }
        }
    }
}