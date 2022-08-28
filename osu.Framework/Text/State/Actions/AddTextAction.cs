// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using osu.Framework.Text.State.StateChanges;

namespace osu.Framework.Text.State.Actions
{
    public class AddTextAction : EditorAction
    {
        public readonly string Added;

        private Selection? oldSelection;

        public AddTextAction(string added)
        {
            Added = added;
        }

        protected override bool ApplyTo(EditorState state, IEditorStateChangeHandler handler)
        {
            oldSelection = state.InsertAtSelectionCaret(Added);

            int textStart = state.SelectionCaret - Added.Length;

            if (oldSelection.HasValue)
                handler.HandleTextStateChange(new TextReplacedStateChange(state, this, false, oldSelection.Value.Text, Added));
            else
                handler.HandleTextStateChange(new TextAddedStateChange(state, this, false, Added, textStart));

            return true;
        }

        protected override bool UndoFrom(EditorState state, IEditorStateChangeHandler handler)
        {
            int length = Added.Length;
            int start = state.SelectionCaret - length;

            Debug.Assert(state.Text.ToString(start, length) == Added);

            state.Remove(start, length);

            if (oldSelection.HasValue)
            {
                state.RollbackSelection(oldSelection.Value);
                handler.HandleTextStateChange(new TextReplacedStateChange(state, this, true, Added, oldSelection.Value.Text));
            }
            else
            {
                handler.HandleTextStateChange(new TextRemovedStateChange(state, this, true, Added, start));
            }

            return true;
        }
    }
}
