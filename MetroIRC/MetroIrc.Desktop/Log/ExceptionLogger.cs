// Copyright (C) 2011-2013, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;

namespace MetroIrc.Desktop.Log
{
    /// <summary>
    /// A simple logger for exceptions, logging the exception, its inner exceptions and the last message sent by the client.
    /// </summary>
    public static class ExceptionLogger
    {
        private const string FileNameFormat = "crashlog.{0}.txt";
        private const string DateFormat = "yyyy-MM-dd hh-mm-ss";

        public static void LogAppDomainException( object sender, UnhandledExceptionEventArgs e )
        {
            LogException( (Exception) e.ExceptionObject );

#if !DEBUG
            if ( !e.IsTerminating )
            {
                App.Current.Shutdown();
            }
#endif
        }

        public static void LogException( Exception e )
        {
            var builder = new StringBuilder();
            builder.Append( "MetroIRC version " );
            builder.AppendLine( Assembly.GetExecutingAssembly().GetName().Version.ToString() );
            builder.AppendLine( "The application crashed at " + DateTime.Now.ToLongTimeString() );
            builder.AppendLine();
            builder.AppendLine( "Main exception:" );
            builder.AppendLine( "Type: " + e.GetType().ToString() );
            builder.AppendLine( e.Message );
            builder.AppendLine();
            builder.AppendLine( e.StackTrace );
            builder.AppendLine();
            while ( e.InnerException != null )
            {
                e = e.InnerException;
                builder.AppendLine( "Inner exception:" );
                builder.AppendLine( "Type: " + e.GetType().ToString() );
                builder.AppendLine( e.Message );
                builder.AppendLine();
                builder.AppendLine( e.StackTrace );
                builder.AppendLine();
            }

            foreach ( var pair in WpfIrcLogger.CurrentLoggers )
            {
                builder.AppendLine();
                builder.AppendLine( "Network: " + pair.Key );
                builder.AppendLine( "Last messages:" );

                foreach ( var message in pair.Value.LastMessages )
                {
                    builder.AppendLine( message );
                }

                if ( pair.Value.Channels.Any() )
                {
                    builder.AppendLine( "Channels:" );
                    builder.AppendLine( string.Join( ", ", pair.Value.Channels.Select( c => c.FullName ) ) );
                }
                else
                {
                    builder.AppendLine( "No channels." );
                }

                builder.AppendLine();
            }

            if ( WpfIrcLogger.CurrentLoggers.Count == 0 )
            {
                builder.AppendLine( "No networks." );
                builder.AppendLine();
            }

            builder.AppendLine( "EOF." );

            string fileName = string.Format( FileNameFormat, DateTime.Now.ToString( DateFormat ) );
            string path = Path.Combine( App.Current.DataFolderPath, fileName );
            File.WriteAllText( path, builder.ToString() );

            MessageBox.Show( "Sorry. The application crashed.\n The crash log has been saved in " + path );

        }
    }
}