namespace SmartAudio
{
    using CxHDAudioAPILib;
    using System;
    using System.IO;
    using System.Text;
    using System.Windows;
    using System.Xml;
    using System.Xml.Serialization;

    [XmlRoot("SmartAudioSettings", Namespace="", IsNullable=false)]
    public class SmartAudioSettings
    {
        private bool _beamForming = false;
        private string _blueStreamFileName = "BlueStream.wav";
        private string _centerSpeakerToneName = "CenterSpeakerStream.wav";
        private bool _hpLimiterImage = false;
        private bool _hpLimiterSetting = false;
        private bool _IsSettingsExisted;
        private string _leftSpeakerToneName = "LeftSpeakerStream.wav";
        private string _musicFileName = "CnxtMusic.wav";
        private bool _overrideCurrentLocale = false;
        private string _rearLeftSpeakerToneName = "RearLeftSpeakerStream.wav";
        private string _rearLeftSpeakerToneNameQuad = "RearLeftSpeakerStreamQuad.wav";
        private string _rearRightSpeakerToneName = "RearRightSpeakerStream.wav";
        private string _rearRightSpeakerToneNameQuad = "RearRightSpeakerStreamQuad.wav";
        private string _redStreamFileName = "RedStream.wav";
        private string _rightSpeakerToneName = "RightSpeakerStream.wav";
        private bool _runInSystemTray = false;
        private SmartAudioINFSettings _saInfSettings;
        private SmartAudioINISettings _saINISettings;
        private int _selectedEQEndPoint = 0;
        private string _selectedLocale = "en-US";
        private Guid _selectedSkin = Guid.Empty;
        private bool _showJackpopup = true;
        private CxHDAudio3DEffects _SP3DEffect = CxHDAudio3DEffects.No3dEffects;
        private double _spectrumAnalyzerMaxValue = 0.0;
        private double _spectrumAnalyzerMinValue = -120.0;
        private int _spectrumAnalyzerRefreshTimer = 100;
        private string _subwooferToneName = "SubwooferStream.wav";
        private string _voiceFileName = "CnxtVoice.wav";

        private SmartAudioSettings()
        {
        }

        public static SmartAudioSettings CreateSettings()
        {
            SmartAudioSettings settings = Load();
            if (settings == null)
            {
                settings = new SmartAudioSettings {
                    IsSettingsExisted = false
                };
                settings.Save();
                return settings;
            }
            settings.IsSettingsExisted = true;
            return settings;
        }

        public void InitHeadPhoneLimiterImage()
        {
            try
            {
                App current = Application.Current as App;
                if (current.AudioFactory != null)
                {
                    this._hpLimiterImage = current.AudioFactory.DeviceIOConfig.HeadPhoneLimiterImage;
                }
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("SmartAudioSettings::InitHeadPhoneLimiterImage(): Failed to read HeadPhoneLimiterImage. " + exception.ToString(), new object[] { Severity.FATALERROR });
            }
        }

        public void InitHPLimiterSetting()
        {
            try
            {
                App current = Application.Current as App;
                if (current.AudioFactory != null)
                {
                    this._hpLimiterSetting = current.AudioFactory.DeviceIOConfig.HeadPhoneLimiter;
                }
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("SmartAudioSettings::InitHeadPhoneLimiterSetting(): Failed to read HeadPhoneLimiterSetting. " + exception.ToString(), new object[] { Severity.FATALERROR });
            }
        }

        public void InitRunInTaskBarSetting()
        {
            this._runInSystemTray = this.ReadValueFromRegistry();
        }

        public static SmartAudioSettings Load()
        {
            try
            {
                TextReader reader = new StreamReader(SettingsPath);
                string pXmlString = reader.ReadToEnd();
                reader.Close();
                XmlSerializer serializer = new XmlSerializer(typeof(SmartAudioSettings));
                MemoryStream w = new MemoryStream(StringToUTF8ByteArray(pXmlString));
                new XmlTextWriter(w, Encoding.UTF8);
                return (SmartAudioSettings) serializer.Deserialize(w);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return null;
            }
        }

        public bool LoadInfSettings(CxHDAudioFactory audioFactory)
        {
            this._saInfSettings = new SmartAudioINFSettings(audioFactory);
            return true;
        }

        public bool LoadINISettings()
        {
            this._saINISettings = new SmartAudioINISettings();
            return true;
        }

        public bool ReadValueFromRegistry()
        {
            bool runInTaskbarSetting = false;
            try
            {
                App current = Application.Current as App;
                if (current.AudioFactory != null)
                {
                    runInTaskbarSetting = current.AudioFactory.DeviceIOConfig.RunInTaskbarSetting;
                }
                return runInTaskbarSetting;
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("SmartAudioSettings::ReadValueFromRegistry(): Failed to read SmartAudio AutoRun Settings. " + exception.ToString(), new object[] { Severity.FATALERROR });
                return runInTaskbarSetting;
            }
        }

        public bool Save()
        {
            bool flag = false;
            if (SettingsPath != null)
            {
                try
                {
                    MemoryStream w = new MemoryStream();
                    XmlSerializer serializer = new XmlSerializer(typeof(SmartAudioSettings));
                    XmlTextWriter writer = new XmlTextWriter(w, Encoding.UTF8);
                    serializer.Serialize((XmlWriter) writer, this);
                    string str = this.UTF8ByteArrayToString(((MemoryStream) writer.BaseStream).ToArray());
                    TextWriter writer2 = new StreamWriter(SettingsPath);
                    writer2.Write(str);
                    writer2.Close();
                    flag = true;
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                }
            }
            return flag;
        }

        private static byte[] StringToUTF8ByteArray(string pXmlString)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            return encoding.GetBytes(pXmlString);
        }

        private string UTF8ByteArrayToString(byte[] characters)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            return encoding.GetString(characters);
        }

        public static string ApplicationFolder
        {
            get
            {
                string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                if (folderPath.Length == 0)
                {
                    return null;
                }
                folderPath = folderPath + @"\Conexant\SmartAudio";
                new FileInfo(folderPath);
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                return folderPath;
            }
        }

        public bool BeamForming
        {
            get => 
                this._beamForming;
            set
            {
                this._beamForming = value;
            }
        }

        public string BlueStreamFileName
        {
            get => 
                this._blueStreamFileName;
            set
            {
                this._blueStreamFileName = value;
            }
        }

        public string CenterSpeakerToneName
        {
            get => 
                this._centerSpeakerToneName;
            set
            {
                this._centerSpeakerToneName = value;
            }
        }

        public bool HPLimiterImage =>
            this._hpLimiterImage;

        public bool HPLimiterSetting
        {
            get => 
                this._hpLimiterSetting;
            set
            {
                this._hpLimiterSetting = value;
            }
        }

        public SmartAudioINFSettings INFSettings =>
            this._saInfSettings;

        public SmartAudioINISettings INISettings =>
            this._saINISettings;

        public bool IsSettingsExisted
        {
            get => 
                this._IsSettingsExisted;
            set
            {
                this._IsSettingsExisted = value;
            }
        }

        public string LeftSpeakerToneName
        {
            get => 
                this._leftSpeakerToneName;
            set
            {
                this._leftSpeakerToneName = value;
            }
        }

        public static string LogFilePath
        {
            get
            {
                if (ApplicationFolder != null)
                {
                    return (ApplicationFolder + @"\SALog.xml");
                }
                return null;
            }
        }

        public string MusicFileName
        {
            get => 
                this._musicFileName;
            set
            {
                this._musicFileName = value;
            }
        }

        public bool OverrideCurrentLocale
        {
            get => 
                this._overrideCurrentLocale;
            set
            {
                this._overrideCurrentLocale = value;
            }
        }

        public string RearLeftSpeakerToneName
        {
            get => 
                this._rearLeftSpeakerToneName;
            set
            {
                this._rearLeftSpeakerToneName = value;
            }
        }

        public string RearLeftSpeakerToneNameQuad
        {
            get => 
                this._rearLeftSpeakerToneNameQuad;
            set
            {
                this._rearLeftSpeakerToneNameQuad = value;
            }
        }

        public string RearRightSpeakerToneName
        {
            get => 
                this._rearRightSpeakerToneName;
            set
            {
                this._rearRightSpeakerToneName = value;
            }
        }

        public string RearRightSpeakerToneNameQuad
        {
            get => 
                this._rearRightSpeakerToneNameQuad;
            set
            {
                this._rearRightSpeakerToneNameQuad = value;
            }
        }

        public string RedStreamFileName
        {
            get => 
                this._redStreamFileName;
            set
            {
                this._redStreamFileName = value;
            }
        }

        public string RightSpeakerToneName
        {
            get => 
                this._rightSpeakerToneName;
            set
            {
                this._rightSpeakerToneName = value;
            }
        }

        public bool RunOnSystemTray
        {
            get => 
                this._runInSystemTray;
            set
            {
                this._runInSystemTray = value;
            }
        }

        public int SelectedEQEndPoint
        {
            get => 
                this._selectedEQEndPoint;
            set
            {
                this._selectedEQEndPoint = value;
            }
        }

        public string SelectedLocale
        {
            get => 
                this._selectedLocale;
            set
            {
                this._selectedLocale = value;
            }
        }

        public Guid SelectedSkin
        {
            get => 
                this._selectedSkin;
            set
            {
                this._selectedSkin = value;
            }
        }

        public static string SettingsPath
        {
            get
            {
                if (ApplicationFolder != null)
                {
                    return (ApplicationFolder + @"\SASettings.xml");
                }
                return null;
            }
        }

        public bool ShowJackpopup
        {
            get => 
                this._showJackpopup;
            set
            {
                this._showJackpopup = value;
            }
        }

        public CxHDAudio3DEffects SP3DEffect
        {
            get => 
                this._SP3DEffect;
            set
            {
                this._SP3DEffect = value;
            }
        }

        public double SpectrumAnalyzerMaxValue
        {
            get => 
                this._spectrumAnalyzerMaxValue;
            set
            {
                this._spectrumAnalyzerMaxValue = value;
            }
        }

        public double SpectrumAnalyzerMinValue
        {
            get => 
                this._spectrumAnalyzerMinValue;
            set
            {
                this._spectrumAnalyzerMinValue = value;
            }
        }

        public int SpectrumAnalyzerRefreshTime
        {
            get => 
                this._spectrumAnalyzerRefreshTimer;
            set
            {
                this._spectrumAnalyzerRefreshTimer = value;
            }
        }

        public string SubwooferToneName
        {
            get => 
                this._subwooferToneName;
            set
            {
                this._subwooferToneName = value;
            }
        }

        public string VoiceFileName
        {
            get => 
                this._voiceFileName;
            set
            {
                this._voiceFileName = value;
            }
        }
    }
}

