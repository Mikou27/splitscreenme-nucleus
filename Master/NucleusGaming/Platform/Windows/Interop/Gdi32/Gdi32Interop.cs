﻿using System;
using System.Runtime.InteropServices;

namespace Nucleus.Gaming.Windows.Interop
{
    public static class Gdi32Interop
    {
        [DllImport("gdi32.dll")]
        public static extern int GetDeviceCaps(IntPtr hdc, int nIndex);
    }
}
