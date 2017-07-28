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

    public class VoiceEffectsPageNoBeamForming : UserControl, ISmartAudioPage, IComponentConnector
    {
        internal LEDUserControl _aec;
        internal TextBlock _AECNRLEDText;
        private CxHDAudioFactory _audioFactory;
        private bool _contentLoaded;
        private CxHDAudioConfig _cxHDAudioconfig;
        internal TextBlock _MicNRLEDText;
        internal LEDUserControl _microphoneNR;
        internal LEDUserControl _speakerNR;
        internal TextBlock _speakerNRLEDText;
        private bool _updating;
        internal StackPanel _voiceControls;
        private CxHDAudioVoiceEffectsClass _voiceEffects;
        internal TextBlock _VoiceOverIPText;
        internal ImageCheckBox _voiceRecording;
        internal TextBlock _VoiceRecordingText;
        internal ImageCheckBox _voip;
        internal Canvas canvas1;
        internal GlassBackPlate glassBackPlate1;

        public VoiceEffectsPageNoBeamForming()
        {
            try
            {
                this.InitializeComponent();
                this.ApplyNewStyle();
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("VoiceEffectsPageNoBeamForming::VoiceEffectsPageNoBeamForming() ", Severity.FATALERROR, exception);
            }
        }

        [DebuggerNonUserCode]
        internal Delegate _CreateDelegate(Type delegateType, string handler) => 
            Delegate.CreateDelegate(delegateType, this, handler);

        private void _voiceControls_OnItemStateChanged(ImageCheckBox item, bool newState)
        {
            try
            {
                if (item == this._voiceRecording)
                {
                    if (item.Selected)
                    {
                        this._voip.Selected = false;
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
                }
                else if (item == this._voip)
                {
                    if (item.Selected)
                    {
                        this._voiceRecording.Selected = false;
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
                }
                this.RefreshSettings();
                if (this._cxHDAudioconfig != null)
                {
                    this._cxHDAudioconfig.NotifyRegistryViaSAIIService = 0x52;
                }
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("VoiceEffectsPageNoBeamForming::_voiceControls_OnItemStateChanged() ", Severity.FATALERROR, exception);
            }
        }

        private void _voiceEffects_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this._voiceEffects != null)
            {
                this.IsBeamFormingEnabled();
            }
        }

        private void _voipItem_Selected(object sender, RoutedEventArgs e)
        {
        }

        public void ApplyNewStyle()
        {
            this._voiceRecording.ApplyStyle("SA_VoiceRecording");
            this._voip.ApplyStyle("SA_VoiceOverIP");
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (!this._contentLoaded)
            {
                this._contentLoaded = true;
                Uri resourceLocator = new Uri("/SmartAudio;component/voiceeffectspagenobeamforming.xaml", UriKind.Relative);
                Application.LoadComponent(this, resourceLocator);
            }
        }

        public bool InitializePage(CxHDAudioFactory audioFactory)
        {
            try
            {
                this.AudioVoiceEffects = (CxHDAudioVoiceEffectsClass) audioFactory.VoiceEffects;
                this.HDAudioconfig = audioFactory.DeviceIOConfig;
                Keyboard.Focus(this._voiceRecording);
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("VoiceEffectsPageNoBeamForming::InitializePage() ", Severity.FATALERROR, exception);
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

        public bool Localize()
        {
            this._speakerNRLEDText.Text = Resources.SA_CHKSPNOISERED;
            this._AECNRLEDText.Text = Resources.SA_XAML_AcousticEchoCancellation;
            this._MicNRLEDText.Text = Resources.SA_MICNR;
            this._VoiceRecordingText.Text = Resources.SA_VOICE_RECORDING;
            this._VoiceOverIPText.Text = Resources.SA_VOICE_OVER_IP;
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
                SmartAudioLog.Log("VoiceEffectsPageNoBeamForming::RefreshSettings() ", Severity.FATALERROR, exception);
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
            if (isEnableMicNR && !isEnableAEC)
            {
                this._voiceEffects.VoiceOverIP = false;
                this._voiceRecording.Selected = true;
                this._voiceControls_OnItemStateChanged(this._voiceRecording, true);
            }
            else if (isEnableMicNR && isEnableAEC)
            {
                this._voiceEffects.VoiceOverIPMode = CxVOIPMode.VOIP_Mode_ConferenceRoom;
                this._voiceEffects.VoiceRecording = false;
                this._voip.Selected = true;
                this._voiceControls_OnItemStateChanged(this._voip, true);
            }
            else
            {
                this._voiceEffects.VoiceOverIP = false;
                this._voiceEffects.VoiceRecording = false;
                this._voiceRecording.Selected = false;
                this._voip.Selected = false;
                this._voiceControls_OnItemStateChanged(this._voip, false);
                this._voiceControls_OnItemStateChanged(this._voiceRecording, false);
            }
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
                }
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("VoiceEffectsPageNoBeamForming::ShowCurrentSettings() ", Severity.FATALERROR, exception);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), DebuggerNonUserCode]
        void IComponentConnector.Connect(int connectionId, object target)
        {
            switch (connectionId)
            {
                case 1:
                    this.glassBackPlate1 = (GlassBackPlate) target;
                    return;

                case 2:
                    this._voiceControls = (StackPanel) target;
                    return;

                case 3:
                    this._voiceRecording = (ImageCheckBox) target;
                    return;

                case 4:
                    this._voip = (ImageCheckBox) target;
                    return;

                case 5:
                    this._VoiceOverIPText = (TextBlock) target;
                    return;

                case 6:
                    this.canvas1 = (Canvas) target;
                    return;

                case 7:
                    this._microphoneNR = (LEDUserControl) target;
                    return;

                case 8:
                    this._speakerNR = (LEDUserControl) target;
                    return;

                case 9:
                    this._aec = (LEDUserControl) target;
                    return;

                case 10:
                    this._speakerNRLEDText = (TextBlock) target;
                    return;

                case 11:
                    this._AECNRLEDText = (TextBlock) target;
                    return;

                case 12:
                    this._MicNRLEDText = (TextBlock) target;
                    return;

                case 13:
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

