// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK.Input;

namespace osu.Framework.Input.StateChanges
{
    /// <summary>
    /// Denotes a pen touching or lifting from the tablet surface.
    /// </summary>
    /// <remarks>This currently emulates a left mouse press or release.</remarks>
    public class TabletPenTouchInput : MouseButtonInput
    {
        public TabletPenTouchInput(bool isPressed)
            : base(MouseButton.Left, isPressed)
        {
        }
    }
}
