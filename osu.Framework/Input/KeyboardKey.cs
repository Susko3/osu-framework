// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Extensions;
using osu.Framework.Extensions.TypeExtensions;
using osuTK.Input;

namespace osu.Framework.Input
{
    public readonly struct KeyboardKey : IEquatable<KeyboardKey>
        //, IEquatable<Key>
    {
        public readonly Key Key;
        public readonly char Character;
        private readonly bool isInput;

        /// <summary>
        /// Ctor for native input key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="character"></param>
        public KeyboardKey(Key key, char character)
        {
            Key = key;
            Character = character;
            isInput = true;
        }

        public KeyboardKey(Key key)
        {
            Key = key;
            Character = '\0';
            isInput = false;
        }

        public KeyboardKey(char character)
        {
            Key = Key.LastKey;
            Character = character;
            isInput = false;
        }

        // public KeyboardKey(InputKey inputKey)
        //     : this((Key)inputKey)
        // {
        // }

        /// <summary>
        /// Creates a new <see cref="KeyboardKey"/> from the specified <see cref="Key"/> while filling in a default character combination.
        /// For purposes of input. Compare with <see cref="KeyboardKey(osuTK.Input.Key)"/> which will use a <c>'\0'</c> char.
        /// </summary>
        public static KeyboardKey ForInputFromKey(Key key) => new KeyboardKey(key, key.GetCharacter());

        public override string ToString()
        {
            string charToString(char c)
            {
                switch (c)
                {
                    case '\a': return @"\a";

                    case '\b': return @"\b";

                    case '\f': return @"\f";

                    case '\n': return @"\n";

                    case '\r': return @"\r";

                    case '\t': return @"\t";

                    case '\v': return @"\v";

                    default: return c.ToString();
                }
            }

            string c = Character == '\0' ? null : $", '{charToString(Character)}'";
            return $@"{GetType().ReadableName()}({Key}{c})";
        }

        private bool inputEquals(KeyboardKey other)
        {
            // Debug.Assert(isInput && other.isInput || !isInput && !other.isInput);
            return Key == other.Key && Character == other.Character;
        }

        private bool handleEquals(KeyboardKey other)
        {
            // Debug.Assert(isInput ^ other.isInput);
            // one of the keys is not an input key. a consumer has asked to match "their" implicit KeyboardKey to an input KeyboardKey.
            return Key == other.Key || Character == other.Character;
        }

        private bool twoNonInputEquals(KeyboardKey other)
        {
            return Key == other.Key && Character == other.Character;
        }

        public bool Equals(KeyboardKey other) => isInput == other.isInput ? inputEquals(other) : handleEquals(other);

        public static bool operator ==(KeyboardKey left, KeyboardKey right) => left.Equals(right);
        public static bool operator !=(KeyboardKey left, KeyboardKey right) => !(left == right);

        #region operators with osuTK Key

        // [Obsolete("Use new KeyboardKey(Key)")]
        // public static implicit operator KeyboardKey(Key tkKey) => new KeyboardKey(tkKey);

        // [Obsolete("Use KeyboardKey.Key")]
        public static implicit operator Key(KeyboardKey key) => key.Key;

        // public bool Equals(Key other) => Key == other;
        // public static bool operator ==(KeyboardKey key, Key tkKey) => key.Equals(tkKey);
        // public static bool operator !=(KeyboardKey key, Key tkKey) => !(key == tkKey);

        // public static bool operator ==(Key tkKey, KeyboardKey key) => key == tkKey;
        // public static bool operator !=(Key tkKey, KeyboardKey key) => key != tkKey;

        // public int CompareTo(Key other) => Key.CompareTo(other);

        // public static bool operator <(KeyboardKey key, Key tkKey) => key.CompareTo(tkKey) < 0;
        // public static bool operator >(KeyboardKey key, Key tkKey) => key.CompareTo(tkKey) > 0;
        // public static bool operator <=(KeyboardKey key, Key tkKey) => key.CompareTo(tkKey) <= 0;
        // public static bool operator >=(KeyboardKey key, Key tkKey) => key.CompareTo(tkKey) >= 0;

        #endregion

        public override bool Equals(object obj)
            => obj is KeyboardKey other && Equals(other);

        public override int GetHashCode() => HashCode.Combine((int)Key, Character);
    }
}
