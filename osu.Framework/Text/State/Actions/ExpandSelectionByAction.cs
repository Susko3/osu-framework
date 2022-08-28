// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Framework.Text.State.Actions
{
    public class ExpandSelectionByAction : EditorAction
    {
        public readonly int Amount;

        public ExpandSelectionByAction(int amount)
        {
            Amount = amount;
        }

        protected override bool ApplyTo(EditorState state, IEditorStateChangeHandler handler)
        {
            return state.ExpandSelectionBy(Amount);
        }

        protected override bool UndoFrom(EditorState state, IEditorStateChangeHandler handler)
        {
            throw new System.NotImplementedException();
        }
    }
}
