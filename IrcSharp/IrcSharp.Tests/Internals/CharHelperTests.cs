// Copyright (C) 2013, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using IrcSharp.Internals;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IrcSharp.Tests.Internals
{
    [TestClass]
    public class CharHelperTests
    {
        [TestMethod]
        public void IsBasicLetter_BasicLetters()
        {
            for ( char c = 'a'; c <= 'z'; c++ )
            {
                Assert.IsTrue( CharHelper.IsBasicLetter( c ) );
            }

            for ( char c = 'A'; c <= 'Z'; c++ )
            {
                Assert.IsTrue( CharHelper.IsBasicLetter( c ) );
            }
        }

        [TestMethod]
        public void IsBasicLetter_Accents()
        {
            foreach ( char c in "àäâáîôöüù" )
            {
                Assert.IsFalse( CharHelper.IsBasicLetter( c ) );
            }
        }

        [TestMethod]
        public void IsBasicLetter_Digits()
        {
            for ( char c = '0'; c <= '9'; c++ )
            {
                Assert.IsFalse( CharHelper.IsBasicLetter( c ) );
            }
        }

        [TestMethod]
        public void IsBasicLetter_Others()
        {
            foreach ( char c in "_-{}[]^~!?|/" )
            {
                Assert.IsFalse( CharHelper.IsBasicLetter( c ) );
            }
        }
    }
}