// Copyright (C) 2011-2012, Solal Pirelli
// This code is licensed under a modified MIT License (see Properties\Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

// This is not pretty, but it works.

namespace CommonStuff
{
    public sealed class AutoComplete : DependencyObject
    {
        #region Constants
        private const char WordSeparator = ' ';
        #endregion

        #region MatchesSource Attached Property
        public static IEnumerable<string> GetMatchesSource( DependencyObject obj )
        {
            return (IEnumerable<string>) obj.GetValue( MatchesSourceProperty );
        }

        public static void SetMatchesSource( DependencyObject obj, IEnumerable<string> value )
        {
            obj.SetValue( MatchesSourceProperty, value );
        }

        public static readonly DependencyProperty MatchesSourceProperty =
            DependencyProperty.RegisterAttached( "MatchesSource", typeof( IEnumerable<string> ), typeof( AutoComplete ), new UIPropertyMetadata( MatchesSourceChanged ) );

        private static void MatchesSourceChanged( DependencyObject obj, DependencyPropertyChangedEventArgs args )
        {
            var textBox = obj as TextBox;
            if ( textBox == null )
            {
                return;
            }

            if ( args.OldValue == null )
            {
                textBox.AcceptsTab = true;

                textBox.PreviewKeyDown += TextBox_PreviewKeyDown;
                textBox.PreviewMouseLeftButtonDown += TextBox_PreviewMouseLeftButtonDown;
            }
        }
        #endregion

        #region (private) HasTextChanged Attached Property
        private static bool GetHasTextChanged( DependencyObject obj )
        {
            return (bool) obj.GetValue( HasTextChangedProperty );
        }

        private static void SetHasTextChanged( DependencyObject obj, bool value )
        {
            obj.SetValue( HasTextChangedProperty, value );
        }

        private static readonly DependencyProperty HasTextChangedProperty =
            DependencyProperty.RegisterAttached( "HasTextChanged", typeof( bool ), typeof( AutoComplete ), new UIPropertyMetadata( true ) );
        #endregion

        #region (private) LastWordPart Attached Property
        private static string GetLastWordPart( DependencyObject obj )
        {
            return (string) obj.GetValue( LastWordPartProperty );
        }

        private static void SetLastWordPart( DependencyObject obj, string value )
        {
            obj.SetValue( LastWordPartProperty, value );
        }

        private static readonly DependencyProperty LastWordPartProperty =
            DependencyProperty.RegisterAttached( "LastWordPart", typeof( string ), typeof( AutoComplete ), new UIPropertyMetadata( null ) );
        #endregion

        #region (private) MatchesIndex Attached Property
        private static int GetMatchesIndex( DependencyObject obj )
        {
            return (int) obj.GetValue( MatchesIndexProperty );
        }

        private static void SetMatchesIndex( DependencyObject obj, int value )
        {
            obj.SetValue( MatchesIndexProperty, value );
        }

        private static readonly DependencyProperty MatchesIndexProperty =
            DependencyProperty.RegisterAttached( "MatchesIndex", typeof( int ), typeof( AutoComplete ), new UIPropertyMetadata( 0 ) );
        #endregion

        private static void TextBox_PreviewKeyDown( object sender, KeyEventArgs e )
        {
            var textBox = (TextBox) sender;
                
            if ( e.Key == Key.Tab && textBox.Text.HasText() )
            {
                int caretIndex = textBox.CaretIndex;

                // The word the caret was after, before hitting Tab for the first time
                string prefix;

                if ( GetHasTextChanged( textBox ) )
                {
                    string wordsBeforeCaret = textBox.Text.Substring( 0, caretIndex );
                    prefix = wordsBeforeCaret.Substring( wordsBeforeCaret.LastIndexOf( ' ' ) + 1 );
                    SetLastWordPart( textBox, prefix );
                }
                else
                {
                    prefix = GetLastWordPart( textBox );
                }

                if ( prefix.IsEmpty() )
                {
                    return; // everything matches a space, that's not what we want
                }

                // The index of the next match in the matches collection
                int matchIndex = GetMatchesIndex( textBox );

                int nextCaretPosition = caretIndex;

                var matches = from s in GetMatchesSource( textBox ) where s.BeginsWith( prefix ) orderby s select s;

                if ( matches.Any() )
                {
                    if ( e.KeyboardDevice.Modifiers.HasFlag( ModifierKeys.Shift ) )
                    {
                        // Shift + Tab => go back
                        matchIndex = matchIndex <= 0 ? matches.Count() - 1 : matchIndex - 1;
                    }
                    else
                    {
                        // Tab => Go forward
                        matchIndex = matchIndex >= matches.Count() - 1 ? 0 : matchIndex + 1;
                    }

                    SetMatchesIndex( textBox, matchIndex );

                    string match = matches.ElementAt( matchIndex );
                    string[] textParts = textBox.Text.Split( WordSeparator );
                    int totalLength = 0;
                    int partIndex = textParts.TakeWhile( s => ( totalLength += s.Length + 1 ) < caretIndex ).Count();
                    // get the "old" match length to compare it to the new one, to place the caret
                    int oldMatchLength = textParts[partIndex].Length;
                    textParts[partIndex] = match;
                    textBox.Text = string.Join( WordSeparator.ToString(), textParts );

                    nextCaretPosition = caretIndex + ( match.Length - oldMatchLength );

                    // Add a space in case it's the first element.
                    if ( textBox.Text.TrimStart() == match )
                    {
                        textBox.Text += WordSeparator;
                        nextCaretPosition++;
                    }
                }

                // Set the caret position, after the last char
                textBox.CaretIndex = nextCaretPosition;
                SetHasTextChanged( textBox, false );

                e.Handled = true; // do not show tab char
            }
            else if ( KeyHelper.IsModifierKey( e.Key ) == false ) // Don't reset for modifier keys
            {
                AutoComplete.SetHasTextChanged( textBox, true );
                AutoComplete.SetMatchesIndex( textBox, -1 );
            }
        }

        // if the caret moves, reset LastWordPart etc
        private static void TextBox_PreviewMouseLeftButtonDown( object sender, MouseEventArgs e )
        {
            var textBox = (TextBox) sender;
            AutoComplete.SetHasTextChanged( textBox, true );
            AutoComplete.SetMatchesIndex( textBox, -1 );
        }
    }
}