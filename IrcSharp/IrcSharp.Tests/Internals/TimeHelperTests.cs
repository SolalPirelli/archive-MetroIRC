// Copyright (C) 2013, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using IrcSharp.Internals;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IrcSharp.Tests.Internals
{
    [TestClass]
    public class TimeHelperTests
    {
        [TestMethod]
        public void DateTimeFromUnixSeconds_UnixStart()
        {
            Assert.AreEqual( new DateTime( 1970, 1, 1 ), TimeHelper.DateTimeFromUnixSeconds( 0 ) );
        }

        [TestMethod]
        public void DateTimeFromUnixSeconds_WikiExample1()
        {
            // from http://en.wikipedia.org/wiki/Unix_time
            Assert.AreEqual( DateTime.Parse( "2004-09-17T00:00:00.00" ), TimeHelper.DateTimeFromUnixSeconds( 1095379200 ) );
        }

        [TestMethod]
        public void DateTimeFromUnixSeconds_WikiExample2()
        {
            // from http://en.wikipedia.org/wiki/Unix_time
            Assert.AreEqual( DateTime.Parse( "1999-01-01T00:00:00.00" ), TimeHelper.DateTimeFromUnixSeconds( 915148800 ) );
        }

        [TestMethod]
        public void DateTimeToUnixMilliseconds_NoMilliseconds()
        {
            Assert.AreEqual( 1362575700000, TimeHelper.DateTimeToUnixMilliseconds( DateTime.Parse( "2013-03-06T13:15:00.00" ) ) );
        }


        [TestMethod]
        public void DateTimeToUnixMilliseconds_WithMilliseconds()
        {
            Assert.AreEqual( 1362575700420, TimeHelper.DateTimeToUnixMilliseconds( DateTime.Parse( "2013-03-06T13:15:00.42" ) ) );
        }
    }
}