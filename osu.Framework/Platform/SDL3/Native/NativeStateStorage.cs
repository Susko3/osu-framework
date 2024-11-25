// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Drawing;
using osu.Framework.Bindables;
using osu.Framework.Graphics;

namespace osu.Framework.Platform.SDL3.Native
{
    /// <summary>
    /// Exposes properties in <see cref="IReadOnlyNativeState"/> as <see cref="Bindable{T}">bindables</see>
    /// </summary>
    public class NativeStateStorage : IReadOnlyNativeState, IWriteOnlyNativeState
    {
        public void UpdateFrom(IReadOnlyNativeState state)
        {
            ((IWriteOnlyNativeState)this).SetFrom(state);
        }

        // ReSharper disable ArrangeObjectCreationWhenTypeEvident

        // TODO: set default values to defaults from SDL_CreateWindow()

        /// <summary>
        /// Last <code>TextInputParams</code> set by <see cref="NativeSDLState"/>.
        /// </summary>
        public readonly Bindable<TextInputParams?> TextInputParams = new();

        public readonly Bindable<bool> RelativeMouseMode = new();

        public readonly Bindable<DisplayMode> CurrentDisplayMode = new();

        /// <remarks>Updated by events.</remarks>
        public readonly Bindable<Display> Display = new();

        /// <remarks>Updated by events.</remarks>
        public readonly Bindable<float> PixelDensity = new();

        /// <remarks>Updated by events.</remarks>
        public readonly Bindable<float> DisplayScale = new();

        /// <remarks>Updated by events.</remarks>
        public readonly Bindable<DisplayMode?> FullscreenMode = new();

        public readonly Bindable<string> Title = new();

        /// <remarks>Updated by events.</remarks>
        public readonly Bindable<Point> Position = new();

        /// <remarks>Updated by events.</remarks>
        public readonly Bindable<Size> Size = new();

        /// <remarks>Updated by events.</remarks>
        public readonly Bindable<Rectangle> SafeArea = new();

        public readonly Bindable<AspectRatio> AspectRatio = new();

        public readonly Bindable<MarginPadding> BordersSize = new();

        /// <remarks>Updated by events.</remarks>
        public readonly Bindable<Size> SizeInPixels = new();

        public readonly Bindable<Size> MinimumSize = new();

        public readonly Bindable<Size> MaximumSize = new();

        public readonly Bindable<bool> Bordered = new();

        public readonly Bindable<bool> Resizable = new();

        public readonly Bindable<bool> AlwaysOnTop = new();

        /// <remarks>Updated by events.</remarks>
        public readonly Bindable<bool> Visible = new();

        /// <remarks>Updated by events.</remarks>
        public readonly Bindable<NativeState> WindowState = new();

        /// <remarks>Updated by events.</remarks>
        public readonly Bindable<bool> Fullscreen = new();

        public readonly Bindable<bool> KeyboardGrab = new();

        public readonly Bindable<bool> MouseGrab = new();

        public readonly Bindable<Rectangle?> MouseRect = new();

        public readonly Bindable<float> Opacity = new();

        public readonly Bindable<bool> Focusable = new();

        /// <remarks>Updated by events.</remarks>
        public readonly Bindable<bool> Occluded = new();

        /// <remarks>Updated by events.</remarks>
        public readonly Bindable<bool> InputFocus = new();

        /// <remarks>Updated by events.</remarks>
        public readonly Bindable<bool> MouseFocus = new();

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
        MarginPadding IReadOnlyNativeState.BordersSize => BordersSize.Value;
        Size IReadOnlyNativeState.SizeInPixels => SizeInPixels.Value;
        Size IReadOnlyNativeState.MinimumSize => MinimumSize.Value;
        Size IReadOnlyNativeState.MaximumSize => MaximumSize.Value;
        bool IReadOnlyNativeState.Bordered => Bordered.Value;
        bool IReadOnlyNativeState.Resizable => Resizable.Value;
        bool IReadOnlyNativeState.AlwaysOnTop => AlwaysOnTop.Value;
        bool IReadOnlyNativeState.Visible => Visible.Value;
        NativeState IReadOnlyNativeState.WindowState => WindowState.Value;
        bool IReadOnlyNativeState.Fullscreen => Fullscreen.Value;
        bool IReadOnlyNativeState.KeyboardGrab => KeyboardGrab.Value;
        bool IReadOnlyNativeState.MouseGrab => MouseGrab.Value;
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

        void IWriteOnlyNativeState.SetMouseGrab(bool value) => MouseGrab.Value = value;

        void IWriteOnlyNativeState.SetKeyboardGrab(bool value) => KeyboardGrab.Value = value;

        void IWriteOnlyNativeState.SetMouseRect(Rectangle? value) => MouseRect.Value = value;

        void IWriteOnlyNativeState.SetOpacity(float value) => Opacity.Value = value;

        void IWriteOnlyNativeState.SetFocusable(bool value) => Focusable.Value = value;
    }
}
