// Copyright (C) 2013, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Linq;
using System.Reflection;
using IrcSharp.Internals;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IrcSharp.Tests.Internals
{
    [TestClass]
    public class ReflectionHelperTests
    {
        [AttributeUsage( AttributeTargets.All, AllowMultiple = true )]
        private sealed class MarkAttribute : Attribute
        {
            public int Value { get; set; }
        }

        [AttributeUsage( AttributeTargets.All )]
        private sealed class OtherAttribute : Attribute { }

        private static class NestedClass
        {
            public static void NoAttributes() { }

            [Mark( Value = 0 )]
            public static void MarkAttributeIsZero() { }

            [Mark( Value = 1 ), Mark( Value = 2 )]
            public static void MarkAttributeIsOneAndTwo() { }

            [Other]
            public static void Other() { }

            [Other, Mark( Value = 3 )]
            public static void OtherAndMarkThree() { }

            public static int Returns42() { return 42; }
        }

        [TestMethod]
        public void GetAttributedMethods()
        {
            var result = ReflectionHelper.GetAttributedMethods<MarkAttribute>( typeof( NestedClass ) );
            var expected = new[]
            {
                Tuple.Create<int, Action>(0, NestedClass.MarkAttributeIsZero),
                Tuple.Create<int, Action>(1, NestedClass.MarkAttributeIsOneAndTwo),
                Tuple.Create<int, Action>(2, NestedClass.MarkAttributeIsOneAndTwo),
                Tuple.Create<int, Action>(3, NestedClass.OtherAndMarkThree)
            };

            foreach ( var exp in expected )
            {
                Assert.IsTrue( result.Single( t => t.Item1.Value == exp.Item1 ).Item2.Name == exp.Item2.GetMethodInfo().Name );
            }
        }

        [TestMethod]
        public void GetStaticDelegate()
        {
            var info = typeof( NestedClass ).GetMethods().Single( mi => mi.Name == "Returns42" );
            Assert.AreEqual( 42, info.GetStaticDelegate<Func<int>>()() );
        }
    }
}