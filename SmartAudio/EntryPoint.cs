namespace SmartAudio
{
    using System;
    using System.Windows;

    public class EntryPoint
    {
        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                SingleInstanceManager manager = new SingleInstanceManager();
                if ((args == null) || (args.Length == 0))
                {
                    manager.IsRunningInBackground = false;
                    manager.IsRunningSliently = false;
                }
                else if (args[0].Equals("/t"))
                {
                    manager.IsRunningInBackground = true;
                    manager.IsRunningSliently = false;
                }
                else if (args[0].Equals("/c"))
                {
                    manager.IsRunningInBackground = false;
                    manager.IsRunningSliently = true;
                }
                else if (args[0].ToLower().Equals("/demo"))
                {
                    manager.IsDemoMode = true;
                }
                else
                {
                    manager.IsRunningInBackground = false;
                    manager.IsRunningSliently = false;
                }
                manager.Run(args);
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("Entry point", Severity.FATALERROR, exception);
                if (Application.Current != null)
                {
                    int exitCode = -1;
                    Application.Current.Shutdown(exitCode);
                }
            }
        }
    }
}

