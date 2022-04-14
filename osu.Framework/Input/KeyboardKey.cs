// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System;
using osu.Framework.Extensions;
using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.Graphics;
using osu.Framework.Input.Handlers.Keyboard;
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
    public readonly struct KeyboardKey : IKeyChar<Key>, IEquatable<KeyboardKey>
    {

        public Key Key { get; }

        public char Character { get; }

        public KeyboardKey(Key key, char character)
        {
            Key = key;
            Character = character;
        }

        /// <summary>
        /// Creates a new <see cref="KeyboardKey"/> from the specified <see cref="Key"/> while filling in the default character (if available).
        /// </summary>
        public static KeyboardKey From(Key key) => new KeyboardKey(key, key.GetCharacter());

        public override string ToString()
        {
            string? c = Character == '\0' ? null : $", {Character.StringRepresentation()}";
            return $@"{GetType().ReadableName()}({Key}{c})";
        }

        public bool Equals(KeyboardKey other) => Key == other.Key && Character == other.Character;

        public static bool operator ==(KeyboardKey left, KeyboardKey right) => left.Equals(right);
        public static bool operator !=(KeyboardKey left, KeyboardKey right) => !(left == right);

        public override bool Equals(object? obj) => obj is KeyboardKey other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Key, Character);
    }

    /// <summary>
    /// Ensures consistency between different ways of storing keyboard keys.
    /// </summary>
    public interface IKeyChar<TKey>
        where TKey : Enum
    {
        /// <summary>
        /// The key that was pressed (roughly equivalent to a scancode).
        /// Independent of the system keyboard layout.
        /// </summary>
        /// <remarks>
        /// Should be matched against when the location of a key on the keyboard is more important than the character printed on it.
        /// Also see <see cref="Character"/>.
        /// </remarks>
        TKey Key { get; }

        /// <summary>
        /// The character that this key would generate if entered (roughly equivalent to the keycode - the character printed on the key).
        /// Dependant on the system keyboard layout.
        /// </summary>
        /// <remarks>
        /// Should be matched against for common platform actions (eg. copy, paste) and actions that match mnemonically to the character (eg. 'o' for "open file").
        /// Generally, only alphanumeric characters [a-z, 0-9] are safe to match against. Other characters are likely to be absent from international keyboard layouts,
        /// or appear in a shifted / alt gr state (something not currently provided by <see cref="IKeyChar"/>).
        /// Also see <see cref="Key"/>.
        /// </remarks>
        char Character { get; }
    }
}
