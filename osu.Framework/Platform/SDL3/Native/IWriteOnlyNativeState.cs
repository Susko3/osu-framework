// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Drawing;

namespace osu.Framework.Platform.SDL3.Native
{
    public interface IWriteOnlyNativeState
    {
        #region From SDL_keyboard.h

        void SetTextInputParams(TextInputParams? value);

        #endregion

        #region From SDL_mouse.h

        void SetRelativeMouseMode(bool value);

        #endregion

        #region From SDL_video.h functions

        void SetFullscreenMode(DisplayMode? value);

        void SetTitle(string value);

        void SetPosition(Point value);

        void SetSize(Size value);

        void SetAspectRatio(AspectRatio value);

        void SetMinimumSize(Size value);

        void SetMaximumSize(Size value);

        void SetBordered(bool value);

        void SetResizable(bool value);

        void SetAlwaysOnTop(bool value);

        void SetVisible(bool value);

        // missing SDL_RaiseWindow

        // for:
        // SDL_MaximizeWindow
        // SDL_MinimizeWindow
        // SDL_RestoreWindow
        void SetWindowState(NativeState value);

        void SetFullscreen(bool value);

        // missing SDL_SyncWindow

        // missing SDL_WindowSet*Surface

        void SetMouseRect(Rectangle? value);

        void SetOpacity(float value);

        void SetFocusable(bool value);

        // missing SDL_ShowWindowSystemMenu

        // missing SDL_SetWindowHitTest

        // missing SDL_SetWindowShape

        // missing SDL_FlashWindow

        // missing SDL_DestroyWindow

        #endregion

        void SetFrom(IReadOnlyNativeState o)
        {
            // TODO: change order to make more sense
            SetTextInputParams(o.TextInputParams);
            SetRelativeMouseMode(o.RelativeMouseMode);
            SetTextInputParams(o.TextInputParams);
            SetRelativeMouseMode(o.RelativeMouseMode);
            SetFullscreenMode(o.FullscreenMode);
            SetTitle(o.Title);
            SetPosition(o.Position);
            SetSize(o.Size);
            SetAspectRatio(o.AspectRatio);
            SetMinimumSize(o.MinimumSize);
            SetMaximumSize(o.MaximumSize);
            SetBordered(o.Bordered);
            SetResizable(o.Resizable);
            SetAlwaysOnTop(o.AlwaysOnTop);
            SetVisible(o.Visible);
            SetWindowState(o.WindowState);
            SetFullscreen(o.Fullscreen);
            SetMouseRect(o.MouseRect);
            SetOpacity(o.Opacity);
            SetFocusable(o.Focusable);
        }
    }
}
