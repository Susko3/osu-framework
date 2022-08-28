// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Text.State.Actions;

namespace osu.Framework.Text.State.StateChanges
{
    public class TextRemovedStateChange : EditorStateChange
    {
        public readonly string Text;
        public readonly int StartIndex;

        public int Count => Text.Length;

        public TextRemovedStateChange(EditorState state, EditorAction action, bool fromUndo, string text, int startIndex)
            : base(state, action, fromUndo)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentException("Removed text should not be empty.", nameof(text));

            Text = text;
            StartIndex = startIndex;
        }
    }
}
