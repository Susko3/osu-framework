// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Input.States;

namespace osu.Framework.Input.StateChanges
{
    public class KeyboardKeyInput : ButtonInput<KeyboardKey>
    {
        public KeyboardKeyInput(IEnumerable<ButtonInputEntry<KeyboardKey>> entries)
            : base(entries)
        {
        }

        public KeyboardKeyInput(KeyboardKey button, bool isPressed)
            : base(button, isPressed)
        {
        }

        public KeyboardKeyInput(ButtonStates<KeyboardKey> current, ButtonStates<KeyboardKey> previous)
            : base(current, previous)
        {
        }

        protected override ButtonStates<KeyboardKey> GetButtonStates(InputState state) => state.Keyboard.Keys;
    }
}
