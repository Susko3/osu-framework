// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Logging;
using osuTK;
using SDL;
using static SDL.SDL3;

namespace osu.Framework.Platform.SDL3.Native
{
    public class DisplayManager
    {
        private readonly BindableDictionary<SDL_DisplayID, Display> displays = new BindableDictionary<SDL_DisplayID, Display>();

        public IEnumerable<Display> Displays => displays.Values;

        private readonly Bindable<Display> primaryDisplay = new Bindable<Display>();
        public IBindable<Display> PrimaryDisplay => primaryDisplay;

        public bool TryGet(int index, [NotNullWhen(true)] out Display? display)
        {
            using var ids = SDL_GetDisplays();

            if (ids == null)
                throw new KeyNotFoundException($"Unable to get displays.");

            if (index >= ids.Count)
            {
                display = null;
                return false;
            }

            return TryGet(ids[index], out display);
        }

        public Display GetDisplay(SDL_DisplayID id)
        {
            if (TryGet(id, out var display))
                return display;

            throw new KeyNotFoundException($"Display with id '{id}' not found.");
        }

        public DisplayMode ToDisplayMode(SDL_DisplayMode mode)
        {
            var display = GetDisplay(mode.displayID);
            return mode.ToDisplayMode(display.Index);
        }

        public bool TryGet(SDL_DisplayID id, [NotNullWhen(true)] out Display? display)
        {
            if (displays.TryGetValue(id, out display))
                return true;

            synchronize();
            return displays.TryGetValue(id, out display);
        }

        public bool IsValid(SDL_DisplayID id) => displays.ContainsKey(id);

        private void synchronize()
        {
            using var ids = SDL_GetDisplays();

            if (ids == null)
                throw new SDLException("Unable to get displays.");

            SDL_DisplayID[] arr = new SDL_DisplayID[ids.Count];

            for (int i = 0; i < ids.Count; i++)
                arr[i] = ids[i];

            foreach (var id in displays.Keys.ToArray())
            {
                if (Array.IndexOf(arr, id) == -1)
                    displays.Remove(id);
            }

            foreach (var id in arr)
                addOrUpdateDisplay(id);
        }

        private void addDisplay(SDL_DisplayID id) => displays.Add(id, new Display(id));

        private void addOrUpdateDisplay(SDL_DisplayID id)
        {
            if (displays.TryGetValue(id, out var display))
                display.Update();
            else
                addDisplay(id);
        }

        public DisplayManager()
        {
            primaryDisplay.Value = new Display(SDL_GetPrimaryDisplay().ThrowIfFailed());

            using var ids = SDL_GetDisplays();

            if (ids == null)
                throw new SDLException("Unable to get displays.");

            for (int i = 0; i < ids.Count; i++)
                addDisplay(ids[i]);
        }

        public SDL_DisplayID GetFromIndex(int index)
        {
            using var arr = SDL_GetDisplays();
            SDLException.ThrowIfNull(arr, "Failed to get SDL displays.");
            return arr[index];
        }

        public Display GetFromIndex(DisplayIndex index)
        {
            if (index == DisplayIndex.Primary)
                return PrimaryDisplay.Value;

            using var arr = SDL_GetDisplays();
            SDLException.ThrowIfNull(arr, "Failed to get SDL displays.");

            int i = (int)index;

            if (i >= arr.Count)
                return PrimaryDisplay.Value;

            return new Display(arr[i]);
        }

        public void HandleDisplayEvent(SDL_DisplayEvent evt)
        {
            // static assert
            Debug.Assert(SDL_EventType.SDL_EVENT_DISPLAY_LAST == SDL_EventType.SDL_EVENT_DISPLAY_CONTENT_SCALE_CHANGED);

            switch (evt.type)
            {
                case SDL_EventType.SDL_EVENT_DISPLAY_ADDED:
                    addDisplay(evt.displayID);
                    break;

                case SDL_EventType.SDL_EVENT_DISPLAY_REMOVED:
                    if (!displays.Remove(evt.displayID))
                        Logger.Log($"Unable to remove display id={evt.displayID}, doesn't exist.");

                    break;

                case SDL_EventType.SDL_EVENT_DISPLAY_ORIENTATION:
                case SDL_EventType.SDL_EVENT_DISPLAY_MOVED:
                case SDL_EventType.SDL_EVENT_DISPLAY_DESKTOP_MODE_CHANGED:
                case SDL_EventType.SDL_EVENT_DISPLAY_CURRENT_MODE_CHANGED:
                case SDL_EventType.SDL_EVENT_DISPLAY_CONTENT_SCALE_CHANGED:
                    displays[evt.displayID].HandleDisplayEvent(evt);
                    break;

                default:
                    Logger.Log($"Unknown display event: {evt.type}.");
                    break;
            }
        }
    }
}
