// Copyright (C) 2013, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Linq;
using IrcSharp.Internals;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IrcSharp.Tests
{
    [TestClass]
    public sealed class CapabilityParserTests
    {
        [TestMethod]
        public void CaseMapping_Ascii()
        {
            var p = Test( "CASEMAPPING=ascii" );
            Assert.AreEqual( IrcCaseMapping.Ascii, p.CaseMapping );
        }

        [TestMethod]
        public void CaseMapping_Rfc1459()
        {
            var p = Test( "CASEMAPPING=rfc1459" );
            Assert.AreEqual( IrcCaseMapping.Rfc1459, p.CaseMapping );
        }

        [TestMethod]
        public void CaseMapping_StrictRfc1459()
        {
            var p = Test( "CASEMAPPING=strict-rfc1459" );
            Assert.AreEqual( IrcCaseMapping.StrictRfc1459, p.CaseMapping );
        }

        [TestMethod]
        public void ChanLimit_SingleValue()
        {
            var p = Test( "CHANLIMIT=#:10" );
            Assert.IsNotNull( p.ChannelCountLimits );
            Assert.AreEqual( 1, p.ChannelCountLimits.Count );
            Assert.AreEqual( 10, p.ChannelCountLimits[IrcChannelKind.Standard] );
        }

        [TestMethod]
        public void ChanLimit_TwoEqualValues()
        {
            var p = Test( "CHANLIMIT=#&:10" );
            Assert.AreEqual( 2, p.ChannelCountLimits.Count );
            Assert.AreEqual( 10, p.ChannelCountLimits[IrcChannelKind.Standard] );
            Assert.AreEqual( 10, p.ChannelCountLimits[IrcChannelKind.SupportsAnonymousConversations] );
        }

        [TestMethod]
        public void ChanLimit_MultipleValues()
        {
            var p = Test( "CHANLIMIT=#:10,+:20" );
            Assert.AreEqual( 10, p.ChannelCountLimits[IrcChannelKind.Standard] );
            Assert.AreEqual( 20, p.ChannelCountLimits[IrcChannelKind.NoModes] );
        }


        [TestMethod]
        public void ChanLimit_UnknownPrefix()
        {
            var p = Test( "CHANLIMIT=#:10,~:10" );
            Assert.AreEqual( 10, p.ChannelCountLimits[IrcChannelKind.Standard] );
        }

        [TestMethod]
        public void ChanModes()
        {
            var p = Test( "CHANMODES=aA,bB,cC,dD" );
            Assert.IsNotNull( p.ChannelModes );
            CollectionAssert.AreEquivalent( new[] { 'a', 'A' }, p.ChannelModes.ListModes );
            CollectionAssert.AreEquivalent( new[] { 'b', 'B' }, p.ChannelModes.ParameterizedModes );
            CollectionAssert.AreEquivalent( new[] { 'c', 'C' }, p.ChannelModes.ParameterizedOnSetModes );
            CollectionAssert.AreEquivalent( new[] { 'd', 'D' }, p.ChannelModes.ParameterlessModes );
        }

        [TestMethod]
        public void ChannelLen()
        {
            var p = Test( "CHANNELLEN=10" );
            Assert.AreEqual( p.MaxChannelNameLength, 10 );
        }

        [TestMethod]
        public void ChanTypes_One()
        {
            var p = Test( "CHANTYPES=#" );
            Assert.IsNotNull( p.ChannelKinds );
            Assert.AreEqual( 1, p.ChannelKinds.Count );
            Assert.AreEqual( '#', p.ChannelKinds.First().Key );
            Assert.AreEqual( IrcChannelKind.Standard, p.ChannelKinds['#'] );
        }

        [TestMethod]
        public void ChanTypes_Many()
        {
            var p = Test( "CHANTYPES=#+&!" );
            Assert.AreEqual( 4, p.ChannelKinds.Count );
            Assert.AreEqual( IrcChannelKind.Standard, p.ChannelKinds['#'] );
            Assert.AreEqual( IrcChannelKind.NoModes, p.ChannelKinds['+'] );
            Assert.AreEqual( IrcChannelKind.Safe, p.ChannelKinds['!'] );
            Assert.AreEqual( IrcChannelKind.SupportsAnonymousConversations, p.ChannelKinds['&'] );
        }

        [TestMethod]
        public void ChanTypes_Unknown()
        {
            var p = Test( "CHANTYPES=#~" );
            Assert.AreEqual( 2, p.ChannelKinds.Count );
            Assert.AreEqual( IrcChannelKind.Standard, p.ChannelKinds['#'] );
            Assert.AreEqual( IrcChannelKind.Standard, p.ChannelKinds['~'] );
        }

        [TestMethod]
        public void Excepts_NoArg()
        {
            var p = Test( "EXCEPTS" );
            Assert.IsNotNull( p.ChannelModes );
            Assert.AreEqual( 'e', p.ChannelModes.BanExceptionMode );
            Assert.AreEqual( true, p.AreBanExceptionsEnabled );
        }

        [TestMethod]
        public void Excepts_WithArg()
        {
            var p = Test( "EXCEPTS=Z" );
            Assert.AreEqual( 'Z', p.ChannelModes.BanExceptionMode );
        }

        [TestMethod]
        public void InvEx_NoArg()
        {
            var p = Test( "INVEX" );
            Assert.IsNotNull( p.ChannelModes );
            Assert.AreEqual( 'I', p.ChannelModes.InviteExceptionMode );
            Assert.AreEqual( true, p.AreInviteExceptionsEnabled );
        }

        [TestMethod]
        public void InvEx_WithArg()
        {
            var p = Test( "INVEX=Z" );
            Assert.AreEqual( 'Z', p.ChannelModes.InviteExceptionMode );
        }

        [TestMethod]
        public void KickLen()
        {
            var p = Test( "KICKLEN=10" );
            Assert.AreEqual( 10, p.MaxKickReasonLength );
        }

        [TestMethod]
        public void MaxChannels()
        {
            var p = Test( "MAXCHANNELS=10" );
            Assert.IsNotNull( p.ChannelCountLimits );
            foreach ( var val in Enum.GetValues( typeof( IrcChannelKind ) ).Cast<IrcChannelKind>() )
            {
                Assert.AreEqual( 10, p.ChannelCountLimits[val] );
            }
        }

        [TestMethod]
        public void MaxChannels_DoesNotOverrideChanLimit()
        {
            var p = Test( "CHANLIMIT=#:20" );
            Test( p, "MAXCHANNELS=10" );

            Assert.AreEqual( 1, p.ChannelCountLimits.Count );
            Assert.AreEqual( 20, p.ChannelCountLimits[IrcChannelKind.Standard] );
        }

        [TestMethod]
        public void MaxChannels_IsOverridenByChanLimit()
        {
            var p = Test( "MAXCHANNELS=10" );
            Test( p, "CHANLIMIT=#:20" );

            Assert.AreEqual( 1, p.ChannelCountLimits.Count );
            Assert.AreEqual( 20, p.ChannelCountLimits[IrcChannelKind.Standard] );
        }

        [TestMethod]
        public void Network()
        {
            var p = Test( "NETWORK=ExampleNetwork" );
            Assert.AreEqual( "ExampleNetwork", p.NetworkName );
        }

        [TestMethod]
        public void NickLen()
        {
            var p = Test( "NICKLEN=10" );
            Assert.AreEqual( 10, p.MaxNicknameLength );
        }

        [TestMethod]
        public void Prefix_Single()
        {
            var p = Test( "PREFIX=(o)@" );
            Assert.IsNotNull( p.ChannelModes );
            Assert.IsNotNull( p.ChannelModes.UserModes );
            Assert.AreEqual( 1, p.ChannelModes.UserModes.Count );
            Assert.AreEqual( IrcChannelUserModes.Op, p.ChannelModes.UserModes['o'] );
            Assert.IsNotNull( p.ChannelModes.UserPrefixes );
            Assert.AreEqual( 1, p.ChannelModes.UserPrefixes.Count );
            Assert.AreEqual( IrcChannelUserModes.Op, p.ChannelModes.UserPrefixes['@'] );
        }

        [TestMethod]
        public void Prefix_Multiple()
        {
            var p = Test( "PREFIX=(oh)@%" );
            Assert.AreEqual( 2, p.ChannelModes.UserModes.Count );
            Assert.AreEqual( IrcChannelUserModes.Op, p.ChannelModes.UserModes['o'] );
            Assert.AreEqual( IrcChannelUserModes.HalfOp, p.ChannelModes.UserModes['h'] );
            Assert.AreEqual( 2, p.ChannelModes.UserPrefixes.Count );
            Assert.AreEqual( IrcChannelUserModes.Op, p.ChannelModes.UserPrefixes['@'] );
            Assert.AreEqual( IrcChannelUserModes.HalfOp, p.ChannelModes.UserPrefixes['%'] );
        }

        [TestMethod]
        public void TopicLen()
        {
            var p = Test( "TOPICLEN=10" );
            Assert.AreEqual( 10, p.MaxTopicLength );
        }

        [TestMethod]
        public void Parser_MultipleTokens()
        {
            var p = Test( "TOPICLEN=10", "CHANNELLEN=20" );
            Assert.AreEqual( 10, p.MaxTopicLength );
            Assert.AreEqual( 20, p.MaxChannelNameLength );
        }

        [TestMethod]
        public void Parser_IgnoresInvalidTokens()
        {
            var p = Test( "INVALIDTOKEN", "TOPICLEN=10", "ANOTHERINVALIDTOKEN" );
            Assert.AreEqual( 10, p.MaxTopicLength );
        }

        private static IrcNetworkParameters Test( params string[] tokens )
        {
            var parameters = new IrcNetworkParameters();
            CapabilityParser.Parse( parameters, tokens );
            return parameters;
        }

        private static IrcNetworkParameters Test( IrcNetworkParameters existing, params string[] tokens )
        {
            CapabilityParser.Parse( existing, tokens );
            return existing;
        }
    }
}