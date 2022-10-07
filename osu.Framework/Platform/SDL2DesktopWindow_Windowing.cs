// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Logging;
using osu.Framework.Platform.SDL2;
using osuTK;
using SDL2;

namespace osu.Framework.Platform
{
    public partial class SDL2DesktopWindow
    {
        private void setupWindowing(FrameworkConfigManager config)
        {
            updateDisplays();

            DisplaysChanged += _ => CurrentDisplay.Default = PrimaryDisplay;
            CurrentDisplay.Default = PrimaryDisplay;
            CurrentDisplay.BindValueChanged(display =>
            {
                if (display.NewValue.Equals(internalCurrentDisplay))
                    // if the values match, that means that the this set operation originates from within SDL2DesktopWindow
                    // updating the display would lead to a feedback loop.
                    return;

                Debug.Assert(display.OldValue.Equals(internalCurrentDisplay));

                commandScheduler.AddOnce(updateDisplay, display.NewValue.Index);
            });

            config.BindWith(FrameworkSetting.LastDisplayDevice, windowDisplayIndexBindable);
            windowDisplayIndexBindable.BindValueChanged(displayIndex =>
            {
                int newIndex = displayIndex.NewValue == DisplayIndex.Default ? 0 : (int)displayIndex.NewValue;

                if (newIndex == internalCurrentDisplay.Index)
                    // if the values match, that means that the this set operation originates from within SDL2DesktopWindow
                    // updating the display would lead to a feedback loop.
                    return;

                Debug.Assert((int)displayIndex.OldValue == internalCurrentDisplay.Index);

                commandScheduler.AddOnce(updateDisplay, newIndex);
            });

            // set the appropriate startup display.
            commandScheduler.AddOnce(updateDisplay, (int)windowDisplayIndexBindable.Value);

            sizeFullscreen.ValueChanged += _ =>
            {
                if (storingSizeToConfig) return;
                if (windowState != WindowState.Fullscreen) return;

                pendingWindowState = windowState;
            };

            sizeWindowed.ValueChanged += _ =>
            {
                if (storingSizeToConfig) return;
                if (windowState != WindowState.Normal) return;

                pendingWindowState = windowState;
            };

            sizeWindowed.MinValueChanged += min =>
            {
                if (min.Width < 0 || min.Height < 0)
                    throw new InvalidOperationException($"Expected zero or positive size, got {min}");

                if (min.Width > sizeWindowed.MaxValue.Width || min.Height > sizeWindowed.MaxValue.Height)
                    throw new InvalidOperationException($"Expected a size less than max window size ({sizeWindowed.MaxValue}), got {min}");

                ScheduleCommand(() => SDL.SDL_SetWindowMinimumSize(SDLWindowHandle, min.Width, min.Height));
            };

            sizeWindowed.MaxValueChanged += max =>
            {
                if (max.Width <= 0 || max.Height <= 0)
                    throw new InvalidOperationException($"Expected positive size, got {max}");

                if (max.Width < sizeWindowed.MinValue.Width || max.Height < sizeWindowed.MinValue.Height)
                    throw new InvalidOperationException($"Expected a size greater than min window size ({sizeWindowed.MinValue}), got {max}");

                ScheduleCommand(() => SDL.SDL_SetWindowMaximumSize(SDLWindowHandle, max.Width, max.Height));
            };

            config.BindWith(FrameworkSetting.SizeFullscreen, sizeFullscreen);
            config.BindWith(FrameworkSetting.WindowedSize, sizeWindowed);

            config.BindWith(FrameworkSetting.WindowedPositionX, windowPositionX);
            config.BindWith(FrameworkSetting.WindowedPositionY, windowPositionY);

            config.BindWith(FrameworkSetting.WindowMode, WindowMode);

            WindowMode.BindValueChanged(evt =>
            {
                switch (evt.NewValue)
                {
                    case Configuration.WindowMode.Fullscreen:
                        WindowState = WindowState.Fullscreen;
                        break;

                    case Configuration.WindowMode.Borderless:
                        WindowState = WindowState.FullscreenBorderless;
                        break;

                    case Configuration.WindowMode.Windowed:
                        WindowState = windowMaximised ? WindowState.Maximised : WindowState.Normal;
                        break;
                }
            });
        }

        private void initialiseWindowingAfterCreation()
        {
            fetchCurrentDisplay();
            updateWindowSpecifics();
            updateWindowSize();

            sizeWindowed.TriggerChange();

            WindowMode.TriggerChange();
        }

        private bool focused;

        /// <summary>
        /// Whether the window currently has focus.
        /// </summary>
        public bool Focused
        {
            get => focused;
            private set
            {
                if (value == focused)
                    return;

                isActive.Value = focused = value;
            }
        }

        public WindowMode DefaultWindowMode => Configuration.WindowMode.Windowed;

        /// <summary>
        /// Returns the window modes that the platform should support by default.
        /// </summary>
        protected virtual IEnumerable<WindowMode> DefaultSupportedWindowModes => Enum.GetValues(typeof(WindowMode)).OfType<WindowMode>();

        private Point position;

        /// <summary>
        /// Returns or sets the window's position in screen space. Only valid when in <see cref="osu.Framework.Configuration.WindowMode.Windowed"/>
        /// </summary>
        public Point Position
        {
            get => position;
            set
            {
                position = value;
                ScheduleCommand(() => SDL.SDL_SetWindowPosition(SDLWindowHandle, value.X, value.Y));
            }
        }

        private bool resizable = true;

        /// <summary>
        /// Returns or sets whether the window is resizable or not. Only valid when in <see cref="osu.Framework.Platform.WindowState.Normal"/>.
        /// </summary>
        public bool Resizable
        {
            get => resizable;
            set
            {
                if (resizable == value)
                    return;

                resizable = value;
                ScheduleCommand(() => SDL.SDL_SetWindowResizable(SDLWindowHandle, value ? SDL.SDL_bool.SDL_TRUE : SDL.SDL_bool.SDL_FALSE));
            }
        }

        private Size size = new Size(default_width, default_height);

        /// <summary>
        /// Returns or sets the window's internal size, before scaling.
        /// </summary>
        public virtual Size Size
        {
            get => size;
            protected set
            {
                if (value.Equals(size)) return;

                size = value;
                Resized?.Invoke();
            }
        }

        public Size MinSize
        {
            get => sizeWindowed.MinValue;
            set => sizeWindowed.MinValue = value;
        }

        public Size MaxSize
        {
            get => sizeWindowed.MaxValue;
            set => sizeWindowed.MaxValue = value;
        }

        /// <summary>
        /// Bound to <see cref="FrameworkSetting.WindowMode"/>.
        /// </summary>
        public Bindable<WindowMode> WindowMode { get; } = new Bindable<WindowMode>();

        private readonly BindableBool isActive = new BindableBool();

        public IBindable<bool> IsActive => isActive;

        private readonly BindableBool cursorInWindow = new BindableBool();

        public IBindable<bool> CursorInWindow => cursorInWindow;

        public IBindableList<WindowMode> SupportedWindowModes { get; }

        private bool visible;

        /// <summary>
        /// Enables or disables the window visibility.
        /// </summary>
        public bool Visible
        {
            get => visible;
            set
            {
                visible = value;
                ScheduleCommand(() =>
                {
                    if (value)
                        SDL.SDL_ShowWindow(SDLWindowHandle);
                    else
                        SDL.SDL_HideWindow(SDLWindowHandle);
                });
            }
        }

        private WindowState windowState = WindowState.Normal;

        private WindowState? pendingWindowState;

        /// <summary>
        /// Returns or sets the window's current <see cref="WindowState"/>.
        /// </summary>
        public WindowState WindowState
        {
            get => windowState;
            set
            {
                if (pendingWindowState == null && windowState == value)
                    return;

                pendingWindowState = value;
            }
        }

        /// <summary>
        /// Stores whether the window used to be in maximised state or not.
        /// Used to properly decide what window state to pick when switching to windowed mode (see <see cref="WindowMode"/> change event)
        /// </summary>
        private bool windowMaximised;

        /// <summary>
        /// Returns the drawable area, after scaling.
        /// </summary>
        public Size ClientSize => new Size(Size.Width, Size.Height);

        public float Scale = 1;

        #region Displays (mostly self-contained)

        /// <summary>
        /// Queries the physical displays and their supported resolutions.
        /// </summary>
        public IEnumerable<Display> Displays { get; private set; } = null!;

        public event Action<IEnumerable<Display>>? DisplaysChanged;

        // ReSharper disable once UnusedParameter.Local
        private void handleDisplayEvent(SDL.SDL_DisplayEvent evtDisplay) => updateDisplays();

        /// <summary>
        /// Updates <see cref="Displays"/> with the latest display information reported by SDL.
        /// </summary>
        /// <remarks>
        /// Has no effect on value of <see cref="CurrentDisplay"/>.
        /// </remarks>
        private void updateDisplays()
        {
            Displays = getSDLDisplays();
            DisplaysChanged?.Invoke(Displays);
        }

        /// <summary>
        /// Asserts that the current <see cref="Displays"/> match the actual displays as reported by SDL.
        /// </summary>
        /// <remarks>
        /// This assert is not fatal, as the <see cref="Displays"/> will get updated sooner or later
        /// in <see cref="handleDisplayEvent"/> or <see cref="handleWindowEvent"/>.
        /// </remarks>
        [Conditional("DEBUG")]
        private void assertDisplaysMatchSDL()
        {
            var actualDisplays = getSDLDisplays();
            Debug.Assert(actualDisplays.SequenceEqual(Displays), $"Stored {nameof(Displays)} don't match actual displays",
                $"Stored displays:\n  {string.Join("\n  ", Displays)}\n\nActual displays:\n  {string.Join("\n  ", actualDisplays)}");
        }

        private IEnumerable<Display> getSDLDisplays()
        {
            return Enumerable.Range(0, SDL.SDL_GetNumVideoDisplays()).Select(i =>
            {
                Debug.Assert(tryGetDisplayFromSDL(i, out var display));
                return display;
            }).ToArray();
        }

        private static bool tryGetDisplayFromSDL(int displayIndex, [NotNullWhen(true)] out Display? display)
        {
            if (displayIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(displayIndex), displayIndex, $"{nameof(displayIndex)} must be non-negative.");

            if (SDL.SDL_GetDisplayBounds(displayIndex, out var rect) < 0)
            {
                display = null;
                return false;
            }

            int numModes = SDL.SDL_GetNumDisplayModes(displayIndex);

            if (numModes <= 0)
            {
                display = null;
                return false;
            }

            var displayModes = Enumerable.Range(0, numModes)
                                         .Select(modeIndex =>
                                         {
                                             SDL.SDL_GetDisplayMode(displayIndex, modeIndex, out var mode);
                                             return mode.ToDisplayMode(displayIndex);
                                         })
                                         .ToArray();

            display = new Display(displayIndex, SDL.SDL_GetDisplayName(displayIndex), new Rectangle(rect.x, rect.y, rect.w, rect.h), displayModes);
            return true;
        }

        #endregion

        /// <summary>
        /// Gets the <see cref="Display"/> that has been set as "primary" or "default" in the operating system.
        /// </summary>
        public virtual Display PrimaryDisplay => Displays.First();

        #region CurrentDisplay

        /// <summary>
        /// The display the window is placed on currently, as reported by SDL.
        /// This is always sourced from the SDL display query method.
        /// </summary>
        private Display internalCurrentDisplay = null!;

        public Bindable<Display> CurrentDisplay { get; } = new Bindable<Display>();

        private void updateDisplay(int displayIndex)
        {
            if (tryGetDisplayFromSDL(displayIndex, out var display))
                updateWindowStateAndSize(WindowState, display);

            fetchCurrentDisplay();
        }

        private void fetchCurrentDisplay()
        {
            int newIndex = SDL.SDL_GetWindowDisplayIndex(SDLWindowHandle);

            if (tryGetDisplayFromSDL(newIndex, out var display))
            {
                bool changed = !display.Equals(internalCurrentDisplay);

                internalCurrentDisplay = display;
                CurrentDisplay.Value = internalCurrentDisplay;
                windowDisplayIndexBindable.Value = (DisplayIndex)display.Index;

                if (changed)
                    fetchCurrentDisplayMode(display, WindowState == WindowState.Fullscreen);
            }
        }

        #endregion

        private readonly Bindable<DisplayMode> currentDisplayMode = new Bindable<DisplayMode>();

        /// <summary>
        /// The <see cref="DisplayMode"/> for the display that this window is currently on.
        /// </summary>
        public IBindable<DisplayMode> CurrentDisplayMode => currentDisplayMode;

        private Rectangle windowDisplayBounds
        {
            get
            {
                SDL.SDL_GetDisplayBounds(CurrentDisplay.Value.Index, out var rect);
                return new Rectangle(rect.x, rect.y, rect.w, rect.h);
            }
        }

        /// <summary>
        /// Bound to <see cref="FrameworkSetting.SizeFullscreen"/>.
        /// </summary>
        private readonly BindableSize sizeFullscreen = new BindableSize();

        /// <summary>
        /// Bound to <see cref="FrameworkSetting.WindowedSize"/>.
        /// </summary>
        private readonly BindableSize sizeWindowed = new BindableSize();

        /// <summary>
        /// Bound to <see cref="FrameworkSetting.WindowedPositionX"/>.
        /// </summary>
        private readonly BindableDouble windowPositionX = new BindableDouble();

        /// <summary>
        /// Bound to <see cref="FrameworkSetting.WindowedPositionY"/>.
        /// </summary>
        private readonly BindableDouble windowPositionY = new BindableDouble();

        /// <summary>
        /// Bound to <see cref="FrameworkSetting.LastDisplayDevice"/>.
        /// </summary>
        private readonly Bindable<DisplayIndex> windowDisplayIndexBindable = new Bindable<DisplayIndex>();

        /// <summary>
        /// Updates the client size and the scale according to the window.
        /// </summary>
        /// <returns>Whether the window size has been changed after updating.</returns>
        private void updateWindowSize()
        {
            SDL.SDL_GL_GetDrawableSize(SDLWindowHandle, out int w, out int h);
            SDL.SDL_GetWindowSize(SDLWindowHandle, out int actualW, out int _);

            // When minimised on windows, values may be zero.
            // If we receive zeroes for either of these, it seems safe to completely ignore them.
            if (actualW <= 0 || w <= 0)
                return;

            Scale = (float)w / actualW;
            Size = new Size(w, h);

            // This function may be invoked before the SDL internal states are all changed. (as documented here: https://wiki.libsdl.org/SDL_SetEventFilter)
            // Scheduling the store to config until after the event poll has run will ensure the window is in the correct state.
            EventScheduler.AddOnce(storeWindowSizeToConfig);
        }

        #region SDL Event Handling

        private void handleWindowEvent(SDL.SDL_WindowEvent evtWindow)
        {
            updateWindowSpecifics();

            switch (evtWindow.windowEvent)
            {
                case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_MOVED:
                    // explicitly requery as there are occasions where what SDL has provided us with is not up-to-date.
                    SDL.SDL_GetWindowPosition(SDLWindowHandle, out int x, out int y);
                    var newPosition = new Point(x, y);

                    if (!newPosition.Equals(Position))
                    {
                        position = newPosition;
                        Moved?.Invoke(newPosition);

                        if (WindowMode.Value == Configuration.WindowMode.Windowed)
                            storeWindowPositionToConfig();
                    }

                    // in case the window moved to another display.
                    fetchCurrentDisplay();

                    // we may get a SDL_WINDOWEVENT_MOVED when the resolution of a display changes.
                    updateDisplays();
                    break;

                case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_SIZE_CHANGED:
                    updateWindowSize();
                    break;

                case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_ENTER:
                    cursorInWindow.Value = true;
                    MouseEntered?.Invoke();
                    break;

                case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_LEAVE:
                    cursorInWindow.Value = false;
                    MouseLeft?.Invoke();
                    break;

                case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_RESTORED:
                case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_FOCUS_GAINED:
                    Focused = true;
                    // displays can change without a SDL_DISPLAYEVENT being sent, eg. changing resolution.
                    // force update displays when gaining keyboard focus to always have up-to-date information.
                    // eg. this covers scenarios when changing resolution outside of the game, and then tabbing in.
                    updateDisplays();
                    break;

                case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_MINIMIZED:
                case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_FOCUS_LOST:
                    Focused = false;
                    break;

                case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_CLOSE:
                    break;

                case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_DISPLAY_CHANGED:
                    fetchCurrentDisplay();
                    break;
            }

            assertDisplaysMatchSDL();
        }

        /// <summary>
        /// Should be run on a regular basis to check for external window state changes.
        /// </summary>
        private void updateWindowSpecifics()
        {
            // don't attempt to run before the window is initialised, as Create() will do so anyway.
            if (SDLWindowHandle == IntPtr.Zero)
                return;

            var stateBefore = windowState;

            // check for a pending user state change and give precedence.
            if (pendingWindowState != null)
            {
                windowState = pendingWindowState.Value;
                pendingWindowState = null;

                updateWindowStateAndSize(windowState, CurrentDisplay.Value);
            }
            else
            {
                windowState = ((SDL.SDL_WindowFlags)SDL.SDL_GetWindowFlags(SDLWindowHandle)).ToWindowState();
            }

            if (windowState != stateBefore)
            {
                WindowStateChanged?.Invoke(windowState);
                updateMaximisedState(windowState);
            }
        }

        /// <summary>
        /// Should be run after a local window state change, to propagate the correct SDL actions.
        /// </summary>
        private void updateWindowStateAndSize(WindowState state, Display display)
        {
            // this reset is required even on changing from one fullscreen resolution to another.
            // if it is not included, the GL context will not get the correct size.
            // this is mentioned by multiple sources as an SDL issue, which seems to resolve by similar means (see https://discourse.libsdl.org/t/sdl-setwindowsize-does-not-work-in-fullscreen/20711/4).
            SDL.SDL_SetWindowBordered(SDLWindowHandle, SDL.SDL_bool.SDL_TRUE);
            SDL.SDL_SetWindowFullscreen(SDLWindowHandle, (uint)SDL.SDL_bool.SDL_FALSE);

            switch (state)
            {
                case WindowState.Normal:
                    Size = (sizeWindowed.Value * Scale).ToSize();

                    SDL.SDL_RestoreWindow(SDLWindowHandle);
                    SDL.SDL_SetWindowSize(SDLWindowHandle, sizeWindowed.Value.Width, sizeWindowed.Value.Height);
                    SDL.SDL_SetWindowResizable(SDLWindowHandle, Resizable ? SDL.SDL_bool.SDL_TRUE : SDL.SDL_bool.SDL_FALSE);

                    readWindowPositionFromConfig(state, display);
                    break;

                case WindowState.Fullscreen:
                    var closestMode = getClosestDisplayMode(sizeFullscreen.Value, currentDisplayMode.Value.RefreshRate, display);

                    Size = new Size(closestMode.w, closestMode.h);

                    moveWindowTo(display, new Vector2(0.5f));

                    SDL.SDL_SetWindowDisplayMode(SDLWindowHandle, ref closestMode);
                    SDL.SDL_SetWindowFullscreen(SDLWindowHandle, (uint)SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN);
                    break;

                case WindowState.FullscreenBorderless:
                    Size = SetBorderless(display);
                    break;

                case WindowState.Maximised:
                    SDL.SDL_RestoreWindow(SDLWindowHandle);
                    SDL.SDL_MaximizeWindow(SDLWindowHandle);

                    SDL.SDL_GL_GetDrawableSize(SDLWindowHandle, out int w, out int h);
                    Size = new Size(w, h);
                    break;

                case WindowState.Minimised:
                    SDL.SDL_MinimizeWindow(SDLWindowHandle);
                    break;
            }

            updateMaximisedState(state);

            fetchCurrentDisplayMode(display, state == WindowState.Fullscreen);
        }

        private void fetchCurrentDisplayMode(Display display, bool queryFullscreenMode)
        {
            // TODO: displayIndex should be valid here at all times.
            // on startup, the displayIndex will be invalid (-1) due to it being set later in the startup sequence.
            // related to order of operations in `updateWindowSpecifics()`.
            int localIndex = SDL.SDL_GetWindowDisplayIndex(SDLWindowHandle);

            int displayIndex = display.Index;

            if (localIndex != displayIndex)
                Logger.Log($"Stored display index ({displayIndex}) doesn't match current index ({localIndex})");

            bool successful = false;

            if (queryFullscreenMode)
            {
                if (SDL.SDL_GetWindowDisplayMode(SDLWindowHandle, out var mode) >= 0)
                {
                    currentDisplayMode.Value = mode.ToDisplayMode(localIndex);
                    successful = true;
                }
            }
            else
            {
                if (SDL.SDL_GetCurrentDisplayMode(localIndex, out var mode) >= 0)
                {
                    currentDisplayMode.Value = mode.ToDisplayMode(localIndex);
                    successful = true;
                }
            }

            string state = queryFullscreenMode ? "fullscreen" : "desktop";

            if (successful)
                Logger.Log($"Updated display mode to {state} resolution: {currentDisplayMode.Value.ReadableString()}");
            else
                Logger.Log($"Failed to get {state} display mode. Display index: {localIndex}. SDL error: {SDL.SDL_GetError()}", level: LogLevel.Error);
        }

        private void updateMaximisedState(WindowState state)
        {
            if (state is WindowState.Normal or WindowState.Maximised)
                windowMaximised = state == WindowState.Maximised;
        }

        private void readWindowPositionFromConfig(WindowState state, Display display)
        {
            if (state != WindowState.Normal)
                return;

            moveWindowTo(display, new Vector2((float)windowPositionX.Value, (float)windowPositionY.Value));
        }

        /// <summary>
        /// Moves the window to be centered around the normalized <paramref name="position"/> on a <paramref name="display"/>.
        /// </summary>
        /// <param name="display">The <see cref="Display"/> to move the window to.</param>
        /// <param name="position">Relative position on the display, normalized to <c>[-0.5, 1.5]</c>.</param>
        private void moveWindowTo(Display display, Vector2 position)
        {
            var displayBounds = display.Bounds;

            int windowWidth = sizeWindowed.Value.Width;
            int windowHeight = sizeWindowed.Value.Height;

            int windowX = (int)Math.Round((displayBounds.Width - windowWidth) * position.X);
            int windowY = (int)Math.Round((displayBounds.Height - windowHeight) * position.Y);

            Position = new Point(windowX + displayBounds.X, windowY + displayBounds.Y);
        }

        private void storeWindowPositionToConfig()
        {
            if (WindowState != WindowState.Normal)
                return;

            var displayBounds = CurrentDisplay.Value.Bounds;

            int windowX = Position.X - displayBounds.X;
            int windowY = Position.Y - displayBounds.Y;

            var windowSize = sizeWindowed.Value;

            windowPositionX.Value = displayBounds.Width > windowSize.Width ? (float)windowX / (displayBounds.Width - windowSize.Width) : 0;
            windowPositionY.Value = displayBounds.Height > windowSize.Height ? (float)windowY / (displayBounds.Height - windowSize.Height) : 0;
        }

        /// <summary>
        /// Set to <c>true</c> while the window size is being stored to config to avoid bindable feedback.
        /// </summary>
        private bool storingSizeToConfig;

        private void storeWindowSizeToConfig()
        {
            if (WindowState != WindowState.Normal)
                return;

            storingSizeToConfig = true;
            sizeWindowed.Value = (Size / Scale).ToSize();
            storingSizeToConfig = false;
        }

        /// <summary>
        /// Prepare display of a borderless window.
        /// </summary>
        /// <param name="display">The display to set borderless on.</param>
        /// <returns>
        /// The size of the borderless window's draw area.
        /// </returns>
        protected virtual Size SetBorderless(Display display)
        {
            moveWindowTo(display, new Vector2(0.5f));

            // this is a generally sane method of handling borderless, and works well on macOS and linux.
            SDL.SDL_SetWindowFullscreen(SDLWindowHandle, (uint)SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP);

            return display.Bounds.Size;
        }

        #endregion

        public void CycleMode()
        {
            var currentValue = WindowMode.Value;

            do
            {
                switch (currentValue)
                {
                    case Configuration.WindowMode.Windowed:
                        currentValue = Configuration.WindowMode.Borderless;
                        break;

                    case Configuration.WindowMode.Borderless:
                        currentValue = Configuration.WindowMode.Fullscreen;
                        break;

                    case Configuration.WindowMode.Fullscreen:
                        currentValue = Configuration.WindowMode.Windowed;
                        break;
                }
            } while (!SupportedWindowModes.Contains(currentValue) && currentValue != WindowMode.Value);

            WindowMode.Value = currentValue;
        }

        #region Helper functions

        private SDL.SDL_DisplayMode getClosestDisplayMode(Size size, int refreshRate, Display display)
        {
            var targetMode = new SDL.SDL_DisplayMode { w = size.Width, h = size.Height, refresh_rate = refreshRate };

            if (SDL.SDL_GetClosestDisplayMode(display.Index, ref targetMode, out var mode) != IntPtr.Zero)
                return mode;

            // fallback to current display's native bounds
            targetMode.w = display.Bounds.Width;
            targetMode.h = display.Bounds.Height;
            targetMode.refresh_rate = 0;

            if (SDL.SDL_GetClosestDisplayMode(display.Index, ref targetMode, out mode) != IntPtr.Zero)
                return mode;

            // finally return the current mode if everything else fails.
            // not sure this is required.
            if (SDL.SDL_GetWindowDisplayMode(SDLWindowHandle, out mode) >= 0)
                return mode;

            throw new InvalidOperationException("couldn't retrieve valid display mode");
        }

        #endregion

        #region Events

        /// <summary>
        /// Invoked after the window has resized.
        /// </summary>
        public event Action? Resized;

        /// <summary>
        /// Invoked after the window's state has changed.
        /// </summary>
        public event Action<WindowState>? WindowStateChanged;

        /// <summary>
        /// Invoked when the window moves.
        /// </summary>
        public event Action<Point>? Moved;

        #endregion
    }
}
