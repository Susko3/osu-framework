// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Extensions.EnumExtensions;
using osu.Framework.Input.Bindings;
using osu.Framework.Platform.SDL2;
using osuTK.Input;
using SDL2;

namespace osu.Framework.Tests.Input
{
    [TestFixture]
    public class KeyboardKeysInvariantTest
    {
        private static IEnumerable<Key> getAllKeys() => EnumExtensions.GetValuesInOrder<Key>();

        private static IEnumerable<InputKey> getAllKeyboardInputKeys() =>
            EnumExtensions.GetValuesInOrder<InputKey>().Where(key => key < InputKey.LastKey || key >= InputKey.Mute && key <= InputKey.RSuper);

        private static IEnumerable<SDL.SDL_Scancode> getAllScancodes() => EnumExtensions.GetValuesInOrder<SDL.SDL_Scancode>();

        [TestCaseSource(nameof(getAllKeys))]
        public void TestKeyInvariant(Key key)
        {
            if (key == Key.LastKey)
                Assert.Ignore("LastKey is not a valid key.");

            if (key >= Key.F25 && key <= Key.F35)
                Assert.Ignore("SDL scancodes only go up to F24.");

            var inputKey = KeyCombination.FromKey(key);
            var scancode = inputKey.ToScancode();
            Assert.That(scancode.ToKey(true), Is.EqualTo(key));
        }

        [TestCaseSource(nameof(getAllKeyboardInputKeys))]
        public void TestInputKeyInvariant(InputKey inputKey)
        {
            switch (inputKey)
            {
                case InputKey.Alt:
                case InputKey.Control:
                case InputKey.Shift:
                case InputKey.Super:
                    Assert.Ignore("Left-right agnostic modifier keys don't map to scancodes.");
                    break;
            }

            if (inputKey >= InputKey.F25 && inputKey <= InputKey.F35)
                Assert.Ignore("SDL scancodes only go up to F24.");

            var scancode = inputKey.ToScancode();
            var key = scancode.ToKey(true);
            Assert.That(KeyCombination.FromKey(key), Is.EqualTo(inputKey));
        }

        [TestCaseSource(nameof(getAllScancodes))]
        public void TestScancodeInvariant(SDL.SDL_Scancode scancode)
        {
            var key = scancode.ToKey(true);

            if (key == Key.Unknown)
                Assert.Ignore($"Unknown scancode: {scancode}");

            var expectedScancode = normalizeScancode(scancode);

            var inputKey = KeyCombination.FromKey(key);
            Assert.That(inputKey.ToScancode(), Is.EqualTo(expectedScancode));
        }

        /// <summary>
        /// For scancodes that map to the same key, this returns the "default" scancode for that key (the one returned by <see cref="SDL2Extensions.ToScancode"/>).
        /// </summary>
        private SDL.SDL_Scancode normalizeScancode(SDL.SDL_Scancode scancode)
        {
            switch (scancode)
            {
                default:
                    return scancode;

                case SDL.SDL_Scancode.SDL_SCANCODE_DECIMALSEPARATOR:
                    return SDL.SDL_Scancode.SDL_SCANCODE_KP_PERIOD;

                case SDL.SDL_Scancode.SDL_SCANCODE_CLEAR:
                    return SDL.SDL_Scancode.SDL_SCANCODE_KP_CLEAR;

                case SDL.SDL_Scancode.SDL_SCANCODE_KP_A:
                    return SDL.SDL_Scancode.SDL_SCANCODE_A;

                case SDL.SDL_Scancode.SDL_SCANCODE_KP_B:
                    return SDL.SDL_Scancode.SDL_SCANCODE_B;

                case SDL.SDL_Scancode.SDL_SCANCODE_KP_C:
                    return SDL.SDL_Scancode.SDL_SCANCODE_C;

                case SDL.SDL_Scancode.SDL_SCANCODE_KP_D:
                    return SDL.SDL_Scancode.SDL_SCANCODE_D;

                case SDL.SDL_Scancode.SDL_SCANCODE_KP_E:
                    return SDL.SDL_Scancode.SDL_SCANCODE_E;

                case SDL.SDL_Scancode.SDL_SCANCODE_KP_F:
                    return SDL.SDL_Scancode.SDL_SCANCODE_F;

                case SDL.SDL_Scancode.SDL_SCANCODE_KP_BACKSPACE:
                    return SDL.SDL_Scancode.SDL_SCANCODE_BACKSPACE;

                case SDL.SDL_Scancode.SDL_SCANCODE_KP_COMMA:
                    return SDL.SDL_Scancode.SDL_SCANCODE_COMMA;

                case SDL.SDL_Scancode.SDL_SCANCODE_KP_SPACE:
                    return SDL.SDL_Scancode.SDL_SCANCODE_SPACE;

                case SDL.SDL_Scancode.SDL_SCANCODE_KP_TAB:
                    return SDL.SDL_Scancode.SDL_SCANCODE_TAB;

                case SDL.SDL_Scancode.SDL_SCANCODE_MUTE:
                    return SDL.SDL_Scancode.SDL_SCANCODE_AUDIOMUTE;

                case SDL.SDL_Scancode.SDL_SCANCODE_STOP:
                    return SDL.SDL_Scancode.SDL_SCANCODE_AUDIOSTOP;
            }
        }
    }
}
