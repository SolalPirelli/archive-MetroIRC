// Copyright (C) 2013, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using IrcSharp.Internals;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IrcSharp.Tests.Internals
{
    [TestClass]
    public class ObservableObjectTests
    {
        private class TestObject : ObservableObject
        {
            private int _value;
            public int Value
            {
                get { return this._value; }
                set { this.SetProperty( ref this._value, value ); }
            }
        }

        [TestMethod]
        public void SetProperty_PropertyIsSet()
        {
            var obj = new TestObject();
            obj.Value = 42;
            Assert.AreEqual( 42, obj.Value );
        }

        [TestMethod]
        public void SetProperty_FiresPropertyChanged()
        {
            var obj = new TestObject();
            bool fired = false;

            obj.PropertyChanged += ( s, e ) =>
            {
                fired = true;
            };

            obj.Value = 42;
            Assert.IsTrue( fired );
        }

        [TestMethod]
        public void SetProperty_DoesNotChange_DoesNotFireEvent()
        {
            var obj = new TestObject();
            bool fired = false;
            obj.Value = 42;

            obj.PropertyChanged += ( s, e ) =>
            {
                fired = true;
            };

            obj.Value = obj.Value;
            Assert.IsFalse( fired );
        }
    }
}