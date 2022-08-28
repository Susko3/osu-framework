// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Extensions.TypeExtensions;

namespace osu.Framework.Text.State.Actions
{
    public abstract class EditorAction
    {
        public bool Applied { get; private set; }

        /// <summary>
        /// <c>true</c> if <see cref="Undo"/> has been called once.
        /// </summary>
        public bool Undone { get; private set; }

        public bool Apply(EditorState state, IEditorStateChangeHandler handler)
        {
            if (Applied)
                throw new InvalidOperationException($"Cannot {nameof(Apply)} a {GetType().ReadableName()} more than once.");

            Applied = true;

            return ApplyTo(state, handler);
        }

        protected abstract bool ApplyTo(EditorState state, IEditorStateChangeHandler handler);

        public bool Undo(EditorState state, IEditorStateChangeHandler handler)
        {
            if (!Applied)
                throw new InvalidOperationException($"Cannot {nameof(Undo)} a {GetType().ReadableName()} if it wasn't applied.");

            if (Undone)
                throw new InvalidOperationException($"Cannot {nameof(Undo)} a {GetType().ReadableName()} more than once.");

            Undone = true;

            return UndoFrom(state, handler);
        }

        protected abstract bool UndoFrom(EditorState state, IEditorStateChangeHandler handler);
    }
}
