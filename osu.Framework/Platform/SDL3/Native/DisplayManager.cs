// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Drawing;
using JetBrains.Annotations;
using osuTK;
using SDL;
using static SDL.SDL3;

namespace osu.Framework.Platform.SDL3.Native
{
    public class DisplayManager
    {
        [MustDisposeResource]
        private SDLArray<SDL_DisplayID> getSDLDisplays()
        {
            var arr = SDL_GetDisplays();
            SDLException.ThrowIfNull(arr, "Failed to get SDL displays.");
            return arr;
        }

        public Display PrimaryDisplay => createDisplay(SDL_GetPrimaryDisplay().ThrowIfFailed());

        public Display GetDisplay(SDL_DisplayID id) => createDisplay(id);

        public int GetDisplayIndex(SDL_DisplayID id)
        {
            using var arr = getSDLDisplays();

            for (int i = 0; i < arr.Count; i++)
            {
                if (arr[i] == id)
                    return i;
            }

            throw new ArgumentException("Invalid display ID.", nameof(id));
        }

        public DisplayMode ToDisplayMode(SDL_DisplayMode mode, int? displayIndex = null) => new DisplayMode(
            SDL_GetPixelFormatName(mode.format).ThrowIfFailed(),
            new Size(mode.w, mode.h),
            32,
            mode.refresh_rate,
            displayIndex ?? GetDisplayIndex(mode.displayID));

        public SDL_DisplayID GetFromIndex(int index)
        {
            using var arr = getSDLDisplays();
            return arr[index];
        }

        public Display GetFromIndexForgiving(DisplayIndex index)
        {
            if (index == DisplayIndex.Primary)
                return PrimaryDisplay;

            int i = (int)index;
            ArgumentOutOfRangeException.ThrowIfNegative(i, nameof(index));

            using var arr = getSDLDisplays();

            if (i >= arr.Count)
                return PrimaryDisplay;

            return createDisplay(arr[i], i);
        }

        private Display createDisplay(SDL_DisplayID id) => createDisplay(id, GetDisplayIndex(id));

        private Display createDisplay(SDL_DisplayID id, int index)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index);

            SDL_Rect bounds;

            unsafe { SDL_GetDisplayBounds(id, &bounds).ThrowIfFailed(); }

            using var arr = SDL_GetFullscreenDisplayModes(id);
            SDLException.ThrowIfNull(arr, "Failed to get SDL display modes.");

            DisplayMode[] modes = new DisplayMode[arr.Count];

            for (int i = 0; i < arr.Count; i++)
                modes[i] = ToDisplayMode(arr[i], index);

            return new Display(index, SDL_GetDisplayName(id).ThrowIfFailed(), bounds.ToRectangle(), modes);
        }

        public void HandleDisplayEvent(SDL_DisplayEvent evt)
        {
            // static assert
            Debug.Assert(SDL_EventType.SDL_EVENT_DISPLAY_LAST == SDL_EventType.SDL_EVENT_DISPLAY_CONTENT_SCALE_CHANGED);
        }
    }
}
