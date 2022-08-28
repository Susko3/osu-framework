// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Text.State.StateChanges;

namespace osu.Framework.Text.State.Actions
{
    public class DeleteByAction : EditorAction
    {
        public readonly int Amount;

        private Selection? removedSelection;

        public DeleteByAction(int amount)
        {
            Amount = amount;
        }

        protected override bool ApplyTo(EditorState state, IEditorStateChangeHandler handler)
        {
            removedSelection = state.DeleteBy(Amount);

            if (removedSelection == null)
                return false;

            handler.HandleTextStateChange(new TextRemovedStateChange(state, this, false, removedSelection.Value.Text, removedSelection.Value.Left));

            return true;
        }

        protected override bool UndoFrom(EditorState state, IEditorStateChangeHandler handler)
        {
            throw new System.NotImplementedException();
        }
    }
}
