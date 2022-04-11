// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Input.Bindings;
using osuTK.Input;

namespace osu.Framework.Extensions
{
    public static class InputExtensions
    {
        private static readonly EqualityComparer<InputKey> comparer = new InputKeyComparer();

        public static bool ContainsKey(this IEnumerable<InputKey> source, InputKey value) => source.Contains(value, comparer);

        public static InputKey Encode(this InputKey key, char c)
        {
            // use the upper 16 bits to encode the char

            // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
            key |= (InputKey)((ulong)c << 48);
            return key;
        }

        public static void Decode(this InputKey key, out InputKey outKey, out char c)
        {
            const ulong char_mask = 0xFFFF000000000000;
            c = (char)(((ulong)key & char_mask) >> 48);

            // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
            outKey = key & (InputKey)(~char_mask);
        }

        /// <summary>
        /// Gets a default character assiciated with this <see cref="Key"/>.
        /// </summary>
        /// <remarks>
        /// This corresponds to which character this key would produce on a en-us keyboard layout.
        /// </remarks>
        /// <param name="key"></param>
        /// <returns></returns>
        public static char GetCharacter(this Key key)
        {
            static bool inBetween(Key key, Key first, Key last, char firstCharacter, out char result)
            {
                if (key >= first && key <= last)
                {
                    result = (char)(key - first + firstCharacter);
                    return true;
                }

                result = '\0';
                return false;
            }

            if (inBetween(key, Key.Keypad0, Key.Keypad9, '0', out char result))
                return result;

            if (inBetween(key, Key.A, Key.Z, 'a', out result))
                return result;

            if (inBetween(key, Key.Number0, Key.Number9, '0', out result))
                return result;

            switch (key)
            {
                default:
                    return '\0';

                case Key.Enter:
                case Key.KeypadEnter:
                    return '\n';

                case Key.Space:
                    return ' ';

                case Key.Tab:
                    return '\t';

                case Key.Slash:
                case Key.KeypadDivide:
                    return '/';

                case Key.KeypadMultiply:
                    return '*';

                case Key.Minus:
                case Key.KeypadMinus:
                    return '-';

                case Key.Plus:
                case Key.KeypadPlus:
                    return '+';

                case Key.Period:
                case Key.KeypadPeriod:
                    return '.';

                case Key.Tilde:
                    return '~';

                case Key.BracketLeft:
                    return '[';

                case Key.BracketRight:
                    return ']';

                case Key.Semicolon:
                    return ';';

                case Key.Quote:
                    return '\'';

                case Key.Comma:
                    return ',';

                case Key.BackSlash:
                case Key.NonUSBackSlash:
                    return '\\';
            }
        }
    }
}
