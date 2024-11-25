// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace osu.Framework.Platform.SDL3.Native
{
    public record struct SDLIcon(Image<Rgba32> Normal, Image<Rgba32>[] HighDpi)
    {
        public static SDLIcon Consumed = new SDLIcon();
    }
}
