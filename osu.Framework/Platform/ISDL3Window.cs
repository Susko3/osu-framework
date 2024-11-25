// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using SDL;

namespace osu.Framework.Platform
{
    internal interface ISDL3Window : ISDLWindow
    {
        unsafe SDL_Window* SDLWindowHandle { get; }

        IntPtr WindowHandle { get; }

        IntPtr DisplayHandle { get; }

        IntPtr SurfaceHandle { get; }

        bool IsWayland { get; }
    }
}
