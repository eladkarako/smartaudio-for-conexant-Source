namespace SmartAudio
{
    using CxHDAudioAPILib;
    using System;
    using System.Windows;

    public class SmartAudioINFSettings
    {
        private uint m_dwConfigValue = 0;
        public const int SA_3DPageAvailable = 0x100000;
        public const int SA_ACERForteEnhancements = 0x40000000;
        public const int SA_ACERVoiceEnhancements = -2147483648;
        public const int SA_BeamForming = 8;
        public const int SA_BluetoothAvaiable = 0x20000;
        public const int SA_ConfigValueAndAudioTest = 1;
        public const int SA_DisableAudioDirector = 0x2000000;
        public const int SA_DisableDolbyDigitalLive = 0x2000;
        public const int SA_DisableRunInTaskbarOption = 0x400000;
        public const int SA_DOLBY = 0x800;
        public const int SA_DolbyHotKey = 0x4000;
        public const int SA_DSR = 0x100;
        public const int SA_EnableGSMarkLimiter = 0x8000;
        public const int SA_EnableSkinMenu = 0x1000000;
        public const int SA_EnhanceAudio = 4;
        public const int SA_EQHotKey = 0x400;
        public const int SA_Equalizer = 2;
        public const int SA_FreezeBFSettings = 0x800000;
        public const int SA_HDMIAvailable = 0x40000;
        public const int SA_Hide3DImmersionSlider = 2;
        public const int SA_HidePhantomBassSlider = 0x10000000;
        public const int SA_HidePhantomSpeakerSlider = 1;
        public const int SA_Information = 0x40;
        public const int SA_InvisibleBeamForming = 0x10000;
        public const int SA_LanguageSelection = 0x80;
        public const int SA_LenovoPackage = 0x20000000;
        public const int SA_MaxxAudioAvailable = 0x80000;
        public const int SA_PerformanceTestButton = 0x20;
        public const int SA_PhantomBassAvailable = 0x8000000;
        public const int SA_RunInSytemTray = 0x200000;
        public const int SA_SAEQ = 0x1000;
        public const int SA_SRS = 0x200;
        public const int SA_VPA = 0x10;

        public SmartAudioINFSettings(CxHDAudioFactory audioFactory)
        {
            this.LoadINFSettings(audioFactory);
        }

        private void LoadINFSettings(CxHDAudioFactory audioFactory)
        {
            try
            {
                this.m_dwConfigValue = audioFactory.DeviceIOConfig.SmartAudioSettings;
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("SmartAudioINFSettings::SmartAudioINFSettings(): Failed to load SmartAudioINF Settings", Severity.FATALERROR, exception);
            }
        }

        public bool BeamFormingEnabled
        {
            get
            {
                if ((this.m_dwConfigValue & 8) <= 0)
                {
                    return false;
                }
                return true;
            }
        }

        public bool BluetoothAvailable
        {
            get
            {
                if ((this.m_dwConfigValue & 0x20000) <= 0)
                {
                    return false;
                }
                return true;
            }
        }

        public bool DOLBYEnabled
        {
            get
            {
                if ((this.m_dwConfigValue & 0x800) <= 0)
                {
                    return false;
                }
                return true;
            }
        }

        public bool DSREnabled
        {
            get
            {
                if ((this.m_dwConfigValue & 0x100) <= 0)
                {
                    return false;
                }
                return true;
            }
        }

        public bool EnhancedAudioEnabled
        {
            get
            {
                if ((this.m_dwConfigValue & 4) <= 0)
                {
                    return false;
                }
                return true;
            }
        }

        public bool EqualizerEnabled
        {
            get
            {
                if ((this.m_dwConfigValue & 2) <= 0)
                {
                    return false;
                }
                return true;
            }
        }

        private bool GSMarkLimiterEnabled
        {
            get
            {
                if ((this.m_dwConfigValue & 0x8000) <= 0)
                {
                    return false;
                }
                return true;
            }
        }

        public bool HDMIAvailable
        {
            get
            {
                if ((this.m_dwConfigValue & 0x40000) <= 0)
                {
                    return false;
                }
                return true;
            }
        }

        public bool LanguageSelectionEnabled
        {
            get
            {
                if ((this.m_dwConfigValue & 0x80) <= 0)
                {
                    return false;
                }
                return true;
            }
        }

        public bool MaxxAudioAvailable
        {
            get
            {
                App current = Application.Current as App;
                return (((this.m_dwConfigValue & 0x80000) > 0) && current.AudioFactory.IsMaxxAppEnabled);
            }
        }

        public bool PerformanceTestEnabled
        {
            get
            {
                if ((this.m_dwConfigValue & 0x20) <= 0)
                {
                    return false;
                }
                return true;
            }
        }

        public bool RunInSystemTrayEnabled
        {
            get
            {
                if ((this.m_dwConfigValue & 0x200000) <= 0)
                {
                    return false;
                }
                return true;
            }
        }

        public bool ThreeDimensionPageEnabled
        {
            get
            {
                if ((this.m_dwConfigValue & 0x100000) >= 1)
                {
                    return false;
                }
                return true;
            }
        }

        public bool VPAEnabled
        {
            get
            {
                if ((this.m_dwConfigValue & 0x10) <= 0)
                {
                    return false;
                }
                return true;
            }
        }
    }
}

