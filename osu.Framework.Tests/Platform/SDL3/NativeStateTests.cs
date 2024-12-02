// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Platform.SDL3;
using osu.Framework.Platform.SDL3.Native;

namespace osu.Framework.Tests.Platform.SDL3
{
    [TestFixture]
    public class NativeStateTests : SDLTest
    {
        [Test]
        public void TestEquality()
        {
            var window = new BaseSDL3Window(new NativeStateStorage
            {
                Visible = { Value = false },
            });

            window.Create(0);
            window.PrepareForRun();

            PollEvents(window.HandleEvent);

            var nativeState = window.UnsafeGetNativeSDLState();
            Assert.That(nativeState, Is.Not.Null);
            AssertEqual(window.UnsafeGetStateStorage(), nativeState!);
        }

        public static void AssertEqual(IReadOnlyNativeState actual, IReadOnlyNativeState expected)
        {
            Assert.Multiple(() =>
            {
                foreach (var prop in typeof(IReadOnlyNativeState).GetProperties())
                {
                    Assert.That(actual, Has.Property(prop.Name).EqualTo(prop.GetValue(expected)));
                }
            });
        }
    }
}
