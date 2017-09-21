// Copyright (C) 2013, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;

namespace IrcSharp.Tests.TestInternals
{
    public static class Initializer
    {
        public static Tuple<IrcNetwork, TestTcpWrapper> GetAuthenticatedNetwork( string nick = "nick", string real = "real" )
        {
            var wrapper = new TestTcpWrapper();
            var network = new IrcNetwork( wrapper );
            network.ConnectAsync( nick, real, IrcUserLoginModes.None ).Wait();
            wrapper.EndAuthenticate();
            wrapper.LinesSent.Clear();
            return Tuple.Create( network, wrapper );
        }
    }
}