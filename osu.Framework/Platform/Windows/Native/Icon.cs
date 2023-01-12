// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace osu.Framework.Platform.Windows.Native
{
    internal class Icon : SafeHandleZeroOrMinusOneIsInvalid
    {
        [DllImport("user32.dll")]
        private static extern IntPtr CreateIconFromResourceEx(byte[] pbIconBits, uint cbIconBits, bool fIcon, uint dwVersion, int cxDesired, int cyDesired, uint uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr hIcon);

        public readonly int Width;

        public readonly int Height;

        internal Icon(byte[] pbIconBits, uint cbIconBits, bool fIcon, uint dwVersion, int cxDesired, int cyDesired, uint uFlags)
            : base(true)
        {
            SetHandle(CreateIconFromResourceEx(pbIconBits, cbIconBits, fIcon, dwVersion, cxDesired, cyDesired, uFlags));
            Width = cxDesired;
            Height = cyDesired;
        }

        protected override bool ReleaseHandle() => DestroyIcon(handle);
    }
}
