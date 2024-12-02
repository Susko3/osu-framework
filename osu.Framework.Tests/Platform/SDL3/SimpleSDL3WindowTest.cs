// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Drawing;
using NUnit.Framework;
using osu.Framework.Platform.SDL3.Native;
using SDL;

namespace osu.Framework.Tests.Platform.SDL3
{
    [TestFixture]
    public class SimpleSDL3WindowTests : SDLWindowTest
    {
        [Test]
        public void TestMaximiseMinimiseNormal()
        {
            InitialState.SetResizable(true);
            InitialState.SetWindowState(NativeState.Maximised);
            Window.Create(0);
            Window.PrepareForRun();
            Assert.That(State.WindowState, Is.EqualTo(NativeState.Maximised));

            PollEvents(Window.HandleEvent);
            Assert.That(State.WindowState, Is.EqualTo(NativeState.Maximised));

            NewState.SetWindowState(NativeState.Minimised);
            PollEventsExpecting(SDL_EventType.SDL_EVENT_WINDOW_MINIMIZED);
            Assert.That(State.WindowState, Is.EqualTo(NativeState.Minimised));

            NewState.SetWindowState(NativeState.Restored);
            PollEventsExpecting(SDL_EventType.SDL_EVENT_WINDOW_RESTORED);
            Assert.That(State.WindowState, Is.EqualTo(NativeState.Restored));
        }

        public static TestCaseData Create<T>(string property, T value, Action<IWriteOnlyNativeState>? preconditions = null)
            => new TestCaseData(new Helper<T>($"Set{property}", property, value, preconditions));

        public static TestCaseData[] SetAndGetCases =
        [
            Create<TextInputParams?>(nameof(IReadOnlyNativeState.TextInputParams), null),
            Create<TextInputParams?>(nameof(IReadOnlyNativeState.TextInputParams),
                new TextInputParams(SDL_TextInputType.SDL_TEXTINPUT_TYPE_TEXT_EMAIL, SDL_Capitalization.SDL_CAPITALIZE_LETTERS, true, true, new Rectangle(10, 20, 50, 60), 6)),
            Create(nameof(IReadOnlyNativeState.RelativeMouseMode), true),
            Create(nameof(IReadOnlyNativeState.RelativeMouseMode), false),
            Create(nameof(IReadOnlyNativeState.Title), "Test!"),
            Create(nameof(IReadOnlyNativeState.Position), new Point(100, 200)),
            Create(nameof(IReadOnlyNativeState.Size), new Size(100, 200)),
            Create(nameof(IReadOnlyNativeState.AspectRatio), new AspectRatio(null, 2.0f)),
            Create(nameof(IReadOnlyNativeState.MinimumSize), new Size(800, 0)),
            Create(nameof(IReadOnlyNativeState.MaximumSize), new Size(0, 300)),
            Create(nameof(IReadOnlyNativeState.Bordered), true),
            Create(nameof(IReadOnlyNativeState.Bordered), false),
            Create(nameof(IReadOnlyNativeState.Resizable), true),
            Create(nameof(IReadOnlyNativeState.Resizable), false),
            Create(nameof(IReadOnlyNativeState.AlwaysOnTop), true),
            Create(nameof(IReadOnlyNativeState.AlwaysOnTop), false),
            Create(nameof(IReadOnlyNativeState.Visible), true),
            Create(nameof(IReadOnlyNativeState.Visible), false),
            Create(nameof(IReadOnlyNativeState.WindowState), NativeState.Restored),
            Create(nameof(IReadOnlyNativeState.WindowState), NativeState.Minimised),
            Create(nameof(IReadOnlyNativeState.WindowState), NativeState.Maximised, newState => newState.SetResizable(true)),
            Create(nameof(IReadOnlyNativeState.Fullscreen), true),
            Create(nameof(IReadOnlyNativeState.Fullscreen), false),
            Create<Rectangle?>(nameof(IReadOnlyNativeState.MouseRect), null),
            Create<Rectangle?>(nameof(IReadOnlyNativeState.MouseRect), new Rectangle(10, 10, 20, 30)),
            Create(nameof(IReadOnlyNativeState.Opacity), 1.0f),
            Create(nameof(IReadOnlyNativeState.Opacity), 0.7f),
            Create(nameof(IReadOnlyNativeState.Opacity), 0.0f),
            Create(nameof(IReadOnlyNativeState.Focusable), true),
            Create(nameof(IReadOnlyNativeState.Focusable), false),
        ];

        [TestCaseSource(nameof(SetAndGetCases))]
        public void TestSetAndGetInitial<T>(Helper<T> helper)
        {
            helper.Set(InitialState, helper.Value);

            Window.Create(0);
            Window.PrepareForRun();

            PollEvents(Window.HandleEvent);
            Assert.That(helper.Get(State), Is.EqualTo(helper.Value));
        }

        [TestCaseSource(nameof(SetAndGetCases))]
        public void TestSetAndGetAfterCreation<T>(Helper<T> helper)
        {
            Window.Create(0);
            helper.Set(NewState, helper.Value);
            Window.PrepareForRun();

            PollEvents(Window.HandleEvent);
            Assert.That(helper.Get(State), Is.EqualTo(helper.Value));
        }

        [TestCaseSource(nameof(SetAndGetCases))]
        public void TestSetAndGetAfterPrepare<T>(Helper<T> helper)
        {
            Window.Create(0);
            Window.PrepareForRun();
            helper.Set(NewState, helper.Value);

            PollEvents(Window.HandleEvent);
            Assert.That(helper.Get(State), Is.EqualTo(helper.Value));
        }

        [TestCaseSource(nameof(SetAndGetCases))]
        public void TestSetAndGetAfterFirstPoll<T>(Helper<T> helper)
        {
            Window.Create(0);
            Window.PrepareForRun();

            PollEvents(Window.HandleEvent);
            helper.Set(NewState, helper.Value);

            PollEvents(Window.HandleEvent);
            Assert.That(helper.Get(State), Is.EqualTo(helper.Value));
        }
    }

    public class Helper<T>(string setter, string getter, T value, Action<IWriteOnlyNativeState>? preconditions)
    {
        public T Value = value;

        public void Set(IWriteOnlyNativeState newState, T value)
        {
            preconditions?.Invoke(newState);
            typeof(IWriteOnlyNativeState).GetMethod(setter)!.Invoke(newState, [value]);
        }

        public T Get(IReadOnlyNativeState state)
        {
            return (T)typeof(IReadOnlyNativeState).GetProperty(getter)!.GetValue(state)!;
        }

        public override string ToString() => $"{getter}, {(Value != null ? Value : "<null>")}";
    }
}
