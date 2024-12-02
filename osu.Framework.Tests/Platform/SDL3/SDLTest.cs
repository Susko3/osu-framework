// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Platform.SDL3;
using SDL;
using static SDL.SDL3;

namespace osu.Framework.Tests.Platform.SDL3
{
    [TestFixture]
    [NonParallelizable]
    public abstract class SDLTest
    {
        public virtual SDL_InitFlags InitFlags => SDL_InitFlags.SDL_INIT_VIDEO;

        [SetUp]
        public void SetUp()
        {
            SDL_Init(InitFlags).ThrowIfFailed();
        }

        [TearDown]
        public void TearDown()
        {
            SDL_Quit();
        }

        private const int events_per_peep = 64;
        private readonly SDL_Event[] events = new SDL_Event[events_per_peep];

        /// <summary>
        /// Poll for all pending events.
        /// </summary>
        public void PollEvents(Action<SDL_Event> eventHandler)
        {
            SDL_PumpEvents();

            int eventsRead;

            do
            {
                eventsRead = SDL_PeepEvents(events, SDL_EventAction.SDL_GETEVENT, SDL_EventType.SDL_EVENT_FIRST, SDL_EventType.SDL_EVENT_LAST).ThrowIfFailed();
                for (int i = 0; i < eventsRead; i++)
                    eventHandler(events[i]);
            } while (eventsRead == events_per_peep);
        }
    }
}
