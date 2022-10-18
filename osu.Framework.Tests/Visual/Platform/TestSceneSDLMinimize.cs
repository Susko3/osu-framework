// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Drawing;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Platform;
using SDL2;

namespace osu.Framework.Tests.Visual.Platform
{
    [Ignore("This test cannot run in headless mode (a window instance is required).")]
    public class TestSceneSDLMinimize : FrameworkTestScene
    {
        private SDL2DesktopWindow window = null!;

        [BackgroundDependencyLoader]
        private void load(GameHost host)
        {
            // so the test doesn't switch to borderless on startup.
            AddStep("nothing", () => { });

            if (host.Window is not SDL2DesktopWindow sdlWindow)
            {
                Child = new SpriteText
                {
                    Text = "Only supported on desktop SDL platforms",
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                };
                return;
            }

            window = sdlWindow;

            Child = new WindowDisplaysPreview { RelativeSizeAxes = Axes.Both };
        }

        [Test]
        public void TestMinimizeFromBorderless()
        {
            Display display = default!;
            Size clientSize = default;
            Point position = default;

            AddStep("set to borderless", () => window.WindowMode.Value = WindowMode.Borderless);
            AddStep("store size and position", () =>
            {
                display = window.CurrentDisplayBindable.Value;
                clientSize = window.ClientSize;
                position = window.Position;
            });
            AddStep("minimize window", () => SDL.SDL_MinimizeWindow(window.SDLWindowHandle));
            AddStep("restore and focus window", () =>
            {
                SDL.SDL_RestoreWindow(window.SDLWindowHandle);
                SDL.SDL_RaiseWindow(window.SDLWindowHandle);
            });
            AddAssert("display matches", () => window.CurrentDisplayBindable.Value, () => Is.EqualTo(display));
            AddAssert("client size matches", () => window.ClientSize, () => Is.EqualTo(clientSize));
            AddAssert("position matches", () => window.Position, () => Is.EqualTo(position));
        }
    }
}
