// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Drawing;
using osu.Framework.Extensions.EnumExtensions;
using osu.Framework.Graphics;
using SDL;
using static SDL.SDL3;

namespace osu.Framework.Platform.SDL3.Native
{
    public sealed unsafe class NativeSDLState(SDL_Window* window, DisplayManager displayManager, NativeStateStorage backingStore) : IReadOnlyNativeState, IWriteOnlyNativeState
    {
        public void UpdateFrom(IReadOnlyNativeState state)
        {
            ((IWriteOnlyNativeState)this).SetFrom(state);
        }

        public TextInputParams? TextInputParams
        {
            get
            {
                if (!SDL_TextInputActive(window))
                    return null;

                SDL_Rect rect;
                int cursor;
                SDL_GetTextInputArea(window, &rect, &cursor).ThrowIfFailed();

                var current = backingStore.TextInputParams.Value;

                if (current == null)
                    throw new InvalidOperationException("Invalid state! Expecting valid text input parameters.");

                return new TextInputParams(current.Type, current.Capitalization, current.Autocorrect, current.Multiline, rect.ToRectangle(), cursor);
            }
        }

        public bool RelativeMouseMode => SDL_GetWindowRelativeMouseMode(window);

        public DisplayMode CurrentDisplayMode
        {
            get
            {
                var display = SDL_GetDisplayForWindow(window).ThrowIfFailed();
                var mode = SDL_GetCurrentDisplayMode(display);
                SDLException.ThrowIfNull(mode);
                return displayManager.ToDisplayMode(*mode);
            }
        }

        public Display Display => displayManager.GetDisplay(SDL_GetDisplayForWindow(window).ThrowIfFailed());

        public float PixelDensity => SDL_GetWindowPixelDensity(window).ThrowIfFailed();

        public float DisplayScale => SDL_GetWindowDisplayScale(window).ThrowIfFailed();

        public DisplayMode? FullscreenMode
        {
            get
            {
                var mode = SDL_GetWindowFullscreenMode(window);

                if (mode == null)
                    return null;

                return displayManager.ToDisplayMode(*mode);
            }
        }

        public string Title => SDL_GetWindowTitle(window) ?? string.Empty;

        public Point Position
        {
            get
            {
                int x, y;
                SDL_GetWindowPosition(window, &x, &y).ThrowIfFailed();
                return new Point(x, y);
            }
        }

        public Size Size
        {
            get
            {
                int w, h;
                SDL_GetWindowSize(window, &w, &h).ThrowIfFailed();
                return new Size(w, h);
            }
        }

        public Rectangle SafeArea
        {
            get
            {
                SDL_Rect rect;
                SDL_GetWindowSafeArea(window, &rect).ThrowIfFailed();
                return rect.ToRectangle();
            }
        }

        public AspectRatio AspectRatio
        {
            get
            {
                float min, max;
                SDL_GetWindowAspectRatio(window, &min, &max).ThrowIfFailed();
                return AspectRatio.FromSDL(min, max);
            }
        }

        public MarginPadding BordersSize
        {
            get
            {
                int top, left, bottom, right;
                SDL_GetWindowBordersSize(window, &top, &left, &bottom, &right).ThrowIfFailed();
                return new MarginPadding
                {
                    Top = top,
                    Left = left,
                    Bottom = bottom,
                    Right = right
                };
            }
        }

        public Size SizeInPixels
        {
            get
            {
                int w, h;
                SDL_GetWindowSizeInPixels(window, &w, &h).ThrowIfFailed();
                return new Size(w, h);
            }
        }

        public Size MinimumSize
        {
            get
            {
                int w, h;
                SDL_GetWindowMinimumSize(window, &w, &h).ThrowIfFailed();
                return new Size(w, h);
            }
        }

        public Size MaximumSize
        {
            get
            {
                int w, h;
                SDL_GetWindowMaximumSize(window, &w, &h).ThrowIfFailed();
                return new Size(w, h);
            }
        }

        private SDL_WindowFlags flags => SDL_GetWindowFlags(window);

        public bool Bordered => !flags.HasFlagFast(SDL_WindowFlags.SDL_WINDOW_BORDERLESS);
        public bool Resizable => flags.HasFlagFast(SDL_WindowFlags.SDL_WINDOW_RESIZABLE);
        public bool AlwaysOnTop => flags.HasFlagFast(SDL_WindowFlags.SDL_WINDOW_ALWAYS_ON_TOP);
        public bool Visible => !flags.HasFlagFast(SDL_WindowFlags.SDL_WINDOW_HIDDEN);

        public NativeState WindowState
        {
            get
            {
                var f = flags;

                if (f.HasFlagFast(SDL_WindowFlags.SDL_WINDOW_MINIMIZED))
                {
                    Debug.Assert(!f.HasFlagFast(SDL_WindowFlags.SDL_WINDOW_MAXIMIZED));
                    return NativeState.Minimised;
                }

                if (f.HasFlagFast(SDL_WindowFlags.SDL_WINDOW_MAXIMIZED))
                    return NativeState.Maximised;

                return NativeState.Restored;
            }
        }

        public bool Fullscreen => flags.HasFlagFast(SDL_WindowFlags.SDL_WINDOW_FULLSCREEN);

        public bool KeyboardGrab => SDL_GetWindowKeyboardGrab(window);

        public bool MouseGrab => SDL_GetWindowMouseGrab(window);

        public Rectangle? MouseRect
        {
            get
            {
                var rect = SDL_GetWindowMouseRect(window);

                if (rect == null)
                    return null;

                return rect->ToRectangle();
            }
        }

        public float Opacity => SDL_GetWindowOpacity(window);

        public bool Focusable => !flags.HasFlagFast(SDL_WindowFlags.SDL_WINDOW_NOT_FOCUSABLE);
        public bool Occluded => flags.HasFlagFast(SDL_WindowFlags.SDL_WINDOW_OCCLUDED);
        public bool InputFocus => flags.HasFlagFast(SDL_WindowFlags.SDL_WINDOW_INPUT_FOCUS);
        public bool MouseFocus => flags.HasFlagFast(SDL_WindowFlags.SDL_WINDOW_MOUSE_FOCUS);

        public void SetTextInputParams(TextInputParams? value)
        {
            if (value is not TextInputParams textInputParams)
            {
                SDL_StopTextInput(window).ThrowIfFailed();
            }
            else
            {
                SDL_Rect rect = textInputParams.Area.ToSDLRect();
                SDL_SetTextInputArea(window, &rect, textInputParams.CursorOffset).ThrowIfFailed();

                var props = SDL_CreateProperties().ThrowIfFailed();

                try
                {
                    textInputParams.FillProps(props);
                    SDL_StartTextInputWithProperties(window, props).ThrowIfFailed();
                }
                finally
                {
                    SDL_DestroyProperties(props);
                }
            }

            backingStore.TextInputParams.Value = value;
            backingStore.TextInputParams.Value = TextInputParams;
        }

        public void SetRelativeMouseMode(bool value)
        {
            SDL_SetWindowRelativeMouseMode(window, value).ThrowIfFailed();
            backingStore.RelativeMouseMode.Value = RelativeMouseMode;
        }

        public void SetFullscreenMode(DisplayMode? value)
        {
            if (value is not DisplayMode mode)
            {
                SDL_SetWindowFullscreenMode(window, null).ThrowIfFailed();
            }
            else
            {
                SDL_DisplayMode sdlMode;
                SDL_GetClosestFullscreenDisplayMode(displayManager.GetFromIndex(mode.DisplayIndex), mode.Size.Width, mode.Size.Height, mode.RefreshRate, true, &sdlMode).ThrowIfFailed();
                SDL_SetWindowFullscreenMode(window, &sdlMode).ThrowIfFailed();
            }
        }

        public void SetTitle(string value)
        {
            SDL_SetWindowTitle(window, value).ThrowIfFailed();
            backingStore.Title.Value = Title;
        }

        public void SetPosition(Point value)
        {
            SDL_SetWindowPosition(window, value.X, value.Y).ThrowIfFailed();
        }

        public void SetSize(Size value)
        {
            SDL_SetWindowSize(window, value.Width, value.Height).ThrowIfFailed();
        }

        public void SetAspectRatio(AspectRatio value)
        {
            SDL_SetWindowAspectRatio(window, value.SDLMin, value.SDLMax).ThrowIfFailed();
            backingStore.AspectRatio.Value = AspectRatio;
        }

        public void SetMinimumSize(Size value)
        {
            SDL_SetWindowMinimumSize(window, value.Width, value.Height).ThrowIfFailed();
            backingStore.MinimumSize.Value = MinimumSize;
        }

        public void SetMaximumSize(Size value)
        {
            SDL_SetWindowMaximumSize(window, value.Width, value.Height).ThrowIfFailed();
            backingStore.MaximumSize.Value = MaximumSize;
        }

        public void SetBordered(bool value)
        {
            SDL_SetWindowBordered(window, value).ThrowIfFailed();
        }

        public void SetResizable(bool value)
        {
            SDL_SetWindowResizable(window, value).ThrowIfFailed();
        }

        public void SetAlwaysOnTop(bool value)
        {
            SDL_SetWindowAlwaysOnTop(window, value).ThrowIfFailed();
        }

        public void SetVisible(bool value)
        {
            if (value)
            {
                SDL_ShowWindow(window).ThrowIfFailed();
            }
            else
            {
                SDL_HideWindow(window).ThrowIfFailed();
            }
        }

        public void SetWindowState(NativeState value)
        {
            switch (value)
            {
                case NativeState.Restored:
                    SDL_RestoreWindow(window).ThrowIfFailed();

                    // SDL bug: restoring a minimised that was previously maximised window will make it maximised, and not restored/normal.
                    // Restore it again to bring the window to a restored/normal state.
                    if (flags.HasFlagFast(SDL_WindowFlags.SDL_WINDOW_MAXIMIZED))
                        SDL_RestoreWindow(window).ThrowIfFailed();

                    break;

                case NativeState.Minimised:
                    SDL_MinimizeWindow(window).ThrowIfFailed();
                    break;

                case NativeState.Maximised:
                    SDL_MaximizeWindow(window).ThrowIfFailed();
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }

        public void SetFullscreen(bool value)
        {
            SDL_SetWindowFullscreen(window, value).ThrowIfFailed();
        }

        public void SetMouseGrab(bool value)
        {
            SDL_SetWindowMouseGrab(window, value).ThrowIfFailed();
        }

        public void SetKeyboardGrab(bool value)
        {
            SDL_SetWindowKeyboardGrab(window, value).ThrowIfFailed();
        }

        public void SetMouseRect(Rectangle? value)
        {
            if (value == null)
            {
                SDL_SetWindowMouseRect(window, null).ThrowIfFailed();
            }
            else
            {
                SDL_Rect rect = value.Value.ToSDLRect();
                SDL_SetWindowMouseRect(window, &rect).ThrowIfFailed();
            }
        }

        public void SetOpacity(float value)
        {
            SDL_SetWindowOpacity(window, value).ThrowIfFailed();
        }

        public void SetFocusable(bool value)
        {
            SDL_SetWindowFocusable(window, value).ThrowIfFailed();
        }
    }
}
