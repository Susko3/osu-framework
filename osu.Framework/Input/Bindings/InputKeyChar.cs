// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Framework.Input.Bindings
{
    public readonly struct InputKeyChar
    {
        public readonly InputKey Key;
        public readonly char Character;

        internal InputKeyChar(InputKey key, char character)
        {
            Key = key;
            Character = character;
        }

        public InputKeyChar(InputKey key)
            : this(key, '\0')
        {
        }

        public InputKeyChar(char character)
            : this(InputKey.Any, character)
        {
        }

        public static implicit operator InputKeyChar(InputKey key) => new InputKeyChar(key);
    }
}
