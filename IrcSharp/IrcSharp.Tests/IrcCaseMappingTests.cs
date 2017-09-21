// Copyright (C) 2013, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IrcSharp.Tests
{
    [TestClass]
    public class IrcCaseMappingTests
    {
        [TestMethod]
        public void Ascii()
        {
            var builder = new StringBuilder();
            for ( int n = 0; n < 256; n++ )
            {
                builder.Append( (char) n );
            }

            string lower = IrcCaseMapping.Ascii.ToLower( builder.ToString() );

            Assert.AreEqual( builder.ToString().ToLowerInvariant(), lower );
        }

        [TestMethod]
        public void Rfc1459()
        {
            var builder = new StringBuilder();
            for ( int n = 0; n < 256; n++ )
            {
                builder.Append( (char) n );
            }

            string actual = IrcCaseMapping.Rfc1459.ToLower( builder.ToString() );
            string expected = builder.ToString().ToLowerInvariant()
                                     .Replace( '[', '{' ).Replace( ']', '}' ).Replace( '\\', '|' ).Replace( '^', '~' );

            Assert.AreEqual( expected, actual );
        }


        [TestMethod]
        public void StrictRfc1459()
        {
            var builder = new StringBuilder();
            for ( int n = 0; n < 256; n++ )
            {
                builder.Append( (char) n );
            }

            string actual = IrcCaseMapping.StrictRfc1459.ToLower( builder.ToString() );
            string expected = builder.ToString().ToLowerInvariant()
                                     .Replace( '[', '{' ).Replace( ']', '}' ).Replace( '\\', '|' );

            Assert.AreEqual( expected, actual );
        }

        [TestMethod]
        public void AreEqual_Equal()
        {
            Assert.IsTrue( IrcCaseMapping.Rfc1459.AreEqual( "[abc]", "{ABC}" ) );
        }

        [TestMethod]
        public void AreEqual_NotEqual()
        {
            Assert.IsFalse( IrcCaseMapping.Rfc1459.AreEqual( "[abc]", "{XYZ}" ) );
        }

        [TestMethod]
        public void Compare_Greater()
        {
            Assert.AreEqual( 1, IrcCaseMapping.Rfc1459.Compare( "}", "[" ) );
        }

        [TestMethod]
        public void Compare_Equal()
        {
            Assert.AreEqual( 0, IrcCaseMapping.Rfc1459.Compare( "}", "]" ) );
        }

        [TestMethod]
        public void Compare_Lesser()
        {
            Assert.AreEqual( -1, IrcCaseMapping.Rfc1459.Compare( "{", "]" ) );
        }
    }
}