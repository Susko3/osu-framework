// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Input.StateChanges;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Platform.SDL2;
using osu.Framework.Statistics;
using SDL2;
using osuTK.Input;

namespace osu.Framework.Input.Handlers.Keyboard
{
    public class KeyboardHandler : InputHandler
    {
        public override string Description => "Keyboard";

        public override bool IsActive => true;

        public override bool Initialize(GameHost host)
        {
            if (!base.Initialize(host))
                return false;

            if (!(host.Window is SDL2DesktopWindow window))
                return false;

            Enabled.BindValueChanged(e =>
            {
                if (e.NewValue)
                {
                    window.KeyDown += handleKeyDown;
                    window.KeyUp += handleKeyUp;
                }
                else
                {
                    window.KeyDown -= handleKeyDown;
                    window.KeyUp -= handleKeyUp;
                }
            }, true);

            return true;
        }

        private void enqueueInput(IInput input)
        {
            PendingInputs.Enqueue(input);
            FrameStatistics.Increment(StatisticsCounterType.KeyEvents);
        }

        private void handleKeyDown(SDL.SDL_Keysym sdlKeysym)
        {
            var key = sdlKeysym.ToKeyboardKey();

            if (key.Key == Key.Unknown)
                return;

            Logger.Log($"keykey: {key}");

            enqueueInput(new KeyboardKeyInput(key, true));
        }

        private void handleKeyUp(SDL.SDL_Keysym sdlKeysym)
        {
            var key = sdlKeysym.ToKeyboardKey();

            if (key.Key == Key.Unknown)
                return;

            enqueueInput(new KeyboardKeyInput(key, false));
        }
    }
}
