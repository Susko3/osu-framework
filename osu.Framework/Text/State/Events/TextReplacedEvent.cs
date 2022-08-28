// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Framework.Text.State.Events
{
    public class TextReplacedEvent : TextEvent
    {
        public readonly string Added;

        public TextReplacedEvent(EditorState currentState, bool fromUndo, string added)
            : base(currentState, fromUndo)
        {
            Added = added;
        }
    }
}
