// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Framework.Text.State.Actions
{
    public class SetSelectionAction : EditorAction
    {
        private Selection oldSelection;

        public readonly int Start;
        public readonly int End;

        public SetSelectionAction(int start, int end)
        {
            Start = start;
            End = end;
        }

        protected override bool ApplyTo(EditorState state, IEditorStateChangeHandler handler)
        {
            oldSelection = state.GetSelection();
            bool ret = state.SetSelectionStartEnd(Start, End);

            if (ret)
            {
                // TOOD: selection change
            }

            return ret;
        }

        protected override bool UndoFrom(EditorState state, IEditorStateChangeHandler handler)
        {
            bool ret = state.SetSelectionStartEnd(oldSelection);

            return ret;
        }
    }
}
