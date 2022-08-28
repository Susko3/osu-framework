// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Framework.Text.State
{
    public readonly struct Selection : IEquatable<Selection>
    {
        public readonly int Start;

        public readonly int End;

        public readonly string Text;

        public Selection(int start, int end, string text)
        {
            Start = start;
            End = end;
            Text = text;
        }

        public int Length => Math.Abs(End - Start);
        public int Left => Math.Min(Start, End);
        public int Right => Math.Max(Start, End);

        /// <summary>
        /// Position of the text selection caret. <c>n</c> means before the <c>n</c>-th character.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public int Caret
        {
            get
            {
                if (Start != End)
                    throw new InvalidOperationException($"{nameof(Caret)} is only valid when ");

                return Start;
            }
        }

        public bool Equals(Selection other) => Start == other.Start
                                               && End == other.End
                                               && Text == other.Text;
    }
}
