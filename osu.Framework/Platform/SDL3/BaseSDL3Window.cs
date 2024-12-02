// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Platform.SDL3.Native;
using osu.Framework.Threading;
using SDL;
using static SDL.SDL3;

namespace osu.Framework.Platform.SDL3
{
    internal abstract class BaseSDL3Window
    {
        protected readonly Scheduler EventScheduler = new Scheduler();
        protected readonly Scheduler CommandScheduler = new Scheduler();

        protected readonly DisplayManager DisplayManager = new DisplayManager();

        private NativeSDLState? nativeSDLState;

        /// <summary>
        /// Stores the state provided by <see cref="nativeSDLState"/> and <see cref="HandleWindowEvent"/>.
        /// </summary>
        private readonly NativeStateStorage nativeStateStorage = new NativeStateStorage();

        public NativeStateStorage UnsafeGetState() => nativeStateStorage;

        internal unsafe SDL_Window* SDLWindowHandle { get; private set; }

        protected bool UpdatingDerivedState { get; private set; }

        public IDisposable? StartUpdatingDerivedState()
        {
            if (UpdatingDerivedState)
                return null;

            UpdatingDerivedState = true;
            return new InvokeOnDisposal<BaseSDL3Window>(this, static w => w.UpdatingDerivedState = false);
        }

        protected void SetupNativeDependencies()
        {
            SetupNativeDependencies(nativeStateStorage);
        }

        protected abstract void SetupNativeDependencies(NativeStateStorage nativeState);

        /// <remarks>Should only be called inside <see cref="SetupNativeDependencies(NativeStateStorage)"/></remarks>
        public void DependsOnNative<T>(UpdateDerivedStateDelegate updateMethod, IBindable<T> stateStorageBindable)
        {
            stateStorageBindable.BindValueChanged(_ =>
            {
                lock (scheduledDerivedUpdates)
                {
                    if (!scheduledDerivedUpdates.Contains(updateMethod))
                        scheduledDerivedUpdates.Add(updateMethod);
                }
            });
        }

        private readonly List<UpdateDerivedStateDelegate> scheduledDerivedUpdates = [];
        private readonly List<UpdateNativeStateDelegate> scheduledNativeUpdates = [];

        protected void UpdateDerivedState()
        {
            lock (scheduledDerivedUpdates)
            {
                UpdatingDerivedState = true;

                foreach (var task in scheduledDerivedUpdates)
                    task(nativeStateStorage);

                scheduledDerivedUpdates.Clear();

                UpdatingDerivedState = false;
            }
        }

        protected void UpdateNativeState()
        {
            lock (scheduledNativeUpdates)
            {
                foreach (var task in scheduledNativeUpdates)
                    task(nativeSDLState != null ? nativeSDLState : nativeStateStorage);

                scheduledNativeUpdates.Clear();
            }
        }

        protected void ScheduleNativeStateUpdate(UpdateNativeStateDelegate update)
        {
            lock (scheduledNativeUpdates)
            {
                if (!scheduledNativeUpdates.Contains(update))
                    scheduledNativeUpdates.Add(update);
            }
        }

        protected void DependsOnDerived<T>(UpdateNativeStateDelegate updateMethod, IBindable<T> windowBindable)
        {
            windowBindable.BindValueChanged(_ =>
            {
                if (UpdatingDerivedState)
                    return;

                ScheduleNativeStateUpdate(updateMethod);
            }, true);
        }

        protected unsafe void Create(SDL_WindowFlags flags)
        {
            Debug.Assert(SDLWindowHandle == null);

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
                SDL_SetBooleanProperty(props, SDL_PROP_WINDOW_CREATE_MOUSE_GRABBED_BOOLEAN, nativeStateStorage.MouseGrab.Value).ThrowIfFailed();
                SDL_SetBooleanProperty(props, SDL_PROP_WINDOW_CREATE_RESIZABLE_BOOLEAN, nativeStateStorage.Resizable.Value).ThrowIfFailed();
                SDL_SetStringProperty(props, SDL_PROP_WINDOW_CREATE_TITLE_STRING, nativeStateStorage.Title.Value).ThrowIfFailed();
                SDL_SetNumberProperty(props, SDL_PROP_WINDOW_CREATE_WIDTH_NUMBER, nativeStateStorage.Size.Value.Width).ThrowIfFailed();
                SDL_SetNumberProperty(props, SDL_PROP_WINDOW_CREATE_X_NUMBER, nativeStateStorage.Position.Value.X).ThrowIfFailed();
                SDL_SetNumberProperty(props, SDL_PROP_WINDOW_CREATE_Y_NUMBER, nativeStateStorage.Position.Value.Y).ThrowIfFailed();

                SDLWindowHandle = SDL_CreateWindowWithProperties(props);
                SDLException.ThrowIfNull(SDLWindowHandle, "Failed to create window.");
            }
            finally
            {
                SDL_DestroyProperties(props);
            }

            Debug.Assert(nativeSDLState == null);
            nativeSDLState = new NativeSDLState(SDLWindowHandle, DisplayManager, nativeStateStorage);
            nativeSDLState.UpdateFrom(nativeStateStorage);
        }

        protected void PrepareForRun()
        {
            Debug.Assert(nativeSDLState != null);
            nativeStateStorage.UpdateFrom(nativeSDLState);
        }

        protected unsafe void Destroy()
        {
            if (SDLWindowHandle != null)
            {
                SDL_DestroyWindow(SDLWindowHandle);
                SDLWindowHandle = null;
            }
        }

        protected void HandleWindowEvent(SDL_WindowEvent evt)
        {
            Debug.Assert(nativeSDLState != null);

            switch (evt.type)
            {
                case SDL_EventType.SDL_EVENT_WINDOW_SHOWN:
                    nativeStateStorage.Visible.Value = true;
                    break;

                case SDL_EventType.SDL_EVENT_WINDOW_HIDDEN:
                    nativeStateStorage.Visible.Value = false;
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

        protected void HandleDisplayEvent(SDL_DisplayEvent evt)
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

    public delegate void UpdateDerivedStateDelegate(IReadOnlyNativeState state);

    public delegate void UpdateNativeStateDelegate(IWriteOnlyNativeState newState);
}
