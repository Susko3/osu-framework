// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Platform.SDL3.Native;
using osuTK;

namespace osu.Framework.Platform.SDL3
{
    internal partial class NewSDL3Window : NotQuiteBaseSDL3Window, ISDL3Window
    {
        private readonly SDL3GraphicsSurface graphicsSurface;
        IGraphicsSurface IWindow.GraphicsSurface => graphicsSurface;

        public WindowMode DefaultWindowMode { get; }

        public IEnumerable<WindowMode> SupportedWindowModes => Enum.GetValues<WindowMode>();

        #region FrameworkConfigManager bindables

        /// <summary>
        /// Bound to <see cref="FrameworkSetting.WindowedSize"/>.
        /// </summary>
        protected readonly BindableSize WindowedSize = new BindableSize();

        /// <summary>
        /// Bound to <see cref="FrameworkSetting.WindowedPositionX"/>.
        /// </summary>
        protected readonly BindableDouble WindowedPositionX = new BindableDouble();

        /// <summary>
        /// Bound to <see cref="FrameworkSetting.WindowedPositionY"/>.
        /// </summary>
        protected readonly BindableDouble WindowedPositionY = new BindableDouble();

        /// <summary>
        /// Bound to <see cref="FrameworkSetting.LastDisplayDevice"/>.
        /// </summary>
        protected readonly Bindable<DisplayIndex> LastDisplayDevice = new Bindable<DisplayIndex>();

        private readonly BindableBool cursorInWindow = new BindableBool();
        public IBindable<bool> CursorInWindow => cursorInWindow;

        /// <summary>
        /// Bound to <see cref="FrameworkSetting.SizeFullscreen"/>.
        /// </summary>
        protected BindableSize SizeFullscreen { get; } = new BindableSize();

        /// <summary>
        /// Bound to <see cref="FrameworkSetting.WindowMode"/>.
        /// </summary>
        public Bindable<WindowMode> WindowMode => WindowModeMagic.Bindable;

        #endregion

        public Bindable<Display> CurrentDisplayBindable { get; } = new Bindable<Display>();

        private Bindable<DisplayMode> currentDisplayMode { get; } = new Bindable<DisplayMode>();

        public IBindable<DisplayMode> CurrentDisplayMode => currentDisplayMode;

        public Size ClientSize { get; private set; }

        public Size Size { get; private set; }

        public Point Position { get; private set; }

        public float Scale { get; private set; }

        bool IWindow.Resizable
        {
            get => UnsafeGetStateStorage().Resizable.Value;
            set => ScheduleNativeStateUpdate(newState => newState.SetResizable(value));
        }

        protected NewSDL3Window(GraphicsSurfaceType surfaceType, string appName)
        {
            graphicsSurface = new SDL3GraphicsSurface(this, surfaceType);
        }

        public void SetupWindow(FrameworkConfigManager config)
        {
            // 1. želim da se Config bindable prebaci u lokalni bindable u NewSDL3Window
            config.BindWith(FrameworkSetting.WindowedSize, WindowedSize);
            config.BindWith(FrameworkSetting.WindowedPositionX, WindowedPositionX);
            config.BindWith(FrameworkSetting.WindowedPositionY, WindowedPositionY);
            config.BindWith(FrameworkSetting.LastDisplayDevice, LastDisplayDevice);
            config.BindWith(FrameworkSetting.SizeFullscreen, SizeFullscreen);
            config.BindWith(FrameworkSetting.WindowMode, WindowMode);
            // 2. Svi bindable u NewSDL3Window koji ovise o configu trebaju poprimiti svoju konačnu vrijednost

            SetupDerivedDependencies();

            // 3. Ažuriram IWriteOnlyNativeState sa pomoću UpdateNativeSize, UpdateNativePosition, etc.
            UpdateDerivedState();
        }

        protected virtual void SetupDerivedDependencies()
        {
            CurrentDisplayBindable.BindValueChanged(e =>
            {
                if (UpdatingDerivedState)
                    return;

                LastDisplayDevice.Value = (DisplayIndex)e.NewValue.Index;
            });

            DependsOnDerived(UpdateNativeFullscreenMode, LastDisplayDevice);
            DependsOnDerived(UpdateNativeFullscreenMode, SizeFullscreen);
            DependsOnDerived(UpdateNativeFullscreenMode, WindowMode);

            DependsOnDerived(UpdateNativePosition, WindowedPositionX);
            DependsOnDerived(UpdateNativePosition, WindowedPositionY);
            DependsOnDerived(UpdateNativePosition, WindowedSize);
            DependsOnDerived(UpdateNativePosition, LastDisplayDevice);

            DependsOnDerived(UpdateNativeSize, WindowedSize);
        }

        public virtual void Create()
        {
            // this calls SDL_CreateWindowWithProperties and fills out NativeStateStorage with current SDL state
            // TODO: run a single event frame to pick up on any changes?
            Create(graphicsSurface.Type.ToFlags());

            // TODO: test za ovo:
            // teoretski, niti jedan bindable iz NewSDL3Window ne bi trebao biti promijenjen

            SetupNativeDependencies();
        }

        protected override void SetupNativeDependencies(NativeStateStorage nativeState)
        {
            DependsOnNative(updateCursorInWindow, nativeState.MouseFocus);

            DependsOnNative(UpdateWindowState, nativeState.WindowState);
            DependsOnNative(UpdateWindowState, nativeState.Fullscreen);
            DependsOnNative(UpdateWindowState, nativeState.FullscreenMode);

            DependsOnNative(updateIsActive, nativeState.InputFocus);

            DependsOnNative(UpdateWindowMode, nativeState.FullscreenMode);
            DependsOnNative(UpdateWindowMode, nativeState.Fullscreen);

            DependsOnNative(updateCurrentDisplay, nativeState.Display);

            DependsOnNative(updateCurrentDisplayMode, nativeState.CurrentDisplayMode);

            DependsOnNative(UpdateSize, nativeState.PixelDensity);
            DependsOnNative(UpdateSize, nativeState.Size);
            DependsOnNative(UpdateSize, nativeState.SizeInPixels);

            DependsOnNative(updatePosition, nativeState.Position);
        }

        private void updateCurrentDisplay(IReadOnlyNativeState state)
        {
            CurrentDisplayBindable.Value = state.Display;
        }

        public IBindable<bool> IsActive => isActive;

        private void updateIsActive(IReadOnlyNativeState state)
        {
            isActive.Value = state.InputFocus;
        }

        private void updateCursorInWindow(IReadOnlyNativeState state)
        {
            cursorInWindow.Value = state.MouseFocus;
        }

        private readonly BindableBool isActive = new BindableBool();

        internal readonly SettableTroughBindableMagic<WindowMode> WindowModeMagic = new SettableTroughBindableMagic<WindowMode>
        {
            SetupDependencies = (@this, state) =>
            {
                if (RuntimeInfo.OS == RuntimeInfo.Platform.Windows)
                    @this.DependsOnNative(state.Bordered);

                @this.DependsOnNative(state.Fullscreen);
                @this.DependsOnNative(state.FullscreenMode);
            },
            GetValue = state =>
            {
                if (RuntimeInfo.OS == RuntimeInfo.Platform.Windows && !state.Bordered)
                    return Configuration.WindowMode.Borderless;

                if (state.Fullscreen)
                    return state.FullscreenMode == null ? Configuration.WindowMode.Borderless : Configuration.WindowMode.Fullscreen;

                return Configuration.WindowMode.Windowed;
            }
        };

        internal readonly SettableMagic<WindowState> WindowStateMagic = new SettableMagic<WindowState>
        {
            SetupDependencies = (@this, state) =>
            {
                if (RuntimeInfo.OS == RuntimeInfo.Platform.Windows)
                    @this.DependsOnNative(state.Bordered);

                @this.DependsOnNative(state.Fullscreen);
                @this.DependsOnNative(state.FullscreenMode);
                @this.DependsOnNative(state.WindowState);
            },
            GetValue = state =>
            {
                switch (state.WindowState)
                {
                    case NativeState.Restored:
                        if (RuntimeInfo.OS == RuntimeInfo.Platform.Windows && !state.Bordered)
                            return WindowState.FullscreenBorderless;

                        if (state.Fullscreen)
                            return state.FullscreenMode == null ? WindowState.FullscreenBorderless : WindowState.Fullscreen;

                        return WindowState.Normal;

                    case NativeState.Minimised:
                        return WindowState.Minimised;

                    case NativeState.Maximised:
                        return WindowState.Maximised;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        };

        // TODO: in IWindow, this should be IBindable<WindowState> and SetWindowState(value)
        WindowState IWindow.WindowState
        {
            get => WindowStateMagic.Bindable.Value;

            set
            {
                // TODO: we are reading an input thread bindable from the update thread
                if (value == WindowStateMagic.Bindable.Value)
                    return;

                CommandScheduler.AddOnce(value =>
                {
                    pendingWindowState = value;

                    switch (value)
                    {
                        case WindowState.Maximised:
                        case WindowState.Normal:
                            pendingWindowMode = Configuration.WindowMode.Windowed;
                            break;

                        case WindowState.Fullscreen:
                            pendingWindowMode = Configuration.WindowMode.Fullscreen;
                            break;

                        case WindowState.FullscreenBorderless:
                            pendingWindowMode = Configuration.WindowMode.Borderless;
                            break;
                    }
                }, value);

                // TODO: how do we know what depends on us?
                ScheduleNativeStateUpdate(updateNativeWindowState);
            }
        }

        private void updateNativeWindowState(IWriteOnlyNativeState state)
        {
            switch (pendingWindowState ?? WindowState.Value)
            {
                case WindowState.Normal:
                case WindowState.Fullscreen:
                case WindowState.FullscreenBorderless:
                    state.SetWindowState(NativeState.Restored);
                    break;

                case WindowState.Minimised:
                    state.SetWindowState(NativeState.Minimised);
                    break;

                case WindowState.Maximised:
                    state.SetWindowState(NativeState.Maximised);
                    break;
            }
        }

        public event Action<WindowState>? WindowStateChanged;

        protected virtual bool IsBorderlessFullscreen(IReadOnlyNativeState state) => state.Fullscreen && state.FullscreenMode == null;

        protected void UpdateWindowState(IReadOnlyNativeState state)
        {
            WindowState newState;

            switch (state.WindowState)
            {
                case NativeState.Restored:
                    if (IsBorderlessFullscreen(state))
                        newState = WindowState.FullscreenBorderless;
                    else if (state.Fullscreen)
                        newState = WindowState.Fullscreen;
                    else
                        newState = WindowState.Normal;
                    break;

                case NativeState.Minimised:
                    newState = WindowState.Minimised;
                    break;

                case NativeState.Maximised:
                    newState = WindowState.Maximised;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(state), $"Invalid {nameof(NativeState)}: {state.WindowState}");
            }

            if (newState != WindowState)
            {
                WindowState = newState;
                WindowStateChanged?.Invoke(WindowState);
            }
        }

        protected void UpdateWindowMode(IReadOnlyNativeState state)
        {
            WindowMode windowMode;

            if (IsBorderlessFullscreen(state))
                windowMode = Configuration.WindowMode.Borderless;
            else if (state.Fullscreen)
                windowMode = Configuration.WindowMode.Fullscreen;
            else
                windowMode = Configuration.WindowMode.Windowed;

            Debug.Assert(SupportedWindowModes.Contains(windowMode));
            WindowMode.Value = windowMode;
        }

        protected virtual void UpdateNativeFullscreenMode(IWriteOnlyNativeState newState)
        {
            switch (pendingWindowMode ?? WindowMode.Value)
            {
                case Configuration.WindowMode.Borderless:
                    newState.SetFullscreenMode(null);
                    break;

                case Configuration.WindowMode.Fullscreen:
                    var mode = DisplayManager.GetFromIndex(LastDisplayDevice.Value).FindDisplayMode(SizeFullscreen.Value);
                    newState.SetFullscreenMode(mode);
                    break;
            }
        }

        protected virtual Size GetClientSize(IReadOnlyNativeState state)
        {
            return state.SizeInPixels;
        }

        protected void UpdateSize(IReadOnlyNativeState state)
        {
            Size = state.Size;
            ClientSize = GetClientSize(state);
            Scale = state.PixelDensity;
            Resized?.Invoke();
        }

        private void updatePosition(IReadOnlyNativeState state)
        {
            Position = state.Position;
        }

        private void updateCurrentDisplayMode(IReadOnlyNativeState state)
        {
            currentDisplayMode.Value = state.CurrentDisplayMode;
        }

        protected virtual void UpdateNativePosition(IWriteOnlyNativeState newState)
        {
            var displayBounds = DisplayManager.GetFromIndex(LastDisplayDevice.Value).Bounds;
            var windowSize = WindowedSize.Value;

            // TODO: this math is wrong, ensure that we never overflow into the next display for [-0.5, 1.5]
            int windowX = (int)Math.Round((displayBounds.Width - windowSize.Width) * WindowedPositionX.Value);
            int windowY = (int)Math.Round((displayBounds.Height - windowSize.Height) * WindowedPositionY.Value);

            newState.SetPosition(new Point(windowX + displayBounds.X, windowY + displayBounds.Y));
        }

        protected virtual void UpdateNativeSize(IWriteOnlyNativeState newState)
        {
            newState.SetSize(WindowedSize.Value);
        }

        public event Action? Resized;
    }
}
