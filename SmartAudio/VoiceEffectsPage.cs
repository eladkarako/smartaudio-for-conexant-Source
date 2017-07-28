namespace SmartAudio
{
    using CxHDAudioAPILib;
    using SmartAudio.Properties;
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Markup;
    using System.Windows.Media.Effects;

    public class VoiceEffectsPage : UserControl, ISmartAudioPage, IComponentConnector
    {
        internal LEDUserControl _aec;
        internal TextBlock _AECNRLEDText;
        private CxHDAudioFactory _audioFactory;
        private bool _beamForming;
        internal ImageListItem _conferenceRoom;
        internal TextBlock _ConferenceRoomText;
        private bool _contentLoaded;
        private CxHDAudioConfig _cxHDAudioconfig;
        internal TextBlock _MicNRLEDText;
        internal LEDUserControl _microphoneNR;
        internal ImageListItem _privateChat;
        internal TextBlock _PrivateChatText;
        internal LEDUserControl _speakerNR;
        internal TextBlock _speakerNRLEDText;
        private bool _updating;
        internal ListBox _voiceChatSettings;
        internal GlassBackPlate _voiceChatSettingsPanel;
        internal StackPanel _voiceControls;
        private CxHDAudioVoiceEffectsClass _voiceEffects;
        internal TextBlock _VoiceOverIPText;
        internal ImageCheckBox _voiceRecording;
        internal TextBlock _VoiceRecordingText;
        internal ImageCheckBox _voip;
        private bool _voipUpdate;
        internal Canvas canvas1;
        internal GlassBackPlate glassBackPlate1;
        internal TextBlock textBlock2;

        public VoiceEffectsPage()
        {
            try
            {
                this.InitializeComponent();
                this._voiceChatSettings.SelectionChanged += new SelectionChangedEventHandler(this._voiceEffects_SelectionChanged);
                this._beamForming = false;
                this.ShowVOIPControlsPanel(true);
                this.ApplyNewStyle();
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("VoiceEffectsPage::VoiceEffectsPage()", Severity.FATALERROR, exception);
            }
        }

        [DebuggerNonUserCode]
        internal Delegate _CreateDelegate(Type delegateType, string handler) => 
            Delegate.CreateDelegate(delegateType, this, handler);

        private void _voiceChatSettings_ListBoxItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (this._voiceChatSettings.SelectedIndex != -1)
            {
                this._voiceChatSettings.SelectedIndex = -1;
            }
        }

        private void _voiceControls_OnItemStateChanged(ImageCheckBox item, bool newState)
        {
            try
            {
                item.Selected = newState;
                if (item == this._voiceRecording)
                {
                    if (item.Selected)
                    {
                        this._voip.Selected = false;
                        if (!this.IsFreezeBFEnabled())
                        {
                            this.SetUpGrayedOutControls(0, false);
                        }
                    }
                    else if (!this._voip.Selected)
                    {
                        if (this._voiceChatSettings.SelectedIndex != 1)
                        {
                            this._voipUpdate = true;
                            this._voiceChatSettings.SelectedIndex = 1;
                        }
                        if (!this.IsFreezeBFEnabled())
                        {
                            this.SetUpGrayedOutControls(0, true);
                        }
                    }
                    if ((this._voiceEffects != null) && !this._updating)
                    {
                        if (this.isAECDisabled())
                        {
                            this._voiceEffects.MicrophoneNoiseReduction = item.Selected;
                            this._voiceEffects.SpeakerNoiseReduction = false;
                        }
                        else
                        {
                            this._voiceEffects.VoiceRecording = item.Selected;
                        }
                    }
                    if (this._beamForming)
                    {
                        this._voiceEffects.VoiceOverIPMode = CxVOIPMode.VOIP_Mode_PrivateChat;
                    }
                    else
                    {
                        this._voiceEffects.VoiceOverIPMode = CxVOIPMode.VOIP_Mode_ConferenceRoom;
                    }
                    goto Label_02A5;
                }
                if (item != this._voip)
                {
                    goto Label_02A5;
                }
                if (item.Selected)
                {
                    this._voiceRecording.Selected = false;
                    if (!this.IsFreezeBFEnabled())
                    {
                        this.SetUpGrayedOutControls(0, false);
                    }
                }
                else if (!this._voiceRecording.Selected)
                {
                    if (this._voiceChatSettings.SelectedIndex != 1)
                    {
                        this._voipUpdate = true;
                        this._voiceChatSettings.SelectedIndex = 1;
                    }
                    if (!this.IsFreezeBFEnabled())
                    {
                        this.SetUpGrayedOutControls(0, true);
                    }
                }
                if ((this._voiceEffects != null) && !this._updating)
                {
                    if (this.isAECDisabled())
                    {
                        this._voiceEffects.MicrophoneNoiseReduction = item.Selected;
                        this._voiceEffects.SpeakerNoiseReduction = item.Selected;
                    }
                    else
                    {
                        this._voiceEffects.VoiceOverIP = item.Selected;
                    }
                }
                if ((this._voiceEffects == null) || !this.ShouldUpdateBeamForming())
                {
                    goto Label_02A5;
                }
                if (this._beamForming)
                {
                    this._voiceEffects.VoiceOverIPMode = CxVOIPMode.VOIP_Mode_PrivateChat;
                }
                else
                {
                    this._voiceEffects.VoiceOverIPMode = CxVOIPMode.VOIP_Mode_ConferenceRoom;
                }
                switch (this._voiceEffects.VoiceOverIPMode)
                {
                    case CxVOIPMode.VOIP_Mode_ConferenceRoom:
                        this._voiceChatSettings.SelectedIndex = 1;
                        if (!this._conferenceRoom.SAIsEnabled)
                        {
                            goto Label_0273;
                        }
                        this._conferenceRoom.Selected = true;
                        goto Label_0297;

                    case CxVOIPMode.VOIP_Mode_PrivateChat:
                        this._voiceChatSettings.SelectedIndex = 0;
                        if (!this._privateChat.SAIsEnabled)
                        {
                            break;
                        }
                        this._privateChat.Selected = true;
                        goto Label_0297;

                    default:
                        goto Label_0297;
                }
                this._privateChat.SAIsEnabled = true;
                this._privateChat.Selected = true;
                this._privateChat.SAIsEnabled = false;
                goto Label_0297;
            Label_0273:
                this._conferenceRoom.SAIsEnabled = true;
                this._conferenceRoom.Selected = true;
                this._conferenceRoom.SAIsEnabled = false;
            Label_0297:
                if (this.IsFreezeBFEnabled())
                {
                    this.SetUpGrayedOutControls();
                }
            Label_02A5:
                this.RefreshSettings();
                if (this._cxHDAudioconfig != null)
                {
                    this._cxHDAudioconfig.NotifyRegistryViaSAIIService = 0x52;
                }
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("VoiceEffectsPage::_voiceControls_OnItemStateChanged", Severity.FATALERROR, exception);
            }
        }

        private void _voiceEffects_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if ((this._voiceEffects == null) || this.ShouldUpdateBeamForming())
                {
                    if ((this.IsFreezeBFEnabled() && !this._voipUpdate) || ((!this._voiceRecording.Selected && !this._voip.Selected) && !this._voipUpdate))
                    {
                        if (this._voiceEffects.VoiceOverIPMode == CxVOIPMode.VOIP_Mode_PrivateChat)
                        {
                            this._voiceChatSettings.SelectedIndex = 0;
                        }
                        else if (this._voiceEffects.VoiceOverIPMode == CxVOIPMode.VOIP_Mode_ConferenceRoom)
                        {
                            this._voiceChatSettings.SelectedIndex = 1;
                        }
                    }
                    else
                    {
                        for (int i = 0; i < this._voiceChatSettings.Items.Count; i++)
                        {
                            ImageListItem content = ((ListBoxItem) this._voiceChatSettings.Items[i]).Content as ImageListItem;
                            if (content != null)
                            {
                                if (content.SAIsEnabled)
                                {
                                    content.Selected = ((ListBoxItem) this._voiceChatSettings.Items[i]).IsSelected;
                                }
                                else
                                {
                                    content.SAIsEnabled = true;
                                    content.Selected = ((ListBoxItem) this._voiceChatSettings.Items[i]).IsSelected;
                                    content.SAIsEnabled = false;
                                }
                            }
                        }
                        switch (this._voiceChatSettings.SelectedIndex)
                        {
                            case 0:
                                this._voiceEffects.VoiceOverIPMode = CxVOIPMode.VOIP_Mode_PrivateChat;
                                this.SaveBeamForming(true);
                                break;

                            case 1:
                                this._voiceEffects.VoiceOverIPMode = CxVOIPMode.VOIP_Mode_ConferenceRoom;
                                this.SaveBeamForming(false);
                                break;
                        }
                        if (this.IsFreezeBFEnabled())
                        {
                            this.SetUpGrayedOutControls();
                        }
                        this._voipUpdate = false;
                        if (this._cxHDAudioconfig != null)
                        {
                            this._cxHDAudioconfig.NotifyRegistryViaSAIIService = 0x52;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("VoiceEffectsPage::_voiceEffects_SelectionChanged()", Severity.FATALERROR, exception);
            }
        }

        public void ApplyNewStyle()
        {
            this._voiceRecording.ApplyStyle("SA_VoiceRecording");
            this._voip.ApplyStyle("SA_VoiceOverIP");
        }

        private bool DoesDriverSupportNewIOCTLForBeamForming() => 
            this._voiceEffects.DriverSupportsBeamFormingIOCTL;

        public bool GetBeamForming() => 
            this._voiceEffects.BeamForming;

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (!this._contentLoaded)
            {
                this._contentLoaded = true;
                Uri resourceLocator = new Uri("/SmartAudio;component/voiceeffectspage.xaml", UriKind.Relative);
                Application.LoadComponent(this, resourceLocator);
            }
        }

        public bool InitializePage(CxHDAudioFactory audioFactory)
        {
            try
            {
                this.AudioVoiceEffects = (CxHDAudioVoiceEffectsClass) audioFactory.VoiceEffects;
                this.HDAudioconfig = audioFactory.DeviceIOConfig;
                if (this.IsFreezeBFEnabled())
                {
                    this.SetUpGrayedOutControls();
                }
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("VoiceEffectsPage::InitializePage()", Severity.FATALERROR, exception);
                throw exception;
            }
            return true;
        }

        private bool isAECDisabled()
        {
            App current = Application.Current as App;
            return current.Settings.INISettings.IsAECControlDisabled;
        }

        private bool IsBeamFormingEnabled()
        {
            App current = Application.Current as App;
            return current.Settings.INFSettings.BeamFormingEnabled;
        }

        private bool IsBeamFormingSupportedByHW() => 
            this._voiceEffects.HWSupportsBeamForming;

        private bool IsFreezeBFEnabled()
        {
            App current = Application.Current as App;
            return current.Settings.INISettings.FreezBFSettings;
        }

        private void LoadBeamForming()
        {
            App current = Application.Current as App;
            if (!current.Settings.IsSettingsExisted)
            {
                this.SaveBeamForming(this.AudioVoiceEffects.BeamForming);
            }
            else if (this.AudioVoiceEffects.VoiceRecording)
            {
                this.SaveBeamForming(this.AudioVoiceEffects.BeamForming);
            }
            else
            {
                this.SaveBeamForming(this.AudioVoiceEffects.BeamForming);
            }
        }

        private bool LoadBeamFormingFromSASettings()
        {
            App current = Application.Current as App;
            return current.Settings.BeamForming;
        }

        public bool Localize()
        {
            this._speakerNRLEDText.Text = Resources.SA_CHKSPNOISERED;
            this._AECNRLEDText.Text = Resources.SA_XAML_AcousticEchoCancellation;
            this._MicNRLEDText.Text = Resources.SA_MICNR;
            this._VoiceRecordingText.Text = Resources.SA_VOICE_RECORDING;
            this._VoiceOverIPText.Text = Resources.SA_VOICE_OVER_IP;
            this._PrivateChatText.Text = Resources.SA_XAML_PrivateChat;
            this._ConferenceRoomText.Text = Resources.SA_CONFERENCE_ROOM;
            return true;
        }

        private void RefreshSettings()
        {
            try
            {
                if (this._voiceEffects != null)
                {
                    this._microphoneNR.State = this._voiceEffects.MicrophoneNoiseReduction;
                    this._speakerNR.State = this._voiceEffects.SpeakerNoiseReduction;
                    if (this.isAECDisabled())
                    {
                        this._aec.Visibility = Visibility.Hidden;
                        this._AECNRLEDText.Visibility = Visibility.Hidden;
                        this._speakerNR.Margin = new Thickness(-35.0, 20.0, -60.0, -20.0);
                        this._speakerNRLEDText.Margin = new Thickness(0.0, 20.0, -60.0, -20.0);
                    }
                    else
                    {
                        this._aec.State = this._voiceEffects.AcousticEchoCanceller;
                    }
                }
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("VoiceEffectsPage::RefreshSettings() ", Severity.FATALERROR, exception);
            }
        }

        public void RefreshVOIPControl(bool HPAndMicPlugInState)
        {
            try
            {
                this.RefreshSettings();
                this._voip.IsEnabled = !HPAndMicPlugInState;
                if (!this._voip.IsEnabled)
                {
                    BlurBitmapEffect effect = new BlurBitmapEffect {
                        Radius = 10.0
                    };
                    this._voip.BitmapEffect = effect;
                    this._aec.State = false;
                }
                else
                {
                    this._voip.BitmapEffect = null;
                    this._aec.State = this._voiceEffects.AcousticEchoCanceller;
                }
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("VoiceEffectsPage::RefreshVOIPControl() ", Severity.FATALERROR, exception);
            }
        }

        public void ResetToDefault(bool isEnableBeamforming, bool isEnableMicNR, bool isEnableSpeakerNR, bool isEnableAEC)
        {
            this._privateChat.SAIsEnabled = true;
            this._conferenceRoom.SAIsEnabled = true;
            this.SaveBeamForming(isEnableBeamforming);
            this._voipUpdate = true;
            this._voiceChatSettings.SelectedIndex = isEnableBeamforming ? 0 : 1;
            if ((isEnableMicNR && !isEnableAEC) && !isEnableBeamforming)
            {
                this._voiceEffects.VoiceOverIP = false;
                this._voiceRecording.Selected = true;
                this._voiceControls_OnItemStateChanged(this._voiceRecording, true);
            }
            else if ((isEnableMicNR && !isEnableAEC) && isEnableBeamforming)
            {
                this._voiceEffects.VoiceOverIP = false;
                this._voiceRecording.Selected = true;
                this._voiceControls_OnItemStateChanged(this._voiceRecording, true);
            }
            else if ((isEnableMicNR && isEnableAEC) && !isEnableBeamforming)
            {
                this._voiceEffects.VoiceOverIPMode = CxVOIPMode.VOIP_Mode_ConferenceRoom;
                this._voiceEffects.VoiceRecording = false;
                this._voip.Selected = true;
                this._voiceControls_OnItemStateChanged(this._voip, true);
            }
            else if ((isEnableMicNR && isEnableAEC) && isEnableBeamforming)
            {
                this._voiceEffects.VoiceOverIPMode = CxVOIPMode.VOIP_Mode_PrivateChat;
                this._voiceEffects.VoiceRecording = false;
                this._voip.Selected = true;
                this._voiceControls_OnItemStateChanged(this._voip, true);
            }
            else
            {
                this._voiceEffects.VoiceOverIP = false;
                this._voiceRecording.Selected = false;
            }
            this._privateChat.SAIsEnabled = false;
            this._conferenceRoom.SAIsEnabled = false;
            this._voipUpdate = false;
        }

        private void SaveBeamForming(bool newState)
        {
            this._beamForming = newState;
            App current = Application.Current as App;
            current.Settings.BeamForming = newState;
            current.Settings.Save();
        }

        public bool SetBeamForming(int BFSetting)
        {
            if (BFSetting == 0)
            {
                this._voiceEffects.BeamForming = false;
                if ((this._voiceRecording.Selected || this._voip.Selected) && (this._voiceChatSettings.SelectedIndex != 1))
                {
                    this._voipUpdate = true;
                    this._privateChat.SAIsEnabled = true;
                    this._conferenceRoom.SAIsEnabled = true;
                    this._voiceChatSettings.SelectedIndex = 1;
                }
                return !this._voiceEffects.BeamForming;
            }
            this._voiceEffects.BeamForming = true;
            if (this._voiceChatSettings.SelectedIndex != 0)
            {
                this._voipUpdate = true;
                this._privateChat.SAIsEnabled = true;
                this._conferenceRoom.SAIsEnabled = true;
                this._voiceChatSettings.SelectedIndex = 0;
            }
            return this._voiceEffects.BeamForming;
        }

        private void SetUpGrayedOutControls()
        {
            for (int i = 0; i < this._voiceChatSettings.Items.Count; i++)
            {
                ListBoxItem item = (ListBoxItem) this._voiceChatSettings.Items[i];
                if (!item.IsSelected)
                {
                    BlurBitmapEffect effect = new BlurBitmapEffect {
                        Radius = 10.0
                    };
                    ((ImageListItem) item.Content).BitmapEffect = effect;
                }
                else
                {
                    ((ImageListItem) item.Content).BitmapEffect = null;
                }
                ((ImageListItem) item.Content).IsEnabled = false;
                ((ImageListItem) item.Content).SAIsEnabled = false;
            }
            this._voipUpdate = false;
        }

        private void SetUpGrayedOutControls(int index, bool isGrayedOut)
        {
            ListBoxItem item = (ListBoxItem) this._voiceChatSettings.Items[index];
            if (isGrayedOut)
            {
                BlurBitmapEffect effect = new BlurBitmapEffect {
                    Radius = 10.0
                };
                ((ImageListItem) item.Content).BitmapEffect = effect;
                ((ImageListItem) item.Content).IsEnabled = false;
                ((ImageListItem) item.Content).SAIsEnabled = false;
            }
            else if (((ImageListItem) item.Content).BitmapEffect != null)
            {
                ((ImageListItem) item.Content).BitmapEffect = null;
            }
        }

        private bool ShouldUpdateBeamForming()
        {
            if (this.DoesDriverSupportNewIOCTLForBeamForming())
            {
                return this.IsBeamFormingSupportedByHW();
            }
            return this.IsBeamFormingEnabled();
        }

        public void ShowCurrentSettings()
        {
            try
            {
                this._updating = true;
                if (this._voiceEffects != null)
                {
                    if (this.isAECDisabled())
                    {
                        this._voip.Selected = this._voiceEffects.SpeakerNoiseReduction;
                        if (this._voip.Selected)
                        {
                            this._voiceRecording.Selected = false;
                        }
                        else
                        {
                            this._voiceRecording.Selected = this._voiceEffects.MicrophoneNoiseReduction;
                        }
                    }
                    else
                    {
                        this._voiceRecording.Selected = this._voiceEffects.VoiceRecording;
                        this._voip.Selected = this._voiceEffects.VoiceOverIP;
                    }
                    this.RefreshSettings();
                    this._updating = false;
                    this._voipUpdate = true;
                    if (this._voiceRecording.Selected || this._voip.Selected)
                    {
                        switch (this._voiceEffects.VoiceOverIPMode)
                        {
                            case CxVOIPMode.VOIP_Mode_ConferenceRoom:
                                this._voiceChatSettings.SelectedIndex = 1;
                                this._conferenceRoom.Selected = true;
                                return;

                            case CxVOIPMode.VOIP_Mode_PrivateChat:
                                this._voiceChatSettings.SelectedIndex = 0;
                                this._privateChat.Selected = true;
                                return;
                        }
                    }
                    else
                    {
                        this._voiceEffects.VoiceOverIPMode = CxVOIPMode.VOIP_Mode_ConferenceRoom;
                        this._voiceChatSettings.SelectedIndex = 1;
                        this._conferenceRoom.Selected = true;
                        this.SetUpGrayedOutControls(0, true);
                    }
                }
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("VoiceEffectsPage::ShowCurrentSettings() ", Severity.FATALERROR, exception);
            }
        }

        public void ShowVOIPControlsPanel(bool flag)
        {
            this._voiceChatSettings.Visibility = flag ? Visibility.Visible : Visibility.Hidden;
            this._voiceChatSettingsPanel.Visibility = flag ? Visibility.Visible : Visibility.Hidden;
            this._ConferenceRoomText.Visibility = flag ? Visibility.Visible : Visibility.Hidden;
            this._PrivateChatText.Visibility = flag ? Visibility.Visible : Visibility.Hidden;
        }

        [DebuggerNonUserCode, EditorBrowsable(EditorBrowsableState.Never)]
        void IComponentConnector.Connect(int connectionId, object target)
        {
            switch (connectionId)
            {
                case 1:
                    this.glassBackPlate1 = (GlassBackPlate) target;
                    return;

                case 2:
                    this._voiceChatSettingsPanel = (GlassBackPlate) target;
                    return;

                case 3:
                    this._voiceChatSettings = (ListBox) target;
                    this._voiceChatSettings.MouseLeftButtonDown += new MouseButtonEventHandler(this._voiceChatSettings_ListBoxItem_MouseDown);
                    return;

                case 4:
                    ((ListBoxItem) target).MouseLeftButtonDown += new MouseButtonEventHandler(this._voiceChatSettings_ListBoxItem_MouseDown);
                    return;

                case 5:
                    this._privateChat = (ImageListItem) target;
                    return;

                case 6:
                    ((ListBoxItem) target).MouseLeftButtonDown += new MouseButtonEventHandler(this._voiceChatSettings_ListBoxItem_MouseDown);
                    return;

                case 7:
                    this._conferenceRoom = (ImageListItem) target;
                    return;

                case 8:
                    this._voiceControls = (StackPanel) target;
                    return;

                case 9:
                    this._voiceRecording = (ImageCheckBox) target;
                    return;

                case 10:
                    this._voip = (ImageCheckBox) target;
                    return;

                case 11:
                    this.canvas1 = (Canvas) target;
                    return;

                case 12:
                    this._microphoneNR = (LEDUserControl) target;
                    return;

                case 13:
                    this._speakerNR = (LEDUserControl) target;
                    return;

                case 14:
                    this._aec = (LEDUserControl) target;
                    return;

                case 15:
                    this._MicNRLEDText = (TextBlock) target;
                    return;

                case 0x10:
                    this._speakerNRLEDText = (TextBlock) target;
                    return;

                case 0x11:
                    this._AECNRLEDText = (TextBlock) target;
                    return;

                case 0x12:
                    this._PrivateChatText = (TextBlock) target;
                    return;

                case 0x13:
                    this._ConferenceRoomText = (TextBlock) target;
                    return;

                case 20:
                    this._VoiceOverIPText = (TextBlock) target;
                    return;

                case 0x15:
                    this.textBlock2 = (TextBlock) target;
                    return;

                case 0x16:
                    this._VoiceRecordingText = (TextBlock) target;
                    return;
            }
            this._contentLoaded = true;
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

        public CxHDAudioVoiceEffectsClass AudioVoiceEffects
        {
            get => 
                this._voiceEffects;
            set
            {
                this._voiceEffects = value;
                base.DataContext = value;
                this.LoadBeamForming();
                this.ShowCurrentSettings();
            }
        }

        public string FriendlyName =>
            "Voice Effects Page";

        public CxHDAudioConfig HDAudioconfig
        {
            get => 
                this._cxHDAudioconfig;
            set
            {
                this._cxHDAudioconfig = value;
            }
        }
    }
}

