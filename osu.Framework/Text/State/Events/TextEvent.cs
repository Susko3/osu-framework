// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Framework.Text.State.Events
{
    public abstract class TextEvent
    {
        public readonly EditorState CurrentState;

        public readonly bool FromUndo;

        protected TextEvent(EditorState currentState, bool fromUndo)
        {
            CurrentState = currentState;
            FromUndo = fromUndo;
        }
    }
}
