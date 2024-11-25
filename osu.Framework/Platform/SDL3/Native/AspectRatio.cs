// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Framework.Platform.SDL3.Native
{
    public readonly record struct AspectRatio(float? Min, float? Max)
    {
        private const float no_limit = 0.0f;

        internal static AspectRatio FromSDL(float min, float max)
        {
            return new AspectRatio(min == no_limit ? null : min, max == no_limit ? null : max);
        }

        internal float SDLMin => Min ?? no_limit;
        internal float SDLMax => Max ?? no_limit;
    }
}
