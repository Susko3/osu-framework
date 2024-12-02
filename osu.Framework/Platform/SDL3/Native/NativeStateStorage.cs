// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Drawing;
using osu.Framework.Bindables;

namespace osu.Framework.Platform.SDL3.Native
{
    /// <summary>
    /// Exposes properties in <see cref="IReadOnlyNativeState"/> as <see cref="Bindable{T}">bindables</see>
    /// </summary>
    public class NativeStateStorage : IReadOnlyNativeState, IWriteOnlyNativeState
    {
        public void UpdateFrom(IReadOnlyNativeState state)
        {
            // ((IWriteOnlyNativeState)this).SetFrom(state);
            TextInputParams.Value = state.TextInputParams;
            RelativeMouseMode.Value = state.RelativeMouseMode;
            CurrentDisplayMode.Value = state.CurrentDisplayMode;
            Display.Value = state.Display;
            PixelDensity.Value = state.PixelDensity;
            DisplayScale.Value = state.DisplayScale;
            FullscreenMode.Value = state.FullscreenMode;
            Title.Value = state.Title;
            Position.Value = state.Position;
            Size.Value = state.Size;
            SafeArea.Value = state.SafeArea;
            AspectRatio.Value = state.AspectRatio;
            // BordersSize.Value = state.BordersSize;
            SizeInPixels.Value = state.SizeInPixels;
            MinimumSize.Value = state.MinimumSize;
            MaximumSize.Value = state.MaximumSize;
            Bordered.Value = state.Bordered;
            Resizable.Value = state.Resizable;
            AlwaysOnTop.Value = state.AlwaysOnTop;
            Visible.Value = state.Visible;
            WindowState.Value = state.WindowState;
            Fullscreen.Value = state.Fullscreen;
            MouseRect.Value = state.MouseRect;
            Opacity.Value = state.Opacity;
            Focusable.Value = state.Focusable;
            Occluded.Value = state.Occluded;
            InputFocus.Value = state.InputFocus;
            MouseFocus.Value = state.MouseFocus;
        }

        // ReSharper disable ArrangeObjectCreationWhenTypeEvident

        // TODO: set default values to defaults from SDL_CreateWindow()

        /// <summary>
        /// Last <code>TextInputParams</code> set by <see cref="NativeSDLState"/>.
        /// </summary>
        public readonly Bindable<TextInputParams?> TextInputParams = new();

        public readonly BindableBool RelativeMouseMode = new();

        public readonly Bindable<DisplayMode> CurrentDisplayMode = new();

        /// <remarks>Updated by events.</remarks>
        public readonly Bindable<Display> Display = new();

        /// <remarks>Updated by events.</remarks>
        public readonly BindableFloat PixelDensity = new() { MinValue = float.Epsilon };

        /// <remarks>Updated by events.</remarks>
        public readonly BindableFloat DisplayScale = new() { MinValue = float.Epsilon };

        /// <remarks>Updated by events.</remarks>
        public readonly Bindable<DisplayMode?> FullscreenMode = new();

        public readonly Bindable<string> Title = new(string.Empty);

        /// <remarks>Updated by events.</remarks>
        public readonly Bindable<Point> Position = new();

        /// <remarks>Updated by events.</remarks>
        public readonly BindableSize Size = new(new Size(640, 480)) { MinValue = new Size(1, 1) };

        /// <remarks>Updated by events.</remarks>
        public readonly Bindable<Rectangle> SafeArea = new();

        public readonly Bindable<AspectRatio> AspectRatio = new();

        public readonly BindableMarginPadding BordersSize = new();

        /// <remarks>Updated by events.</remarks>
        public readonly BindableSize SizeInPixels = new();

        public readonly BindableSize MinimumSize = new();

        public readonly BindableSize MaximumSize = new();

        public readonly BindableBool Bordered = new(true);

        public readonly BindableBool Resizable = new();

        public readonly BindableBool AlwaysOnTop = new();

        /// <remarks>Updated by events.</remarks>
        public readonly BindableBool Visible = new(true);

        /// <remarks>Updated by events.</remarks>
        public readonly Bindable<NativeState> WindowState = new();

        /// <remarks>Updated by events.</remarks>
        public readonly BindableBool Fullscreen = new();

        public readonly Bindable<Rectangle?> MouseRect = new();

        public readonly BindableFloat Opacity = new(1.0f) { MinValue = 0.0f, MaxValue = 1.0f };

        public readonly BindableBool Focusable = new(true);

        /// <remarks>Updated by events.</remarks>
        public readonly BindableBool Occluded = new();

        /// <remarks>Updated by events.</remarks>
        public readonly BindableBool InputFocus = new();

        /// <remarks>Updated by events.</remarks>
        public readonly BindableBool MouseFocus = new();

        // ReSharper restore ArrangeObjectCreationWhenTypeEvident

        TextInputParams? IReadOnlyNativeState.TextInputParams => TextInputParams.Value;
        bool IReadOnlyNativeState.RelativeMouseMode => RelativeMouseMode.Value;
        DisplayMode IReadOnlyNativeState.CurrentDisplayMode => CurrentDisplayMode.Value;
        Display IReadOnlyNativeState.Display => Display.Value;
        float IReadOnlyNativeState.PixelDensity => PixelDensity.Value;
        float IReadOnlyNativeState.DisplayScale => DisplayScale.Value;
        DisplayMode? IReadOnlyNativeState.FullscreenMode => FullscreenMode.Value;
        string IReadOnlyNativeState.Title => Title.Value;
        Point IReadOnlyNativeState.Position => Position.Value;
        Size IReadOnlyNativeState.Size => Size.Value;
        Rectangle IReadOnlyNativeState.SafeArea => SafeArea.Value;
        AspectRatio IReadOnlyNativeState.AspectRatio => AspectRatio.Value;
        // MarginPadding IReadOnlyNativeState.BordersSize => BordersSize.Value;
        Size IReadOnlyNativeState.SizeInPixels => SizeInPixels.Value;
        Size IReadOnlyNativeState.MinimumSize => MinimumSize.Value;
        Size IReadOnlyNativeState.MaximumSize => MaximumSize.Value;
        bool IReadOnlyNativeState.Bordered => Bordered.Value;
        bool IReadOnlyNativeState.Resizable => Resizable.Value;
        bool IReadOnlyNativeState.AlwaysOnTop => AlwaysOnTop.Value;
        bool IReadOnlyNativeState.Visible => Visible.Value;
        NativeState IReadOnlyNativeState.WindowState => WindowState.Value;
        bool IReadOnlyNativeState.Fullscreen => Fullscreen.Value;
        Rectangle? IReadOnlyNativeState.MouseRect => MouseRect.Value;
        float IReadOnlyNativeState.Opacity => Opacity.Value;
        bool IReadOnlyNativeState.Focusable => Focusable.Value;
        bool IReadOnlyNativeState.Occluded => Occluded.Value;
        bool IReadOnlyNativeState.InputFocus => InputFocus.Value;
        bool IReadOnlyNativeState.MouseFocus => MouseFocus.Value;

        void IWriteOnlyNativeState.SetTextInputParams(TextInputParams? value) => TextInputParams.Value = value;

        void IWriteOnlyNativeState.SetRelativeMouseMode(bool value) => RelativeMouseMode.Value = value;

        void IWriteOnlyNativeState.SetFullscreenMode(DisplayMode? value) => FullscreenMode.Value = value;

        void IWriteOnlyNativeState.SetTitle(string value) => Title.Value = value;

        void IWriteOnlyNativeState.SetPosition(Point value) => Position.Value = value;

        void IWriteOnlyNativeState.SetSize(Size value) => Size.Value = value;

        void IWriteOnlyNativeState.SetAspectRatio(AspectRatio value) => AspectRatio.Value = value;

        void IWriteOnlyNativeState.SetMinimumSize(Size value) => MinimumSize.Value = value;

        void IWriteOnlyNativeState.SetMaximumSize(Size value) => MaximumSize.Value = value;

        void IWriteOnlyNativeState.SetBordered(bool value) => Bordered.Value = value;

        void IWriteOnlyNativeState.SetResizable(bool value) => Resizable.Value = value;

        void IWriteOnlyNativeState.SetAlwaysOnTop(bool value) => AlwaysOnTop.Value = value;

        void IWriteOnlyNativeState.SetVisible(bool value) => Visible.Value = value;

        void IWriteOnlyNativeState.SetWindowState(NativeState value) => WindowState.Value = value;

        void IWriteOnlyNativeState.SetFullscreen(bool value) => Fullscreen.Value = value;

        void IWriteOnlyNativeState.SetMouseRect(Rectangle? value) => MouseRect.Value = value;

        void IWriteOnlyNativeState.SetOpacity(float value) => Opacity.Value = value;

        void IWriteOnlyNativeState.SetFocusable(bool value) => Focusable.Value = value;
    }
}
