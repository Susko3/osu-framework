// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using osu.Framework.Platform.SDL3;
using osu.Framework.Platform.SDL3.Native;

namespace osu.Framework.Platform.Windows
{
    internal class NewSDL3WindowsWindow : NewSDL3Window
    {
        /// <summary>
        /// Whether to apply the <see cref="windows_borderless_width_hack"/>.
        /// </summary>
        private readonly bool applyBorderlessWindowHack;

        /// <summary>
        /// Amount of extra width added to window size when in borderless mode on Windows.
        /// Some drivers require this to avoid the window switching to exclusive fullscreen automatically.
        /// </summary>
        /// <remarks>Used on <see cref="GraphicsSurfaceType.OpenGL"/> and <see cref="GraphicsSurfaceType.Vulkan"/>.</remarks>
        private const int windows_borderless_width_hack = 1;

        public NewSDL3WindowsWindow(GraphicsSurfaceType surfaceType, string appName)
            : base(surfaceType, appName)
        {
        }

        protected override void SetupNativeDependencies(NativeStateStorage nativeState)
        {
            base.SetupNativeDependencies(nativeState);
            DependsOnNative(UpdateSize, nativeState.Bordered);
        }

        protected override void SetupDerivedDependencies()
        {
            base.SetupDerivedDependencies();
            DependsOnDerived(UpdateNativeFullscreenMode, CurrentDisplayBindable);
        }

        protected override void UpdateNativeFullscreenMode(IWriteOnlyNativeState newState)
        {
            if (WindowMode.Value == Configuration.WindowMode.Borderless)
            {
                newState.SetBordered(false);
                var newSize = CurrentDisplayBindable.Value.Bounds.Size;

                if (applyBorderlessWindowHack)
                    // use the 1px hack we've always used, but only expand the width.
                    // we also trick the game into thinking the window has normal size: see Size setter override
                    newSize += new Size(windows_borderless_width_hack, 0);

                newState.SetSize(newSize);
                newState.SetPosition(CurrentDisplayBindable.Value.Bounds.Location);
                return;
            }

            base.UpdateNativeFullscreenMode(newState);
        }

        protected override Size GetClientSize(IReadOnlyNativeState state)
        {
            var size = base.GetClientSize(state);

            // trick the game into thinking the borderless window has normal size so that it doesn't render into the extra space.
            if (applyBorderlessWindowHack && !state.Bordered)
                size.Width -= windows_borderless_width_hack;

            return size;
        }

        /// <summary>
        /// On Windows, SDL will use the same image for both large and small icons (scaled as necessary).
        /// This can look bad if scaling down a large image, so we use the Windows API directly so as
        /// to get a cleaner icon set than SDL can provide.
        /// If called before the window has been created, or we do not find two separate icon sizes, we fall back to the base method.
        /// </summary>
        internal override void SetIconFromGroup(IconGroup iconGroup)
        {
            smallIcon = iconGroup.CreateIcon(small_icon_size, small_icon_size);
            largeIcon = iconGroup.CreateIcon(large_icon_size, large_icon_size);

            IntPtr windowHandle = WindowHandle;

            if (windowHandle == IntPtr.Zero || largeIcon == null || smallIcon == null)
                base.SetIconFromGroup(iconGroup);
            else
            {
                SendMessage(windowHandle, seticon_message, icon_small, smallIcon.Handle);
                SendMessage(windowHandle, seticon_message, icon_big, largeIcon.Handle);
            }
        }

        public override Point PointToClient(Point point)
        {
            ScreenToClient(WindowHandle, ref point);
            return point;
        }

        public override Point PointToScreen(Point point)
        {
            ClientToScreen(WindowHandle, ref point);
            return point;
        }

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool ScreenToClient(IntPtr hWnd, ref Point point);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool ClientToScreen(IntPtr hWnd, ref Point point);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
    }
}
