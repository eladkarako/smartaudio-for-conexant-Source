namespace SmartAudio
{
    using CxHDAudioAPILib;
    using SmartAudio.Properties;
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Markup;
    using System.Windows.Media.Imaging;
    using System.Windows.Threading;

    public class PreviewHeadphone : UserControl, IHDAudioPreview, IComponentConnector
    {
        private CxHDAudioChannelEnumeratorClass _audioChannelEnumerator;
        internal Slider _balance;
        private ChannelBalance _channelBalance;
        private bool _contentLoaded;
        private string _HeadphoneLimiterOff;
        private string _HeadphoneLimiterOn;
        internal ImageCheckBox _hpLimiter;
        internal TextBlock _Left;
        private CxHDMasterVolumeControl _masterVolumeControl;
        internal TextBlock _Right;

        public PreviewHeadphone()
        {
            Application current = Application.Current;
            this.InitializeComponent();
            this._channelBalance = new ChannelBalance();
            this._balance.ValueChanged += new RoutedPropertyChangedEventHandler<double>(this.VolumeBar_ValueChanged);
            this._hpLimiter.OnItemStateChanged += new ItemStateChanged(this._hpLimiter_OnItemStateChanged);
            this._hpLimiter.OnItemStateChanging += new ItemStateChanging(this._hpLimiter_OnItemStateChanging);
            this.UpdatePreviewImage();
            this.Localize();
            this.initHPLimiter();
        }

        public PreviewHeadphone(CxRenderDeviceType deviceType)
        {
            Application current = Application.Current;
            this.InitializeComponent();
            this._channelBalance = new ChannelBalance();
            this._balance.ValueChanged += new RoutedPropertyChangedEventHandler<double>(this.VolumeBar_ValueChanged);
            this._hpLimiter.OnItemStateChanged += new ItemStateChanged(this._hpLimiter_OnItemStateChanged);
            this._hpLimiter.OnItemStateChanging += new ItemStateChanging(this._hpLimiter_OnItemStateChanging);
            this.UpdatePreviewImage();
            this.Localize();
            this.initHPLimiter(deviceType);
        }

        [DebuggerNonUserCode]
        internal Delegate _CreateDelegate(Type delegateType, string handler) => 
            Delegate.CreateDelegate(delegateType, this, handler);

        private void _hpLimiter_OnItemStateChanged(ImageCheckBox item, bool newState)
        {
            try
            {
                App current = Application.Current as App;
                current.AudioFactory.DeviceIOConfig.HeadPhoneLimiter = this._hpLimiter.Selected;
                this._hpLimiter.ToolTip = this._hpLimiter.Selected ? this._HeadphoneLimiterOn : this._HeadphoneLimiterOff;
                current.Settings.HPLimiterSetting = this._hpLimiter.Selected;
                current.Settings.Save();
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("PreviewHeadphone", Severity.INFORMATION, exception);
                item.Selected = !newState;
            }
        }

        private bool _hpLimiter_OnItemStateChanging(ImageCheckBox item, bool newState)
        {
            bool flag = true;
            try
            {
                App current = Application.Current as App;
                if (current.AudioFactory.DeviceIOConfig.HeadPhoneLimiter && !newState)
                {
                    HeadphoneLimiterPopup popup = new HeadphoneLimiterPopup();
                    if (popup.ShowDialog() != true)
                    {
                        flag = false;
                    }
                }
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("_hpLimiter_OnItemStateChanging", Severity.INFORMATION, exception);
                flag = false;
            }
            return flag;
        }

        private void _masterVolumeControl_OnVolumeChanged(double newValue, string context)
        {
            base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new SmartAudio.OnRefreshBalance(this.OnRefreshBalance));
        }

        public BitmapImage GetBitmap(Uri uri)
        {
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.UriSource = uri;
            image.EndInit();
            return image;
        }

        public void initHPLimiter()
        {
            App current = Application.Current as App;
            if (current.Settings.HPLimiterImage)
            {
                current.AudioFactory.DeviceIOConfig.HeadPhoneLimiter = current.Storage.HPLimiterDefaultSetting;
                this._hpLimiter.Selected = current.Storage.HPLimiterDefaultSetting;
                this._hpLimiter.ToolTip = this._hpLimiter.Selected ? this._HeadphoneLimiterOn : this._HeadphoneLimiterOff;
                current.Settings.HPLimiterSetting = this._hpLimiter.Selected;
                this.UpdatePreviewImage();
                current.Settings.Save();
            }
        }

        private void initHPLimiter(CxRenderDeviceType deviceType)
        {
            App current = Application.Current as App;
            if (current.Settings.HPLimiterImage)
            {
                SmartAudioLog.Log("initHPLimiter: HPLimiterImage is TRUE", new object[] { Severity.INFORMATION });
                if (current.AudioFactory.DeviceIOConfig.get_IsHeadPhoneLimiterSupported(deviceType))
                {
                    SmartAudioLog.Log("initHPLimiter: Call to driver for HPLimiter setting returned TRUE", new object[] { Severity.INFORMATION });
                    if (current.Settings.HPLimiterSetting != current.AudioFactory.DeviceIOConfig.HeadPhoneLimiter)
                    {
                        current.AudioFactory.DeviceIOConfig.HeadPhoneLimiter = current.Settings.HPLimiterSetting;
                        SmartAudioLog.Log("initHPLimiter: Overwriting the Driver HPLimter setting with the one in .xml file ", new object[] { Severity.INFORMATION });
                    }
                    this._hpLimiter.Selected = current.AudioFactory.DeviceIOConfig.HeadPhoneLimiter;
                    this._hpLimiter.ToolTip = this._hpLimiter.Selected ? this._HeadphoneLimiterOn : this._HeadphoneLimiterOff;
                    this._hpLimiter.IsEnabled = true;
                }
                else
                {
                    SmartAudioLog.Log("initHPLimiter: Call to driver for HPLimiter setting returned FALSE", new object[] { Severity.INFORMATION });
                    this._hpLimiter.Selected = false;
                    this._hpLimiter.ToolTip = "";
                    this._hpLimiter.IsEnabled = false;
                }
            }
            else
            {
                SmartAudioLog.Log("initHPLimiter: HPLimiterImage is FALSE", new object[] { Severity.INFORMATION });
                this._hpLimiter.Selected = false;
                this._hpLimiter.ToolTip = "";
                this._hpLimiter.IsEnabled = false;
            }
            this.UpdatePreviewImage();
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (!this._contentLoaded)
            {
                this._contentLoaded = true;
                Uri resourceLocator = new Uri("/SmartAudio;component/previewheadphone.xaml", UriKind.Relative);
                Application.LoadComponent(this, resourceLocator);
            }
        }

        public void Localize()
        {
            this._Left.Text = Resources.SA_LeftChannelShort;
            this._Right.Text = Resources.SA_RightChannelShort;
            this._HeadphoneLimiterOff = Resources.SA_HeadphoneLimiterOFF;
            this._HeadphoneLimiterOn = Resources.SA_HeadphoneLimiterON;
            this._hpLimiter.ToolTip = this._hpLimiter.Selected ? this._HeadphoneLimiterOn : this._HeadphoneLimiterOff;
        }

        public void OnMasterVolumeChanged(double newValue)
        {
            this.OnRefreshBalance();
        }

        public void OnMasterVolumeChanging(double newValue)
        {
        }

        private void OnRefreshBalance()
        {
            try
            {
                this._balance.ValueChanged -= new RoutedPropertyChangedEventHandler<double>(this.VolumeBar_ValueChanged);
                this._balance.Value = this._channelBalance.Value;
                this._balance.ValueChanged += new RoutedPropertyChangedEventHandler<double>(this.VolumeBar_ValueChanged);
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("PreviewHeadphone : OnRefreshBalance - failed", Severity.INFORMATION, exception);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), DebuggerNonUserCode]
        void IComponentConnector.Connect(int connectionId, object target)
        {
            switch (connectionId)
            {
                case 1:
                    this._hpLimiter = (ImageCheckBox) target;
                    return;

                case 2:
                    this._balance = (Slider) target;
                    return;

                case 3:
                    this._Left = (TextBlock) target;
                    return;

                case 4:
                    this._Right = (TextBlock) target;
                    return;
            }
            this._contentLoaded = true;
        }

        public void UpdatePreviewImage()
        {
            App current = Application.Current as App;
            try
            {
                if (current.AudioFactory.DeviceIOConfig.HeadphonePresent || current.Settings.INISettings.ShowPluggedHPLimiterImages)
                {
                    if (current.Settings.HPLimiterImage & this._hpLimiter.IsEnabled)
                    {
                        this._hpLimiter.SelectedImage = this.GetBitmap(new Uri("pack://application:,,,/Resources/Images/Previews/Hp-Limiter-GS-Limiter-On.png", UriKind.RelativeOrAbsolute));
                        this._hpLimiter.UnselectedImage = this.GetBitmap(new Uri("pack://application:,,,/Resources/Images/Previews/Hp-Limiter-GS-Limiter-Off.png", UriKind.RelativeOrAbsolute));
                    }
                    else
                    {
                        this._hpLimiter.SelectedImage = this._hpLimiter.UnselectedImage = this.GetBitmap(new Uri("pack://application:,,,/Resources/Images/Previews/Hp-Limiter-GS-No-Bars.png", UriKind.RelativeOrAbsolute));
                    }
                }
                else if (current.Settings.HPLimiterImage)
                {
                    this._hpLimiter.SelectedImage = this.GetBitmap(new Uri("pack://application:,,,/Resources/Images/Previews/Hp-Limiter-GS-Unplug-Limiter-On.png", UriKind.RelativeOrAbsolute));
                    this._hpLimiter.UnselectedImage = this.GetBitmap(new Uri("pack://application:,,,/Resources/Images/Previews/Hp-Limiter-GS-Unplug-Limiter-Off.png", UriKind.RelativeOrAbsolute));
                }
                else
                {
                    this._hpLimiter.SelectedImage = this._hpLimiter.UnselectedImage = this.GetBitmap(new Uri("pack://application:,,,/Resources/Images/Previews/Hp-Limiter-GS-Unplug-No-Bars.png", UriKind.RelativeOrAbsolute));
                }
            }
            catch
            {
                SmartAudioLog.Log("UpdatePreviewImage: Could not instantiate 'App app = Application.Current as App'", new object[] { Severity.FATALERROR });
            }
        }

        private void VolumeBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Math.Abs((double) (this._channelBalance.Value - e.NewValue)) > 1E-10)
            {
                this._channelBalance.Value = e.NewValue;
            }
        }

        public CxHDAudioChannelEnumeratorClass AudioChannelEnumerator
        {
            get => 
                this._audioChannelEnumerator;
            set
            {
                this._audioChannelEnumerator = value;
                this._channelBalance.AudioChannelEmulator = this._audioChannelEnumerator;
                try
                {
                    App current = Application.Current as App;
                    this._hpLimiter.Selected = current.AudioFactory.DeviceIOConfig.HeadPhoneLimiter;
                }
                catch (Exception exception)
                {
                    SmartAudioLog.Log("PreviewHeadphone: AudioChannelEnumerator - Error getting HeadphoneLimiter status from Driver", Severity.FATALERROR, exception);
                }
            }
        }

        public CxHDMasterVolumeControl MasterVolume
        {
            get => 
                this._masterVolumeControl;
            set
            {
                this._masterVolumeControl = value;
                this._channelBalance.MasterVolumeControlClass = value;
                this._balance.Minimum = this._channelBalance.Minimum;
                this._balance.Maximum = this._channelBalance.Maximum;
                this._balance.Value = this._channelBalance.Value;
                this._balance.Ticks.Clear();
                this._balance.Ticks.Add((this._balance.Maximum - this._balance.Minimum) / 2.0);
                this._masterVolumeControl.add_OnVolumeChanged(new _ICxHDAudioVolumeEvents_OnVolumeChangedEventHandler(this._masterVolumeControl_OnVolumeChanged));
                App current = Application.Current as App;
                try
                {
                    this._hpLimiter.OnItemStateChanged -= new ItemStateChanged(this._hpLimiter_OnItemStateChanged);
                    this._hpLimiter.Selected = current.AudioFactory.DeviceIOConfig.HeadPhoneLimiter;
                    this._hpLimiter.ToolTip = this._hpLimiter.Selected ? this._HeadphoneLimiterOn : this._HeadphoneLimiterOff;
                    this._hpLimiter.OnItemStateChanged += new ItemStateChanged(this._hpLimiter_OnItemStateChanged);
                }
                catch (Exception exception)
                {
                    SmartAudioLog.Log("PreviewHeadphone: MasterVolume - Error getting HeadphoneLimiter status from Driver", Severity.FATALERROR, exception);
                }
            }
        }
    }
}

