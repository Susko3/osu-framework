// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Platform.SDL3;
using osu.Framework.Platform.SDL3.Native;
using SDL;

namespace osu.Framework.Tests.Platform.SDL3
{
    public abstract class SDLWindowTest : SDLTest
    {
        protected BaseSDL3Window Window { get; private set; } = null!;

        [SetUp]
        public void SetUpWindow()
        {
            Window = new BaseSDL3Window(new NativeStateStorage());
        }

        protected IWriteOnlyNativeState InitialState => (NativeStateStorage)Window.UnsafeGetWriteableState();

        protected IReadOnlyNativeState State => Window.UnsafeGetStateStorage();

        protected IWriteOnlyNativeState NewState => (NativeSDLState)Window.UnsafeGetWriteableState();

        protected override void PostPoll()
        {
            var nativeSDLState = Window.UnsafeGetNativeSDLState();
            Assume.That(nativeSDLState, Is.Not.Null);
            NativeStateTests.AssertEqual(State, nativeSDLState!);
        }

        public void PollEventsExpecting(SDL_EventType type)
        {
            Assert.That(PollEventsEnumerate(Window.HandleEvent).ToArray(), Has.Some.Property(nameof(SDL_Event.Type)).EqualTo(type));
        }
    }
}
