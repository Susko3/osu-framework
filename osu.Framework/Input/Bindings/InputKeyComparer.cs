// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Extensions;
using osu.Framework.Logging;

namespace osu.Framework.Input.Bindings
{
    public class InputKeyComparer : EqualityComparer<InputKey>
    {
        public override bool Equals(InputKey left, InputKey right)
        {
            // Logger.Log("-------", level: LogLevel.Debug);

            left.Decode(out var leftKey, out char leftChar);
            right.Decode(out var rightKey, out char rightChar);
            //
            // if (leftKey == InputKey.None)
            //     // Logger.Log("left key is none", level: LogLevel.Debug);
            //
            // if (rightKey == InputKey.None)
            //     // Logger.Log("right key is none", level: LogLevel.Debug);

            if (leftKey == rightKey)
            {
                // Logger.Log("rl key same", level: LogLevel.Debug);
                return true;
            }

            if (leftKey == InputKey.Any || rightKey == InputKey.Any
                && leftChar == rightChar)
            {
                // Logger.Log("any key and lr char same", level: LogLevel.Debug);
                return true;
            }

            // Logger.Log("false", level: LogLevel.Debug);
            return false;
        }

        public override int GetHashCode(InputKey obj) => obj.GetHashCode();
    }
}
