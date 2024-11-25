// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Framework.Platform.SDL3.Native
{
    public enum NativeState
    {
        /// <summary>
        /// The window is restored.
        /// </summary>
        /// <remarks>
        /// A restored window can be fullscreen, borderless, or neither ("normal").
        /// </remarks>
        Restored,
        Minimised,
        Maximised,
    }
}
