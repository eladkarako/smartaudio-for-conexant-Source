namespace SmartAudio
{
    using CxHDAudioAPILib;
    using SmartAudio.Properties;
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Input;
    using System.Windows.Markup;
    using System.Windows.Media;
    using System.Windows.Threading;

    public class EndPointVolumeBar : UserControl, INotifyPropertyChanged, IComponentConnector
    {
        private CxHDAudioEndPoint _audioEndPointItem;
        private bool _contentLoaded;
        internal Image _defaultEndPoint;
        internal Image _disconnectedIcon;
        internal ImageListItem _endPoint;
        private bool _isUnpluggedEnabled = true;
        internal Grid _mainGrid;
        private CxHDMasterVolumeControl _masterVolumeControl;
        private double _maxVolume;
        private double _minVolume;
        internal ImageCheckBox _muteButton;
        private bool _showAdvancedSettings;
        private bool _updatingMuteState;
        private bool _updatingVolume;
        private ICxHDVolumeControl _volumeControl;
        internal FormattedSlider _volumeSlider;
        public double precision;

        public event MuteChangedHandler MuteChanged;

        public event SmartAudio.OnDefaultEndPointChanged OnDefaultEndPointChanged;

        public event SmartAudio.OnMasterVolumeChanged OnMasterVolumeChanged;

        public event SmartAudio.OnShowAdvancedSettings OnShowAdvancedSettings;

        public event PropertyChangedEventHandler PropertyChanged;

        public event SelectionChangedHandler SelectionChanged;

        public EndPointVolumeBar()
        {
            this.InitializeComponent();
            this._masterVolumeControl = null;
            this._volumeControl = null;
            this._volumeSlider.Minimum = 0.0;
            this._volumeSlider.Maximum = 100.0;
            this.MaxVolume = 100.0;
            this.MinVolume = 0.0;
            this._masterVolumeControl = null;
            this._volumeControl = null;
            this._volumeSlider.MouseLeave += new MouseEventHandler(this._volumeSlider_MouseLeave);
            this._defaultEndPoint.DataContext = this;
            this._muteButton.OnItemStateChanged += new ItemStateChanged(this._muteButton_OnItemStateChanged);
            base.MouseDoubleClick += new MouseButtonEventHandler(this.EndPointVolumeBar_MouseDoubleClick);
            this._volumeSlider.GotFocus += new RoutedEventHandler(this._volumeSlider_GotFocus);
            this._volumeSlider.MouseDown += new MouseButtonEventHandler(this._volumeSlider_MouseDown);
            base.MouseDown += new MouseButtonEventHandler(this.EndPointVolumeBar_MouseDown);
            this._endPoint.Focusable = true;
            this._volumeSlider.Focusable = true;
            this._muteButton.Focusable = true;
            this._updatingVolume = false;
            this._updatingMuteState = false;
            this._volumeSlider.ValueChanged += new RoutedPropertyChangedEventHandler<double>(this._volumeSlider_ValueChanged);
        }

        private void _advancedSettings_Click(object sender, RoutedEventArgs e)
        {
            if (this.OnShowAdvancedSettings != null)
            {
                this.OnShowAdvancedSettings();
            }
        }

        [DebuggerNonUserCode]
        internal Delegate _CreateDelegate(Type delegateType, string handler) => 
            Delegate.CreateDelegate(delegateType, this, handler);

        private void _masterVolumeControl_OnMuted(int bValue, string context)
        {
            if (!this._updatingMuteState)
            {
                base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new OnMuteChanged(this.OnDeviceMuteStateChanged));
            }
        }

        private void _masterVolumeControl_OnVolumeChanged(double newValue, string context)
        {
            if (!this._updatingVolume)
            {
                base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new OnVolumeChanged(this.OnDeviceVolumeChanged));
            }
        }

        private void _muteButton_OnItemStateChanged(ImageCheckBox item, bool newState)
        {
            base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new SmartAudio.OnChangeMuteState(this.OnChangeMuteState), newState);
        }

        private void _volumeSlider_GotFocus(object sender, RoutedEventArgs e)
        {
            this.Selected = true;
            if (this.SelectionChanged != null)
            {
                this.SelectionChanged(this, this.Selected);
            }
        }

        private void _volumeSlider_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Selected = true;
            if (this.SelectionChanged != null)
            {
                this.SelectionChanged(this, this.Selected);
            }
        }

        private void _volumeSlider_MouseLeave(object sender, MouseEventArgs e)
        {
            if (this.OnMasterVolumeChanged != null)
            {
                this.OnMasterVolumeChanged(this._volumeSlider.Value);
            }
        }

        private void _volumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
        }

        public double ConvertFromSliderValueToVolume(double value)
        {
            double minVolume = this.MinVolume;
            double maxVolume = this.MaxVolume;
            return (minVolume + (((maxVolume - minVolume) * value) / 100.0));
        }

        public double ConvertFromVolumeToSliderValue(double volume)
        {
            double minVolume = this.MinVolume;
            double maxVolume = this.MaxVolume;
            return ((100.0 * (volume - minVolume)) / (maxVolume - minVolume));
        }

        private void EndPointVolumeBar_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (!this._muteButton.IsMouseOver)
                {
                    Application current = Application.Current;
                    if ((this._audioEndPointItem != null) && !this._audioEndPointItem.DefaultEndPoint)
                    {
                        this._audioEndPointItem.DefaultEndPoint = true;
                        if (this.OnDefaultEndPointChanged != null)
                        {
                            this.OnDefaultEndPointChanged(this._audioEndPointItem.DefaultEndPoint);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("EndPointVolumeBar::EndPointVolumeBar_MouseDoubleClick(): Failed to get_DefaultEndPoint()", Severity.FATALERROR, exception);
            }
        }

        private void EndPointVolumeBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Selected = true;
            if (this.SelectionChanged != null)
            {
                this.SelectionChanged(this, this.Selected);
            }
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (!this._contentLoaded)
            {
                this._contentLoaded = true;
                Uri resourceLocator = new Uri("/SmartAudio;component/endpointvolumebar.xaml", UriKind.Relative);
                Application.LoadComponent(this, resourceLocator);
            }
        }

        public bool IsDefaultEndPoint()
        {
            App current = Application.Current as App;
            if ((current.AudioFactory.HostOperatingSystem == CxHostOperatingSystemType.WindowsXP) || (current.AudioFactory.HostOperatingSystem == CxHostOperatingSystemType.WindowsXP64))
            {
                return this.Selected;
            }
            return this._audioEndPointItem?.DefaultEndPoint;
        }

        public void LocalizeMuteToolTip()
        {
            bool mute = false;
            if (this._muteButton.Visibility == Visibility.Visible)
            {
                try
                {
                    mute = this._masterVolumeControl.Mute;
                }
                catch (Exception exception)
                {
                    SmartAudioLog.Log("EndPointVolumeBar::RefreshMuteState(): Failed to get_DefaultEndPoint()", Severity.FATALERROR, exception);
                }
            }
            this._muteButton.ToolTip = mute ? Resources.SA_UnMute : Resources.SA_Mute;
        }

        public void OnChangeMuteState(bool newValue)
        {
            bool selected = this._muteButton.Selected;
            try
            {
                this._updatingMuteState = true;
                this._masterVolumeControl.Mute = this._muteButton.Selected;
                selected = this._masterVolumeControl.Mute;
                this._updatingMuteState = false;
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("EndPointVolumeBar.OnChangeMuteState", Severity.FATALERROR, exception);
            }
            this._muteButton.ToolTip = selected ? Resources.SA_UnMute : Resources.SA_Mute;
        }

        public void OnChangeVolume(double newValue)
        {
            this._updatingVolume = true;
            this._masterVolumeControl.Volume = newValue;
            this._updatingVolume = false;
            this.Selected = true;
            if (this.SelectionChanged != null)
            {
                this.SelectionChanged(this, this.Selected);
            }
            if ((newValue == 0.0) && (this.OnMasterVolumeChanged != null))
            {
                this.OnMasterVolumeChanged(newValue);
            }
        }

        private void OnDeviceMuteStateChanged()
        {
            try
            {
                this.RefreshMuteState();
                App current = Application.Current as App;
                if ((current.AudioFactory.HostOperatingSystem == CxHostOperatingSystemType.WindowsXP) || (current.AudioFactory.HostOperatingSystem == CxHostOperatingSystemType.WindowsXP64))
                {
                    this.RefreshDefaultEndPoint();
                }
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("MainAudioControlPage::OnRenderDeviceMuteChanged()", Severity.FATALERROR, exception);
            }
        }

        private void OnDeviceVolumeChanged()
        {
            try
            {
                this.RefreshVolume();
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("MainAudioControlPage::OnRenderDeviceVolumeChanged()", Severity.FATALERROR, exception);
            }
        }

        public void RefreshDefaultEndPoint()
        {
            this._defaultEndPoint.Visibility = this.ShowDefault;
        }

        public void RefreshMuteState()
        {
            if (this._muteButton.Visibility != Visibility.Hidden)
            {
                bool mute = false;
                try
                {
                    mute = this._masterVolumeControl.Mute;
                }
                catch (NotImplementedException exception)
                {
                    SmartAudioLog.Log("EndPointVolumeBar::RefreshMuteState(): Mute option not implemented", Severity.INFORMATION, exception);
                    this.ShowMute = false;
                    return;
                }
                catch (Exception exception2)
                {
                    SmartAudioLog.Log("EndPointVolumeBar::RefreshMuteState(): Failed to get_DefaultEndPoint()", Severity.FATALERROR, exception2);
                    this.ShowMute = false;
                    return;
                }
                this._muteButton.Selected = mute;
                this._muteButton.ToolTip = mute ? Resources.SA_UnMute : Resources.SA_Mute;
                if (this.IsDefaultEndPoint() && (this.MuteChanged != null))
                {
                    this.MuteChanged(this.IsMuted);
                }
            }
        }

        public void RefreshVolume()
        {
            if (this._masterVolumeControl != null)
            {
                this._volumeSlider.GetBindingExpression(RangeBase.ValueProperty).UpdateTarget();
            }
        }

        public void RefreshVolumeChanged()
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs("Volume"));
            }
        }

        [DebuggerNonUserCode, EditorBrowsable(EditorBrowsableState.Never)]
        void IComponentConnector.Connect(int connectionId, object target)
        {
            switch (connectionId)
            {
                case 1:
                    this._mainGrid = (Grid) target;
                    return;

                case 2:
                    this._endPoint = (ImageListItem) target;
                    return;

                case 3:
                    this._volumeSlider = (FormattedSlider) target;
                    return;

                case 4:
                    this._muteButton = (ImageCheckBox) target;
                    return;

                case 5:
                    this._defaultEndPoint = (Image) target;
                    return;

                case 6:
                    this._disconnectedIcon = (Image) target;
                    return;
            }
            this._contentLoaded = true;
        }

        public CxHDAudioEndPoint AudioEndPointItem
        {
            get => 
                this._audioEndPointItem;
            set
            {
                this._audioEndPointItem = value;
                this.MasteVolumeControl = this._audioEndPointItem.MasterVolume as CxHDMasterVolumeControl;
                App current = Application.Current as App;
                if (current.Settings.INISettings.IsXSignEnabled)
                {
                    this._disconnectedIcon.Visibility = this.IsUnpluggedIn;
                }
                else
                {
                    this._disconnectedIcon.Visibility = Visibility.Hidden;
                }
            }
        }

        public bool IsMuted
        {
            get
            {
                try
                {
                    return this._masterVolumeControl.Mute;
                }
                catch (Exception exception)
                {
                    SmartAudioLog.Log("EndPointVolumeBar::IsMuted()", Severity.WARNING, exception);
                    return false;
                }
            }
        }

        public bool IsUnpluggedEnabled
        {
            set
            {
                this._isUnpluggedEnabled = value;
                if (!this._isUnpluggedEnabled)
                {
                    this._disconnectedIcon.Visibility = Visibility.Hidden;
                }
            }
        }

        public Visibility IsUnpluggedIn
        {
            get
            {
                if (!this._isUnpluggedEnabled)
                {
                    return Visibility.Hidden;
                }
                try
                {
                    Application current = Application.Current;
                    return (this._audioEndPointItem?.IsUnplugged ? Visibility.Visible : Visibility.Hidden);
                }
                catch (Exception exception)
                {
                    SmartAudioLog.Log("MainAudioControlPage::IsUnpluggedIn()", Severity.WARNING, exception);
                    return Visibility.Hidden;
                }
            }
        }

        private CxHDMasterVolumeControl MasteVolumeControl
        {
            get => 
                this._masterVolumeControl;
            set
            {
                this._masterVolumeControl = value;
                double minRange = 0.0;
                double maxRange = 0.0;
                uint steps = 0;
                uint stepCount = 0;
                this._masterVolumeControl.GetInternalRange(out minRange, out maxRange, out steps, out stepCount);
                this.VolumeControl = value as ICxHDVolumeControl;
                base.DataContext = value;
                this._volumeSlider.DataContext = this;
                if (this._masterVolumeControl != null)
                {
                    this._masterVolumeControl.add_OnVolumeChanged(new _ICxHDAudioVolumeEvents_OnVolumeChangedEventHandler(this._masterVolumeControl_OnVolumeChanged));
                    this._masterVolumeControl.add_OnMuted(new _ICxHDAudioVolumeEvents_OnMutedEventHandler(this._masterVolumeControl_OnMuted));
                }
                this.RefreshMuteState();
            }
        }

        public double MaxVolume
        {
            get => 
                this._maxVolume;
            set
            {
                this._maxVolume = value;
            }
        }

        public double MinVolume
        {
            get => 
                this._minVolume;
            set
            {
                this._minVolume = value;
            }
        }

        public bool Selected
        {
            get => 
                this._endPoint.Selected;
            set
            {
                this._endPoint.Selected = value;
            }
        }

        public ImageSource SelectedImage
        {
            get => 
                this._endPoint.SelectedImage;
            set
            {
                this._endPoint.SelectedImage = value;
            }
        }

        public bool ShowAdvancedSettings
        {
            get => 
                this._showAdvancedSettings;
            set
            {
                this._showAdvancedSettings = value;
            }
        }

        public Visibility ShowDefault
        {
            get
            {
                Application current = Application.Current;
                if (this._audioEndPointItem == null)
                {
                    return Visibility.Hidden;
                }
                if (!this._audioEndPointItem.DefaultEndPoint)
                {
                    return Visibility.Hidden;
                }
                return Visibility.Visible;
            }
        }

        public bool ShowMute
        {
            get
            {
                if (this._muteButton.Visibility != Visibility.Visible)
                {
                    return false;
                }
                return true;
            }
            set
            {
                this._muteButton.Visibility = value ? Visibility.Visible : Visibility.Hidden;
                if (!value)
                {
                    this._mainGrid.RowDefinitions.RemoveAt(2);
                }
            }
        }

        public ImageSource UnselectedImage
        {
            get => 
                this._endPoint.UnselectedImage;
            set
            {
                this._endPoint.UnselectedImage = value;
            }
        }

        public double Volume
        {
            get
            {
                if (this._masterVolumeControl != null)
                {
                    return this._masterVolumeControl.Volume;
                }
                if (this._volumeControl != null)
                {
                    return this.ConvertFromVolumeToSliderValue(this._volumeControl.Volume);
                }
                return 0.0;
            }
            set
            {
                if (this._masterVolumeControl != null)
                {
                    base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new SmartAudio.OnChangeVolume(this.OnChangeVolume), value);
                }
                else
                {
                    ICxHDVolumeControl control1 = this._volumeControl;
                }
            }
        }

        public ICxHDVolumeControl VolumeControl
        {
            get => 
                this._volumeControl;
            set
            {
                this._volumeControl = value;
                this._volumeSlider.DataContext = value;
                double minRange = 0.0;
                double maxRange = 0.0;
                if (this._volumeControl != null)
                {
                    this._volumeControl.GetRange(out minRange, out maxRange);
                    this.MinVolume = minRange;
                    this.MaxVolume = maxRange;
                    this._volumeSlider.Minimum = 0.0;
                    this._volumeSlider.Maximum = 100.0;
                    this.precision = this._volumeSlider.Maximum - (this._volumeSlider.Minimum / 100.0);
                    this._volumeSlider.Ticks.Clear();
                    double num3 = (this._volumeSlider.Maximum - this._volumeSlider.Minimum) / 5.0;
                    for (int i = 0; i < 5; i++)
                    {
                        this._volumeSlider.Ticks.Add(num3 * i);
                    }
                }
            }
        }

        public Slider VolumeSlider =>
            this._volumeSlider;
    }
}

