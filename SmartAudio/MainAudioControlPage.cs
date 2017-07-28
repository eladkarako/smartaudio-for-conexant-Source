namespace SmartAudio
{
    using CxHDAudioAPILib;
    using Microsoft.Win32;
    using SmartAudio.Properties;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Markup;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Threading;

    public class MainAudioControlPage : UserControl, ISmartAudioPage, IComponentConnector
    {
        private CxHDAudioEndPointEnumerator _audioEndPointEnumerator;
        private CxHDAudioFactory _audioFactory;
        internal ListBox _audioInputsPanel;
        internal GlassBackPlate _audioInputsPanelBack;
        internal GlassBackPlate _audioOutputPanelBack;
        internal ListBox _audioOutputsPanel;
        private List<EndPointVolumeBar> _captureEndPoints;
        private bool _contentLoaded;
        private int _currentCaptureEndpointIndex;
        private int _currentRenderEndpointIndex;
        private bool _enumeratingEndPoints;
        private List<CxHDAudioEndPoint> _inputEndPoints;
        private List<UserControl> _inputPreviewsList;
        private List<CxHDAudioEndPoint> _outputEndPoints;
        private List<UserControl> _outputPreviewsList;
        internal ContentControl _previewContent;
        private List<EndPointVolumeBar> _renderEndPoints;
        private bool _renderEndpointSelected = true;
        internal VolumeLevelMeter _volumeLevelMeter;
        internal Canvas canvas1;

        public event OnSpeakerConfigChangeHandler OnSpeakerConfigChange;

        public MainAudioControlPage()
        {
            try
            {
                this.InitializeComponent();
                this._enumeratingEndPoints = false;
                base.IsVisibleChanged += new DependencyPropertyChangedEventHandler(this.MainAudioControlPage_IsVisibleChanged);
                this._audioInputsPanel.GotFocus += new RoutedEventHandler(this._audioInputsPanel_GotFocus);
                this._audioOutputsPanel.GotFocus += new RoutedEventHandler(this._audioOutputsPanel_GotFocus);
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("MainAudioControlPage::MainAudioControlPage()", Severity.FATALERROR, exception);
            }
        }

        private void _audioEndPointEnumerator_OnDefaultDeviceChanged(string pwstrDeviceId)
        {
            base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new DefaultEndPointChangedHandler(this.MainAudioControlPage_DefaultEndPointChanged), pwstrDeviceId);
        }

        private void _audioEndPointEnumerator_OnDeviceStateChanged(string pwstrDeviceId, NewEndPointChange newState)
        {
            base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new OnDeviceCommand(this.OnDeviceStateChanged));
        }

        private void _audioFactory_OnDeviceChanged(CxWindowsDeviceChange newChange, int line)
        {
            base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new OnDeviceCommand(this.OnDeviceStateChanged));
        }

        private void _audioInputsPanel_GotFocus(object sender, RoutedEventArgs e)
        {
            if (((this._currentCaptureEndpointIndex >= 0) && (this._inputPreviewsList != null)) && (this._currentCaptureEndpointIndex < this._inputPreviewsList.Count))
            {
                this._previewContent.Content = this._inputPreviewsList[this._currentCaptureEndpointIndex];
                this._renderEndpointSelected = false;
            }
        }

        private void _audioInputsPanel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.OnInputsPanelSelectionChanged();
        }

        private void _audioOutputsPanel_GotFocus(object sender, RoutedEventArgs e)
        {
            if (((this._currentRenderEndpointIndex >= 0) && (this._outputPreviewsList != null)) && (this._currentRenderEndpointIndex < this._outputPreviewsList.Count))
            {
                this._previewContent.Content = this._outputPreviewsList[this._currentRenderEndpointIndex];
                this._renderEndpointSelected = true;
            }
        }

        private void _audioOutputsPanel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.OnOutputsPanelSelectionChanged();
        }

        [DebuggerNonUserCode]
        internal Delegate _CreateDelegate(Type delegateType, string handler) => 
            Delegate.CreateDelegate(delegateType, this, handler);

        public EndPointVolumeBar AddCaptureDevice(CxHDAudioCaptureDevice audioCapture)
        {
            EndPointVolumeBar bar = new EndPointVolumeBar();
            string resourceKey = "";
            string str2 = "";
            switch (audioCapture.CaptureDeviceType)
            {
                case CxCaptureDeviceType.BluetoothMicrophone:
                case CxCaptureDeviceType.BluetoothHeadsetMicrophone:
                    resourceKey = "BluetoothMic_UnSelected";
                    str2 = "BluetoothMic_Selected";
                    break;

                case CxCaptureDeviceType.AudioInput:
                    resourceKey = "AudioIN_UnSelected";
                    str2 = "AudioIN_Selected";
                    break;

                case CxCaptureDeviceType.InternalMicrophone:
                    resourceKey = "InternalMIC_UnSelected";
                    str2 = "InternalMIC_Selected";
                    break;

                case CxCaptureDeviceType.MicrophoneArray:
                    resourceKey = "EXTMicArray_UnSelected";
                    str2 = "EXTMicArray_Selected";
                    break;

                case CxCaptureDeviceType.SPDIFInput:
                    resourceKey = "SPDIF_UnSelected";
                    str2 = "SPDIF_Selected";
                    break;

                case CxCaptureDeviceType.StereoMix:
                    resourceKey = "StereoMIX_UnSelected";
                    str2 = "StereoMIX_Selected";
                    break;

                default:
                    resourceKey = "ExternalMIC_UnSelected";
                    str2 = "ExternalMIC_Selected";
                    break;
            }
            bar.ToolTip = this.GetToolTipForDevice(audioCapture);
            try
            {
                bar.UnselectedImage = ((Image) base.FindResource(resourceKey)).Source;
                bar.SelectedImage = ((Image) base.FindResource(str2)).Source;
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("MainAudioControlPage::AddCaptureDevice(): Failed to load image for " + audioCapture.CaptureDeviceType.ToString() + " [Error: " + exception.Message + "]", Severity.FATALERROR, exception);
            }
            bar.Selected = false;
            bar.ShowMute = true;
            bar.Height = 172.0;
            bar.Width = 48.0;
            bar.SelectionChanged += new SelectionChangedHandler(this.EndPointVolumeBar_SelectionChanged);
            return bar;
        }

        public EndPointVolumeBar AddRenderDevice(CxHDAudioRenderDevice audioRender)
        {
            EndPointVolumeBar bar = new EndPointVolumeBar();
            string resourceKey = "";
            string str2 = "";
            switch (audioRender.RenderDeviceType)
            {
                case CxRenderDeviceType.BluetoothHeadsetSpeakers:
                    resourceKey = "BluetoothHeadset_UnSelected";
                    str2 = "BluetoothHeadset_Selected";
                    break;

                case CxRenderDeviceType.BluetoothSpeakers:
                    resourceKey = "Bluetooth_UnSelected";
                    str2 = "Bluetooth_Selected";
                    break;

                case CxRenderDeviceType.USBSpeakers:
                    resourceKey = "USBSpeaker_UnSelected";
                    str2 = "USBSpeaker_Selected";
                    break;

                case CxRenderDeviceType.HDMIDevice:
                    resourceKey = "HDMIEndPoint_UnSelected";
                    str2 = "HDMIEndPoint_Selected";
                    break;

                case CxRenderDeviceType.InternalSpeakers:
                    if (!this._audioFactory.DeviceIOConfig.HeadphonePresent || (this.AudioDirectorMode != CxHDAudioAudioDirectorMode.SingleStream))
                    {
                        resourceKey = "IntSpeaker_UnSelected";
                        str2 = "IntSpeaker_Selected";
                    }
                    else
                    {
                        resourceKey = "Headphone_UnSelected";
                        str2 = "Headphone_Selected";
                    }
                    break;

                case CxRenderDeviceType.ExternalSpeakers:
                    resourceKey = "ExtSpeaker_UnSelected";
                    str2 = "ExtSpeaker_Selected";
                    break;

                case CxRenderDeviceType.HeadphonesDevice:
                    resourceKey = "Headphone_UnSelected";
                    str2 = "Headphone_Selected";
                    break;

                case CxRenderDeviceType.SPDIFDevice:
                    resourceKey = "SPDIF_UnSelected";
                    str2 = "SPDIF_Selected";
                    break;

                default:
                    resourceKey = "ExtSpeaker_UnSelected";
                    str2 = "ExtSpeaker_Selected";
                    break;
            }
            bar.ToolTip = this.GetToolTipForDevice(audioRender);
            try
            {
                bar.UnselectedImage = ((Image) base.FindResource(resourceKey)).Source;
                bar.SelectedImage = ((Image) base.FindResource(str2)).Source;
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("MainAudioControlPage::AddRenderDevice(): Failed to load image for " + audioRender.RenderDeviceType.ToString() + " [Error: " + exception.Message + "]", Severity.FATALERROR, exception);
            }
            bar.Selected = false;
            bar.Height = 172.0;
            bar.Width = 48.0;
            bar.MuteChanged += new MuteChangedHandler(this.endPointVolumeBar_MuteChanged);
            bar.SelectionChanged += new SelectionChangedHandler(this.EndPointVolumeBar_SelectionChanged);
            return bar;
        }

        private void audioFactory_OnHeadphoneStatusChanged(CxJackPluginStatus newStatus)
        {
            this._audioEndPointEnumerator.remove_OnDeviceStateChanged(new _ICxHDAudioEndPointEnumeratorEvents_OnDeviceStateChangedEventHandler(this._audioEndPointEnumerator_OnDeviceStateChanged));
            this._audioFactory.remove_OnHeadphoneStatusChanged(new _ICxHDAudioFactoryEvents_OnHeadphoneStatusChangedEventHandler(this.audioFactory_OnHeadphoneStatusChanged));
            this._audioFactory.remove_OnDeviceChanged(new _ICxHDAudioFactoryEvents_OnDeviceChangedEventHandler(this._audioFactory_OnDeviceChanged));
            base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new ReEnumerate(this.OnReEnumerate));
        }

        private void Capture_endPointVolumeBar_SelectionChanged(EndPointVolumeBar sender, bool newState)
        {
            for (int i = 0; i < this._audioInputsPanel.Items.Count; i++)
            {
                if (((EndPointVolumeBar) ((ListBoxItem) this._audioInputsPanel.Items[i]).Content) == sender)
                {
                    ((ListBoxItem) this._audioInputsPanel.Items[i]).IsSelected = newState;
                }
                else
                {
                    ((EndPointVolumeBar) ((ListBoxItem) this._audioInputsPanel.Items[i]).Content).Selected = !newState;
                }
            }
        }

        private void CaptureDeviceMasterVolume_OnMuted(int bValue, string context)
        {
            base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new OnMuteChanged(this.OnCaptureDeviceMuteChanged));
        }

        private void CaptureDeviceMasterVolume_OnVolumeChanged(double newValue, string context)
        {
            base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new OnVolumeChanged(this.OnCaptureDeviceVolumeChanged));
        }

        private void ClearCurrentEndPoints()
        {
            EndPointVolumeBar content;
            foreach (ListBoxItem item in (IEnumerable) this._audioInputsPanel.Items)
            {
                content = (EndPointVolumeBar) item.Content;
                content.OnDefaultEndPointChanged -= new OnDefaultEndPointChanged(this.InputEndpoint_OnDefaultEndPointChanged);
                content.VolumeControl = null;
            }
            this._audioInputsPanel.Items.Clear();
            foreach (ListBoxItem item2 in (IEnumerable) this._audioOutputsPanel.Items)
            {
                content = (EndPointVolumeBar) item2.Content;
                content.OnDefaultEndPointChanged -= new OnDefaultEndPointChanged(this.OutputEndpoint_OnDefaultEndPointChanged);
                content.VolumeControl = null;
            }
            this._audioOutputsPanel.Items.Clear();
            this._audioInputsPanel.Items.Clear();
            this._outputEndPoints.Clear();
            this._inputEndPoints.Clear();
            this._captureEndPoints.Clear();
            this._renderEndPoints.Clear();
        }

        public void ClearSpeakerHeadPhoneList()
        {
            AudioDirectorPreview.clearSpeakerHeadPhoneALs();
        }

        private int CompareEndPoints(EndPointVolumeBar endPoint1, EndPointVolumeBar endPoint2)
        {
            CxHDAudioCaptureDevice audioEndPointItem = endPoint1.AudioEndPointItem as CxHDAudioCaptureDevice;
            CxHDAudioRenderDevice device2 = endPoint1.AudioEndPointItem as CxHDAudioRenderDevice;
            CxHDAudioCaptureDevice device3 = endPoint2.AudioEndPointItem as CxHDAudioCaptureDevice;
            CxHDAudioRenderDevice device4 = endPoint2.AudioEndPointItem as CxHDAudioRenderDevice;
            if ((audioEndPointItem != null) && (device3 != null))
            {
                return (int) (audioEndPointItem.CaptureDeviceType - device3.CaptureDeviceType);
            }
            if ((device2 != null) && (device4 != null))
            {
                return (int) (device2.RenderDeviceType - device4.RenderDeviceType);
            }
            return -1;
        }

        private void endPointVolumeBar_MuteChanged(bool newValue)
        {
            if (!this._volumeLevelMeter.IsMeterDisabled)
            {
                this._volumeLevelMeter.IsMuted = newValue;
            }
        }

        private void EndPointVolumeBar_SelectionChanged(EndPointVolumeBar sender, bool newState)
        {
            for (int i = 0; i < this._audioOutputsPanel.Items.Count; i++)
            {
                if (((EndPointVolumeBar) ((ListBoxItem) this._audioOutputsPanel.Items[i]).Content) == sender)
                {
                    ((ListBoxItem) this._audioOutputsPanel.Items[i]).IsSelected = newState;
                    this._renderEndpointSelected = true;
                }
                else
                {
                    ((EndPointVolumeBar) ((ListBoxItem) this._audioOutputsPanel.Items[i]).Content).Selected = !newState;
                }
            }
            for (int j = 0; j < this._audioInputsPanel.Items.Count; j++)
            {
                if (((EndPointVolumeBar) ((ListBoxItem) this._audioInputsPanel.Items[j]).Content) == sender)
                {
                    ((ListBoxItem) this._audioInputsPanel.Items[j]).IsSelected = newState;
                    this._renderEndpointSelected = false;
                }
                else
                {
                    ((EndPointVolumeBar) ((ListBoxItem) this._audioInputsPanel.Items[j]).Content).Selected = !newState;
                }
            }
            if (this._renderEndpointSelected)
            {
                this._currentRenderEndpointIndex = this._audioOutputsPanel.SelectedIndex;
            }
            else
            {
                this._currentCaptureEndpointIndex = this._audioInputsPanel.SelectedIndex;
            }
        }

        public string getAudioDeviceName(CxRenderDeviceType deviceType)
        {
            if (this._renderEndPoints != null)
            {
                foreach (EndPointVolumeBar bar in this._renderEndPoints)
                {
                    CxHDAudioRenderDevice audioEndPointItem = bar.AudioEndPointItem as CxHDAudioRenderDevice;
                    if ((audioEndPointItem != null) && (audioEndPointItem.RenderDeviceType == deviceType))
                    {
                        return this.GetToolTipForDevice(audioEndPointItem);
                    }
                }
            }
            return null;
        }

        public string getAudioDeviceOSName(CxRenderDeviceType deviceType)
        {
            if (this._renderEndPoints != null)
            {
                foreach (EndPointVolumeBar bar in this._renderEndPoints)
                {
                    CxHDAudioRenderDevice audioEndPointItem = bar.AudioEndPointItem as CxHDAudioRenderDevice;
                    if ((audioEndPointItem != null) && (audioEndPointItem.RenderDeviceType == deviceType))
                    {
                        return ((CxHDAudioEndPoint) audioEndPointItem).FriendlyName;
                    }
                }
            }
            return null;
        }

        public UserControl GetCaptureDevicePreview(CxHDAudioEndPoint audioEndPoint, CxHDAudioCaptureDevice audioCapture)
        {
            Application current = Application.Current;
            switch (audioCapture.CaptureDeviceType)
            {
                case CxCaptureDeviceType.BluetoothMicrophone:
                case CxCaptureDeviceType.BluetoothHeadsetMicrophone:
                    return new PreviewBTHeadphone();

                case CxCaptureDeviceType.AudioInput:
                    return new PreviewLineIn();

                case CxCaptureDeviceType.GenericMicrophone:
                case CxCaptureDeviceType.InternalMicrophone:
                case CxCaptureDeviceType.ExternalMicrophone:
                case CxCaptureDeviceType.MicrophoneArray:
                case CxCaptureDeviceType.DockingMicrophone:
                {
                    PreviewExternalMicrophone microphone = new PreviewExternalMicrophone();
                    microphone.InitializeSlider();
                    return microphone;
                }
                case CxCaptureDeviceType.StereoMix:
                    return new PreviewMixer();
            }
            return new PreviewEmpty();
        }

        public ImageSource GetImage(Uri imageURL)
        {
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.UriSource = imageURL;
            image.EndInit();
            return image;
        }

        public UserControl GetRenderDevicePreview(CxHDAudioEndPoint audioEndPoint, CxHDAudioRenderDevice audioRender)
        {
            int count = 2;
            try
            {
                count = audioEndPoint.Channels.Count;
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("Failed to get Channels count", Severity.FATALERROR, exception);
            }
            App current = Application.Current as App;
            if (current.MaxNumOfSupportedChannels < count)
            {
                current.MaxNumOfSupportedChannels = count;
            }
            if (count > 2)
            {
                current.IsMultiChannelSupported = true;
                current.SPAudioEndPoint = audioEndPoint;
                if (audioRender != null)
                {
                    current.SPToolTip = this.GetToolTipForDevice(audioRender);
                }
            }
            switch (count)
            {
                case 2:
                    switch (audioRender.RenderDeviceType)
                    {
                        case CxRenderDeviceType.BluetoothHeadsetSpeakers:
                        case CxRenderDeviceType.BluetoothSpeakers:
                            return new PreviewBTHeadphone();

                        case CxRenderDeviceType.USBSpeakers:
                            return new PreviewTwoDotOne();

                        case CxRenderDeviceType.HDMIDevice:
                            return new PreviewHDMI();

                        case CxRenderDeviceType.InternalSpeakers:
                            if (!this._audioFactory.DeviceIOConfig.HeadphonePresent || (((this._audioFactory.AudioDirector.AudioDirectorMode != CxHDAudioAudioDirectorMode.SingleStream) || (((this._audioFactory.HostOperatingSystem != CxHostOperatingSystemType.WindowsVista) && (this._audioFactory.HostOperatingSystem != CxHostOperatingSystemType.WindowsVista64)) && (this._audioFactory.HostOperatingSystem != CxHostOperatingSystemType.Windows7))) && ((this._audioFactory.HostOperatingSystem != CxHostOperatingSystemType.WindowsXP) && (this._audioFactory.HostOperatingSystem != CxHostOperatingSystemType.WindowsXP64))))
                            {
                                return new PreviewInternalSpeakers();
                            }
                            this.HPPlugInStatus = true;
                            return new PreviewHeadphone(CxRenderDeviceType.InternalSpeakers);

                        case CxRenderDeviceType.ExternalSpeakers:
                            return new PreviewTwoChannel();

                        case CxRenderDeviceType.HeadphonesDevice:
                            this.HPPlugInStatus = true;
                            return new PreviewHeadphone(CxRenderDeviceType.HeadphonesDevice);

                        case CxRenderDeviceType.SPDIFDevice:
                            return new PreviewTwoChannel();
                    }
                    break;

                case 3:
                case 6:
                    if (!this._audioFactory.DeviceIOConfig.HeadphonePresent || (this._audioFactory.AudioDirector.AudioDirectorMode != CxHDAudioAudioDirectorMode.SingleStream))
                    {
                        PreviewMultiChannelSpeakerSetup setup = new PreviewMultiChannelSpeakerSetup {
                            IsPositionAdjustable = false,
                            LayoutTransform = new ScaleTransform(0.6, 0.6),
                            SpeakerConfiguration = current.AudioFactory.DeviceIOConfig.GetSpeakerConfig(((App) Application.Current).SPAudioEndPoint.FriendlyName)
                        };
                        if (this.OnSpeakerConfigChange != null)
                        {
                            this.OnSpeakerConfigChange(setup.SpeakerConfiguration);
                        }
                        return setup;
                    }
                    return new PreviewHeadphone(CxRenderDeviceType.InternalSpeakers);
            }
            return null;
        }

        private string GetToolTipForDevice(CxHDAudioCaptureDevice audioCapture)
        {
            string str = "Internal Microphone";
            string str2 = "External Microphone";
            string str3 = "Docking Microphone";
            string str4 = "Microphone";
            string friendlyName = ((CxHDAudioEndPoint) audioCapture).FriendlyName;
            if (this.IsLenovoPackage())
            {
                if (((CxHDAudioEndPoint) audioCapture).DriverSupportsMicNameIOCTL)
                {
                    switch (audioCapture.CaptureDeviceType)
                    {
                        case CxCaptureDeviceType.GenericMicrophone:
                            return Resources.SA_Microphone;

                        case CxCaptureDeviceType.InternalMicrophone:
                            return Resources.SA_MISC_INTERNAL_MICROPHONE;

                        case CxCaptureDeviceType.ExternalMicrophone:
                            return Resources.SA_MISC_EXTERNAL_MICROPHONE;

                        case CxCaptureDeviceType.MicrophoneArray:
                        case CxCaptureDeviceType.SPDIFInput:
                            return friendlyName;

                        case CxCaptureDeviceType.DockingMicrophone:
                            return Resources.SA_MISC_DOCKING_MICROPHONE;
                    }
                    return friendlyName;
                }
                string str6 = ((CxHDAudioEndPoint) audioCapture).FriendlyName;
                if (str6.Contains(str))
                {
                    return Resources.SA_MISC_INTERNAL_MICROPHONE;
                }
                if (str6.Contains(str2))
                {
                    return Resources.SA_MISC_EXTERNAL_MICROPHONE;
                }
                if (str6.Contains(str3))
                {
                    return Resources.SA_MISC_DOCKING_MICROPHONE;
                }
                if (str6.Contains(str4))
                {
                    friendlyName = Resources.SA_Microphone;
                }
            }
            return friendlyName;
        }

        private string GetToolTipForDevice(CxHDAudioRenderDevice audioRender)
        {
            if (((this._audioFactory.HostOperatingSystem != CxHostOperatingSystemType.WindowsVista) && (this._audioFactory.HostOperatingSystem != CxHostOperatingSystemType.WindowsVista64)) && (this._audioFactory.HostOperatingSystem != CxHostOperatingSystemType.Windows7))
            {
                if ((this._audioFactory.HostOperatingSystem != CxHostOperatingSystemType.WindowsXP) && (this._audioFactory.HostOperatingSystem != CxHostOperatingSystemType.WindowsXP64))
                {
                    return null;
                }
                if (audioRender.RenderDeviceType == CxRenderDeviceType.InternalSpeakers)
                {
                    if (this.AudioFactory.DeviceIOConfig.HeadphonePresent)
                    {
                        return Resources.SA_CHeadPhones;
                    }
                    return Resources.SA_MISC_SPEAKERS;
                }
                if (audioRender.RenderDeviceType == CxRenderDeviceType.HDMIDevice)
                {
                    try
                    {
                        if (base.FindResource("SpecialDPTag") != null)
                        {
                            return Resources.SA_DisplayPort;
                        }
                    }
                    catch (Exception)
                    {
                    }
                    return Resources.SA_HDMI;
                }
                if (audioRender.RenderDeviceType == CxRenderDeviceType.HeadphonesDevice)
                {
                    return Resources.SA_CHeadPhones;
                }
                if (!this.IsLenovoPackage())
                {
                    return ((CxHDAudioEndPoint) audioRender).FriendlyName;
                }
                switch (audioRender.RenderDeviceType)
                {
                    case CxRenderDeviceType.HeadphonesDevice:
                        return Resources.SA_CHeadPhones;
                }
                return Resources.SA_MISC_SPEAKERS;
            }
            if (audioRender.RenderDeviceType == CxRenderDeviceType.HDMIDevice)
            {
                try
                {
                    if (base.FindResource("SpecialDPTag") != null)
                    {
                        return Resources.SA_DisplayPort;
                    }
                }
                catch (Exception)
                {
                }
                return Resources.SA_HDMI;
            }
            if (((this.AudioDirectorMode == CxHDAudioAudioDirectorMode.SingleStream) && (audioRender.RenderDeviceType == CxRenderDeviceType.InternalSpeakers)) && this.AudioFactory.DeviceIOConfig.HeadphonePresent)
            {
                return Resources.SA_CHeadPhones;
            }
            if ((this.AudioDirectorMode == CxHDAudioAudioDirectorMode.SingleStream) && (audioRender.RenderDeviceType == CxRenderDeviceType.HeadphonesDevice))
            {
                return Resources.SA_CHeadPhones;
            }
            if (!this.IsLenovoPackage())
            {
                return ((CxHDAudioEndPoint) audioRender).FriendlyName;
            }
            switch (audioRender.RenderDeviceType)
            {
                case CxRenderDeviceType.HeadphonesDevice:
                    return Resources.SA_CHeadPhones;
            }
            return Resources.SA_MISC_SPEAKERS;
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (!this._contentLoaded)
            {
                this._contentLoaded = true;
                Uri resourceLocator = new Uri("/SmartAudio;component/mainaudiocontrolpage.xaml", UriKind.Relative);
                Application.LoadComponent(this, resourceLocator);
            }
        }

        public bool InitializePage(CxHDAudioFactory audioFactory)
        {
            try
            {
                this._outputPreviewsList = new List<UserControl>();
                this._inputPreviewsList = new List<UserControl>();
                this._audioInputsPanel.SelectedIndex = 0;
                this._audioOutputsPanel.SelectedIndex = 0;
                this._audioOutputsPanel.Focus();
                if (this.IsVolumeLevelMeterEnabled())
                {
                    this._volumeLevelMeter.IsMeterDisabled = false;
                    this._volumeLevelMeter._energyBarsPanel.Background = Brushes.Black;
                    this._volumeLevelMeter._energyBarsPanel.Margin = new Thickness(0.0, -10.0, 0.0, 0.0);
                    this._volumeLevelMeter.Initialize();
                    this._volumeLevelMeter.Initialize(0.0, 101.0, 15);
                    this._volumeLevelMeter.Simulate = true;
                }
                else
                {
                    this._volumeLevelMeter._audioOutputPanelBack.Margin = new Thickness(25.0, 10.0, 0.0, 5.0);
                    this._volumeLevelMeter._glassLook.Visibility = Visibility.Hidden;
                    this._volumeLevelMeter.IsMeterDisabled = true;
                    SmartAudioLog.Log("MainAudioControlPage::InitializePage() - _volumeLevelMeter is disabled", new object[] { Severity.INFORMATION });
                }
                this._audioEndPointEnumerator = audioFactory.get_EndPointEnumerator(true);
                this._audioEndPointEnumerator.add_OnDefaultDeviceChanged(new _ICxHDAudioEndPointEnumeratorEvents_OnDefaultDeviceChangedEventHandler(this._audioEndPointEnumerator_OnDefaultDeviceChanged));
                this._audioEndPointEnumerator.add_OnDeviceStateChanged(new _ICxHDAudioEndPointEnumeratorEvents_OnDeviceStateChangedEventHandler(this._audioEndPointEnumerator_OnDeviceStateChanged));
                audioFactory.add_OnHeadphoneStatusChanged(new _ICxHDAudioFactoryEvents_OnHeadphoneStatusChangedEventHandler(this.audioFactory_OnHeadphoneStatusChanged));
                this._audioFactory.add_OnDeviceChanged(new _ICxHDAudioFactoryEvents_OnDeviceChangedEventHandler(this._audioFactory_OnDeviceChanged));
                SystemEvents.UserPreferenceChanged += new UserPreferenceChangedEventHandler(this.userPreferenceChanged);
                this._outputEndPoints = new List<CxHDAudioEndPoint>();
                this._inputEndPoints = new List<CxHDAudioEndPoint>();
                this._captureEndPoints = new List<EndPointVolumeBar>();
                this._renderEndPoints = new List<EndPointVolumeBar>();
                this.RefreshEndPoints();
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("MainAudioControlPage::InitializePage()", Severity.FATALERROR, exception);
                throw exception;
            }
            return true;
        }

        private void InputEndpoint_OnDefaultEndPointChanged(bool newState)
        {
            this.RefreshInputDefaultEndPoints();
        }

        public bool IsExternalMicPluggedIn()
        {
            bool flag = false;
            foreach (EndPointVolumeBar bar in this._captureEndPoints)
            {
                CxHDAudioCaptureDevice audioEndPointItem = bar.AudioEndPointItem as CxHDAudioCaptureDevice;
                if (((audioEndPointItem != null) && ((audioEndPointItem.CaptureDeviceType == CxCaptureDeviceType.ExternalMicrophone) || (audioEndPointItem.CaptureDeviceType == CxCaptureDeviceType.DockingMicrophone))) && !bar.AudioEndPointItem.IsUnplugged)
                {
                    flag = true;
                }
            }
            return flag;
        }

        private bool IsLenovoPackage()
        {
            App current = Application.Current as App;
            if (current.AudioFactory == null)
            {
                return false;
            }
            return current.Settings.INISettings.IsLenovoPackageEnabled;
        }

        private bool IsVolumeLevelMeterEnabled()
        {
            App current = Application.Current as App;
            if (current.AudioFactory == null)
            {
                return false;
            }
            return current.Settings.INISettings.IsVolumeLevelMeterEnabled;
        }

        public bool Localize()
        {
            for (int i = 0; i < this._outputPreviewsList.Count; i++)
            {
                ((IHDAudioPreview) this._outputPreviewsList[i]).Localize();
            }
            for (int j = 0; j < this._inputPreviewsList.Count; j++)
            {
                ((IHDAudioPreview) this._inputPreviewsList[j]).Localize();
            }
            this.SetupHPSPToolTip();
            try
            {
                for (int k = 0; k < this._audioOutputsPanel.Items.Count; k++)
                {
                    this._renderEndPoints[k].LocalizeMuteToolTip();
                }
                for (int m = 0; m < this._audioInputsPanel.Items.Count; m++)
                {
                    this._captureEndPoints[m].LocalizeMuteToolTip();
                }
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("MainAudioControlPage::Localize()", Severity.FATALERROR, exception);
            }
            return true;
        }

        private void MainAudioControlPage_DefaultEndPointChanged(string pwstrDeviceId)
        {
            this.RefreshInputDefaultEndPoints();
            this.RefreshOutputDefaultEndPoints();
        }

        private void MainAudioControlPage_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!this._volumeLevelMeter.IsMeterDisabled)
            {
                this._volumeLevelMeter.Simulate = (bool) e.NewValue;
            }
        }

        private void OnCaptureDeviceMuteChanged()
        {
            try
            {
                for (int i = 0; i < this._audioInputsPanel.Items.Count; i++)
                {
                    this._captureEndPoints[i].RefreshMuteState();
                }
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("MainAudioControlPage::OnCaptureDeviceVolumeChanged()", Severity.FATALERROR, exception);
            }
        }

        private void OnCaptureDeviceVolumeChanged()
        {
            try
            {
                for (int i = 0; i < this._audioInputsPanel.Items.Count; i++)
                {
                    this._captureEndPoints[i].RefreshVolume();
                }
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("MainAudioControlPage::OnCaptureDeviceVolumeChanged()", Severity.FATALERROR, exception);
            }
        }

        private void OnDeviceStateChanged()
        {
            this._audioEndPointEnumerator.remove_OnDeviceStateChanged(new _ICxHDAudioEndPointEnumeratorEvents_OnDeviceStateChangedEventHandler(this._audioEndPointEnumerator_OnDeviceStateChanged));
            this._audioFactory.remove_OnHeadphoneStatusChanged(new _ICxHDAudioFactoryEvents_OnHeadphoneStatusChangedEventHandler(this.audioFactory_OnHeadphoneStatusChanged));
            this._audioFactory.remove_OnDeviceChanged(new _ICxHDAudioFactoryEvents_OnDeviceChangedEventHandler(this._audioFactory_OnDeviceChanged));
            base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new ReEnumerate(this.OnReEnumerate));
        }

        private void OnInputsPanelSelectionChanged()
        {
            try
            {
                for (int i = 0; i < this._audioInputsPanel.Items.Count; i++)
                {
                    ((EndPointVolumeBar) ((ListBoxItem) this._audioInputsPanel.Items[i]).Content).Selected = ((ListBoxItem) this._audioInputsPanel.Items[i]).IsSelected;
                }
                int selectedIndex = this._audioInputsPanel.SelectedIndex;
                if (((this._audioInputsPanel.SelectedIndex >= 0) && (this._audioInputsPanel.SelectedIndex < this._inputPreviewsList.Count)) && (selectedIndex != -1))
                {
                    if (!this._volumeLevelMeter.IsMeterDisabled)
                    {
                        this._volumeLevelMeter.MasterVolumeControl = this._inputEndPoints[this._audioInputsPanel.SelectedIndex].MasterVolume as CxHDMasterVolumeControl;
                    }
                    this._previewContent.Content = this._inputPreviewsList[this._audioInputsPanel.SelectedIndex];
                    ((IHDAudioPreview) this._inputPreviewsList[this._audioInputsPanel.SelectedIndex]).AudioChannelEnumerator = (CxHDAudioChannelEnumeratorClass) this._inputEndPoints[this._audioInputsPanel.SelectedIndex].Channels;
                    ((IHDAudioPreview) this._inputPreviewsList[this._audioInputsPanel.SelectedIndex]).MasterVolume = this._inputEndPoints[this._audioInputsPanel.SelectedIndex].MasterVolume as CxHDMasterVolumeControl;
                }
                else
                {
                    this._previewContent.Content = null;
                }
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("MainAudioControlPage::_audioInputsPanel_SelectionChanged()", Severity.FATALERROR, exception);
            }
            if (!this._volumeLevelMeter.IsMeterDisabled)
            {
                this._volumeLevelMeter.Reset();
            }
        }

        private void OnOutputsPanelSelectionChanged()
        {
            try
            {
                for (int i = 0; i < this._audioOutputsPanel.Items.Count; i++)
                {
                    ((EndPointVolumeBar) ((ListBoxItem) this._audioOutputsPanel.Items[i]).Content).Selected = ((ListBoxItem) this._audioOutputsPanel.Items[i]).IsSelected;
                }
                int selectedIndex = this._audioOutputsPanel.SelectedIndex;
                if (((this._audioOutputsPanel.SelectedIndex >= 0) && (this._audioOutputsPanel.SelectedIndex < this._outputPreviewsList.Count)) && (selectedIndex != -1))
                {
                    if (!this._volumeLevelMeter.IsMeterDisabled)
                    {
                        this._volumeLevelMeter.MasterVolumeControl = this._outputEndPoints[this._audioOutputsPanel.SelectedIndex].MasterVolume as CxHDMasterVolumeControl;
                    }
                    this._previewContent.Content = this._outputPreviewsList[this._audioOutputsPanel.SelectedIndex];
                    ((IHDAudioPreview) this._outputPreviewsList[this._audioOutputsPanel.SelectedIndex]).AudioChannelEnumerator = (CxHDAudioChannelEnumeratorClass) this._outputEndPoints[this._audioOutputsPanel.SelectedIndex].Channels;
                    ((IHDAudioPreview) this._outputPreviewsList[this._audioOutputsPanel.SelectedIndex]).MasterVolume = this._outputEndPoints[this._audioOutputsPanel.SelectedIndex].MasterVolume as CxHDMasterVolumeControl;
                }
                else
                {
                    this._previewContent.Content = null;
                }
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("MainAudioControlPage::_audioOutputsPanel_SelectionChanged()", Severity.FATALERROR, exception);
            }
            if (!this._volumeLevelMeter.IsMeterDisabled)
            {
                this._volumeLevelMeter.Reset();
            }
        }

        private void OnReEnumerate()
        {
            Cursor cursor = Application.Current.MainWindow.Cursor;
            if (!this._enumeratingEndPoints)
            {
                Application.Current.MainWindow.Cursor = Cursors.Wait;
                this._enumeratingEndPoints = true;
                this._audioFactory.remove_OnDeviceChanged(new _ICxHDAudioFactoryEvents_OnDeviceChangedEventHandler(this._audioFactory_OnDeviceChanged));
                this._audioEndPointEnumerator.remove_OnDeviceStateChanged(new _ICxHDAudioEndPointEnumeratorEvents_OnDeviceStateChangedEventHandler(this._audioEndPointEnumerator_OnDeviceStateChanged));
                this._audioFactory.remove_OnHeadphoneStatusChanged(new _ICxHDAudioFactoryEvents_OnHeadphoneStatusChangedEventHandler(this.audioFactory_OnHeadphoneStatusChanged));
                this._audioEndPointEnumerator.remove_OnDefaultDeviceChanged(new _ICxHDAudioEndPointEnumeratorEvents_OnDefaultDeviceChangedEventHandler(this._audioEndPointEnumerator_OnDefaultDeviceChanged));
                SystemEvents.UserPreferenceChanged -= new UserPreferenceChangedEventHandler(this.userPreferenceChanged);
                this.ClearCurrentEndPoints();
                Thread.Sleep(0x3e8);
                this._audioEndPointEnumerator.Refresh();
                this.RefreshEndPoints();
                object page = MainWindow.theCurrent.GetPage("VoiceSettingsPage");
                if (page != null)
                {
                    App current = Application.Current as App;
                    if (((MainWindow) current.MainWindow).IsBeamFormingEnabled())
                    {
                        ((VoiceEffectsPage) page).RefreshVOIPControl(this.HPPlugInStatus && this.IsExternalMicPluggedIn());
                    }
                    else
                    {
                        ((VoiceEffectsPageNoBeamForming) page).RefreshVOIPControl(this.HPPlugInStatus && this.IsExternalMicPluggedIn());
                    }
                }
                object obj3 = MainWindow.theCurrent.GetPage("GraphicEqualizers");
                if (obj3 != null)
                {
                    ((GraphicEqualizersPage) obj3).SetSpeakerHeadphoneToolTip();
                    ((GraphicEqualizersPage) obj3).OnReEnumerate();
                }
                this._audioEndPointEnumerator.add_OnDefaultDeviceChanged(new _ICxHDAudioEndPointEnumeratorEvents_OnDefaultDeviceChangedEventHandler(this._audioEndPointEnumerator_OnDefaultDeviceChanged));
                this._audioFactory.add_OnHeadphoneStatusChanged(new _ICxHDAudioFactoryEvents_OnHeadphoneStatusChangedEventHandler(this.audioFactory_OnHeadphoneStatusChanged));
                this._audioEndPointEnumerator.add_OnDeviceStateChanged(new _ICxHDAudioEndPointEnumeratorEvents_OnDeviceStateChangedEventHandler(this._audioEndPointEnumerator_OnDeviceStateChanged));
                this._audioFactory.add_OnDeviceChanged(new _ICxHDAudioFactoryEvents_OnDeviceChangedEventHandler(this._audioFactory_OnDeviceChanged));
                SystemEvents.UserPreferenceChanged += new UserPreferenceChangedEventHandler(this.userPreferenceChanged);
                this._enumeratingEndPoints = false;
                Application.Current.MainWindow.Cursor = cursor;
                object obj4 = MainWindow.theCurrent.GetPage("AudioDirector");
                if (obj4 != null)
                {
                    ((AudioDirectorPage) obj4).UpdatedAudioDirectorMode();
                }
            }
        }

        public void OnReenumerateEndPointsInternal()
        {
            this._audioEndPointEnumerator.remove_OnDeviceStateChanged(new _ICxHDAudioEndPointEnumeratorEvents_OnDeviceStateChangedEventHandler(this._audioEndPointEnumerator_OnDeviceStateChanged));
            this._audioFactory.remove_OnHeadphoneStatusChanged(new _ICxHDAudioFactoryEvents_OnHeadphoneStatusChangedEventHandler(this.audioFactory_OnHeadphoneStatusChanged));
            this._audioFactory.remove_OnDeviceChanged(new _ICxHDAudioFactoryEvents_OnDeviceChangedEventHandler(this._audioFactory_OnDeviceChanged));
            base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new ReEnumerate(this.OnReEnumerate));
        }

        private void OnRenderDeviceMuteChanged()
        {
            try
            {
                for (int i = 0; i < this._audioOutputsPanel.Items.Count; i++)
                {
                    this._renderEndPoints[i].RefreshMuteState();
                }
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("MainAudioControlPage::OnRenderDeviceMuteChanged()", Severity.FATALERROR, exception);
            }
        }

        private void OnRenderDeviceVolumeChanged()
        {
            try
            {
                for (int i = 0; i < this._audioOutputsPanel.Items.Count; i++)
                {
                    this._renderEndPoints[i].RefreshVolume();
                }
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("MainAudioControlPage::OnRenderDeviceVolumeChanged()", Severity.FATALERROR, exception);
            }
        }

        private void OutputEndpoint_OnDefaultEndPointChanged(bool newState)
        {
            this.RefreshOutputDefaultEndPoints();
        }

        public void ReenumerateEndPoints()
        {
            base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new OnDeviceCommand(this.OnReenumerateEndPointsInternal));
        }

        public void RefreshEndPoints()
        {
            if (this.AudioEndPointEnumerator != null)
            {
                this._outputPreviewsList.Clear();
                this._inputPreviewsList.Clear();
                int num = 0;
                int num2 = 0;
                this.ClearCurrentEndPoints();
                this.ClearSpeakerHeadPhoneList();
                new List<ListBoxItem>();
                new List<ListBoxItem>();
                try
                {
                    foreach (CxHDAudioEndPoint point in this.AudioEndPointEnumerator)
                    {
                        ICxHDAudioEndPoint point2 = point;
                        if ((point2 != null) && !point2.VirtualEndPoint)
                        {
                            string friendlyName = point.FriendlyName;
                            CxHDAudioCaptureDevice audioCapture = point as CxHDAudioCaptureDevice;
                            CxHDAudioRenderDevice renderDevice = point as CxHDAudioRenderDevice;
                            EndPointVolumeBar bar = null;
                            if (audioCapture != null)
                            {
                                try
                                {
                                    bar = this.AddCaptureDevice(audioCapture);
                                    this._captureEndPoints.Add(bar);
                                }
                                catch (Exception exception)
                                {
                                    SmartAudioLog.Log("MainAudioControlPage::RefreshEndPoints():CaptureDevice", Severity.FATALERROR, exception);
                                }
                            }
                            else if (renderDevice != null)
                            {
                                try
                                {
                                    this.UpdateSpeakerHeadPhoneList(renderDevice);
                                    bar = this.AddRenderDevice(renderDevice);
                                    this._renderEndPoints.Add(bar);
                                }
                                catch (Exception exception2)
                                {
                                    SmartAudioLog.Log("MainAudioControlPage::RefreshEndPoints():RenderDevice", Severity.FATALERROR, exception2);
                                }
                            }
                            if (bar != null)
                            {
                                bar.AudioEndPointItem = point;
                            }
                        }
                    }
                }
                catch (Exception exception3)
                {
                    SmartAudioLog.Log("MainAudioControlPage::RefreshEndPoints()", Severity.FATALERROR, exception3);
                }
                this._renderEndPoints.Sort(new Comparison<EndPointVolumeBar>(this.CompareEndPoints));
                this._captureEndPoints.Sort(new Comparison<EndPointVolumeBar>(this.CompareEndPoints));
                num = 0;
                num2 = 0;
                this._audioInputsPanel.SelectionChanged -= new SelectionChangedEventHandler(this._audioInputsPanel_SelectionChanged);
                this._audioOutputsPanel.SelectionChanged -= new SelectionChangedEventHandler(this._audioOutputsPanel_SelectionChanged);
                foreach (EndPointVolumeBar bar2 in this._renderEndPoints)
                {
                    CxHDAudioEndPoint audioEndPointItem = bar2.AudioEndPointItem;
                    CxHDAudioRenderDevice audioRender = audioEndPointItem as CxHDAudioRenderDevice;
                    if (num2 == 0)
                    {
                        bar2.Margin = new Thickness(15.0, 0.0, 5.0, 0.0);
                    }
                    else
                    {
                        bar2.Margin = new Thickness(5.0, 0.0, 5.0, 0.0);
                    }
                    num2++;
                    ListBoxItem newItem = new ListBoxItem {
                        Content = bar2
                    };
                    this._audioOutputsPanel.Items.Add(newItem);
                    this._outputEndPoints.Add(bar2.AudioEndPointItem);
                    UserControl renderDevicePreview = this.GetRenderDevicePreview(audioEndPointItem, audioRender);
                    IHDAudioPreview preview = (IHDAudioPreview) renderDevicePreview;
                    if (preview != null)
                    {
                        bar2.OnMasterVolumeChanged += new OnMasterVolumeChanged(preview.OnMasterVolumeChanged);
                        try
                        {
                            preview.AudioChannelEnumerator = (CxHDAudioChannelEnumeratorClass) audioEndPointItem.Channels;
                            preview.MasterVolume = audioEndPointItem.MasterVolume as CxHDMasterVolumeControl;
                        }
                        catch (Exception)
                        {
                            SmartAudioLog.Log("Failed to Initialize Preview for " + audioEndPointItem.FriendlyName);
                        }
                        this._outputPreviewsList.Add(renderDevicePreview);
                    }
                    bar2.OnDefaultEndPointChanged += new OnDefaultEndPointChanged(this.OutputEndpoint_OnDefaultEndPointChanged);
                    SmartAudioLog.Log("Render Device " + audioEndPointItem.FriendlyName);
                }
                foreach (EndPointVolumeBar bar3 in this._captureEndPoints)
                {
                    CxHDAudioEndPoint audioEndPoint = bar3.AudioEndPointItem;
                    CxHDAudioCaptureDevice device4 = audioEndPoint as CxHDAudioCaptureDevice;
                    if (num == 0)
                    {
                        bar3.Margin = new Thickness(15.0, 0.0, 5.0, 0.0);
                    }
                    else
                    {
                        bar3.Margin = new Thickness(5.0, 0.0, 5.0, 0.0);
                    }
                    num++;
                    ListBoxItem item2 = new ListBoxItem {
                        Content = bar3
                    };
                    this._audioInputsPanel.Items.Add(item2);
                    this._inputEndPoints.Add(bar3.AudioEndPointItem);
                    UserControl captureDevicePreview = this.GetCaptureDevicePreview(audioEndPoint, device4);
                    IHDAudioPreview preview2 = (IHDAudioPreview) captureDevicePreview;
                    if (preview2 != null)
                    {
                        bar3.OnMasterVolumeChanged += new OnMasterVolumeChanged(preview2.OnMasterVolumeChanged);
                        try
                        {
                            preview2.AudioChannelEnumerator = (CxHDAudioChannelEnumeratorClass) audioEndPoint.Channels;
                            preview2.MasterVolume = audioEndPoint.MasterVolume as CxHDMasterVolumeControl;
                        }
                        catch (Exception)
                        {
                            SmartAudioLog.Log("Failed to Initialize Preview for " + audioEndPoint.FriendlyName);
                        }
                        this._inputPreviewsList.Add(captureDevicePreview);
                    }
                    bar3.OnDefaultEndPointChanged += new OnDefaultEndPointChanged(this.InputEndpoint_OnDefaultEndPointChanged);
                    SmartAudioLog.Log("Capture Device " + audioEndPoint.FriendlyName);
                    CxHDVolumeControl masterVolume = audioEndPoint.MasterVolume;
                }
                SmartAudioLog.Log("Found " + this._audioOutputsPanel.Items.Count.ToString() + " Render Devices");
                this._audioInputsPanel.SelectionChanged += new SelectionChangedEventHandler(this._audioInputsPanel_SelectionChanged);
                this._audioOutputsPanel.SelectionChanged += new SelectionChangedEventHandler(this._audioOutputsPanel_SelectionChanged);
                this.SelectDefaultEndPoint();
                if (this._audioFactory.DeviceIOConfig.HeadphonePresent)
                {
                    this.HPPlugInStatus = true;
                }
                else
                {
                    this.HPPlugInStatus = false;
                }
            }
        }

        private void RefreshInputDefaultEndPoints()
        {
            try
            {
                for (int i = 0; i < this._audioInputsPanel.Items.Count; i++)
                {
                    ListBoxItem item = (ListBoxItem) this._audioInputsPanel.Items[i];
                    ((EndPointVolumeBar) item.Content).RefreshDefaultEndPoint();
                }
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("MainAudioControlPage::InputEndpoint_OnDefaultEndPointChanged()", Severity.FATALERROR, exception);
            }
        }

        private void RefreshOutputDefaultEndPoints()
        {
            try
            {
                for (int i = 0; i < this._audioOutputsPanel.Items.Count; i++)
                {
                    ListBoxItem item = (ListBoxItem) this._audioOutputsPanel.Items[i];
                    ((EndPointVolumeBar) item.Content).RefreshDefaultEndPoint();
                }
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("MainAudioControlPage::OutputEndpoint_OnDefaultEndPointChanged()", Severity.FATALERROR, exception);
            }
        }

        private void Render_endPointVolumeBar_SelectionChanged(EndPointVolumeBar sender, bool newState)
        {
            for (int i = 0; i < this._audioOutputsPanel.Items.Count; i++)
            {
                if (((EndPointVolumeBar) ((ListBoxItem) this._audioOutputsPanel.Items[i]).Content) == sender)
                {
                    ((ListBoxItem) this._audioOutputsPanel.Items[i]).IsSelected = newState;
                }
                else
                {
                    ((EndPointVolumeBar) ((ListBoxItem) this._audioOutputsPanel.Items[i]).Content).Selected = !newState;
                }
            }
        }

        private void RenderDeviceMasterVolume_OnMuted(int bValue, string context)
        {
            base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new OnMuteChanged(this.OnRenderDeviceMuteChanged));
        }

        private void RenderDeviceMasterVolume_OnVolumeChanged(double newValue, string context)
        {
            base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new OnVolumeChanged(this.OnRenderDeviceVolumeChanged));
        }

        public bool ResetHeadphone()
        {
            for (int i = 0; i < this._outputPreviewsList.Count; i++)
            {
                PreviewHeadphone headphone = this._outputPreviewsList[i] as PreviewHeadphone;
                if (headphone != null)
                {
                    headphone.initHPLimiter();
                }
            }
            this.SetupHPSPToolTip();
            return true;
        }

        private void SelectDefaultEndPoint()
        {
            int num = 0;
            try
            {
                for (int i = 0; i < this._outputEndPoints.Count; i++)
                {
                    try
                    {
                        if (this._outputEndPoints[i].DefaultEndPoint)
                        {
                            num = i;
                            goto Label_0051;
                        }
                    }
                    catch (Exception exception)
                    {
                        SmartAudioLog.Log("MainAudioControlPage.SelectDefaultEndPoint", Severity.FATALERROR, exception);
                    }
                }
            }
            catch (Exception exception2)
            {
                SmartAudioLog.Log("MainAudioControlPage::SelectDefaultEndPoint()", Severity.FATALERROR, exception2);
            }
        Label_0051:
            if (num < this._outputEndPoints.Count)
            {
                this._previewContent.Content = this._outputPreviewsList[num];
                this._currentRenderEndpointIndex = num;
                this._renderEndpointSelected = true;
                if (!this._volumeLevelMeter.IsMeterDisabled)
                {
                    this._volumeLevelMeter.MasterVolumeControl = this._outputEndPoints[num].MasterVolume as CxHDMasterVolumeControl;
                }
                if (this._audioOutputsPanel.SelectedIndex == num)
                {
                    this.OnOutputsPanelSelectionChanged();
                    return;
                }
                this._audioOutputsPanel.SelectedIndex = num;
            }
        }

        private void SetupHPSPToolTip()
        {
            foreach (EndPointVolumeBar bar in this._renderEndPoints)
            {
                CxHDAudioRenderDevice audioEndPointItem = bar.AudioEndPointItem as CxHDAudioRenderDevice;
                if (audioEndPointItem != null)
                {
                    bar.ToolTip = this.GetToolTipForDevice(audioEndPointItem);
                }
            }
            foreach (EndPointVolumeBar bar2 in this._captureEndPoints)
            {
                CxHDAudioCaptureDevice audioCapture = bar2.AudioEndPointItem as CxHDAudioCaptureDevice;
                if (audioCapture != null)
                {
                    bar2.ToolTip = this.GetToolTipForDevice(audioCapture);
                }
            }
        }

        public void ShutDownDeviceEvents()
        {
            this._audioFactory.remove_OnDeviceChanged(new _ICxHDAudioFactoryEvents_OnDeviceChangedEventHandler(this._audioFactory_OnDeviceChanged));
            this._audioEndPointEnumerator.remove_OnDeviceStateChanged(new _ICxHDAudioEndPointEnumeratorEvents_OnDeviceStateChangedEventHandler(this._audioEndPointEnumerator_OnDeviceStateChanged));
            this._audioFactory.remove_OnHeadphoneStatusChanged(new _ICxHDAudioFactoryEvents_OnHeadphoneStatusChangedEventHandler(this.audioFactory_OnHeadphoneStatusChanged));
            this._audioEndPointEnumerator.remove_OnDefaultDeviceChanged(new _ICxHDAudioEndPointEnumeratorEvents_OnDefaultDeviceChangedEventHandler(this._audioEndPointEnumerator_OnDefaultDeviceChanged));
        }

        [DebuggerNonUserCode, EditorBrowsable(EditorBrowsableState.Never)]
        void IComponentConnector.Connect(int connectionId, object target)
        {
            switch (connectionId)
            {
                case 1:
                    this.canvas1 = (Canvas) target;
                    return;

                case 2:
                    this._audioOutputPanelBack = (GlassBackPlate) target;
                    return;

                case 3:
                    this._audioOutputsPanel = (ListBox) target;
                    this._audioOutputsPanel.SelectionChanged += new SelectionChangedEventHandler(this._audioOutputsPanel_SelectionChanged);
                    return;

                case 4:
                    this._audioInputsPanelBack = (GlassBackPlate) target;
                    return;

                case 5:
                    this._audioInputsPanel = (ListBox) target;
                    this._audioInputsPanel.SelectionChanged += new SelectionChangedEventHandler(this._audioInputsPanel_SelectionChanged);
                    return;

                case 6:
                    this._previewContent = (ContentControl) target;
                    return;

                case 7:
                    this._volumeLevelMeter = (VolumeLevelMeter) target;
                    return;
            }
            this._contentLoaded = true;
        }

        public void UpdateSpeakerHeadPhoneList(CxHDAudioRenderDevice renderDevice)
        {
            try
            {
                switch (renderDevice.RenderDeviceType)
                {
                    case CxRenderDeviceType.InternalSpeakers:
                        AudioDirectorPreview.AddSpeakerAL(((CxHDAudioEndPoint) renderDevice).DeviceID);
                        SpeakerSetupPage.AddSpeakerAL(((CxHDAudioEndPoint) renderDevice).DeviceID);
                        return;

                    case CxRenderDeviceType.ExternalSpeakers:
                        return;

                    case CxRenderDeviceType.HeadphonesDevice:
                        AudioDirectorPreview.AddHeadPhoneAL(((CxHDAudioEndPoint) renderDevice).DeviceID);
                        return;
                }
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("MainAudioControlPage::UpdateSpeakerHeadPhoneList()", Severity.WARNING, exception);
            }
        }

        private void userPreferenceChanged(object sender, EventArgs e)
        {
            SystemEvents.UserPreferenceChanged -= new UserPreferenceChangedEventHandler(this.userPreferenceChanged);
            SmartAudioLog.Log("UserPreferenceChanged event detected", new object[] { Severity.INFORMATION });
            if (this._renderEndpointSelected)
            {
                this._previewContent.Content = this._outputPreviewsList[this._currentRenderEndpointIndex];
                this._audioOutputsPanel.SelectedIndex = this._currentRenderEndpointIndex;
            }
            else
            {
                this._previewContent.Content = this._inputPreviewsList[this._currentCaptureEndpointIndex];
                this._audioInputsPanel.SelectedIndex = this._currentCaptureEndpointIndex;
            }
            object page = MainWindow.theCurrent.GetPage("GraphicEqualizers");
            if (page != null)
            {
                ((GraphicEqualizersPage) page).SelectCurrentProfile();
                ((GraphicEqualizersPage) page).refreshEndpointType();
            }
            MainWindow.theCurrent.RefreshPageIconSelection();
            SystemEvents.UserPreferenceChanged += new UserPreferenceChangedEventHandler(this.userPreferenceChanged);
        }

        private CxHDAudioAudioDirectorMode AudioDirectorMode
        {
            get
            {
                if ((this._audioFactory.HostOperatingSystem != CxHostOperatingSystemType.WindowsVista) && (this._audioFactory.HostOperatingSystem != CxHostOperatingSystemType.Windows7))
                {
                    return CxHDAudioAudioDirectorMode.SingleStream;
                }
                return this._audioFactory.AudioDirector.AudioDirectorMode;
            }
        }

        public CxHDAudioEndPointEnumerator AudioEndPointEnumerator =>
            this._audioEndPointEnumerator;

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
            "Main Audio Controls Page";

        public bool HPPlugInStatus { get; set; }

        public delegate void OnSpeakerConfigChangeHandler(CxSpeakerConfigType configurationType);
    }
}

