namespace SmartAudio
{
    using Microsoft.Win32;
    using System;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Windows;
    using System.Windows.Threading;

    internal static class Workaround110052078705416
    {
        private static DispatcherTimer timer = new DispatcherTimer();

        static Workaround110052078705416()
        {
            timer.Interval = new TimeSpan(0, 0, 5);
            timer.Tick += new EventHandler(Workaround110052078705416.timer_Tick);
        }

        public static void ApplyWorkaround110052078705416(Window window)
        {
            window.StateChanged += new EventHandler(Workaround110052078705416.MainWindow_StateChanged);
            SystemEvents.DisplaySettingsChanged += new EventHandler(Workaround110052078705416.displaySettingsChanged);
            window.Closed += new EventHandler(Workaround110052078705416.window_Closed);
        }

        private static void displaySettingsChanged(object sender, EventArgs e)
        {
            Window window = sender as Window;
            if (window != null)
            {
                InvokeWorkaround110052078705416(window);
            }
        }

        public static void InvokeWorkaround110052078705416(Window window)
        {
            window.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new WorkaroundDelegate(Workaround110052078705416.Workaround));
            timer.Stop();
            timer.Start();
        }

        private static void MainWindow_StateChanged(object sender, EventArgs e)
        {
            Window window = sender as Window;
            if (window != null)
            {
                InvokeWorkaround110052078705416(window);
            }
        }

        private static void timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();
            Workaround();
        }

        private static void window_Closed(object sender, EventArgs e)
        {
            SystemEvents.DisplaySettingsChanged -= new EventHandler(Workaround110052078705416.displaySettingsChanged);
        }

        private static void Workaround()
        {
            IntPtr dC = SmartAudio.NativeMethods.GetDC(IntPtr.Zero);
            int deviceCaps = SmartAudio.NativeMethods.GetDeviceCaps(dC, 8);
            int high = SmartAudio.NativeMethods.GetDeviceCaps(dC, 10);
            int num3 = SmartAudio.NativeMethods.GetDeviceCaps(dC, 12);
            SmartAudio.NativeMethods.ReleaseDC(IntPtr.Zero, dC);
            foreach (IntPtr ptr2 in NativeHelpers.GetThreadWindows(SmartAudio.NativeMethods.GetCurrentThreadId()))
            {
                StringBuilder pString = new StringBuilder(100);
                if ((SmartAudio.NativeMethods.GetWindowText(ptr2, pString, pString.Capacity) > 0) && pString.ToString().Equals("SystemResourceNotifyWindow", StringComparison.InvariantCultureIgnoreCase))
                {
                    SmartAudio.NativeMethods.PostMessage(ptr2, 0x7e, new IntPtr(num3), new IntPtr((long) SmartAudio.NativeMethods.MAKELONG(deviceCaps, high)));
                    break;
                }
            }
        }

        private delegate void WorkaroundDelegate();
    }
}

