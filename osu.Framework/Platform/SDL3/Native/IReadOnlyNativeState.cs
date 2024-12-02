// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Drawing;

namespace osu.Framework.Platform.SDL3.Native
{
    public interface IReadOnlyNativeState
    {
        #region From SDL_keyboard.h

        TextInputParams? TextInputParams { get; }

        #endregion

        #region From SDL_mouse.h

        bool RelativeMouseMode { get; }

        #endregion

        #region From SDL_video.h functions

        /// <summary>
        /// Current display mode of the <see cref="Display"/>.
        /// </summary>
        DisplayMode CurrentDisplayMode { get; }

        Display Display { get; }

        float PixelDensity { get; }

        float DisplayScale { get; }

        DisplayMode? FullscreenMode { get; }

        // missing SDL_GetWindowICCProfile

        // missing SDL_GetWindowPixelFormat

        // missing SDL_GetWindowID

        // missing SDL_GetWindowParent

        // missing SDL_GetWindowProperties

        // SDL_GetWindowFlags is handled in region: "From SDL_WindowFlags"

        string Title { get; }

        Point Position { get; }

        Size Size { get; }

        Rectangle SafeArea { get; }

        AspectRatio AspectRatio { get; }

        // MarginPadding BordersSize { get; }

        Size SizeInPixels { get; }

        Size MinimumSize { get; }

        Size MaximumSize { get; }

        bool Bordered { get; }

        bool Resizable { get; }

        bool AlwaysOnTop { get; }

        bool Visible { get; }

        NativeState WindowState { get; }

        bool Fullscreen { get; }

        // missing SDL_WindowGet*Surface

        Rectangle? MouseRect { get; }

        float Opacity { get; }

        bool Focusable { get; }

        #endregion

        #region From SDL_WindowFlags

        bool Occluded { get; }

        bool InputFocus { get; }

        bool MouseFocus { get; }

        #endregion
    }
}
