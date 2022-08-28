// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Text;

namespace osu.Framework.Text.State
{
    public class EditorState
    {
        public readonly StringBuilder Text; // newlines are simply one character
        public readonly ImeState ImeState;
        private int selectionStart;
        private int selectionEnd;

        public int SelectionStart
        {
            get => selectionStart;
            set
            {
                assertWithinText(value);
                selectionStart = value;
            }
        }

        public int SelectionEnd
        {
            get => selectionEnd;
            set
            {
                assertWithinText(value);
                selectionEnd = value;
            }
        }

        // public string? LastCommitText { get; set; }

        public EditorState()
            : this(string.Empty)
        {
        }

        public EditorState(string initialText)
        {
            Text = new StringBuilder(initialText);
            ImeState = new ImeState();
            this.SetCaret(initialText.Length);
        }

        public bool ImeCompositionActive => ImeState.Text.Length > 0;

        public int SelectionLength => Math.Abs(SelectionEnd - SelectionStart);
        public int SelectionLeft => Math.Min(SelectionStart, SelectionEnd);
        public int SelectionRight => Math.Max(SelectionStart, SelectionEnd);

        /// <summary>
        /// Position of the text selection caret. <c>n</c> means before the <c>n</c>-th character.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public int SelectionCaret
        {
            get
            {
                if (SelectionStart != SelectionEnd)
                    throw new InvalidOperationException($"{nameof(SelectionCaret)} is only valid when {nameof(SelectionLength)} is 0.");

                return SelectionStart;
            }
        }

        public string SelectedText => Text.ToString(SelectionLeft, SelectionLength);

        // when ime is active, the state (and undo and redo) are paused, this contains the "composition text", only applied to Text on ime result
        // could be useful to validate
        public string FullText =>
            ImeCompositionActive
                ? new StringBuilder().Append(Text, 0, SelectionLeft).Append(ImeState.Text).Append(Text, SelectionRight, Text.Length - SelectionRight).ToString()
                : Text.ToString();

        /// <summary>
        /// Gets a read-only non-mutable copy of the current selection.
        /// </summary>
        public Selection GetSelection() => new Selection(SelectionStart, SelectionEnd, SelectedText);

        private void assertWithinText(int value)
        {
            Debug.Assert(value >= 0 && value <= Text.Length);
        }

        public override string ToString() =>
            $@"({nameof(Text)}=""{Text}"", "
            + $@"{nameof(SelectionStart)}={SelectionStart}, "
            + $@"{nameof(SelectionEnd)}={SelectionEnd}), "
            + $@"{nameof(ImeState)}={ImeState}, "
            + $@"{nameof(FullText)}=""{FullText}"""
            + @")" //;
            + $"\n{PrintTextBox()}";

        public string PrintTextBox()
        {
            StringBuilder selection = new StringBuilder().Append(' ', Text.Length);

            if (SelectionLength == 0)
            {
                if (SelectionCaret == 0)
                {
                    selection[0] = '<';
                }
                else
                {
                    selection[SelectionCaret - 1] = '/';
                    selection[SelectionCaret] = '\\';
                }
            }
            else
            {
                for (int i = SelectionLeft; i < SelectionRight; i++)
                {
                    selection[i] = '*';
                }
            }

            return $"Text: {Text}\n"
                   + $"Sel:  {selection}";
        }
    }
}
