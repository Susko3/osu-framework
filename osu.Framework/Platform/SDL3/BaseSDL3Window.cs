// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Drawing;
using osu.Framework.Platform.SDL3.Native;
using SDL;
using static SDL.SDL3;

namespace osu.Framework.Platform.SDL3
{
    public class BaseSDL3Window : IDisposable
    {
        protected readonly DisplayManager DisplayManager = new DisplayManager();

        private NativeSDLState? nativeSDLState;

        /// <summary>
        /// Stores the state provided by <see cref="nativeSDLState"/> and <see cref="HandleWindowEvent"/>.
        /// </summary>
        private readonly NativeStateStorage nativeStateStorage;

        public NativeStateStorage UnsafeGetStateStorage() => nativeStateStorage;

        public NativeSDLState? UnsafeGetNativeSDLState() => nativeSDLState;

        public IWriteOnlyNativeState UnsafeGetWriteableState() => nativeSDLState != null ? nativeSDLState : nativeStateStorage;

        public unsafe SDL_Window* SDLWindowHandle { get; private set; }
        public SDL_WindowID WindowID { get; private set; }

        public BaseSDL3Window(NativeStateStorage initialState)
        {
            nativeStateStorage = initialState;
        }

        public unsafe void Create(SDL_WindowFlags flags)
        {
            Debug.Assert(SDLWindowHandle == null);

            if (SDL_WasInit(SDL_InitFlags.SDL_INIT_VIDEO) == 0)
                throw new InvalidOperationException("SDL video subsystem not initialized.");

            var props = SDL_CreateProperties().ThrowIfFailed();

            try
            {
                SDL_SetBooleanProperty(props, SDL_PROP_WINDOW_CREATE_ALWAYS_ON_TOP_BOOLEAN, nativeStateStorage.AlwaysOnTop.Value).ThrowIfFailed();
                SDL_SetBooleanProperty(props, SDL_PROP_WINDOW_CREATE_BORDERLESS_BOOLEAN, !nativeStateStorage.Bordered.Value).ThrowIfFailed();
                SDL_SetBooleanProperty(props, SDL_PROP_WINDOW_CREATE_FOCUSABLE_BOOLEAN, nativeStateStorage.Focusable.Value).ThrowIfFailed();
                SDL_SetNumberProperty(props, SDL_PROP_WINDOW_CREATE_FLAGS_NUMBER, (int)flags).ThrowIfFailed();
                SDL_SetBooleanProperty(props, SDL_PROP_WINDOW_CREATE_FULLSCREEN_BOOLEAN, nativeStateStorage.Fullscreen.Value).ThrowIfFailed();
                SDL_SetNumberProperty(props, SDL_PROP_WINDOW_CREATE_HEIGHT_NUMBER, nativeStateStorage.Size.Value.Height).ThrowIfFailed();
                SDL_SetBooleanProperty(props, SDL_PROP_WINDOW_CREATE_HIDDEN_BOOLEAN, !nativeStateStorage.Visible.Value).ThrowIfFailed();
                SDL_SetBooleanProperty(props, SDL_PROP_WINDOW_CREATE_HIGH_PIXEL_DENSITY_BOOLEAN, true).ThrowIfFailed();
                SDL_SetBooleanProperty(props, SDL_PROP_WINDOW_CREATE_MAXIMIZED_BOOLEAN, nativeStateStorage.WindowState.Value == NativeState.Maximised).ThrowIfFailed();
                SDL_SetBooleanProperty(props, SDL_PROP_WINDOW_CREATE_MINIMIZED_BOOLEAN, nativeStateStorage.WindowState.Value == NativeState.Minimised).ThrowIfFailed();
                SDL_SetBooleanProperty(props, SDL_PROP_WINDOW_CREATE_RESIZABLE_BOOLEAN, nativeStateStorage.Resizable.Value).ThrowIfFailed();
                SDL_SetStringProperty(props, SDL_PROP_WINDOW_CREATE_TITLE_STRING, nativeStateStorage.Title.Value).ThrowIfFailed();
                SDL_SetNumberProperty(props, SDL_PROP_WINDOW_CREATE_WIDTH_NUMBER, nativeStateStorage.Size.Value.Width).ThrowIfFailed();
                SDL_SetNumberProperty(props, SDL_PROP_WINDOW_CREATE_X_NUMBER, nativeStateStorage.Position.Value.X).ThrowIfFailed();
                SDL_SetNumberProperty(props, SDL_PROP_WINDOW_CREATE_Y_NUMBER, nativeStateStorage.Position.Value.Y).ThrowIfFailed();

                SDLWindowHandle = SDL_CreateWindowWithProperties(props);
                SDLException.ThrowIfNull(SDLWindowHandle, "Failed to create window.");

                WindowID = SDL_GetWindowID(SDLWindowHandle);
            }
            finally
            {
                SDL_DestroyProperties(props);
            }

            Debug.Assert(nativeSDLState == null);
            nativeSDLState = new NativeSDLState(SDLWindowHandle, DisplayManager, nativeStateStorage);
            nativeSDLState.UpdateFrom(nativeStateStorage);
        }

        public void PrepareForRun()
        {
            Debug.Assert(nativeSDLState != null);
            nativeStateStorage.UpdateFrom(nativeSDLState);
        }

        public unsafe void Destroy()
        {
            if (SDLWindowHandle != null)
            {
                SDL_DestroyWindow(SDLWindowHandle);
                SDLWindowHandle = null;
            }
        }

        public unsafe void HandleEvent(SDL_Event evt)
        {
            if (evt.Type >= SDL_EventType.SDL_EVENT_WINDOW_FIRST && evt.Type <= SDL_EventType.SDL_EVENT_WINDOW_LAST && evt.window.windowID == WindowID)
            {
                HandleWindowEvent(evt.window);
            }
            else if (evt.Type >= SDL_EventType.SDL_EVENT_DISPLAY_FIRST && evt.Type <= SDL_EventType.SDL_EVENT_DISPLAY_LAST)
            {
                HandleDisplayEvent(evt.display);
            }
        }

        public void HandleWindowEvent(SDL_WindowEvent evt)
        {
            Debug.Assert(nativeSDLState != null);
            Debug.Assert(evt.windowID == WindowID);

            switch (evt.type)
            {
                case SDL_EventType.SDL_EVENT_WINDOW_SHOWN:
                    nativeStateStorage.Visible.Value = true;
                    break;

                case SDL_EventType.SDL_EVENT_WINDOW_HIDDEN:
                    nativeStateStorage.Visible.Value = false;
                    nativeStateStorage.MouseFocus.Value = nativeSDLState.MouseFocus;
                    nativeStateStorage.InputFocus.Value = nativeSDLState.InputFocus;
                    break;

                case SDL_EventType.SDL_EVENT_WINDOW_EXPOSED:
                    nativeStateStorage.Occluded.Value = false;
                    break;

                case SDL_EventType.SDL_EVENT_WINDOW_MOVED:
                    nativeStateStorage.Position.Value = new Point(evt.data1, evt.data2);
                    break;

                case SDL_EventType.SDL_EVENT_WINDOW_RESIZED:
                    nativeStateStorage.Size.Value = new Size(evt.data1, evt.data2);
                    nativeStateStorage.PixelDensity.Value = nativeSDLState.PixelDensity;
                    nativeStateStorage.FullscreenMode.Value = nativeSDLState.FullscreenMode;
                    break;

                case SDL_EventType.SDL_EVENT_WINDOW_PIXEL_SIZE_CHANGED:
                    nativeStateStorage.SizeInPixels.Value = new Size(evt.data1, evt.data2);
                    nativeStateStorage.PixelDensity.Value = nativeSDLState.PixelDensity;
                    nativeStateStorage.FullscreenMode.Value = nativeSDLState.FullscreenMode;
                    break;

                case SDL_EventType.SDL_EVENT_WINDOW_MINIMIZED:
                    nativeStateStorage.WindowState.Value = NativeState.Minimised;
                    break;

                case SDL_EventType.SDL_EVENT_WINDOW_MAXIMIZED:
                    nativeStateStorage.WindowState.Value = NativeState.Maximised;
                    break;

                case SDL_EventType.SDL_EVENT_WINDOW_RESTORED:
                    nativeStateStorage.WindowState.Value = NativeState.Restored;
                    break;

                case SDL_EventType.SDL_EVENT_WINDOW_MOUSE_ENTER:
                    nativeStateStorage.MouseFocus.Value = true;
                    break;

                case SDL_EventType.SDL_EVENT_WINDOW_MOUSE_LEAVE:
                    nativeStateStorage.MouseFocus.Value = false;
                    break;

                case SDL_EventType.SDL_EVENT_WINDOW_FOCUS_GAINED:
                    nativeStateStorage.InputFocus.Value = true;
                    break;

                case SDL_EventType.SDL_EVENT_WINDOW_FOCUS_LOST:
                    nativeStateStorage.InputFocus.Value = false;
                    break;

                case SDL_EventType.SDL_EVENT_WINDOW_DISPLAY_CHANGED:
                    nativeStateStorage.CurrentDisplayMode.Value = nativeSDLState.CurrentDisplayMode;
                    nativeStateStorage.Display.Value = DisplayManager.GetDisplay((SDL_DisplayID)evt.data1);
                    break;

                case SDL_EventType.SDL_EVENT_WINDOW_DISPLAY_SCALE_CHANGED:
                    nativeStateStorage.DisplayScale.Value = nativeSDLState.DisplayScale;
                    break;

                case SDL_EventType.SDL_EVENT_WINDOW_SAFE_AREA_CHANGED:
                    nativeStateStorage.SafeArea.Value = nativeSDLState.SafeArea;
                    break;

                case SDL_EventType.SDL_EVENT_WINDOW_OCCLUDED:
                    nativeStateStorage.Occluded.Value = true;
                    break;

                case SDL_EventType.SDL_EVENT_WINDOW_ENTER_FULLSCREEN:
                    nativeStateStorage.Fullscreen.Value = true;
                    break;

                case SDL_EventType.SDL_EVENT_WINDOW_LEAVE_FULLSCREEN:
                    nativeStateStorage.Fullscreen.Value = false;
                    break;

                case SDL_EventType.SDL_EVENT_WINDOW_METAL_VIEW_RESIZED:
                case SDL_EventType.SDL_EVENT_WINDOW_CLOSE_REQUESTED:
                case SDL_EventType.SDL_EVENT_WINDOW_HIT_TEST:
                case SDL_EventType.SDL_EVENT_WINDOW_ICCPROF_CHANGED:
                case SDL_EventType.SDL_EVENT_WINDOW_DESTROYED:
                case SDL_EventType.SDL_EVENT_WINDOW_HDR_STATE_CHANGED:
                    break;
            }
        }

        public void HandleDisplayEvent(SDL_DisplayEvent evt)
        {
            Debug.Assert(nativeSDLState != null);

            switch (evt.type)
            {
                case SDL_EventType.SDL_EVENT_DISPLAY_CURRENT_MODE_CHANGED:
                    unsafe
                    {
                        if (evt.displayID == SDL_GetDisplayForWindow(SDLWindowHandle))
                            nativeStateStorage.CurrentDisplayMode.Value = nativeSDLState.CurrentDisplayMode;
                    }

                    break;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
