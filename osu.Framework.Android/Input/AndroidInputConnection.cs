// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using System.Threading;
using Android.Views;
using Android.Views.InputMethods;
using Java.Lang;

namespace osu.Framework.Android.Input
{
    internal class AndroidInputConnection : BaseInputConnection
    {
        private readonly AndroidGameView targetView;

        private volatile int batchNestingLevel;

        public AndroidInputConnection(AndroidGameView targetView)
            : base(targetView, fullEditor: true)
        {
            this.targetView = targetView;
        }

        public override bool BeginBatchEdit()
        {
            Interlocked.Increment(ref batchNestingLevel);
            return true;
        }

        public override bool EndBatchEdit()
        {
            Debug.Assert(batchNestingLevel > 0, $"{nameof(EndBatchEdit)} called without preceding {nameof(BeginBatchEdit)} call");

            int newValue = Interlocked.Decrement(ref batchNestingLevel);

            // only check for composition after all nested editing batches have ended
            if (newValue == 0)
                invokeComposingEventIfComposingActive();

            return newValue > 0;
        }

        /// <summary>
        /// Invokes <see cref="AndroidGameView.ComposingText"/> if there is an active composition.
        /// </summary>
        private void invokeComposingEventIfComposingActive()
        {
            var editable = Editable;
            if (editable == null)
                return;

            int start = GetComposingSpanStart(editable);
            int end = GetComposingSpanEnd(editable);

            if (start == -1 || end == -1)
                return;

            int left = Math.Min(start, end);
            int right = Math.Max(start, end);
            string composition = editable.SubSequence(left, right);

            // TODO: get selection within the composition
            // for now, let's just have the selection cursor at the end of the composition string
            targetView.OnComposingText(composition, composition.Length, 0);
        }

        public override bool CommitText(ICharSequence? text, int newCursorPosition)
        {
            if (text?.Length() > 0)
            {
                targetView.OnCommitText(text.ToString());
            }

            return base.CommitText(text, newCursorPosition);
        }

        public override bool SendKeyEvent(KeyEvent? e)
        {
            if (e == null)
                return base.SendKeyEvent(e);

            switch (e.Action)
            {
                case KeyEventActions.Down:
                    targetView.OnKeyDown(e.KeyCode, e);
                    return true;

                case KeyEventActions.Up:
                    targetView.OnKeyUp(e.KeyCode, e);
                    return true;

                case KeyEventActions.Multiple:
                    targetView.OnKeyDown(e.KeyCode, e);
                    targetView.OnKeyUp(e.KeyCode, e);
                    return true;
            }

            return base.SendKeyEvent(e);
        }

        public override bool DeleteSurroundingText(int beforeLength, int afterLength)
        {
            for (int i = 0; i < beforeLength; i++)
            {
                KeyEvent ed = new KeyEvent(KeyEventActions.Multiple, Keycode.Del);
                SendKeyEvent(ed);
            }

            return base.DeleteSurroundingText(beforeLength, afterLength);
        }
    }
}
