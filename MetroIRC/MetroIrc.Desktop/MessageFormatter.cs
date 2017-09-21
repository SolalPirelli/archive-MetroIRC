// Copyright (C) 2012-2013, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using BasicMvvm;
using CommonStuff;
using MetroIrc.Parsing;
using MetroIrc.Services;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

// word, containing message, result
using WordProcessor = System.Func<string, MetroIrc.IrcMessage, object>;
// word, containing message, whether to run the processor
using WordProcessorPredicate = System.Func<string, MetroIrc.IrcMessage, bool>;

namespace MetroIrc.Desktop
{
    public static partial class MessageFormatter
    {
        #region Constants/readonly
        private static readonly Dictionary<TextColor, Brush> Colors = new Dictionary<TextColor, Brush>
        {
            { TextColor.White, Brushes.White },
            { TextColor.Black, Brushes.Black },
            { TextColor.Navy, Brushes.Navy },
            { TextColor.Green, Brushes.Green },
            { TextColor.Red, Brushes.Red },
            { TextColor.Maroon, Brushes.Maroon },
            { TextColor.Purple, Brushes.Purple },
            { TextColor.Orange, Brushes.Orange },
            { TextColor.Yellow, Brushes.Yellow },
            { TextColor.Lime, Brushes.Lime },
            { TextColor.Teal, Brushes.Teal },
            { TextColor.Aqua, Brushes.Aqua },
            { TextColor.RoyalBlue, Brushes.Blue },
            { TextColor.Fuchsia, Brushes.Fuchsia },
            { TextColor.Gray, Brushes.Gray },
            { TextColor.Silver, Brushes.LightGray },
            { TextColor.Transparent, Brushes.Transparent }
        };

        private static readonly FontWeight BoldFontWeight = FontWeights.ExtraBold;
        private static readonly FontWeight ColorFontWeight = FontWeights.Bold;
        private static readonly TextDecorationCollection UnderlineTextDecorations = TextDecorations.Underline;
        private static readonly string[] LinksPrefixes = { "http://", "https://", "ftp://", "www." };

        private static readonly ISmileyService SmileyService = Locator.Get<ISmileyService>(); // cache it
        private static readonly IResourceService ResourceService = Locator.Get<IResourceService>();
        private static readonly ISettings Settings = Locator.Get<ISettings>(); // that too, both are used for each word
        private static readonly Func<string, bool> ExcludeSmileysFilter = s => SmileyService.HasSmiley( s );

        private const string SpoilerSpanStyleName = "SpoilerSpan";
        private const string MessageStyleSuffix = "MessageStyle";
        private const TextColor ReversedDefaultForeground = TextColor.White;
        private const TextColor ReversedDefaultBackground = TextColor.Black;
        #endregion

        #region Private members
        private static Dictionary<WordProcessorPredicate, WordProcessor> Processors { get; set; }
        #endregion

        static MessageFormatter()
        {
            InitializeProcessors();
        }

        /// <summary>
        /// Returns a formatted version of the specified string.
        /// </summary>
        public static FlowDocument FormatMessage( string message )
        {
            return Format( message, null );
        }

        /// <summary>
        /// Returns a formatted version of the specified IrcMessage.
        /// </summary>
        public static FlowDocument FormatIrcMessage( IrcMessage message )
        {
            return Locator.Get<IUIService>().Execute( () => Format( message.Content, message ) );
        }

        #region Private methods
        /// <summary>
        /// Formats a message, which may be contained in an IRC message.
        /// </summary>
        private static FlowDocument Format( string message, IrcMessage containingMessage = null )
        {
            if ( message.IsEmpty() )
            {
                return null;
            }

            var style = GetStyle( containingMessage );

            var paragraph = new Paragraph();
            foreach ( TextContainer container in MessageParser.Parse( message, ExcludeSmileysFilter ) )
            {
                var span = CreateSpan( container );
                if ( style != null && span.Style == null )
                {
                    span.Style = style;
                }

                // create as few controls as possible
                var builder = new StringBuilder();
                foreach ( var part in container.Parts )
                {
                    if ( part.Kind == TextPartKind.Punctuation || part.Kind == TextPartKind.Space )
                    {
                        builder.Append( part.Content );
                    }
                    else
                    {
                        var processor = GetProcessor( part.Content, containingMessage );

                        if ( processor == null )
                        {
                            builder.Append( part.Content );
                        }
                        else
                        {
                            if ( builder.Length > 0 )
                            {
                                span.Inlines.Add( new Run( builder.ToString() ) );
                                builder.Clear();
                            }

                            var obj = processor( part.Content, containingMessage );
                            AddInline( obj, span.Inlines );
                        }
                    }
                }

                if ( builder.Length > 0 )
                {
                    span.Inlines.Add( new Run( builder.ToString() ) );
                }
                paragraph.Inlines.Add( span );
            }

            return new FlowDocument( paragraph );
        }

        /// <summary>
        /// Gets the style associated with an IRC message
        /// </summary>
        private static Style GetStyle( IrcMessage message )
        {
            if ( message == null )
            {
                return null;
            }
            return Locator.Get<IResourceService>().GetResource<Style>( message.Type + MessageStyleSuffix );
        }

        /// <summary>
        /// Initializes a Span using a TextContainer's properties (not the text)
        /// </summary>
        private static Span CreateSpan( TextContainer container )
        {
            var span = new Span();

            if ( container.IsBold )
            {
                span.FontWeight = BoldFontWeight;
            }

            if ( container.IsUnderlined )
            {
                span.TextDecorations = UnderlineTextDecorations;
            }

            var backgroundColor = TextColor.Default;
            var foregroundColor = TextColor.Default;

            if ( container.ForegroundColor == TextColor.ReversedDefault )
            {
                foregroundColor = ReversedDefaultForeground;
                backgroundColor = container.BackgroundColor == TextColor.ReversedDefault ? ReversedDefaultBackground
                                                                                         : container.BackgroundColor;
            }
            else
            {
                foregroundColor = container.ForegroundColor;
                backgroundColor = container.BackgroundColor;
            }

            // If the background color is not the default, the foreground color cannot be the default, hence the nested conditions
            if ( foregroundColor != TextColor.Default )
            {
                span.Foreground = Colors[foregroundColor];

                // Messages with same background&foreground are spoilers
                if ( foregroundColor == backgroundColor )
                {
                    span.Style = Locator.Get<IResourceService>().GetResource<Style>( SpoilerSpanStyleName );
                }
                else if ( backgroundColor != TextColor.Default )
                {
                    span.Background = Colors[backgroundColor];
                }
            }

            if ( foregroundColor != backgroundColor && !container.IsBold && foregroundColor != TextColor.Default )
            {
                // To avoid illegible text because of weird color choices
                span.FontWeight = ColorFontWeight;
            }

            return span;
        }

        /// <summary>
        ///  Gets a processor if available for the word
        /// </summary>
        private static WordProcessor GetProcessor( string word, IrcMessage containingMessage )
        {
            var wrapper = Processors.Select( p => new { Pair = p } ) // KeyValuePair is a struct, wrap it in a class
                                    .FirstOrDefault( o => o.Pair.Key( word, containingMessage ) );

            return wrapper == null ? null : wrapper.Pair.Value;
        }

        /// <summary>
        ///  Adds an inline to an InlineCollection
        /// </summary>
        private static void AddInline( object obj, InlineCollection collection )
        {
            var inline = obj as Inline;
            if ( inline != null )
            {
                collection.Add( inline );
                return;
            }

            var uiElement = obj as UIElement;
            if ( uiElement != null )
            {
                collection.Add( uiElement );
            }
            else
            {
                collection.Add( obj.ToString() );
            }
        }

        /// <summary>
        /// Initializes the word processors
        /// </summary>
        private static void InitializeProcessors()
        {
            var type = typeof( MessageFormatter.WordProcessors );
            Processors = ReflectionHelper.GetAttributedMethods<WordProcessorAttribute>( type )
                                         .ToDictionary( tup => type.GetMethod( tup.Item1.ConditionName, BindingFlags.Public | BindingFlags.Static )
                                                                   .GetStaticDelegate<WordProcessorPredicate>(),
                                                        tup => tup.Item2.GetStaticDelegate<WordProcessor>() );
        }
        #endregion

        #region Nested attribute
        [AttributeUsage( AttributeTargets.Method )]
        private class WordProcessorAttribute : Attribute
        {
            public string ConditionName { get; private set; }

            public WordProcessorAttribute( string conditionName )
            {
                this.ConditionName = conditionName;
            }
        }
        #endregion
    }
}