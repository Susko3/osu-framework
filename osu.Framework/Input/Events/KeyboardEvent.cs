// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.Input.States;
using osuTK.Input;

namespace osu.Framework.Input.Events
{
    /// <summary>
    /// Events of a keyboard key.
    /// </summary>
    public abstract class KeyboardEvent : UIEvent, IKeyChar<Key>
    {
        public readonly KeyboardKey KeyboardKey;

        public Key Key => KeyboardKey.Key;

        public char Character => KeyboardKey.Character;

        /// <summary>
        /// Whether a specific key is pressed.
        /// </summary>
        public bool IsPressed(Key key) => CurrentState.Keyboard.Keys.IsPressed(key);

        /// <summary>
        /// Whether any key is pressed.
        /// </summary>
        public bool HasAnyKeyPressed => CurrentState.Keyboard.Keys.HasAnyButtonPressed;

        /// <summary>
        /// List of currently pressed keys.
        /// </summary>
        public IEnumerable<Key> PressedKeys => CurrentState.Keyboard.Keys;

        /// <summary>
        /// List of uhh
        /// </summary>
        public IEnumerable<char> PressedCharacters => CurrentState.Keyboard.Characters.Where(pair => IsPressed(pair.Key)).Select(pair => pair.Value);

        protected KeyboardEvent(InputState state, KeyboardKey key)
            : base(state)
        {
            KeyboardKey = key;
        }

        public override string ToString() => $"{GetType().ReadableName()}({Key})";
    }
}
