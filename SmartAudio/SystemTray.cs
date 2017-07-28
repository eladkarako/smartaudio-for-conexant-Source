namespace SmartAudio
{
    using CxHDAudioAPILib;
    using SmartAudio.Properties;
    using System;
    using System.Windows;
    using System.Windows.Forms;

    public class SystemTray
    {
        private CxHDAudioFactoryClass _audioFactory;
        private bool _IsWaiting;
        private MainWindow _mainWindow;
        private SmartAudioTrayWindow _systemTrayWindow;
        private NotifyIcon _trayIcon;

        public SystemTray(MainWindow mainWindow, CxHDAudioFactoryClass audioFactory)
        {
            App current = System.Windows.Application.Current as App;
            this._mainWindow = mainWindow;
            this._audioFactory = audioFactory;
            this._audioFactory.add_OnPowerManagementStateChanged(new _ICxHDAudioFactoryEvents_OnPowerManagementStateChangedEventHandler(this._audioFactory_OnPowerManagementStateChanged));
            System.Windows.Application.Current.Exit += new ExitEventHandler(this.Exit_Click);
            current.IsResumeFromS3_S4 = false;
        }

        private void _audioFactory_OnPowerManagementStateChanged(CxPowerManagementState newPowerState)
        {
            App current = System.Windows.Application.Current as App;
            switch (newPowerState)
            {
                case CxPowerManagementState.Suspend:
                    current.JackRetaskingPopup.SaveJackConfiguration();
                    current.IsJackConfigurationChangedS3_S4 = true;
                    break;

                case CxPowerManagementState.ResumeSuspend:
                case CxPowerManagementState.ResumeAutomatic:
                case CxPowerManagementState.ResumeCritical:
                    current.JackRetaskingPopup.UpdateJackConfiguration();
                    break;
            }
            if ((this._audioFactory.HostOperatingSystem == CxHostOperatingSystemType.WindowsXP64) || (this._audioFactory.HostOperatingSystem == CxHostOperatingSystemType.WindowsXP))
            {
                switch (newPowerState)
                {
                    case CxPowerManagementState.Suspend:
                    case CxPowerManagementState.ResumeSuspend:
                    case CxPowerManagementState.ResumeAutomatic:
                    case CxPowerManagementState.ResumeCritical:
                        current.IsResumeFromS3_S4 = true;
                        break;

                    default:
                        return;
                }
            }
        }

        public void AddTrayIcon()
        {
            if (this._trayIcon != null)
            {
                this._trayIcon.Visible = true;
            }
        }

        public void CloseTrayIcon()
        {
            if (this._trayIcon != null)
            {
                this._trayIcon.Visible = false;
                this._trayIcon.Dispose();
                this._trayIcon = null;
            }
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            System.Windows.Application.Current.Exit -= new ExitEventHandler(this.Exit_Click);
            this.CloseTrayIcon();
            System.Windows.Application.Current.Shutdown();
        }

        private void Open_Click(object sender, EventArgs e)
        {
            SmartAudioLog.Log("SystemTray : Click to Open detected", new object[] { Severity.INFORMATION });
            this.ShowMainWindow();
        }

        public void RefreshContextMenu()
        {
            ContextMenu menu = new ContextMenu();
            MenuItem item = new MenuItem(Resources.SA_Open);
            item.Click += new EventHandler(this.Open_Click);
            menu.MenuItems.Add(item);
            item = new MenuItem(Resources.SA_Exit);
            item.Click += new EventHandler(this.Exit_Click);
            menu.MenuItems.Add(item);
            if (this._trayIcon != null)
            {
                this._trayIcon.ContextMenu = menu;
                this._trayIcon.Text = Resources.SA_XAML_SmartAudioII;
            }
        }

        public void Run()
        {
            if ((this._trayIcon == null) || !this._trayIcon.Visible)
            {
                this._trayIcon = new NotifyIcon();
                this._trayIcon.Icon = Resources.SAII;
                this._trayIcon.Visible = true;
                this._trayIcon.Text = Resources.SA_XAML_SmartAudioII;
                this._trayIcon.DoubleClick += new EventHandler(this.TrayIconDoubleClik);
                this._trayIcon.Click += new EventHandler(this.TrayIconClik);
                this.RefreshContextMenu();
                this._systemTrayWindow = new SmartAudioTrayWindow();
                this._systemTrayWindow.Visibility = Visibility.Hidden;
            }
        }

        public void ShowMainWindow()
        {
            App current = System.Windows.Application.Current as App;
            if (current.IsResumeFromS3_S4)
            {
                SmartAudioLog.Log("SystemTray::ShowMainWindow - IsResumeFromS3_S4 = true", new object[] { Severity.INFORMATION });
                if (this._IsWaiting)
                {
                    return;
                }
                this._IsWaiting = true;
                ((MainWindow) current.MainWindow).SaveActivePage();
                double left = ((MainWindow) current.MainWindow).Left;
                double top = ((MainWindow) current.MainWindow).Top;
                ((MainWindow) current.MainWindow).Close();
                current.CreatMainWindow();
                this._mainWindow = (MainWindow) current.MainWindow;
                this._mainWindow.Left = left;
                this._mainWindow.Top = top;
                SmartAudioLog.Log("SystemTray::Applying MSWorkaround from ShowMainWindow in case of IsResumeFromS3_S4", new object[] { Severity.INFORMATION });
                Workaround110052078705416.InvokeWorkaround110052078705416(this._mainWindow);
                ((MainWindow) current.MainWindow).ShowLastActivePage();
                this._mainWindow.Visibility = Visibility.Visible;
                current.IsResumeFromS3_S4 = false;
                this._IsWaiting = false;
            }
            else
            {
                SmartAudioLog.Log("SystemTray::ShowMainWindow - IsResumeFromS3_S4 = false", new object[] { Severity.INFORMATION });
                if (this._mainWindow == null)
                {
                    this._mainWindow = (MainWindow) current.MainWindow;
                }
                SmartAudioLog.Log("SystemTray::Applying MSWorkaround from ShowMainWindow", new object[] { Severity.INFORMATION });
                Workaround110052078705416.InvokeWorkaround110052078705416(this._mainWindow);
                this._mainWindow.ShowInTaskbar = true;
                this._mainWindow.Activate();
                this._mainWindow.Show();
                if (!current.Settings.INISettings.IsMinimizeMaximizeWindowDisabled)
                {
                    this._mainWindow.WindowState = WindowState.Minimized;
                }
                this._mainWindow.WindowState = WindowState.Normal;
            }
            if (current.IsRunningInBackground)
            {
                current.IsRunningInBackground = false;
            }
            else if (current.IsRunningSliently)
            {
                current.IsRunningSliently = false;
            }
            SmartAudioLog.Log("SystemTray : ShowMainWindow - About to call StartVolumeLevel from ", new object[] { Severity.INFORMATION });
            ((MainWindow) current.MainWindow).StartVolumeLevel();
        }

        private void TrayIconClik(object sender, EventArgs e)
        {
        }

        private void TrayIconDoubleClik(object sender, EventArgs args)
        {
            try
            {
                this.ShowMainWindow();
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("SystemTray::TrayIconDoubleClik()", Severity.FATALERROR, exception);
            }
        }
    }
}

