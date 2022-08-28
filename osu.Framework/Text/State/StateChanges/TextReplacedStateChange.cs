// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Text.State.Actions;

namespace osu.Framework.Text.State.StateChanges
{
    public class TextReplacedStateChange : EditorStateChange
    {
        public readonly string OldText;
        public readonly string NewText;

        public TextReplacedStateChange(EditorState state, EditorAction action, bool fromUndo, string oldText, string newText)
            : base(state, action, fromUndo)
        {
            if (string.IsNullOrEmpty(oldText))
                throw new ArgumentException("Removed text should not be empty.", nameof(oldText));

            if (string.IsNullOrEmpty(newText))
                throw new ArgumentException("Added text should not be empty.", nameof(newText));

            OldText = oldText;
            NewText = newText;
        }
    }
}
