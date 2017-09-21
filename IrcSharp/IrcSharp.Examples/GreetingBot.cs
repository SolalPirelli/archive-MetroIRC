// Copyright (C) 2012-2013 Solal Pirelli
// This code is licensed under the Do What The Fuck You Want To Public Licence (see Properties\Licence.txt).

using System.Threading.Tasks;
using IrcSharp.External;

namespace IrcSharp.Examples
{
    /// <summary>
    /// A very simple bot that greets people on a channel.
    /// </summary>
    public sealed class GreetingBot
    {
        private string _nickname, _channelName;
        private IrcNetwork _network;

        public GreetingBot( string nickname, string channelName )
        {
            _nickname = nickname;
            _channelName = channelName;
        }

        public async Task<bool> ConnectAsync( string hostName, int port )
        {
            // Create an IrcNetwork, injecting a SocketWrapper
            _network = new IrcNetwork( new SocketWrapper( hostName, port ) );

            // The only options here are to connect as an user or as a service. 
            // We'll keep things simple and be a normal user.
            if ( await _network.ConnectAsync( _nickname, "Greeting Bot", IrcUserLoginModes.None ) )
            {
                // We connected successfully, so join the channel and monitor it.
                // However, we have to wait for a while before joining the channel - some networks are a bit slow
                var channel = _network.GetChannel( _channelName );
                channel.UserJoined += Channel_UserJoined;

                // Wait one second
                await Task.Delay( 1000 );

                channel.Join();
                return true;
            }

            // An error occured.
            return false;
        }

        // Notice the method arguments - the sender parameter is typed, it's not just an object.
        private void Channel_UserJoined( IrcChannel channel, UserEventArgs args )
        {
            string text = string.Format( "Welcome to {0}, {1}!", channel.FullName, args.User.Nickname );
            args.User.SendNotice( text );
        }

        public void Quit()
        {
            // Exit gracefully
            _network.Quit( "Goodbye." );
            _network.Dispose();
        }
    }
}