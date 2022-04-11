// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Input.Bindings;
using osu.Framework.Platform;
using osuTK.Input;
using TKVector2 = osuTK.Vector2;
using SNVector2 = System.Numerics.Vector2;
using SDPoint = System.Drawing.Point;
using SDSize = System.Drawing.Size;
using TKWindowState = osuTK.WindowState;

namespace osu.Framework.Extensions
{
    /// <summary>
    /// Temporary extension functions for bridging between osuTK, System.Drawing, and System.Numerics
    /// Can be removed when the SDL2 migration is complete.
    /// </summary>
    public static class BridgingExtensions
    {
        public static TKVector2 ToOsuTK(this SNVector2 vec) =>
            new TKVector2(vec.X, vec.Y);

        public static SNVector2 ToSystemNumerics(this TKVector2 vec) =>
            new SNVector2(vec.X, vec.Y);

        public static SNVector2 ToSystemNumerics(this SDSize size) =>
            new SNVector2(size.Width, size.Height);

        public static SNVector2 ToSystemNumerics(this SDPoint point) =>
            new SNVector2(point.X, point.Y);

        public static SDSize ToSystemDrawingSize(this SNVector2 vec) =>
            new SDSize((int)vec.X, (int)vec.Y);

        public static SDPoint ToSystemDrawingPoint(this SNVector2 vec) =>
            new SDPoint((int)vec.X, (int)vec.Y);

        public static TKWindowState ToOsuTK(this WindowState state)
        {
            switch (state)
            {
                case WindowState.Normal:
                    return TKWindowState.Normal;

                case WindowState.Fullscreen:
                case WindowState.FullscreenBorderless:
                    return TKWindowState.Fullscreen;

                case WindowState.Maximised:
                    return TKWindowState.Maximized;

                case WindowState.Minimised:
                    return TKWindowState.Minimized;
            }

            return TKWindowState.Normal;
        }

        public static WindowState ToFramework(this TKWindowState state)
        {
            switch (state)
            {
                case TKWindowState.Normal:
                    return WindowState.Normal;

                case TKWindowState.Minimized:
                    return WindowState.Minimised;

                case TKWindowState.Maximized:
                    return WindowState.Maximised;

                case TKWindowState.Fullscreen:
                    return WindowState.Fullscreen;
            }

            return WindowState.Normal;
        }

        public static InputKey ToInputKey(this Key key)
        {
            switch (key)
            {
                case Key.LShift: return InputKey.LShift;

                case Key.RShift: return InputKey.RShift;

                case Key.LControl: return InputKey.LControl;

                case Key.RControl: return InputKey.RControl;

                case Key.LAlt: return InputKey.LAlt;

                case Key.RAlt: return InputKey.RAlt;

                case Key.LWin: return InputKey.LSuper;

                case Key.RWin: return InputKey.RSuper;
            }

            // other then the above, keyboard keys are a one-to-one mapping from Key to InputKey.
            return (InputKey)key;
        }
    }
}
