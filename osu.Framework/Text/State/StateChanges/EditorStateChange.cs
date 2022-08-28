// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Text.State.Actions;

namespace osu.Framework.Text.State.StateChanges
{
    public abstract class EditorStateChange
    {
        public readonly EditorState State;
        public readonly EditorAction Action;
        public readonly bool FromUndo;

        protected EditorStateChange(EditorState state, EditorAction action, bool fromUndo)
        {
            State = state;
            Action = action;
            FromUndo = fromUndo;
        }
    }
}
