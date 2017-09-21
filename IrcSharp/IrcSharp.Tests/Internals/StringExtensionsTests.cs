// Copyright (C) 2013, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using IrcSharp.Internals;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IrcSharp.Tests.Internals
{
    [TestClass]
    public class StringExtensionsTests
    {
        [TestMethod]
        public void IsEmpty_Null()
        {
            Assert.IsTrue( ( (string) null ).IsEmpty() );
        }

        [TestMethod]
        public void IsEmpty_Empty()
        {
            Assert.IsTrue( string.Empty.IsEmpty() );
        }

        [TestMethod]
        public void IsEmpty_Whitespace()
        {
            Assert.IsTrue( "    ".IsEmpty() );
        }

        [TestMethod]
        public void IsEmpty_PartiallyWhitespace()
        {
            Assert.IsFalse( "    a    ".IsEmpty() );
        }

        [TestMethod]
        public void IsEmpty_NotEmpty()
        {
            Assert.IsFalse( "abc def".IsEmpty() );
        }

        [TestMethod]
        public void HasText_Null()
        {
            Assert.IsFalse( ( (string) null ).HasText() );
        }

        [TestMethod]
        public void HasText_Empty()
        {
            Assert.IsFalse( string.Empty.HasText() );
        }

        [TestMethod]
        public void HasText_Whitespace()
        {
            Assert.IsFalse( "    ".HasText() );
        }

        [TestMethod]
        public void HasText_PartiallyWhitespace()
        {
            Assert.IsTrue( "    a    ".HasText() );
        }

        [TestMethod]
        public void HasText_NotEmpty()
        {
            Assert.IsTrue( "abc def".HasText() );
        }

        [TestMethod]
        public void GetFirstWord_Empty()
        {
            Assert.AreEqual( string.Empty, string.Empty.GetFirstWord() );
        }

        [TestMethod]
        public void GetFirstWord()
        {
            Assert.AreEqual( "abc", "abc def ghi".GetFirstWord() );
        }

        [TestMethod]
        public void ReplaceFirst_Empty()
        {
            Assert.AreEqual( string.Empty, string.Empty.ReplaceFirst( "abc", "def" ) );
        }

        [TestMethod]
        public void ReplaceFirst_NonExistent()
        {
            Assert.AreEqual( "abc", "abc".ReplaceFirst( "def", "ghi" ) );
        }

        [TestMethod]
        public void ReplaceFirst()
        {
            Assert.AreEqual( "ghi def", "abc def".ReplaceFirst( "abc", "ghi" ) );
        }

        [TestMethod]
        public void ReplaceFirst_NotOthers()
        {
            Assert.AreEqual( "ghi def abc abc def abc", "abc def abc abc def abc".ReplaceFirst( "abc", "ghi" ) );
        }
    }
}