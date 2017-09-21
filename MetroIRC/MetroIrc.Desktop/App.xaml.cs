using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using BasicMvvm;
using BasicMvvm.Dialogs;
using CommonStuff;
using IrcSharp.Ctcp;
using MetroControls;
using MetroIrc.Desktop.Log;
using MetroIrc.Desktop.Services;
using MetroIrc.Desktop.ViewModels;
using MetroIrc.Desktop.Views;
using MetroIrc.Log;
using MetroIrc.Services;
using WpfLoc;

namespace MetroIrc.Desktop
{
    public sealed partial class App : Application
    {
        #region Constants/readonly
        public const string NewHighlightedMessageSound = "NewHighlightedMessage.mp3";
        private const string PortableFileName = "portable.txt";
        private const string PortableDataFolderName = "Data";
        private const string InstalledDataFolderName = "MetroIRC";
        private static readonly CultureInfo DefaultCulture = new CultureInfo( "en-US" );
        private const string SoundsFolder = "Sounds";
        #endregion

        #region Private members
        private MediaPlayer _player = new MediaPlayer();
        #endregion

        #region Public static properties
        new public static App Current
        {
            get { return (App) Application.Current; }
        }
        #endregion

        #region Public properties
        /// <summary>
        /// Gets a value indicating whether this application is a portable installation.
        /// </summary>
        public bool IsPortableInstall { get; private set; }

        /// <summary>
        /// Gets the folder in which data should be stored.
        /// </summary>
        public string DataFolderPath { get; private set; }

        /// <summary>
        /// Gets the current directory (not the working directory).
        /// </summary>
        public string CurrentDirectory { get; private set; }

        /// <summary>
        /// The current app's settings.
        /// </summary>
        public WpfSettings Settings { get; private set; }
        #endregion

        public void PlaySound( string sound )
        {
            var uri = new Uri( Path.Combine( SoundsFolder, sound ), UriKind.Relative );
            this._player.Open( uri );
            this._player.Play();
        }

        static App()
        {
            AppDomain.CurrentDomain.UnhandledException += ExceptionLogger.LogAppDomainException;

            RegisterServices();

            CtcpClient.ClientSoftware.Name = "MetroIRC";
            CtcpClient.ClientSoftware.Version = Assembly.GetExecutingAssembly().GetName().Version.ToString( 2 );
            CtcpClient.ClientSoftware.OSName = "Windows";
            CtcpClient.ClientSoftware.DownloadLocation = "https://bitbucket.org/Aethec/metroirc/downloads";

            ColorManager.AddMainColorDictionary( Colors.Black, new Uri( "MetroIrc;component/Resources/Colors/Black.xaml", UriKind.Relative ) );
            ColorManager.AddMainColorDictionary( Colors.White, new Uri( "MetroIrc;component/Resources/Colors/White.xaml", UriKind.Relative ) );

            TranslationManager.TranslationProvider = TranslationManager.ResourcesTranslationProvider;

            if ( !TranslationManager.AvailableLanguages.Contains( TranslationManager.CurrentLanguage ) )
            {
                if ( !TranslationManager.AvailableLanguages.Contains( DefaultCulture ) )
                {
                    throw new ApplicationException( "Do not delete the en-US translation file." );
                }

                TranslationManager.CurrentLanguage = DefaultCulture;
            }
        }

        protected override void OnStartup( StartupEventArgs e )
        {
            base.OnStartup( e );

            this.CheckInstall();
            this.Settings = SettingsHelper.LoadSettings();
            Locator.Register<ISettings>( this.Settings );

            if ( UpdateChecker.CheckForUpdate( false ) )
            {
                Locator.Get<IDialogService>().ShowDialog( new UpdateWindowViewModel() );
                return;
            }

            var vm = new MainWindowViewModel();
            var view = new MainWindowView { DataContext = vm };
            this.MainWindow = view;
            vm.ConnectToStartupNetworks();
            this.MainWindow.Show();
        }

        protected override void OnExit( ExitEventArgs e )
        {
            // Networks & settings are already saved ; this is to save things such as the "Join on startup" channel checkbox
            SettingsHelper.SaveSettings( this.Settings );

            base.OnExit( e );
        }

        // Checks the install (installed/portable) and sets related properties.
        private void CheckInstall()
        {
            this.CurrentDirectory = Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location );

            string portableFilePath = Path.Combine( this.CurrentDirectory, PortableFileName );
            if ( File.Exists( portableFilePath ) )
            {
                this.IsPortableInstall = true;
                this.DataFolderPath = Path.Combine( this.CurrentDirectory, PortableDataFolderName );
            }
            else
            {
                this.IsPortableInstall = false;
                this.DataFolderPath = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData ), InstalledDataFolderName );
            }

            if ( !Directory.Exists( this.DataFolderPath ) )
            {
                Directory.CreateDirectory( this.DataFolderPath );
            }
        }

        private static void RegisterServices()
        {
            Locator.Register<IUIService>( new WpfUIService() );
            Locator.Register<IDialogService>( new WpfDialogService() );
            Locator.Register<ITranslationService>( new WpfTranslationService() );
            Locator.Register<ISmileyService>( new WpfSmileyService() );
            Locator.Register<INotificationService>( new WpfNotificationService() );
            Locator.Register<IResourceService>( new WpfResourceService() );
            Locator.Register<ITcpService>( new WpfTcpService() );
            Locator.Register<IrcLogger>( () => new WpfIrcLogger() );
        }
    }
}