// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;

namespace osu.Framework.Text.State
{
    public static class EditorStateExtensions
    {
        #region StringBuilder

        public static void Append(this EditorState state, string text)
        {
            throw new NotImplementedException();
        }

        public static void Clear(this EditorState state)
        {
            state.Text.Clear();
            state.SetCaret(0);
        }

        /// <summary>
        /// Inserts the text at the specified position, expanding or moving the the selection.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="index"></param>
        /// <param name="text"></param>
        public static void Insert(this EditorState state, int index, string text)
        {
            state.Text.Insert(index, text);

            if (state.SelectionStart == state.SelectionEnd)
            {
            }
            else if (state.SelectionStart < state.SelectionEnd)
            {
            }
            else if (state.SelectionStart > state.SelectionEnd)
            {
            }

            throw new NotImplementedException();
        }

        public static void Remove(this EditorState state, int startIndex, int length)
        {
            state.Text.Remove(startIndex, length);
            state.SetCaret(startIndex);

            // TODO: set caret correctly.

            // throw new NotImplementedException();
        }

        #endregion

        public static void SetCaret(this EditorState state, int position)
        {
            state.SelectionStart = position;
            state.SelectionEnd = position;
        }

        public static Selection? RemoveSelection(this EditorState state)
        {
            if (state.SelectionLength == 0)
                return null;

            var selection = state.GetSelection();
            state.Text.Remove(state.SelectionLeft, state.SelectionLength);
            state.SetCaret(state.SelectionLeft);
            return selection;
        }

        /// <summary>
        /// Removes any selected text and inserts new text at the new caret position.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static Selection? InsertAtSelectionCaret(this EditorState state, string text)
        {
            var oldSelection = state.RemoveSelection();

            state.Text.Insert(state.SelectionCaret, text);
            state.SetCaret(state.SelectionCaret + text.Length);

            return oldSelection;
        }

        /// <summary>
        /// Rollback a selection after it was removed with <see cref="RemoveSelection"/>.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="selection"></param>
        public static void RollbackSelection(this EditorState state, Selection selection)
        {
            Debug.Assert(state.SelectionCaret == selection.Left);

            state.InsertAtSelectionCaret(selection.Text);
            state.SelectionStart = selection.Start;
            state.SelectionEnd = selection.End;
        }

        public static bool SetSelectionStartEnd(this EditorState state, Selection selection) => state.SetSelectionStartEnd(selection.Start, selection.End);

        public static bool SetSelectionStartEnd(this EditorState state, int start, int end)
        {
            if (state.SelectionStart == start && state.SelectionEnd == end)
                return false;

            state.SelectionStart = start;
            state.SelectionEnd = end;

            return true;
        }

        /// <summary>
        /// Move the current cursor by the signed <paramref name="amount"/>.
        /// </summary>
        /// <returns>Whether the selection changed.</returns>
        public static bool MoveCursorBy(this EditorState state, int amount)
        {
            state.SelectionStart = state.SelectionEnd;
            return state.moveSelection(amount, false);
        }

        /// <summary>
        /// Expand the current selection by the signed <paramref name="amount"/>.
        /// </summary>
        /// <returns>Whether the selection changed.</returns>
        public static bool ExpandSelectionBy(this EditorState state, int amount)
        {
            return state.moveSelection(amount, true);
        }

        /// <returns>Whether the selection changed.</returns>
        private static bool moveSelection(this EditorState state, int offset, bool expand)
        {
            int oldStart = state.SelectionStart;
            int oldEnd = state.SelectionEnd;

            if (expand)
                state.SelectionEnd = state.clamp(state.SelectionEnd + offset);
            else
            {
                if (state.SelectionLength > 0 && Math.Abs(offset) <= 1)
                {
                    // we don't want to move the location when "removing" an existing selection, just set the new location.
                    if (offset > 0)
                        state.SelectionEnd = state.SelectionStart = state.SelectionRight;
                    else
                        state.SelectionEnd = state.SelectionStart = state.SelectionLeft;
                }
                else
                    state.SelectionEnd = state.SelectionStart = state.clamp((offset > 0 ? state.SelectionRight : state.SelectionLeft) + offset);
            }

            return oldStart != state.SelectionStart || oldEnd != state.SelectionEnd;
        }

        /// <summary>
        /// If there is a selection, delete the selected text.
        /// Otherwise, delete characters from the cursor position by the signed <paramref name="amount"/>.
        /// A negative amount represents a backward deletion, and a positive amount represents a forward deletion.
        /// </summary>
        public static Selection? DeleteBy(this EditorState state, int amount)
        {
            if (state.SelectionLength == 0)
                state.SelectionEnd = state.clamp(state.SelectionStart + amount);

            return state.RemoveSelection();
        }

        public static bool SelectAll(this EditorState state)
        {
            int oldStart = state.SelectionStart;
            int oldEnd = state.SelectionEnd;

            state.SelectionStart = 0;
            state.SelectionEnd = state.Text.Length;

            return oldStart != state.SelectionStart || oldEnd != state.SelectionEnd;
        }

        /// <summary>
        /// Clamps the provided selection to the state's text.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="selection"></param>
        /// <returns></returns>
        private static int clamp(this EditorState state, int selection) => Math.Clamp(selection, 0, state.Text.Length);
    }
}
