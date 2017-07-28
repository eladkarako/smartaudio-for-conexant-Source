namespace SmartAudio
{
    using System;
    using System.Collections.Generic;

    internal static class NativeHelpers
    {
        public static List<IntPtr> GetThreadWindows(uint threadID)
        {
            GetThreadWindowsHelperClass class2 = new GetThreadWindowsHelperClass();
            return class2.GetThreadWindows(threadID);
        }

        private class GetThreadWindowsHelperClass
        {
            private List<IntPtr> hwnds = new List<IntPtr>();

            public List<IntPtr> GetThreadWindows(uint threadID)
            {
                this.hwnds.Clear();
                NativeMethods.EnumThreadWindows(threadID, new NativeMethods.EnumThreadWndProc(this.myEnumThreadWndProc), IntPtr.Zero);
                return new List<IntPtr>(this.hwnds);
            }

            private bool myEnumThreadWndProc(IntPtr hwnd, IntPtr lParam)
            {
                if (this.hwnds.Contains(hwnd))
                {
                    return false;
                }
                this.hwnds.Add(hwnd);
                return true;
            }
        }
    }
}

