// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Text;

namespace osu.Framework.Text.State
{
    public class ImeState
    {
        public StringBuilder Text = new StringBuilder();

        public override string ToString() =>
            $@"({nameof(Text)}=""{Text}"")";
    }
}
