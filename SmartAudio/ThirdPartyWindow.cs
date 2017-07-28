namespace SmartAudio
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Windows;

    internal class ThirdPartyWindow : IDisposable
    {
        private MainWindow _callerWindow;
        private const int ERROR_CLASS_ALREADY_EXISTS = 0x582;
        private bool m_disposed;
        private IntPtr m_hwnd;
        private WNDCLASS wind_class;
        private const int WM_APP = 0x8000;
        private const int WM_BF_GET = 0x87da;
        private const int WM_BF_RESPONSE = 0x87db;
        private const int WM_BF_SET = 0x87dc;
        private const int WM_EQ_REQUEST = 0x87d0;
        private const int WM_EQ_RESPONSE = 0x87d1;
        private const int WM_EQ_SET = 0x87d2;
        private const int WM_SA_ERROR = 0x8834;
        private const int WM_UNINST_CLOSE = 0x8898;

        public ThirdPartyWindow(string class_name)
        {
            if ((class_name != null) && (class_name != string.Empty))
            {
                this.wind_class = new WNDCLASS();
                this.wind_class.lpszClassName = class_name;
                this.wind_class.lpfnWndProc = new WndProc(this.CustomWndProc);
                ushort num = RegisterClassW(ref this.wind_class);
                int num2 = Marshal.GetLastWin32Error();
                if ((num != 0) || (num2 == 0x582))
                {
                    this.m_hwnd = CreateWindowExW(0, class_name, string.Empty, 0, 0, 0, 0, 0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
                    num2 = Marshal.GetLastWin32Error();
                }
            }
        }

        [DllImport("user32.dll", SetLastError=true)]
        private static extern IntPtr CreateWindowExW(uint dwExStyle, [MarshalAs(UnmanagedType.LPWStr)] string lpClassName, [MarshalAs(UnmanagedType.LPWStr)] string lpWindowName, uint dwStyle, int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);
        private IntPtr CustomWndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            switch (msg)
            {
                case 0x87da:
                    this.OnBFGet(wParam, lParam);
                    break;

                case 0x87dc:
                    this.OnBFSet(wParam, lParam);
                    break;

                case 0x8898:
                    this.OnClose();
                    break;
            }
            return DefWindowProcW(hWnd, msg, wParam, lParam);
        }

        [DllImport("user32.dll", SetLastError=true)]
        private static extern IntPtr DefWindowProcW(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", SetLastError=true)]
        private static extern bool DestroyWindow(IntPtr hWnd);
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!this.m_disposed && (this.m_hwnd != IntPtr.Zero))
            {
                DestroyWindow(this.m_hwnd);
                this.m_hwnd = IntPtr.Zero;
                this.m_disposed = true;
            }
        }

        private void OnBFGet(IntPtr wParam, IntPtr lParam)
        {
            if (this._callerWindow.IsBeamFormingSupported())
            {
                int bFSetting = this._callerWindow.GetBFSetting();
                if (wParam != IntPtr.Zero)
                {
                    PostMessage(wParam, 0x87db, bFSetting, 0);
                }
            }
            else if (wParam != IntPtr.Zero)
            {
                PostMessage(wParam, 0x8834, 1, 0);
            }
        }

        private void OnBFSet(IntPtr wParam, IntPtr lParam)
        {
            if (this._callerWindow.IsBeamFormingSupported())
            {
                bool flag = this._callerWindow.SetBFSetting((int) wParam);
                if (lParam != IntPtr.Zero)
                {
                    PostMessage(lParam, 0x8834, flag ? 0 : 1, 0);
                }
            }
            else if (lParam != IntPtr.Zero)
            {
                PostMessage(lParam, 0x8834, 1, 0);
            }
        }

        private void OnClose()
        {
            Application.Current.Shutdown();
        }

        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr PostMessage(IntPtr hWnd, int msg, int wParam, int IParam);
        [DllImport("user32.dll", SetLastError=true)]
        private static extern ushort RegisterClassW([In] ref WNDCLASS lpWndClass);

        public MainWindow CallerWindow
        {
            set
            {
                this._callerWindow = value;
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        private struct WNDCLASS
        {
            public uint style;
            public ThirdPartyWindow.WndProc lpfnWndProc;
            public int cbClsExtra;
            public int cbWndExtra;
            public IntPtr hInstance;
            public IntPtr hIcon;
            public IntPtr hCursor;
            public IntPtr hbrBackground;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpszMenuName;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpszClassName;
        }

        private delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
    }
}

