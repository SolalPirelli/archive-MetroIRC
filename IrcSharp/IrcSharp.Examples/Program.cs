// Copyright (C) 2012-2013 Solal Pirelli
// This code is licensed under the Do What The Fuck You Want To Public Licence (see Properties\Licence.txt).

using System;

namespace IrcSharp.Examples
{
    public class Program
    {
        public const string HostName = "irc.smoothirc.net";
        public const int Port = 6667;

        public static void Main( string[] args )
        {
            StartGreetingBot();
            Console.ReadKey();
        }

        private static async void StartGreetingBot()
        {
            var bot = new GreetingBot( "GreetingBot", "#greetingtest" );

            if ( await bot.ConnectAsync( HostName, Port ) )
            {
                Console.WriteLine( "Greeting bot started. Press any key to stop it." );
                Console.ReadKey();
                bot.Quit();
            }
            else
            {
                Console.WriteLine( "The greeting bot could not connect." );
                Console.ReadKey();
            }
        }
    }
}