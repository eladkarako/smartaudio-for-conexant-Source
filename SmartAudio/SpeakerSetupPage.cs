namespace SmartAudio
{
    using CxHDAudioAPILib;
    using SmartAudio.Properties;
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Markup;
    using System.Windows.Media.Effects;
    using System.Windows.Media.Imaging;
    using System.Windows.Threading;

    public class SpeakerSetupPage : UserControl, ISmartAudioPage, IComponentConnector
    {
        private CxHDAudioFactory _audioFactory;
        internal EndPointVolumeBar _centerSlider;
        private bool _contentLoaded;
        private CxHDAudioConfig _cxHDAudioconfig;
        internal ImageCheckBox _disableSpreading;
        internal JackConfiguration _jackConfiguration;
        internal PreviewMultiChannelSpeakerSetup _preview5Point1;
        internal RearJacksPanel _rearJackPanel;
        private static ArrayList _speakerAL;
        private CxSpeakerConfigType _speakerConfiguration;
        internal GlassBackPlate _speakerPanel;
        internal ComboBox _speakerSelection;
        internal EndPointVolumeBar _subSlider;
        internal ImageCheckBox _swap;
        internal TextBlock _test;
        internal ImageCheckBox _testSpeaker;

        public SpeakerSetupPage()
        {
            this.InitializeComponent();
            this._subSlider.IsUnpluggedEnabled = false;
            this._centerSlider.IsUnpluggedEnabled = false;
            this._speakerSelection.SelectionChanged += new SelectionChangedEventHandler(this._speakerSelection_SelectionChanged);
            this._preview5Point1.OnToneStopped += new PreviewMultiChannelSpeakerSetup.OnToneStoppedHandler(this._preview5Point1_OnToneStopped);
            this._disableSpreading.OnItemStateChanged += new ItemStateChanged(this._disableSpreading_OnItemStateChanged);
            this._swap.OnItemStateChanged += new ItemStateChanged(this._swap_OnItemStateChanged);
            this.initGUI();
            this._preview5Point1.IsPositionAdjustable = true;
            App current = Application.Current as App;
            current.OnRenumeratedEvent = (SmartAudio.OnRenumerated) Delegate.Combine(current.OnRenumeratedEvent, new SmartAudio.OnRenumerated(this.OnRenumerated));
        }

        [DebuggerNonUserCode]
        internal Delegate _CreateDelegate(Type delegateType, string handler) => 
            Delegate.CreateDelegate(delegateType, this, handler);

        private void _disableSpreading_OnItemStateChanged(ImageCheckBox item, bool newState)
        {
            try
            {
                App current = Application.Current as App;
                if ((current.SPAudioEndPoint != null) && (this._cxHDAudioconfig != null))
                {
                    this._cxHDAudioconfig.SetSpreadingEnabled(current.SPAudioEndPoint.DeviceID, item.Selected);
                    this._disableSpreading.ToolTip = newState ? Resources.SA_TTDisableSpreading : Resources.SA_TTEnableSpreading;
                }
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("_disableSpreading_OnItemStateChanged()", Severity.FATALERROR, exception);
            }
        }

        private void _preview5Point1_OnToneStopped()
        {
            this._test.Text = Resources.SA_TEST;
            this._testSpeaker.ToolTip = Resources.SA_TTAUDIOTEST;
            this._testSpeaker.Selected = true;
        }

        private void _speakerSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            App current = Application.Current as App;
            current.ShutdownDeviceEvents();
            switch (this._speakerSelection.SelectedIndex)
            {
                case 0:
                    this._preview5Point1.SpeakerConfiguration = CxSpeakerConfigType.StereoSpeakers;
                    break;

                case 1:
                    this._preview5Point1.SpeakerConfiguration = CxSpeakerConfigType.QuadSpeakers;
                    break;

                case 2:
                    this._preview5Point1.SpeakerConfiguration = CxSpeakerConfigType.FiveDotOneSpeakers;
                    break;

                case 3:
                    this._preview5Point1.SpeakerConfiguration = CxSpeakerConfigType.SevenDotOneSpeakers;
                    break;
            }
            try
            {
                ((App) Application.Current).SpeakerConfigType = this._preview5Point1.SpeakerConfiguration;
                this.InitializeAudioControls(this._preview5Point1.SpeakerConfiguration);
                current.RefreshEndPoints();
            }
            catch (Exception)
            {
                this.InitializeAudioControls(CxSpeakerConfigType.StereoSpeakers);
                try
                {
                    this._disableSpreading.SelectedImage = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/MultiChannel/5.1-Spreading.png"));
                    this._disableSpreading.UnselectedImage = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/MultiChannel/5.1-Spreading-Disabeled.png"));
                    this._disableSpreading.Selected = false;
                    this._disableSpreading.ToolTip = Resources.SA_TTEnableSpreading;
                }
                catch (Exception exception)
                {
                    SmartAudioLog.Log("_speakerSelection_SelectionChanged()", Severity.FATALERROR, exception);
                }
            }
        }

        private void _swap_OnItemStateChanged(ImageCheckBox item, bool newState)
        {
            try
            {
                App current = Application.Current as App;
                if ((current.SPAudioEndPoint != null) && (this._cxHDAudioconfig != null))
                {
                    this._cxHDAudioconfig.SetCenterLFESwapped(current.SPAudioEndPoint.DeviceID, item.Selected);
                }
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("SpeakerSetupPage::_swap_OnItemStateChanged()", Severity.FATALERROR, exception);
            }
        }

        private void _testSpeaker_Click(object sender, RoutedEventArgs e)
        {
        }

        private void _testSpeaker_OnItemStateChanged(ImageCheckBox item, bool newState)
        {
            if (_speakerAL.Count != 0)
            {
                if (item.Selected)
                {
                    this._test.Text = Resources.SA_TEST;
                    this._testSpeaker.ToolTip = Resources.SA_TTAUDIOTEST;
                    this._preview5Point1.StopTestTone((string) _speakerAL[0]);
                }
                else
                {
                    this._test.Text = Resources.SA_STOP;
                    this._testSpeaker.ToolTip = Resources.SA_stopTesting;
                    this._preview5Point1.PlayTestTone((string) _speakerAL[0]);
                }
            }
        }

        public static void AddSpeakerAL(string endPointID)
        {
            if (_speakerAL == null)
            {
                _speakerAL = new ArrayList();
            }
            if (_speakerAL != null)
            {
                _speakerAL.Add(endPointID);
            }
        }

        private void Center_VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this._preview5Point1.MasterVolume.remove_OnVolumeChanged(new _ICxHDAudioVolumeEvents_OnVolumeChangedEventHandler(this.MasterVolume_OnVolumeChanged));
            this._centerSlider.VolumeControl.Volume = this._centerSlider.ConvertFromSliderValueToVolume(this._centerSlider.VolumeSlider.Value);
            this._preview5Point1.MasterVolume.add_OnVolumeChanged(new _ICxHDAudioVolumeEvents_OnVolumeChangedEventHandler(this.MasterVolume_OnVolumeChanged));
        }

        private void EnableImageControl(ImageCheckBox control, bool enable)
        {
            BlurBitmapEffect bitmapEffect = control.BitmapEffect as BlurBitmapEffect;
            if (bitmapEffect != null)
            {
                bitmapEffect.Radius = !enable ? ((double) 5) : ((double) 0);
            }
            control.IsEnabled = enable;
        }

        private void initGUI()
        {
            App current = Application.Current as App;
            this._subSlider.ShowMute = false;
            this._centerSlider.ShowMute = false;
            this._rearJackPanel.Visibility = !current.Settings.INISettings.IsRearPanelSpeakerSetupEnabled ? Visibility.Hidden : Visibility.Visible;
            for (int i = 0; i < (current.MaxNumOfSupportedChannels / 2); i++)
            {
                new ComboBoxItem();
                StackPanel newItem = new StackPanel {
                    Orientation = Orientation.Horizontal
                };
                Image element = new Image {
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                TextBlock block = new TextBlock {
                    Margin = new Thickness(5.0, 0.0, 0.0, 0.0),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center
                };
                switch (i)
                {
                    case 0:
                        element.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/MultiChannel/2.0-box.png"));
                        newItem.Children.Add(element);
                        block.Text = "Stereo";
                        newItem.Children.Add(block);
                        this._speakerSelection.Items.Add(newItem);
                        break;

                    case 1:
                        element.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/MultiChannel/4.0-box.png"));
                        newItem.Children.Add(element);
                        block.Text = "Quadraphonic";
                        newItem.Children.Add(block);
                        this._speakerSelection.Items.Add(newItem);
                        break;

                    case 2:
                        element.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/MultiChannel/5.1-box.png"));
                        newItem.Children.Add(element);
                        block.Text = "Surround";
                        newItem.Children.Add(block);
                        this._speakerSelection.Items.Add(newItem);
                        break;

                    case 3:
                        element.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/MultiChannel/7.1-box.png"));
                        newItem.Children.Add(element);
                        block.Text = "Surround";
                        newItem.Children.Add(block);
                        this._speakerSelection.Items.Add(newItem);
                        break;
                }
            }
            try
            {
                this._disableSpreading.SelectedImage = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/MultiChannel/5.1-Spreading.png"));
                this._disableSpreading.UnselectedImage = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/MultiChannel/5.1-Spreading-Disabeled.png"));
                this._disableSpreading.Selected = false;
                this._disableSpreading.ToolTip = Resources.SA_TTEnableSpreading;
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("_speakerSelection_SelectionChanged()", Severity.FATALERROR, exception);
            }
        }

        private void InitializeAudioControls(CxSpeakerConfigType configSelection)
        {
            App current = Application.Current as App;
            switch (configSelection)
            {
                case CxSpeakerConfigType.StereoSpeakers:
                    this._disableSpreading.Selected = false;
                    this._disableSpreading.ToolTip = Resources.SA_TTEnableSpreading;
                    this.EnableImageControl(this._disableSpreading, false);
                    this.EnableImageControl(this._swap, false);
                    return;

                case CxSpeakerConfigType.QuadSpeakers:
                    try
                    {
                        this._disableSpreading.SelectedImage = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/MultiChannel/4.0-Spreading.png"));
                        this._disableSpreading.UnselectedImage = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/MultiChannel/4.0-Spreading-Disabeled.png"));
                        this._disableSpreading.Selected = this._cxHDAudioconfig.GetSpreadingEnabled(current.SPAudioEndPoint.DeviceID);
                        this._disableSpreading.ToolTip = this._disableSpreading.Selected ? Resources.SA_TTDisableSpreading : Resources.SA_TTEnableSpreading;
                    }
                    catch (Exception exception)
                    {
                        SmartAudioLog.Log("_speakerSelection_SelectionChanged()", Severity.FATALERROR, exception);
                        this._disableSpreading.Selected = this._disableSpreading.Selected;
                        this._disableSpreading.ToolTip = this._disableSpreading.Selected ? Resources.SA_TTDisableSpreading : Resources.SA_TTEnableSpreading;
                    }
                    this.EnableImageControl(this._disableSpreading, true);
                    this.EnableImageControl(this._swap, false);
                    return;

                case CxSpeakerConfigType.FiveDotOneSpeakers:
                    try
                    {
                        this._disableSpreading.SelectedImage = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/MultiChannel/5.1-Spreading.png"));
                        this._disableSpreading.UnselectedImage = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/MultiChannel/5.1-Spreading-Disabeled.png"));
                        this._disableSpreading.Selected = this._cxHDAudioconfig.GetSpreadingEnabled(current.SPAudioEndPoint.DeviceID);
                        this._disableSpreading.ToolTip = this._disableSpreading.Selected ? Resources.SA_TTDisableSpreading : Resources.SA_TTEnableSpreading;
                    }
                    catch (Exception exception2)
                    {
                        SmartAudioLog.Log("_speakerSelection_SelectionChanged()", Severity.FATALERROR, exception2);
                        this._disableSpreading.Selected = this._disableSpreading.Selected;
                        this._disableSpreading.ToolTip = this._disableSpreading.Selected ? Resources.SA_TTDisableSpreading : Resources.SA_TTEnableSpreading;
                    }
                    this.EnableImageControl(this._disableSpreading, true);
                    this.EnableImageControl(this._swap, true);
                    break;

                case CxSpeakerConfigType.SevenDotOneSpeakers:
                    break;

                default:
                    return;
            }
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (!this._contentLoaded)
            {
                this._contentLoaded = true;
                Uri resourceLocator = new Uri("/SmartAudio;component/speakersetuppage.xaml", UriKind.Relative);
                Application.LoadComponent(this, resourceLocator);
            }
        }

        public bool InitializePage(CxHDAudioFactory audioFactory)
        {
            try
            {
                this._speakerSelection.SelectionChanged -= new SelectionChangedEventHandler(this._speakerSelection_SelectionChanged);
                this._audioFactory = audioFactory;
                App current = Application.Current as App;
                this.HDAudioConfig = current.AudioFactory.DeviceIOConfig;
                CxSpeakerConfigType speakerConfig = this._cxHDAudioconfig.GetSpeakerConfig(((App) Application.Current).SPAudioEndPoint.FriendlyName);
                switch (speakerConfig)
                {
                    case CxSpeakerConfigType.StereoSpeakers:
                        this._speakerSelection.SelectedIndex = 0;
                        break;

                    case CxSpeakerConfigType.QuadSpeakers:
                        this._speakerSelection.SelectedIndex = 1;
                        break;

                    case CxSpeakerConfigType.FiveDotOneSpeakers:
                        this._speakerSelection.SelectedIndex = 2;
                        break;

                    case CxSpeakerConfigType.SevenDotOneSpeakers:
                        this._speakerSelection.SelectedIndex = 3;
                        break;
                }
                this.InitializeAudioControls(speakerConfig);
                current.SpeakerConfigType = speakerConfig;
                this.InitPreview();
                this._speakerSelection.SelectionChanged += new SelectionChangedEventHandler(this._speakerSelection_SelectionChanged);
                this._preview5Point1.SpeakerConfiguration = speakerConfig;
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("SpeakerSetupPage::InitializePage()", Severity.FATALERROR, exception);
                this._speakerSelection.SelectedIndex = 0;
            }
            return true;
        }

        public void InitPreview()
        {
            try
            {
                App current = Application.Current as App;
                if (current.SPAudioEndPoint != null)
                {
                    if (this._preview5Point1.MasterVolume != null)
                    {
                        this._preview5Point1.MasterVolume.remove_OnVolumeChanged(new _ICxHDAudioVolumeEvents_OnVolumeChangedEventHandler(this.MasterVolume_OnVolumeChanged));
                    }
                    this._preview5Point1.AudioChannelEnumerator = (CxHDAudioChannelEnumeratorClass) current.SPAudioEndPoint.Channels;
                    this._preview5Point1.MasterVolume = current.SPAudioEndPoint.MasterVolume as CxHDMasterVolumeControl;
                    this._preview5Point1.HDAudioconfig = this._cxHDAudioconfig;
                    this._disableSpreading.Selected = this._cxHDAudioconfig.GetSpreadingEnabled(current.SPAudioEndPoint.DeviceID);
                    this._disableSpreading.ToolTip = this._disableSpreading.Selected ? Resources.SA_TTDisableSpreading : Resources.SA_TTEnableSpreading;
                    this._swap.Selected = this._cxHDAudioconfig.GetCenterLFESwapped(current.SPAudioEndPoint.DeviceID);
                    if (this._preview5Point1.AudioChannelEnumerator.Count >= 6)
                    {
                        double num = this._subSlider.VolumeSlider.Value;
                        this._subSlider.VolumeControl = ((CxHDAudioChannel) this._preview5Point1.AudioChannelEnumerator[4]).VolumeControl;
                        this._subSlider.VolumeSlider.DataContext = this._subSlider;
                        this._subSlider.VolumeSlider.Value = num;
                        num = this._centerSlider.VolumeSlider.Value;
                        this._centerSlider.VolumeControl = ((CxHDAudioChannel) this._preview5Point1.AudioChannelEnumerator[3]).VolumeControl;
                        this._centerSlider.VolumeSlider.DataContext = this._centerSlider;
                        this._centerSlider.VolumeSlider.Value = num;
                    }
                    this._preview5Point1.MasterVolume.add_OnVolumeChanged(new _ICxHDAudioVolumeEvents_OnVolumeChangedEventHandler(this.MasterVolume_OnVolumeChanged));
                    this._preview5Point1.AudioAGC = current.SPAudioEndPoint.AudioAGCControl;
                    this._preview5Point1.Head.CenterPoint = new Point(this._preview5Point1.Width / 2.0, this._preview5Point1.Height / 2.0);
                }
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("SpeakerSetupPage::InitPreview", Severity.FATALERROR, exception);
            }
        }

        public bool Localize()
        {
            this._test.Text = Resources.SA_TEST;
            this._testSpeaker.ToolTip = Resources.SA_TTAUDIOTEST;
            this._swap.ToolTip = Resources.SA_Swap;
            this._speakerSelection.ToolTip = Resources.SA_ConfigSpeaker;
            this._disableSpreading.ToolTip = Resources.SA_TTDisableSpreading;
            this._subSlider.ToolTip = Resources.SA_Subwoofer;
            this._centerSlider.ToolTip = Resources.SA_CenterSpeakers;
            this._rearJackPanel.RefreshJacks();
            this._preview5Point1.Localize();
            return true;
        }

        private void MasterVolume_OnVolumeChanged(double newValue, string context)
        {
            if (Math.Abs((double) (this._preview5Point1.MasterVolume.Volume - newValue)) > 1E-10)
            {
                base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new RefreshVolumeSliders(this.refresh));
            }
        }

        private void OnAutoAdjustToCenter()
        {
            this._preview5Point1.Head.AutoAdjustToCenter();
        }

        private void OnRenumerated()
        {
            this.InitPreview();
        }

        private void OnVolumeChange(CxHDAudioEndPoint spAudioEndPoint, CxHDMasterVolumeControl masterVolumeControl, double newValue, string context)
        {
            base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new RefreshVolumeSliders(this.refresh));
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if ((this._preview5Point1.AudioChannelEnumerator != null) && (this._preview5Point1.AudioChannelEnumerator.Count >= 6))
            {
                this._subSlider.VolumeSlider.ValueChanged -= new RoutedPropertyChangedEventHandler<double>(this.Sub_VolumeSlider_ValueChanged);
                this._centerSlider.VolumeSlider.ValueChanged -= new RoutedPropertyChangedEventHandler<double>(this.Center_VolumeSlider_ValueChanged);
                this._subSlider.VolumeSlider.Value = this._subSlider.ConvertFromVolumeToSliderValue(this._subSlider.VolumeControl.Volume);
                this._centerSlider.VolumeSlider.Value = this._centerSlider.ConvertFromVolumeToSliderValue(this._centerSlider.VolumeControl.Volume);
                this._subSlider.VolumeSlider.ValueChanged += new RoutedPropertyChangedEventHandler<double>(this.Sub_VolumeSlider_ValueChanged);
                this._centerSlider.VolumeSlider.ValueChanged += new RoutedPropertyChangedEventHandler<double>(this.Center_VolumeSlider_ValueChanged);
            }
        }

        private void refresh()
        {
            this._subSlider.RefreshVolumeChanged();
            this._centerSlider.RefreshVolumeChanged();
        }

        private void Sub_VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this._preview5Point1.MasterVolume.remove_OnVolumeChanged(new _ICxHDAudioVolumeEvents_OnVolumeChangedEventHandler(this.MasterVolume_OnVolumeChanged));
            this._subSlider.VolumeControl.Volume = this._subSlider.ConvertFromSliderValueToVolume(this._subSlider.VolumeSlider.Value);
            this._preview5Point1.MasterVolume.add_OnVolumeChanged(new _ICxHDAudioVolumeEvents_OnVolumeChangedEventHandler(this.MasterVolume_OnVolumeChanged));
        }

        [EditorBrowsable(EditorBrowsableState.Never), DebuggerNonUserCode]
        void IComponentConnector.Connect(int connectionId, object target)
        {
            switch (connectionId)
            {
                case 1:
                    ((SpeakerSetupPage) target).Loaded += new RoutedEventHandler(this.Page_Loaded);
                    return;

                case 2:
                    this._speakerPanel = (GlassBackPlate) target;
                    return;

                case 3:
                    this._speakerSelection = (ComboBox) target;
                    return;

                case 4:
                    this._disableSpreading = (ImageCheckBox) target;
                    return;

                case 5:
                    this._testSpeaker = (ImageCheckBox) target;
                    return;

                case 6:
                    this._test = (TextBlock) target;
                    this._test.MouseLeftButtonDown += new MouseButtonEventHandler(this.TextBlock_MouseLeftButtonDown);
                    return;

                case 7:
                    this._centerSlider = (EndPointVolumeBar) target;
                    return;

                case 8:
                    this._subSlider = (EndPointVolumeBar) target;
                    return;

                case 9:
                    this._jackConfiguration = (JackConfiguration) target;
                    return;

                case 10:
                    this._rearJackPanel = (RearJacksPanel) target;
                    return;

                case 11:
                    this._swap = (ImageCheckBox) target;
                    return;

                case 12:
                    this._preview5Point1 = (PreviewMultiChannelSpeakerSetup) target;
                    return;
            }
            this._contentLoaded = true;
        }

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this._testSpeaker.Selected = !this._testSpeaker.Selected;
            this._testSpeaker_OnItemStateChanged(this._testSpeaker, this._testSpeaker.Selected);
        }

        public CxHDAudioFactory AudioFactory
        {
            get => 
                this._audioFactory;
            set
            {
                this._audioFactory = value;
                if (this._audioFactory != null)
                {
                    this.InitializePage(this._audioFactory);
                }
            }
        }

        public string FriendlyName =>
            "Speaker Setup Page";

        public CxHDAudioConfig HDAudioConfig
        {
            set
            {
                try
                {
                    this._cxHDAudioconfig = value;
                    this._preview5Point1.HDAudioconfig = this._cxHDAudioconfig;
                }
                catch (Exception exception)
                {
                    SmartAudioLog.Log("SpeakerSetupPage::HDAudioConfig {set}", Severity.WARNING, exception);
                }
            }
        }

        public CxSpeakerConfigType SpeakerConfiguration
        {
            get => 
                this._speakerConfiguration;
            set
            {
                this._speakerConfiguration = value;
                switch (this._speakerConfiguration)
                {
                    case CxSpeakerConfigType.StereoSpeakers:
                        this._speakerSelection.SelectedIndex = 0;
                        break;

                    case CxSpeakerConfigType.QuadSpeakers:
                        this._speakerSelection.SelectedIndex = 1;
                        break;

                    case CxSpeakerConfigType.FiveDotOneSpeakers:
                        this._speakerSelection.SelectedIndex = 2;
                        break;

                    case CxSpeakerConfigType.SevenDotOneSpeakers:
                        this._speakerSelection.SelectedIndex = 3;
                        break;

                    default:
                        this._speakerSelection.SelectedIndex = 0;
                        break;
                }
                ((App) Application.Current).SpeakerConfigType = this._speakerConfiguration;
            }
        }

        private delegate void AutoAdjustToCenter();

        private delegate void RefreshVolumeSliders();
    }
}

