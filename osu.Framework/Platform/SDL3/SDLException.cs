// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics.CodeAnalysis;

namespace osu.Framework.Platform.SDL3
{
    public class SDLException(string? message, string? sdlError) : Exception($"{message}\nSDL error: {sdlError}")
    {
        public SDLException(string? message)
            : this(message, FrameworkEnvironment.UseSDL3 ? SDL.SDL3.SDL_GetError() : global::SDL2.SDL.SDL_GetError())
        {
        }

        public static unsafe void ThrowIfNull(void* pointer, string message = "Pointer is null.")
        {
            if (pointer == null)
                throw new SDLException(message);
        }

        public static void ThrowIfNull<T>([NotNull] T? obj, string message = "Object is null.")
        {
            if (obj == null)
                throw new SDLException(message);
        }
    }
}
