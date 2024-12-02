// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Drawing;
using osu.Framework.Configuration;
using osu.Framework.Platform.SDL3.Native;
using osu.Framework.Threading;
using osuTK;

namespace osu.Framework.Platform.SDL3
{
    internal partial class NewSDL3Window
    {
        private Scheduler pendingOperationScheduler = new Scheduler();

        private WindowMode? pendingWindowMode;
        private WindowState? pendingWindowState;
        private DisplayIndex? pendingDisplayIndex;
        private Size? pendingMinSize;
        private Size? pendingMaxSize;
        private Size? pendingWindowedSize;
        private double? pendingWindowedPositionX;
        private double? pendingWindowedPositionY;
        private Size? pendingSizeFullscreen;

        public void SetWindowMode(WindowMode value) => pendingOperationScheduler.AddOnce(v => pendingWindowMode = v, value);
        public void SetWindowState(WindowState value) => pendingOperationScheduler.AddOnce(v => pendingWindowState = v, value);
        public void SetDisplayIndex(DisplayIndex value) => pendingOperationScheduler.AddOnce(v => pendingDisplayIndex = v, value);
        public void SetMinSize(Size value) => pendingOperationScheduler.AddOnce(v => pendingMinSize = v, value);
        public void SetMaxSize(Size value) => pendingOperationScheduler.AddOnce(v => pendingMaxSize = v, value);
        public void SetWindowedSize(Size value) => pendingOperationScheduler.AddOnce(v => pendingWindowedSize = v, value);
        public void SetWindowedPositionX(double value) => pendingOperationScheduler.AddOnce(v => pendingWindowedPositionX = v, value);
        public void SetWindowedPositionY(double value) => pendingOperationScheduler.AddOnce(v => pendingWindowedPositionY = v, value);
        public void SetSizeFullscreen(Size value) => pendingOperationScheduler.AddOnce(v => pendingSizeFullscreen = v, value);

        public void RunFrame()
        {
            if (pendingOperationScheduler.Update() > 0)
            {
                updateWindowSpecifics();
            }

            CommandScheduler.Update();

            if (!Exists)
                return;

            if (pendingWindowState != null)
                updateAndFetchWindowSpecifics();

            pollSDLEvents();

            if (!cursorInWindow.Value)
                pollMouse();

            EventScheduler.Update();
            Update?.Invoke();
        }

        private void updateWindowSpecifics(IWriteOnlyNativeState newState)
        {

        }
    }
}
