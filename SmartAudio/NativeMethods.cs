namespace SmartAudio
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;

    internal static class NativeMethods
    {
        public const int BITSPIXEL = 12;
        public const int HORZRES = 8;
        public const int VERTRES = 10;
        public const int WM_DISPLAYCHANGE = 0x7e;

        [DllImport("User32.dll", ExactSpelling=true)]
        public static extern bool EnumThreadWindows(uint dwThreadId, EnumThreadWndProc lpfn, IntPtr lParam);
        [DllImport("kernel32.dll", ExactSpelling=true)]
        public static extern uint GetCurrentThreadId();
        [DllImport("user32.dll")]
        public static extern IntPtr GetDC(IntPtr hwnd);
        [DllImport("gdi32.dll")]
        public static extern int GetDeviceCaps(IntPtr hdc, int nIndex);
        [DllImport("user32.dll")]
        public static extern int GetSystemMetrics(int iIndex);
        [DllImport("User32.dll", CharSet=CharSet.Unicode)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder pString, int nMaxCount);
        public static uint MAKELONG(int low, int high) => 
            ((uint) ((low & 0xffff) + ((high & 0xffff) << 0x10)));

        [DllImport("user32.dll")]
        public static extern IntPtr PostMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")]
        public static extern int ReleaseDC(IntPtr hwnd, IntPtr hDC);
        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam);

        public delegate bool EnumThreadWndProc(IntPtr hwnd, IntPtr lParam);
    }
}

