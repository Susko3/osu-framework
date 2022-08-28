// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Text.State.Actions;

namespace osu.Framework.Text.State.StateChanges
{
    public class SelectionChangedStateChange : EditorStateChange
    {
        public readonly Selection OldSelection;
        public readonly Selection NewSelection;

        public SelectionChangedStateChange(EditorState state, EditorAction action, bool fromUndo, Selection oldSelection, Selection newSelection)
            : base(state, action, fromUndo)
        {
            if (oldSelection.Equals(newSelection))
                throw new ArgumentException($"{nameof(oldSelection)} and {nameof(newSelection)} should not be equal.");

            OldSelection = oldSelection;
            NewSelection = newSelection;
        }
    }
}
