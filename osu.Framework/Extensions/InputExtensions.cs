// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK.Input;

namespace osu.Framework.Extensions
{
    public static class InputExtensions
    {
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
