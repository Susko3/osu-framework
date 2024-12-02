// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Platform;
using osu.Framework.Platform.Windows;

namespace osu.Framework.Tests
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            // myMain();
            // return;

            bool benchmark = args.Contains(@"--benchmark");
            bool portable = args.Contains(@"--portable");

            using (GameHost host = Host.GetSuitableDesktopHost(@"visual-tests", new HostOptions { PortableInstallation = portable }))
            {
                if (benchmark)
                    host.Run(new AutomatedVisualTestGame());
                else
                    host.Run(new VisualTestGame());
            }
        }

        private static void myMain()
        {
            var window = new NewSDL3WindowsWindow(GraphicsSurfaceType.Direct3D11, "tests");

            window.SetupConfig();

        }
    }
}
