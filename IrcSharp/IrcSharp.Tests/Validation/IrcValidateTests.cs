// Copyright (C) 2013, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using IrcSharp.Tests.TestInternals;
using IrcSharp.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IrcSharp.Tests
{
    [TestClass]
    public class IrcValidateTests
    {
        [TestMethod]
        public void IsConnected_Connected()
        {
            IrcValidate.IsConnected( true );
        }

        [TestMethod]
        [ExpectedException( typeof( InvalidOperationException ) )]
        public void IsConnected_NotConnected()
        {
            IrcValidate.IsConnected( false );
        }

        [TestMethod]
        public void IsNotConnected_NotConnected()
        {
            IrcValidate.IsNotConnected( false );
        }

        [TestMethod]
        [ExpectedException( typeof( InvalidOperationException ) )]
        public void IsNotConnected_Connected()
        {
            IrcValidate.IsNotConnected( true );
        }

        [TestMethod]
        public void IsChannelName_ChannelName()
        {
            IrcValidate.IsChannelName( "#aaa", new IrcNetworkParameters(), "N/A" );
        }

        [TestMethod]
        [ExpectedException( typeof( ArgumentException ) )]
        public void IsChannelName_Null()
        {
            IrcValidate.IsChannelName( null, new IrcNetworkParameters(), "N/A" );
        }

        [TestMethod]
        [ExpectedException( typeof( ArgumentException ) )]
        public void IsChannelName_Empty()
        {
            IrcValidate.IsChannelName( string.Empty, new IrcNetworkParameters(), "N/A" );
        }

        [TestMethod]
        [ExpectedException( typeof( ArgumentException ) )]
        public void IsChannelName_NotChannelName()
        {
            IrcValidate.IsChannelName( "aaa", new IrcNetworkParameters(), "N/A" );
        }
    }
}