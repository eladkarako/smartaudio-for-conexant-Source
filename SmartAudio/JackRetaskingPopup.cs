namespace SmartAudio
{
    using CxHDAudioAPILib;
    using Microsoft.Win32;
    using SmartAudio.Properties;
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Markup;
    using System.Windows.Threading;

    public class JackRetaskingPopup : Window, IComponentConnector
    {
        internal Button _apply;
        internal TextBlock _applyText;
        internal Button _cancel;
        internal TextBlock _cancelText;
        internal Button _closeBtn;
        private bool _contentLoaded;
        private double _currentScaleResolution = 1.0;
        private CxHDAudioConfig _cxHDAudioconfig;
        private double _defaultHeight = 209.0;
        private double _defaultWidth = 793.0;
        internal CheckBox _dontShowPopup;
        internal Button _fullMode;
        internal TextBlock _fullModeText;
        internal Grid _grid;
        internal IORetaskingUserControl _jackReassignment;
        internal Button _OK;
        internal TextBlock _OKText;
        private double _primaryScreenHeight = SystemParameters.PrimaryScreenHeight;
        private double _primaryScreenWidth = SystemParameters.PrimaryScreenWidth;
        internal StackPanel stackPanel1;

        public JackRetaskingPopup()
        {
            this.InitializeComponent();
            base.MouseLeftButtonDown += new MouseButtonEventHandler(this.JackRetaskingPopup_MouseLeftButtonDown);
            base.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            base.Visibility = Visibility.Hidden;
            base.Topmost = true;
            this._currentScaleResolution = (Application.Current as App).AdjustWindowSize(this._grid, this, this._defaultWidth, this._defaultHeight, this._currentScaleResolution, false);
            this.Localize();
            this._jackReassignment.JackPortChanged += new JackPortChangedHandler(this._jackReassignment_JackPortChanged);
            this._jackReassignment.JackSelectionChanged += new JackPortSelectionChangedHandler(this._jackReassignment_JackSelectionChanged);
            this._jackReassignment.JackPortUpdateCompleted += new JackPortUpdateCompletedHandler(this._jackReassignment_JackPortUpdateCompleted);
            SystemEvents.DisplaySettingsChanged += new EventHandler(this.SystemEvents_DisplaySettingsChanged);
        }

        private void _apply_Click(object sender, RoutedEventArgs e)
        {
            this._jackReassignment.SetIsJackDelayed(false);
            this._jackReassignment.ApplyChange();
            this._jackReassignment.SetIsJackDelayed(true);
            this.EnableApplyButton(false);
        }

        private void _Cancel_Click(object sender, RoutedEventArgs e)
        {
            base.Visibility = Visibility.Hidden;
        }

        private void _closeBtn_Click(object sender, RoutedEventArgs e)
        {
            base.Visibility = Visibility.Hidden;
        }

        [DebuggerNonUserCode]
        internal Delegate _CreateDelegate(Type delegateType, string handler) => 
            Delegate.CreateDelegate(delegateType, this, handler);

        private void _dontShowPopup_Checked(object sender, RoutedEventArgs e)
        {
            App current = Application.Current as App;
            current.Settings.ShowJackpopup = !this._dontShowPopup.IsChecked.Value;
        }

        private void _fullMode_Click(object sender, RoutedEventArgs e)
        {
            App current = Application.Current as App;
            base.Visibility = Visibility.Hidden;
            current.MainWindow.Show();
            MainWindow.theCurrent.ShowJackPanel();
        }

        private void _jackReassignment_JackPortChanged(int jack, int selectedJack)
        {
            this.EnableApplyButton(true);
        }

        private void _jackReassignment_JackPortUpdateCompleted(int jack)
        {
            this.EnableApplyButton(!this._jackReassignment.AreJackSelectionsApplied());
            if (this._jackReassignment.AreJackSelectionsApplied())
            {
                Mouse.OverrideCursor = null;
            }
        }

        private void _jackReassignment_JackSelectionChanged(int jack, int selectedJack)
        {
            this.EnableApplyButton(!this._jackReassignment.AreJackSelectionsApplied());
        }

        private void _OK_Click(object sender, RoutedEventArgs e)
        {
            this._jackReassignment.SetIsJackDelayed(false);
            this._jackReassignment.ApplyChange();
            this._jackReassignment.SetIsJackDelayed(true);
            base.Visibility = Visibility.Hidden;
        }

        public void ClearPorts()
        {
            this._jackReassignment.SelectPort(0, false);
            this._jackReassignment.SelectPort(1, false);
            this._jackReassignment.SelectPort(2, false);
        }

        private void EnableApplyButton(bool isEnabled)
        {
            this._apply.Opacity = isEnabled ? 1.0 : 0.5;
            this._apply.IsEnabled = isEnabled;
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (!this._contentLoaded)
            {
                this._contentLoaded = true;
                Uri resourceLocator = new Uri("/SmartAudio;component/jackretaskingpopup.xaml", UriKind.Relative);
                Application.LoadComponent(this, resourceLocator);
            }
        }

        private void JackRetaskingPopup_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            base.DragMove();
        }

        public void Localize()
        {
            this._jackReassignment.Localize();
            base.Title = Resources.SA_SmartAudio;
            this._fullModeText.Text = Resources.SA_FullMode;
            this._dontShowPopup.Content = Resources.SA_JPMessage;
            this._OKText.Text = Resources.SA_OK;
            this._cancelText.Text = Resources.SA_CANCEL;
            this._applyText.Text = Resources.SA_Apply;
        }

        public void OnJackPopup(int port)
        {
            App current = Application.Current as App;
            bool flag = false;
            if (current.IsJackConfigurationChangedS3_S4)
            {
                current.IsJackConfigurationChangedS3_S4 = false;
                flag = this._jackReassignment.IsJackConfigurationChanged();
            }
            else
            {
                flag = true;
            }
            if ((current.Settings.INISettings.IsJackRetaskingEnabled && current.Settings.ShowJackpopup) && flag)
            {
                this.EnableApplyButton(false);
                this._jackReassignment.OnJackAssignmentChanged(0);
                this._jackReassignment.OnJackAssignmentChanged(1);
                this._jackReassignment.OnJackAssignmentChanged(2);
                this.ClearPorts();
                base.Visibility = Visibility.Visible;
                this._jackReassignment.SetIsJackDelayed(true);
                base.Activate();
                this._jackReassignment.SelectPort(port, true);
            }
        }

        public void OnJackUnplugged(int port)
        {
            base.Visibility = Visibility.Hidden;
        }

        private void PortUnplugged(int port)
        {
            base.Visibility = Visibility.Hidden;
        }

        public void ResetJack()
        {
            this._jackReassignment.ResetJacks();
        }

        public void SaveJackConfiguration()
        {
            this._jackReassignment.SaveJackConfiguration();
        }

        [DebuggerNonUserCode, EditorBrowsable(EditorBrowsableState.Never)]
        void IComponentConnector.Connect(int connectionId, object target)
        {
            switch (connectionId)
            {
                case 1:
                    this._grid = (Grid) target;
                    return;

                case 2:
                    this.stackPanel1 = (StackPanel) target;
                    return;

                case 3:
                    this._closeBtn = (Button) target;
                    this._closeBtn.Click += new RoutedEventHandler(this._closeBtn_Click);
                    return;

                case 4:
                    this._jackReassignment = (IORetaskingUserControl) target;
                    return;

                case 5:
                    this._fullMode = (Button) target;
                    this._fullMode.Click += new RoutedEventHandler(this._fullMode_Click);
                    return;

                case 6:
                    this._fullModeText = (TextBlock) target;
                    return;

                case 7:
                    this._OK = (Button) target;
                    this._OK.Click += new RoutedEventHandler(this._OK_Click);
                    return;

                case 8:
                    this._OKText = (TextBlock) target;
                    return;

                case 9:
                    this._cancel = (Button) target;
                    this._cancel.Click += new RoutedEventHandler(this._Cancel_Click);
                    return;

                case 10:
                    this._cancelText = (TextBlock) target;
                    return;

                case 11:
                    this._apply = (Button) target;
                    this._apply.Click += new RoutedEventHandler(this._apply_Click);
                    return;

                case 12:
                    this._applyText = (TextBlock) target;
                    return;

                case 13:
                    this._dontShowPopup = (CheckBox) target;
                    this._dontShowPopup.Checked += new RoutedEventHandler(this._dontShowPopup_Checked);
                    this._dontShowPopup.Unchecked += new RoutedEventHandler(this._dontShowPopup_Checked);
                    return;
            }
            this._contentLoaded = true;
        }

        private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
        {
            try
            {
                SmartAudioLog.Log("**************************************************************************: Jack Retasking Popup : Begin");
                SmartAudioLog.Log(string.Concat(new object[] { "App::AdjustWindowSize() New screen resolution Width: ", SystemParameters.PrimaryScreenWidth, " Height: ", SystemParameters.PrimaryScreenHeight }));
                double primaryScreenWidth = SystemParameters.PrimaryScreenWidth;
                double primaryScreenHeight = SystemParameters.PrimaryScreenHeight;
                if ((primaryScreenWidth != this._primaryScreenWidth) || (primaryScreenHeight != this._primaryScreenHeight))
                {
                    this._currentScaleResolution = (Application.Current as App).AdjustWindowSize(this._grid, this, this._defaultWidth, this._defaultHeight, this._currentScaleResolution, true);
                }
                this._primaryScreenWidth = SystemParameters.PrimaryScreenWidth;
                this._primaryScreenHeight = SystemParameters.PrimaryScreenHeight;
                SmartAudioLog.Log("**************************************************************************: Jack Retasking Popup : End");
            }
            catch (Exception)
            {
            }
        }

        public void UpdateJackConfiguration()
        {
            this._jackReassignment.UpdateJackConfiguration();
        }

        public void updateJackPorts(int jack, int selectedJack)
        {
            this._jackReassignment.UpdateJackPorts(jack, selectedJack);
        }

        private void value_OnPortChanged(int port, CxJackPluginStatus newStatus)
        {
            if (newStatus == CxJackPluginStatus.PluggedIN)
            {
                base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new SmartAudio.OnJackPopup(this.OnJackPopup), port);
            }
            else if (newStatus == CxJackPluginStatus.PluggedOut)
            {
                base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new SmartAudio.OnJackUnplugged(this.PortUnplugged), port);
            }
        }

        public CxHDAudioFactory AudioFactory
        {
            set
            {
                if (value != null)
                {
                    value.add_OnPortChanged(new _ICxHDAudioFactoryEvents_OnPortChangedEventHandler(this.value_OnPortChanged));
                }
            }
        }

        public CxHDAudioConfig HDAudioConfig
        {
            get => 
                this._cxHDAudioconfig;
            set
            {
                try
                {
                    this._cxHDAudioconfig = value;
                    this._jackReassignment.HDAudioConfig = value;
                    if (this._jackReassignment.IsUnSupportedLocationExisted)
                    {
                        this._jackReassignment.Margin = new Thickness(0.0, 35.0, 0.0, 0.0);
                    }
                }
                catch (Exception exception)
                {
                    SmartAudioLog.Log("IORetaskingUserControl::HDAudioConfig {set}", Severity.WARNING, exception);
                }
            }
        }

        public IORetaskingUserControl JackReassignment =>
            this._jackReassignment;
    }
}

