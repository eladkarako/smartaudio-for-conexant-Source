namespace SmartAudio
{
    using System;
    using System.Windows;

    public class SmartAudioINISettings
    {
        public const int SA_3DImmersionUnavailable = 0x23;
        public const int SA_3DPageAvailable = 20;
        public const int SA_ACERForteEnhancements = 30;
        public const int SA_ACERVoiceEnhancements = 0x1f;
        public const int SA_BeamForming = 3;
        public const int SA_BluetoothAvaiable = 0x11;
        public const int SA_ConfigValueAndAudioTest = 0;
        public const int SA_DisableAECControl = 0x2a;
        public const int SA_DisableAudioDirector = 0x19;
        public const int SA_DisableDolbyDigitalLive = 13;
        public const int SA_DisableJackRetask = 0x2c;
        public const int SA_DisableLayeredWindow = 0x26;
        public const int SA_DisableMicNRControl = 40;
        public const int SA_DisableMinMaxWindow = 50;
        public const int SA_DisableRunInTaskbarOption = 0x16;
        public const int SA_DisableSpkrNRControl = 0x29;
        public const int SA_DisableVolumeLevelMeter = 0x25;
        public const int SA_DisableVPAPage = 0x27;
        public const int SA_DOLBY = 11;
        public const int SA_DolbyHotKey = 14;
        public const int SA_DSR = 8;
        public const int SA_EnableGSMarkLimiter = 15;
        public const int SA_EnableSkinMenu = 0x18;
        public const int SA_EnableXSign = 0x2d;
        public const int SA_EnhanceAudio = 2;
        public const int SA_EQHotKey = 10;
        public const int SA_Equalizer = 1;
        public const int SA_FreezeBFSettings = 0x17;
        public const int SA_HDMIAvailable = 0x12;
        public const int SA_Hide3DImmersionSlider = 0x21;
        public const int SA_HidePhantomBassSlider = 0x1c;
        public const int SA_HidePhantomSpeakerSlider = 0x20;
        public const int SA_Information = 6;
        public const int SA_InvisibleBeamForming = 0x10;
        public const int SA_IsMultiChannelSupported = 0x2e;
        public const int SA_LanguageSelection = 7;
        public const int SA_LenovoPackage = 0x1d;
        public const int SA_MaxxAudioAvailable = 0x13;
        public const int SA_PerformanceTestButton = 5;
        public const int SA_PhantomBassAvailable = 0x1b;
        public const int SA_PhantomSpeakerUnavailable = 0x22;
        public const int SA_RearPanelSpeakerSetupEnabled = 0x2f;
        public const int SA_RunInSytemTray = 0x15;
        public const int SA_SAEQ = 12;
        public const int SA_ScrollBarEnabled = 0x30;
        public const int SA_SetDefEndPointAfterJackRetask = 0x2b;
        public const int SA_ShowPluggedHPLimiterImages = 0x24;
        public const int SA_SRS = 9;
        public const int SA_UseBigToolbarButton = 0x31;
        public const int SA_VPA = 4;

        private bool IsBitSet(uint bit)
        {
            bool flag = false;
            try
            {
                App current = Application.Current as App;
                if ((current != null) && (current.AudioFactory != null))
                {
                    flag = current.AudioFactory.DeviceIOConfig.get_SAIISmartAudioSettingsBit(bit) > 0;
                }
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("SmartAudioINFSettings::IsBitSet(): Failed to load SmartAudioINI Settings for bit " + bit, Severity.FATALERROR, exception);
            }
            return flag;
        }

        public bool DolbyEnabled =>
            !this.IsBitSet(11);

        public bool EnableSkinMenu =>
            this.IsBitSet(0x18);

        public bool EQPageEnabled =>
            !this.IsBitSet(12);

        public bool FreezBFSettings =>
            this.IsBitSet(0x17);

        public bool Is3DImmersionrUnavailable =>
            this.IsBitSet(0x23);

        public bool Is3DImmersionSliderHidden =>
            this.IsBitSet(0x21);

        public bool IsAECControlDisabled =>
            this.IsBitSet(0x2a);

        public bool IsAudioRedirectorDisabled =>
            this.IsBitSet(0x19);

        public bool IsJackRetaskingEnabled =>
            !this.IsBitSet(0x2c);

        public bool IsLayeredWindowEnabled
        {
            get
            {
                if (!this.IsBitSet(0x26))
                {
                    return this.IsScrollBarsEnabled;
                }
                return true;
            }
        }

        public bool IsLenovoPackageEnabled =>
            this.IsBitSet(0x1d);

        public bool IsMicNRControlDisabled =>
            this.IsBitSet(40);

        public bool IsMinimizeMaximizeWindowDisabled =>
            this.IsBitSet(50);

        public bool IsMultiChannelSupported =>
            this.IsBitSet(0x2e);

        public bool IsPhantomBassEnabled =>
            this.IsBitSet(0x1b);

        public bool IsPhantomBassSliderHidden =>
            this.IsBitSet(0x1c);

        public bool IsPhantomSpeakerUnavailable =>
            this.IsBitSet(0x22);

        public bool IsPhantomSpekaerSliderHidden =>
            this.IsBitSet(0x20);

        public bool IsRearPanelSpeakerSetupEnabled =>
            this.IsBitSet(0x2f);

        public bool IsScrollBarsEnabled =>
            this.IsBitSet(0x30);

        public bool IsSetDefEndPointAfterJackRetaskEnabled =>
            this.IsBitSet(0x2b);

        public bool IsSpkrNRControlDisabled =>
            this.IsBitSet(0x29);

        public bool IsUseBigToolbarButton =>
            this.IsBitSet(0x31);

        public bool IsVolumeLevelMeterEnabled =>
            !this.IsBitSet(0x25);

        public bool IsVPAPageDisabled =>
            this.IsBitSet(0x27);

        public bool IsXSignEnabled =>
            this.IsBitSet(0x2d);

        public bool RunInSystemTrayEnabled =>
            this.IsBitSet(0x15);

        public bool RunInTaskBarOptionDisabled =>
            this.IsBitSet(0x16);

        public bool ShowPluggedHPLimiterImages =>
            this.IsBitSet(0x24);

        public bool SRSEnabled =>
            this.IsBitSet(9);
    }
}

