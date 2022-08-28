// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Input.States;

namespace osu.Framework.Input.Events
{
    /// <summary>
    /// An event representing a release of a keyboard key.
    /// </summary>
    public class KeyUpEvent : KeyboardEvent
    {
        public KeyUpEvent(InputState state, KeyboardKey key)
            : base(state, key)
        {
        }
    }
}
