using System;
using System.Runtime.InteropServices;

namespace Baku.SynchroGazer
{
    //マウスクリックを完全無視するために使うユーティリティ
    internal static class Win32Api
    {
        public const int WS_EX_TOPMOST = 0x00000008;
        public const int WS_EX_TRANSPARENT = 0x00000020;
        public const int WS_EX_LAYERED = 0x00080000;
        public const int WS_EX_APPWINDOW = 0x00040000;
        public const int WS_EX_APPWINDOW_DISABLE = ~WS_EX_APPWINDOW;

        public const int GWL_EXSTYLE = (-20);

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

    }
}
