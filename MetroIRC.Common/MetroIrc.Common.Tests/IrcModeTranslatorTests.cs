using System.Linq;
using BasicMvvm;
using IrcSharp;
using MetroIrc.Common.Tests.Internals;
using MetroIrc.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MetroIrc.Common.Tests
{
    [TestClass]
    public class IrcModeTranslatorTests
    {
        private TestTranslator _translator;
        private IrcNetwork _network;

        [TestInitialize]
        public void Initialize()
        {
            this._translator = new TestTranslator();

            var wrapper = new TestTcpWrapper();
            this._network = IrcNetwork.Get( wrapper );
            this._network.Connect();
            wrapper.EndConnect();
            this._network.Authenticate( "Test", "Test", IrcUserLoginModes.None );

            Locator.Register<ITranslationService>( this._translator );
        }

        [TestMethod]
        public void IrcModeTranslator_ChannelMode_SimplePositive()
        {
            this._translator.Set( "ChannelMode", "SetBy", "{0}" );
            this._translator.Set( "PositiveChannelModes", "z", "z" );
            var user = this._network.GetUser( "a" );

            var lines = IrcModeTranslator.TranslateChannelMode( user, "+z" ).ToArray();

            Assert.AreEqual( 1, lines.Length );
            Assert.AreEqual( "za", lines[0] );
        }

        [TestMethod]
        public void IrcModeTranslator_ChannelMode_Unknown()
        {
            this._translator.Set( "ChannelMode", "SetBy", "{0}" );
            this._translator.Set( "ChannelMode", "Default", "{0}" );
            var user = this._network.GetUser( "a" );

            var lines = IrcModeTranslator.TranslateChannelMode( user, "+z" ).ToArray();

            Assert.AreEqual( 1, lines.Length );
            Assert.AreEqual( "+z a", lines[0] );
        }

        [TestMethod]
        public void IrcModeTranslator_ChannelMode_UnknownWithMode()
        {
            this._translator.Set( "ChannelMode", "SetBy", "{0}" );
            this._translator.Set( "ChannelMode", "Default", "{0}" );
            var user = this._network.GetUser( "a" );

            var lines = IrcModeTranslator.TranslateChannelMode( user, "+z x" ).ToArray();

            Assert.AreEqual( 1, lines.Length );
            Assert.AreEqual( "+z xa", lines[0] );
        }
    }
}