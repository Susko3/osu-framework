// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Extensions;

namespace osu.Framework.Input.Bindings
{
    public class InputKeyComparer : EqualityComparer<InputKey>
    {
        public override bool Equals(InputKey left, InputKey right)
        {
            left.Decode(out var leftKey, out char leftChar);
            right.Decode(out var rightKey, out char rightChar);

            if (leftKey == rightKey && leftKey != InputKey.Any) // two any keys shouldn't match here.
                return true;

            if (leftKey == InputKey.Any || rightKey == InputKey.Any
                && leftChar == rightChar)
                return true;

            return false;
        }

        public override int GetHashCode(InputKey obj) => obj.GetHashCode();
    }
}
