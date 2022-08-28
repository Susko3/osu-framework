// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Framework.Text.State.Actions
{
    public class SelectAllAction : EditorAction
    {
        private Selection? oldSelection;

        protected override bool ApplyTo(EditorState state, IEditorStateChangeHandler handler)
        {
            oldSelection = state.GetSelection();

            return state.SelectAll();
        }

        protected override bool UndoFrom(EditorState state, IEditorStateChangeHandler handler)
        {
            if (oldSelection.HasValue)
                state.SetSelectionStartEnd(oldSelection.Value);

            return true;
        }
    }
}
