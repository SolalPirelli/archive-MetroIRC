// Copyright (C) 2013, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace IrcSharp.Tests
{
    [TestClass]
    public class IrcChannelModeCollectionTests
    {
        [TestMethod]
        public void SplitMode_OneListMode_WithArgument_Added()
        {
            var coll = GetCollection( list: "x" );
            var modes = coll.SplitMode( "+x arg" );
            AssertModesEqual( modes, "+x arg" );
        }

        [TestMethod]
        public void SplitMode_OneListMode_WithoutArgument_Added()
        {
            var coll = GetCollection( list: "x" );
            var modes = coll.SplitMode( "+x" );
            AssertModesEqual( modes, "+x" );
        }

        [TestMethod]
        public void SplitMode_OneListMode_WithArgument_Removed()
        {
            var coll = GetCollection( list: "x" );
            var modes = coll.SplitMode( "-x arg" );
            AssertModesEqual( modes, "-x arg" );
        }

        [TestMethod]
        public void SplitMode_OneListMode_WithoutArgument_Removed()
        {
            var coll = GetCollection( list: "x" );
            var modes = coll.SplitMode( "-x" );
            Assert.IsNull( modes );
        }

        [TestMethod]
        public void SplitMode_OneParameterizedMode_WithArgument_Added()
        {
            var coll = GetCollection( param: "x" );
            var modes = coll.SplitMode( "+x arg" );
            AssertModesEqual( modes, "+x arg" );
        }

        [TestMethod]
        public void SplitMode_OneParameterizedMode_WithoutArgument_Added()
        {
            var coll = GetCollection( param: "x" );
            var modes = coll.SplitMode( "+x" );
            Assert.IsNull( modes );
        }

        [TestMethod]
        public void SplitMode_OneParameterizedMode_WithArgument_Removed()
        {
            var coll = GetCollection( param: "x" );
            var modes = coll.SplitMode( "-x arg" );
            AssertModesEqual( modes, "-x arg" );
        }

        [TestMethod]
        public void SplitMode_OneParameterizedMode_WithoutArgument_Removed()
        {
            var coll = GetCollection( param: "x" );
            var modes = coll.SplitMode( "-x" );
            Assert.IsNull( modes );
        }

        [TestMethod]
        public void SplitMode_OneParameterizedOnSetMode_WithArgument_Added()
        {
            var coll = GetCollection( paramOnSet: "x" );
            var modes = coll.SplitMode( "+x arg" );
            AssertModesEqual( modes, "+x arg" );
        }

        [TestMethod]
        public void SplitMode_OneParameterizedOnSetMode_WithoutArgument_Added()
        {
            var coll = GetCollection( paramOnSet: "x" );
            var modes = coll.SplitMode( "+x" );
            Assert.IsNull( modes );
        }

        [TestMethod]
        public void SplitMode_OneParameterizedOnSetMode_WithArgument_Removed()
        {
            var coll = GetCollection( paramOnSet: "x" );
            var modes = coll.SplitMode( "-x arg" );
            Assert.IsNull( modes );
        }

        [TestMethod]
        public void SplitMode_OneParameterizedOnSetMode_WithoutArgument_Removed()
        {
            var coll = GetCollection( paramOnSet: "x" );
            var modes = coll.SplitMode( "-x" );
            AssertModesEqual( modes, "-x" );
        }

        [TestMethod]
        public void SplitMode_OneParameterlessMode_WithArgument_Added()
        {
            var coll = GetCollection( noParam: "x" );
            var modes = coll.SplitMode( "+x arg" );
            Assert.IsNull( modes );
        }

        [TestMethod]
        public void SplitMode_OneParameterlessMode_WithoutArgument_Added()
        {
            var coll = GetCollection( noParam: "x" );
            var modes = coll.SplitMode( "+x" );
            AssertModesEqual( modes, "+x" );
        }

        [TestMethod]
        public void SplitMode_OneParameterlessMode_WithArgument_Removed()
        {
            var coll = GetCollection( noParam: "x" );
            var modes = coll.SplitMode( "-x arg" );
            Assert.IsNull( modes );
        }

        [TestMethod]
        public void SplitMode_OneParameterlessMode_WithoutArgument_Removed()
        {
            var coll = GetCollection( noParam: "x" );
            var modes = coll.SplitMode( "-x" );
            AssertModesEqual( modes, "-x" );
        }

        [TestMethod]
        public void SplitMode_ManyListModes()
        {
            var coll = GetCollection( list: "lL" );
            var modes = coll.SplitMode( "+ll arg0 arg1 -Ll arg2 arg3 +l" );
            AssertModesEqual( modes, "+l arg0", "+l arg1", "-L arg2", "-l arg3", "+l" );
        }

        [TestMethod]
        public void SplitMode_ManyParameterizedModes()
        {
            var coll = GetCollection( param: "xyz" );
            var modes = coll.SplitMode( "+z arg0 +xyy arg1 arg2 arg3 -z-x arg4 arg5 +x-z arg6 arg7" );
            AssertModesEqual( modes, "+z arg0", "+x arg1", "+y arg2", "+y arg3", "-z arg4", "-x arg5", "+x arg6", "-z arg7" );
        }

        [TestMethod]
        public void SplitMode_ManyParameterizedOnSetModes()
        {
            var coll = GetCollection( paramOnSet: "ghi" );
            var modes = coll.SplitMode( "+gh-i arg0 arg1 -g+h arg2 +i arg3 -i" );
            AssertModesEqual( modes, "+g arg0", "+h arg1", "-i", "-g", "+h arg2", "+i arg3", "-i" );
        }

        [TestMethod]
        public void SplitMode_ManyParameterlessModes()
        {
            var coll = GetCollection( noParam: "abc" );
            var modes = coll.SplitMode( "+ab-c+c -c+a -aab -b" );
            AssertModesEqual( modes, "+a", "+b", "-c", "+c", "-c", "+a", "-a", "-a", "-b", "-b" );
        }

        [TestMethod]
        public void SplitMode_BasicStackedModes()
        {
            var coll = GetCollection( param: "xyz", noParam: "abc", paramOnSet: "ghi" );
            var modes = coll.SplitMode( "+xag arg0 arg1 -y arg2 -ci" );
            AssertModesEqual( modes, "+x arg0", "+a", "+g arg1", "-y arg2", "-c", "-i" );
        }

        [TestMethod]
        public void SplitMode_ComplexStackedModes()
        {
            var coll = GetCollection( list: "lL", param: "xyz", noParam: "abc", paramOnSet: "ghi" );
            var modes = coll.SplitMode( "+lz arg0 arg1 -ah +b +i arg2 +yy-c arg3 arg4 +L arg5 +l" );
            AssertModesEqual( modes, "+l arg0", "+z arg1", "-a", "-h", "+b", "+i arg2", "+y arg3", "+y arg4", "-c", "+L arg5", "+l" );
        }

        [TestMethod]
        public void SplitMode_UserMode()
        {
            var coll = GetCollection();
            coll.UserModes = new ReadOnlyDictionary<char, IrcChannelUserModes>( new Dictionary<char, IrcChannelUserModes>
            {
                { 'o', IrcChannelUserModes.Op }
            } );

            var modes = coll.SplitMode( "+o arg0" );
            AssertModesEqual( modes, "+o arg0" );
        }

        private static IrcChannelModeCollection GetCollection( string list = "", string param = "", string paramOnSet = "", string noParam = "" )
        {
            return new IrcChannelModeCollection()
            {
                ListModes = new ReadOnlyCollection<char>( list.ToCharArray() ),
                ParameterizedModes = new ReadOnlyCollection<char>( param.ToCharArray() ),
                ParameterizedOnSetModes = new ReadOnlyCollection<char>( paramOnSet.ToCharArray() ),
                ParameterlessModes = new ReadOnlyCollection<char>( noParam.ToCharArray() ),
            };
        }

        private static void AssertModesEqual( IEnumerable<IrcMode> actual, params string[] expected )
        {
            Assert.IsNotNull( actual );
            Assert.AreEqual( expected.Length, actual.Count() );

            int index = 0;
            foreach ( var mode in actual )
            {
                string[] expSplit = expected[index].Split( ' ' );
                Assert.AreEqual( expSplit[0][0] == '+', mode.IsAdded );
                Assert.AreEqual( expSplit[0][0] == '-', !mode.IsAdded );
                Assert.AreEqual( expSplit[0][1], mode.Flag );

                if ( expSplit.Length > 1 )
                {
                    Assert.AreEqual( expSplit[1], mode.Argument );
                }
                else
                {
                    Assert.IsNull( mode.Argument );
                }

                index++;
            }
        }
    }
}