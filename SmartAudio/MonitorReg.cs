namespace SmartAudio
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    public class MonitorReg : IDisposable
    {
        private bool _disposed;
        private ManualResetEvent _eventTerminate = new ManualResetEvent(false);
        private RegChangeNotifyFilter _regFilter = (RegChangeNotifyFilter.Security | RegChangeNotifyFilter.Value | RegChangeNotifyFilter.Attribute | RegChangeNotifyFilter.Key);
        private IntPtr _registryRoot;
        private string _registrySubKey;
        private Thread _thread;
        private object _threadLock = new object();
        private const long ERROR_SUCCESS = 0L;
        private static readonly IntPtr HKEY_CURRENT_USER = new IntPtr(-2147483647);
        private static readonly IntPtr HKEY_LOCAL_MACHINE = new IntPtr(-2147483646);
        private const int KEY_NOTIFY = 0x10;
        private const int KEY_QUERY_VALUE = 1;
        private const int KEY_WOW64_64KEY = 0x100;
        private const int STANDARD_RIGHTS_READ = 0x20000;

        public event EventHandler RegChanged;

        public void Dispose()
        {
            this.StopMonitoring();
            this._disposed = true;
            GC.SuppressFinalize(this);
        }

        private void MonitorThread()
        {
            try
            {
                this.ThreadLoop();
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("MonitorReg::MonitorThread() - Exception occured", Severity.FATALERROR, exception);
            }
            this._thread = null;
        }

        protected virtual void OnRegChanged()
        {
            EventHandler regChanged = this.RegChanged;
            if (regChanged != null)
            {
                regChanged(this, null);
            }
        }

        [DllImport("advapi32.dll", SetLastError=true)]
        private static extern int RegCloseKey(IntPtr hKey);
        [DllImport("advapi32.dll", SetLastError=true)]
        private static extern int RegNotifyChangeKeyValue(IntPtr hKey, bool bWatchSubtree, RegChangeNotifyFilter dwNotifyFilter, IntPtr hEvent, bool fAsynchronous);
        [DllImport("advapi32.dll", SetLastError=true)]
        private static extern int RegOpenKeyEx(IntPtr hKey, string subKey, uint options, int samDesired, out IntPtr phkResult);
        public void StartMonitoring()
        {
            if (this._disposed)
            {
                SmartAudioLog.Log("MonitorReg::StartMonitoring() - Registry Monitoring thread already disposed", new object[] { Severity.FATALERROR });
                throw new ObjectDisposedException(null, "Registry Monitoring thread already disposed");
            }
            lock (this._threadLock)
            {
                if (!this.IsMonitoring)
                {
                    this._eventTerminate.Reset();
                    this._thread = new Thread(new ThreadStart(this.MonitorThread));
                    this._thread.IsBackground = true;
                    this._thread.Start();
                }
            }
        }

        public void StopMonitoring()
        {
            if (this._disposed)
            {
                SmartAudioLog.Log("MonitorReg::StopMonitoring() - Registry Monitoring thread already disposed", new object[] { Severity.FATALERROR });
                throw new ObjectDisposedException(null, "Registry Monitoring thread already disposed");
            }
            lock (this._threadLock)
            {
                Thread thread = this._thread;
                if (thread != null)
                {
                    this._eventTerminate.Set();
                    thread.Join();
                }
            }
        }

        private void ThreadLoop()
        {
            IntPtr ptr;
            this._registryRoot = HKEY_LOCAL_MACHINE;
            this._registrySubKey = @"SOFTWARE\Conexant\SAII\CommandLineApp";
            long num = RegOpenKeyEx(this._registryRoot, this._registrySubKey, 0, 0x20011, out ptr);
            if (num != 0L)
            {
                num = RegOpenKeyEx(this._registryRoot, this._registrySubKey, 0, 0x20111, out ptr);
            }
            if (num == 0L)
            {
                try
                {
                    AutoResetEvent event2 = new AutoResetEvent(false);
                    WaitHandle[] waitHandles = new WaitHandle[] { event2, this._eventTerminate };
                    while (!this._eventTerminate.WaitOne(0, true))
                    {
                        num = RegNotifyChangeKeyValue(ptr, true, this._regFilter, event2.Handle, true);
                        if (num != 0L)
                        {
                            throw new Win32Exception("Exception returned from RegNotifyChangeKeyValue");
                        }
                        if (WaitHandle.WaitAny(waitHandles) == 0)
                        {
                            this.OnRegChanged();
                        }
                    }
                }
                finally
                {
                    if (ptr != IntPtr.Zero)
                    {
                        RegCloseKey(ptr);
                    }
                }
            }
            if (num != 0L)
            {
                throw new Win32Exception("Exception in starting the gistry monitor thread");
            }
        }

        public bool IsMonitoring =>
            (this._thread != null);

        public enum RegChangeNotifyFilter
        {
            Attribute = 2,
            Key = 1,
            Security = 8,
            Value = 4
        }
    }
}

