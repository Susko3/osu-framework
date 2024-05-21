// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Runtime.Versioning;
using osu.Framework.Input.Handlers.Mouse;
using osu.Framework.Input.StateChanges;
using osuTK;

namespace osu.Framework.Platform.Windows
{
    /// <summary>
    /// A windows specific mouse input handler which overrides the SDL3 implementation of raw input.
    /// This is done to better handle quirks of some devices.
    /// </summary>
    [SupportedOSPlatform("windows")]
    internal class WindowsMouseHandler : MouseHandler
    {
        private WindowsWindow window = null!;

        public override bool Initialize(GameHost host)
        {
            if (!(host.Window is WindowsWindow desktopWindow))
                return false;

            window = desktopWindow;

            Enabled.BindValueChanged(e =>
            {
                if (e.NewValue)
                {
                    window.TabletMouseEvent += handleTabletMouseEvent;
                }
                else
                {
                    window.TabletMouseEvent -= handleTabletMouseEvent;
                }
            }, true);

            return base.Initialize(host);
        }

        private void handleTabletMouseEvent(Vector2 position)
        {
            AbsolutePositionReceived = true;

            if (window.RelativeMouseMode && Sensitivity.Value != 1)
            {
                var displayBounds = window.CurrentDisplayBindable.Value.Bounds;
                var displayPosition = new Vector2(displayBounds.X, displayBounds.Y);
                var windowPositionOnDisplay = new Vector2(window.Position.X, window.Position.Y) - displayPosition;

                position += windowPositionOnDisplay;

                // apply absolute sensitivity adjustment from the centre of the current display.
                Vector2 halfDisplaySize = new Vector2(displayBounds.Width, displayBounds.Height) / 2;

                position -= halfDisplaySize;
                position *= (float)Sensitivity.Value;
                position += halfDisplaySize;

                position -= windowPositionOnDisplay;
            }

            EnqueueInput(new MousePositionAbsoluteInput { Position = position });
        }

        public override void FeedbackMousePositionChange(Vector2 position, bool isSelfFeedback)
        {
            window.LastMousePosition = position;
            base.FeedbackMousePositionChange(position, isSelfFeedback);
        }
    }
}
