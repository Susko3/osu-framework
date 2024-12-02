// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Threading;
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
        private int pumpID;
        private int pollID;

        public virtual SDL_InitFlags InitFlags => SDL_InitFlags.SDL_INIT_VIDEO;

        [SetUp]
        public void SetUp()
        {
            pumpID = 0;
            pollID = 0;
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
            foreach (var _ in PollEventsEnumerate(eventHandler))
            {
            }
        }

        protected virtual void PostPoll()
        {
        }

        public IEnumerable<SDL_Event> PollEventsEnumerate(Action<SDL_Event> eventHandler)
        {
            pumpID++;
            SDL_PumpEvents();
            pollID++;

            int eventsRead;

            do
            {
                eventsRead = SDL_PeepEvents(events, SDL_EventAction.SDL_GETEVENT, SDL_EventType.SDL_EVENT_FIRST, SDL_EventType.SDL_EVENT_LAST).ThrowIfFailed();

                for (int i = 0; i < eventsRead; i++)
                {
                    Console.WriteLine($"[{pollID}] Handling {events[i].Type} event.");
                    eventHandler(events[i]);
                    yield return events[i];
                }
            } while (eventsRead == events_per_peep);

            Console.WriteLine();

            PostPoll();

            Thread.Sleep(1);
        }
    }
}
