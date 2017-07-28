namespace SmartAudio
{
    using CxHDAudioAPILib;
    using Microsoft.VisualBasic.ApplicationServices;
    using SmartAudio.Properties;
    using System;
    using System.Reflection;
    using System.Windows;

    public class SingleInstanceManager : WindowsFormsApplicationBase
    {
        private bool _isDemoMode;
        private bool _isRunningInBackground;
        private bool _isRunningSliently;
        private App app;

        public SingleInstanceManager()
        {
            base.IsSingleInstance = true;
        }

        protected override bool OnStartup(Microsoft.VisualBasic.ApplicationServices.StartupEventArgs e)
        {
            this.app = new App();
            this.app.IsRunningInBackground = this.IsRunningInBackground;
            this.app.IsRunningSliently = this.IsRunningSliently;
            this.app.IsDemoMode = this.IsDemoMode;
            if (this.app.SetLocale())
            {
                CxHDAudioFactoryClass audioFactory = null;
                try
                {
                    audioFactory = new CxHDAudioFactoryClass();
                }
                catch (Exception exception)
                {
                    MessageBox.Show(Resources.SA_AudioFactoryCreateError, "SmartAudio");
                    SmartAudioLog.Log("SingleInstanceApp.OnStartup.audioFactory Create", Severity.FATALERROR, exception);
                    return false;
                }
                try
                {
                    audioFactory.IsDemoMode = this.IsDemoMode;
                    audioFactory.InitializeSession();
                }
                catch (Exception exception2)
                {
                    MessageBox.Show(Resources.SA_FailedToInitialize, "SmartAudio");
                    SmartAudioLog.Log("SingleInstanceApp.OnStartup.audioFactory Initialize", Severity.FATALERROR, exception2);
                    return false;
                }
                if (!this.IsDemoMode)
                {
                    SmartAudioLog.Log("Driver Version = " + audioFactory.DeviceIOConfig.DriverVersion, new object[] { Severity.INFORMATION });
                    SmartAudioLog.Log("OS Version = " + audioFactory.HostOperatingSystem, new object[] { Severity.INFORMATION });
                    if (audioFactory.ApplicationCompatibility != CxApplicationCompatibility.Compatible)
                    {
                        if (audioFactory.ApplicationCompatibility == CxApplicationCompatibility.RunningInTerminalSession)
                        {
                            MessageBox.Show(Resources.SA_TerminalServicesError, "SmartAudio");
                            return false;
                        }
                        if (audioFactory.ApplicationCompatibility == CxApplicationCompatibility.NoConexantDevicesFound)
                        {
                            MessageBox.Show(Resources.SA_CnxtAudioDeviceNotFound, "SmartAudio");
                            return false;
                        }
                    }
                }
                if (!this.app.InitApplication(audioFactory))
                {
                    SmartAudioLog.Log("SingleInstanceApp.OnStartup: Call to InitApplication failed", new object[] { Severity.INFORMATION });
                    return false;
                }
                AssemblyName name = Assembly.GetExecutingAssembly().GetName(false);
                if (name != null)
                {
                    SmartAudioLog.Log("SmartAudio Version = " + name.Version.ToString(), new object[] { Severity.INFORMATION });
                }
                SmartAudioLog.Log("CxHDAudioAPI Version = " + this.app.HDAudioAPIVersion, new object[] { Severity.INFORMATION });
                if (!this.IsDemoMode)
                {
                    SmartAudioLog.Log("Driver Version = " + audioFactory.DeviceIOConfig.DriverVersion, new object[] { Severity.INFORMATION });
                    SmartAudioLog.Log("OS Version = " + audioFactory.HostOperatingSystem, new object[] { Severity.INFORMATION });
                }
                this.app.Run();
            }
            return false;
        }

        protected override void OnStartupNextInstance(StartupNextInstanceEventArgs eventArgs)
        {
            base.OnStartupNextInstance(eventArgs);
            if (this.app != null)
            {
                this.app.Activate();
            }
        }

        public bool IsDemoMode
        {
            get => 
                this._isDemoMode;
            set
            {
                this._isDemoMode = value;
            }
        }

        public bool IsRunningInBackground
        {
            get => 
                this._isRunningInBackground;
            set
            {
                this._isRunningInBackground = value;
            }
        }

        public bool IsRunningSliently
        {
            get => 
                this._isRunningSliently;
            set
            {
                this._isRunningSliently = value;
            }
        }
    }
}

