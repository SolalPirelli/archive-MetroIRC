// Copyright (C) 2012, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using IrcSharp.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IrcSharp.Tests
{
    [TestClass]
    public class ValidateTests
    {
        [TestMethod]
        public void IsNotNull_NotNull()
        {
            Validate.IsNotNull( string.Empty, "N/A" );
        }

        [TestMethod]
        [ExpectedException( typeof( ArgumentException ), AllowDerivedTypes = true )]
        public void IsNotNull_Null()
        {
            Validate.IsNotNull<string>( null, "N/A" );
        }

        [TestMethod]
        public void HasText_Text()
        {
            Validate.HasText( "not empty", "N/A" );
        }

        [TestMethod]
        [ExpectedException( typeof( ArgumentException ), AllowDerivedTypes = true )]
        public void HasText_Null()
        {
            Validate.HasText( null, "N/A" );
        }

        [TestMethod]
        [ExpectedException( typeof( ArgumentException ), AllowDerivedTypes = true )]
        public void HasText_Empty()
        {
            Validate.HasText( string.Empty, "N/A" );
        }

        [TestMethod]
        [ExpectedException( typeof( ArgumentException ), AllowDerivedTypes = true )]
        public void HasText_Blank()
        {
            Validate.HasText( "   ", "N/A" );
        }

        [TestMethod]
        public void IsPositive_StrictlyPositive()
        {
            Validate.IsPositive( 1, "N/A" );
        }

        [TestMethod]
        public void IsPositive_Zero()
        {
            Validate.IsPositive( 0, "N/A" );
        }

        [TestMethod]
        [ExpectedException( typeof( ArgumentException ), AllowDerivedTypes = true )]
        public void IsPositive_Negative()
        {
            Validate.IsPositive( -1, "N/A" );
        }

        private enum TestEnum { SingleValidValue = 0 }

        [TestMethod]
        public void IsEnumValue_EnumValue()
        {
            Validate.IsEnumValue( TestEnum.SingleValidValue, "N/A" );
        }

        [TestMethod]
        [ExpectedException( typeof( ArgumentException ), AllowDerivedTypes = true )]
        public void IsEnumValue_NotEnumValue()
        {
            Validate.IsEnumValue( (TestEnum) ( -1 ), "N/A" );
        }

        [TestMethod]
        public void IsNotDisposed_NotDisposed()
        {
            Validate.IsNotDisposed( false );
        }

        [TestMethod]
        [ExpectedException( typeof( ObjectDisposedException ) )]
        public void IsNotDisposed_Disposed()
        {
            Validate.IsNotDisposed( true );
        }
    }
}