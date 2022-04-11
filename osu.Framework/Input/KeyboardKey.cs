// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Extensions;
using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.Input.Bindings;
using osuTK.Input;

namespace osu.Framework.Input
{
    public readonly struct KeyboardKey : IEquatable<KeyboardKey>, IEquatable<Key>
    {
        public readonly Key Key;
        public readonly char Character;

        public KeyboardKey(Key key, char character)
        {
            Key = key;
            Character = character;
        }

        public KeyboardKey(Key key)
            : this(key, key.GetCharacter())
        {
        }

        public KeyboardKey(InputKey inputKey)
            : this((Key)inputKey)
        {
        }

        // public KeyboardKey(char character)
        //     : this(Key.LastKey, character) // TODO use Key.Any
        // {
        // }
        //

        // /// <summary>
        // /// Converts an <see cref="osuTK"/> <see cref="Key"/> to <see cref="KeyboardKey"/>.
        // /// </summary>
        // /// <param name="tkKey"></param>
        // /// <returns></returns>
        // public static implicit operator KeyboardKey(Key tkKey) => new KeyboardKey(tkKey);

        // public static implicit operator KeyboardKey(char character) => new KeyboardKey(character);
        //
        // public static implicit operator Key(KeyboardKey keyboardKey) => keyboardKey.Key;

        // public static implicit operator KeyboardKey(InputKey inputKey) => new KeyboardKey((Key)inputKey);
        public override string ToString()
        {
            string charToString(char c)
            {
                switch (c)
                {
                    case '\a':
                        return @"\a";

                    case '\b':
                        return @"\b";

                    case '\f':
                        return @"\f";

                    case '\n':
                        return @"\n";

                    case '\r':
                        return @"\r";

                    case '\t':
                        return @"\t";

                    case '\v':
                        return @"\v";

                    default:
                        return c.ToString();
                }
            }

            string c = Character == '\0' ? null : $", '{charToString(Character)}'";
            return $@"{GetType().ReadableName()}({Key}{c})";
        }

        /// <summary>
        /// Indicates whether the <see cref="Key"/> of this touch is equal to <see cref="Key"/> of the other touch.
        /// </summary>
        /// <param name="other">The other touch.</param>
        public bool Equals(KeyboardKey other) => Key == other.Key && Character == other.Character;

        public bool Equals(Key other) => Key == other;

        public static bool operator ==(KeyboardKey left, KeyboardKey right) => left.Equals(right);
        public static bool operator !=(KeyboardKey left, KeyboardKey right) => !(left == right);
        public static bool operator ==(KeyboardKey left, Key right) => left.Equals(right);
        public static bool operator !=(KeyboardKey left, Key right) => !(left == right);

        // public static bool operator ==(Key left, KeyboardKey right) => left.Equals(right);
        // public static bool operator !=(Key left, KeyboardKey right) => !(left == right);

        public override bool Equals(object obj)
            => obj is KeyboardKey other && Equals(other)
               || obj is Key key && Equals(key);

        public override int GetHashCode() => Key.GetHashCode();
    }
}
