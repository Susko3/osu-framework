// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osuTK.Input;

namespace osu.Framework.Input.States
{
    public class KeyboardState
    {
        public readonly ButtonStates<KeyboardKey> Keys = new ButtonStates<KeyboardKey>();

        public bool IsPressed(Key key) => Keys.Any(keyboardKey => keyboardKey.Key == key);

        public bool IsPressed(char character) => Keys.Any(keyboardKey => keyboardKey.Character == character);

        /// <summary>
        /// Whether left or right control key is pressed.
        /// </summary>
        public bool ControlPressed => IsPressed(Key.LControl) || IsPressed(Key.RControl);

        /// <summary>
        /// Whether left or right alt key is pressed.
        /// </summary>
        public bool AltPressed => IsPressed(Key.LAlt) || IsPressed(Key.RAlt);

        /// <summary>
        /// Whether left or right shift key is pressed.
        /// </summary>
        public bool ShiftPressed => IsPressed(Key.LShift) || IsPressed(Key.RShift);

        /// <summary>
        /// Whether left or right super key (Win key on Windows, or Command key on Mac) is pressed.
        /// </summary>
        public bool SuperPressed => IsPressed(Key.LWin) || IsPressed(Key.RWin);
    }
}
