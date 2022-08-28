// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Logging;
using osu.Framework.Text.State;
using osu.Framework.Text.State.Actions;
using osu.Framework.Text.State.StateChanges;

namespace osu.Framework.Tests.Text
{
    [TestFixture]
    public class TextStateTest
    {
        private TestTextChangeHandler handler = null!;
        private EditorState state = null!;

        private Stack<EditorAction> actions = null!;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            handler = new TestTextChangeHandler();
            state = new EditorState();
            actions = new Stack<EditorAction>();
        }

        [Test]
        public void TestText()
        {
            setInitialState("some text", 2, 4);

            Logger.Log(state.ToString());

            apply(new AddTextAction("hello!"));
            apply(new AddTextAction("THERE"));
            undo();
            undo();
        }

        private const int insert_length = 6;

        [TestCase(0, 0, 0, 0)]
        [TestCase(0, 3, 0, 3 + insert_length)] // inserting at selection end expands selection.
        [TestCase(4, 3, 4 + insert_length, 3)] // inserting at selection end expands selection.
        [TestCase(3, 0, 3, 0)]
        [TestCase(2, 2, 2, 2)]
        [TestCase(3, 3, 3 + insert_length, 3 + insert_length)]
        [TestCase(4, 4, 4 + insert_length, 4 + insert_length)]
        [TestCase(2, 4, 2, 4 + insert_length)]
        [TestCase(4, 2, 4 + insert_length, 2)]
        public void TestInsert(int initialStart, int initialEnd, int expectedStart, int expectedEnd)
        {
            // setInitialState("01234", initialStart, initialEnd);
            //
            // state.Insert(3, "insert");
            //
            // Assert.That(state.Text.ToString(), Is.EqualTo("012insert34"));
            // Assert.That(state.SelectionStart, Is.EqualTo(expectedStart));
            // Assert.That(state.SelectionEnd, Is.EqualTo(expectedEnd));
        }

        private void setInitialState(string text, int selectionStart, int selectionEnd)
        {
            state.Text.Append(text);
            state.SelectionStart = selectionStart;
            state.SelectionEnd = selectionEnd;
        }

        private void apply(EditorAction action)
        {
            actions.Push(action);
            action.Apply(state, handler);
            Logger.Log(state.ToString());
        }

        private void undo()
        {
            actions.Pop().Undo(state, handler);
            Logger.Log(state.ToString());
        }

        private class TestTextChangeHandler : IEditorStateChangeHandler
        {
            public void HandleTextStateChange(EditorStateChange editorEvent)
            {
            }
        }
    }
}
