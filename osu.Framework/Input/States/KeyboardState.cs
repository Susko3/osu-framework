// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK.Input;

namespace osu.Framework.Input.States
{
    public class KeyboardState
    {
        public readonly ButtonStates<KeyboardKey> Keys = new ButtonStates<KeyboardKey>();

        /// <summary>
        /// Whether left or right control key is pressed.
        /// </summary>
        public bool ControlPressed => Keys.IsPressed(new KeyboardKey(Key.LControl)) || Keys.IsPressed(new KeyboardKey(Key.RControl));

        /// <summary>
        /// Whether left or right alt key is pressed.
        /// </summary>
        public bool AltPressed => Keys.IsPressed(new KeyboardKey(Key.LAlt)) || Keys.IsPressed(new KeyboardKey(Key.RAlt));

        /// <summary>
        /// Whether left or right shift key is pressed.
        /// </summary>
        public bool ShiftPressed => Keys.IsPressed(new KeyboardKey(Key.LShift)) || Keys.IsPressed(new KeyboardKey(Key.RShift));

        /// <summary>
        /// Whether left or right super key (Win key on Windows, or Command key on Mac) is pressed.
        /// </summary>
        public bool SuperPressed => Keys.IsPressed(new KeyboardKey(Key.LWin)) || Keys.IsPressed(new KeyboardKey(Key.RWin));
    }
}
