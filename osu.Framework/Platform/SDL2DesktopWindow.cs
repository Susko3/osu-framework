// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using SDL;
using static SDL.SDL3;

namespace osu.Framework.Platform
{
    internal class SDL2DesktopWindow : SDL2Window
    {
        public SDL2DesktopWindow(GraphicsSurfaceType surfaceType)
            : base(surfaceType)
        {
        }

        protected override unsafe void UpdateWindowStateAndSize(WindowState state, Display display, DisplayMode displayMode)
        {
            // this reset is required even on changing from one fullscreen resolution to another.
            // if it is not included, the GL context will not get the correct size.
            // this is mentioned by multiple sources as an SDL issue, which seems to resolve by similar means (see https://discourse.libsdl.org/t/sdl-setwindowsize-does-not-work-in-fullscreen/20711/4).
            SDL_SetWindowBordered(SDLWindowHandle, SDL_bool.SDL_TRUE);
            SDL_SetWindowFullscreen(SDLWindowHandle, SDL_bool.SDL_FALSE);
            SDL_RestoreWindow(SDLWindowHandle);

            base.UpdateWindowStateAndSize(state, display, displayMode);
        }
    }
}
