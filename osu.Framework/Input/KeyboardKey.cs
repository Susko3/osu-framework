// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Extensions;
using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.Graphics;
using osu.Framework.Input.Handlers;
using osu.Framework.Input.Handlers.Keyboard;
using osu.Framework.Testing.Input;
using osuTK.Input;

namespace osu.Framework.Input
{
    /// <summary>
    /// This struct encompasses two usages:
    /// <list type="table">
    ///     <item>Translating native keyboard input into <see cref="KeyboardKey"/>s (eg. <see cref="KeyboardHandler"/>) and propagating that trough the input hierarchy.</item>
    ///     <item>Drawables checking if said keys match some desired action in <see cref="Drawable.OnKeyDown"/>.</item>
    /// </list>
    /// </summary>
    public readonly struct KeyboardKey : IEquatable<KeyboardKey>
    {
        /// <summary>
        /// The key that was pressed (roughly equivalent to a scancode).
        /// Independent of the system keyboard layout.
        /// </summary>
        /// <remarks>
        /// Should be matched against when the location of a key on the keyboard is more important than the character printed on it.
        /// </remarks>
        public readonly Key Key;

        /// <summary>
        /// The character that this key would generate if entered (roughly equivalent to the keycode - the character printed on the key).
        /// Dependant on the system keyboard layout.
        /// </summary>
        /// <remarks>
        /// Should be matched against for common platform actions (eg. copy, paste) and actions that match mnemonically to the character (eg. 'o' for "open file").
        /// Generally, only alphanumeric characters [a-z, 0-9] are safe to match against. Other characters are likely to be absent from international keyboard layouts,
        /// or appear in a shifted / altgr state (something not currently provided by <see cref="KeyboardKey"/>)
        /// </remarks>
        public readonly char Character;

        /// <summary>
        /// Whether this key is the result of input -- whether from an <see cref="InputHandler"/> or synthesized from a <see cref="ManualInputManager"/>.
        /// As opposed to a key that way made in a consumer/drawable context and is used to match to a <see cref="fromInputHandler"/> key.
        /// </summary>
        private readonly bool fromInputHandler;

        public KeyboardKey(Key key, char character)
        {
            Key = key;
            Character = character;
            fromInputHandler = true;
        }

        /// <summary>
        /// Construct a <see cref="KeyboardKey"/> for use in comparison with another <see cref="KeyboardKey"/>.
        /// </summary>
        /// <param name="key"></param>
        public KeyboardKey(Key key)
        {
            Key = key;
            Character = '\0';
            fromInputHandler = false;
        }

        /// <summary>
        /// Construct a <see cref="KeyboardKey"/> for use in comparison with another <see cref="KeyboardKey"/>.
        /// </summary>
        /// <param name="character"></param>
        public KeyboardKey(char character)
        {
            Key = Key.LastKey;
            Character = character;
            fromInputHandler = false;
        }

        /// <summary>
        /// Creates a new <see cref="KeyboardKey"/> from the specified <see cref="Key"/> while filling in a default character combination.
        /// For purposes of input from a <see cref="KeyboardHandler"/>, <see cref="ManualInputManager"/> or similar.
        /// </summary>
        /// <remarks>
        /// Compare with <see cref="KeyboardKey(osuTK.Input.Key)"/> which will not set <see cref="fromInputHandler"/> and doesn't have a <see cref="Character"/>.
        /// </remarks>
        public static KeyboardKey ForInputFromKey(Key key) => new KeyboardKey(key, key.GetCharacter());

        public override string ToString()
        {
            string c = Character == '\0' ? null : $", {Character.StringRepresentation()}";
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

        public bool Equals(KeyboardKey other) => fromInputHandler == other.fromInputHandler ? inputEquals(other) : handleEquals(other);

        public static bool operator ==(KeyboardKey left, KeyboardKey right) => left.Equals(right);
        public static bool operator !=(KeyboardKey left, KeyboardKey right) => !(left == right);

        // [Obsolete("Use new KeyboardKey(Key)")]
        public static implicit operator KeyboardKey(Key tkKey) => new KeyboardKey(tkKey);

        // [Obsolete("Use KeyboardKey.Key")]
        // public static implicit operator Key(KeyboardKey key) => key.Key;

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

        public override bool Equals(object obj)
            => obj is KeyboardKey other && Equals(other);

        public override int GetHashCode() => HashCode.Combine((int)Key, Character);
    }
}
