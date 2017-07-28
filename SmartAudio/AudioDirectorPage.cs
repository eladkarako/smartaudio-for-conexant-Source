namespace SmartAudio
{
    using CxHDAudioAPILib;
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Markup;
    using System.Windows.Media;
    using System.Windows.Threading;

    public class AudioDirectorPage : UserControl, ISmartAudioPage, IComponentConnector
    {
        internal ImageCheckBox _advancedOptions;
        private long _afaDelayValue;
        private CxHDAudioDirector _audioDirector;
        private long _audioDirectorDelayValue;
        private CxHDAudioFactory _audioFactory;
        internal Image _classicModeImage;
        private bool _contentLoaded;
        internal ImageCheckBox _multistreamMode;
        internal Image _multistreamModeImage;
        internal TextBlock _multiStreamModeText;
        internal AudioDirectorPreview _preview;
        internal ImageCheckBox _singleStreamMode;
        internal TextBlock _singleStreamModeText;
        private Brush _unselectedForeground;
        private const string AFA_CLASS_NAME = "Conexant ADR Agent";
        private DateTime DateTimeOfLastChanged;
        internal Image image3;
        private const int MULTISTREAM_CHANGE_BEGIN = 0;
        private const int MULTISTREAM_CHANGE_END = 1;
        private Cursor oldCursor;
        private const int WM_MULTISTREAM_CHANGE = 0x1400;
        private const int WM_USER = 0x400;

        public AudioDirectorPage()
        {
            try
            {
                this.InitializeComponent();
                this._singleStreamModeText.MouseLeftButtonDown += new MouseButtonEventHandler(this._singleStreamModeText_MouseLeftButtonDown);
                this._multiStreamModeText.MouseLeftButtonDown += new MouseButtonEventHandler(this._multiStreamModeText_MouseLeftButtonDown);
                this.DateTimeOfLastChanged = DateTime.Now;
                this._multistreamMode.ApplyStyle("SA_AudioDirectorSingleStreamMode");
                this._singleStreamMode.ApplyStyle("SA_AudioDirectorMultistreamMode");
                this.ApplyNewStyle();
                App current = Application.Current as App;
                if (current.AudioFactory != null)
                {
                    try
                    {
                        this._afaDelayValue = current.AudioFactory.DeviceIOConfig.AFADelayValue;
                    }
                    catch (Exception exception)
                    {
                        SmartAudioLog.Log("OnChangeAudioDirectorMode : Failed to get AFADelayValue", Severity.INFORMATION, exception);
                        this._afaDelayValue = 6L;
                    }
                    try
                    {
                        this._audioDirectorDelayValue = current.AudioFactory.DeviceIOConfig.AudioDirectorDelayValue;
                    }
                    catch (Exception exception2)
                    {
                        SmartAudioLog.Log("OnChangeAudioDirectorMode : Failed to get AudioDirectorDelayValue", Severity.INFORMATION, exception2);
                        this._audioDirectorDelayValue = 30L;
                    }
                }
            }
            catch (Exception exception3)
            {
                SmartAudioLog.Log("AudioDirectorPage::InitializeComponent() ", Severity.FATALERROR, exception3);
            }
            this._unselectedForeground = this._multiStreamModeText.Foreground;
        }

        private void _audioFactory_OnHeadphoneStatusChanged(CxJackPluginStatus newStatus)
        {
            this._audioFactory.remove_OnHeadphoneStatusChanged(new _ICxHDAudioFactoryEvents_OnHeadphoneStatusChangedEventHandler(this._audioFactory_OnHeadphoneStatusChanged));
            base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new ReEnumerate(this.OnReEnumerate));
        }

        [DebuggerNonUserCode]
        internal Delegate _CreateDelegate(Type delegateType, string handler) => 
            Delegate.CreateDelegate(delegateType, this, handler);

        private void _multiStreamModeText_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.AudioDirector_OnItemStateChanged(this._multistreamMode, true);
        }

        private void _singleStreamModeText_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.AudioDirector_OnItemStateChanged(this._singleStreamMode, true);
        }

        public void ApplyNewStyle()
        {
            this._multistreamMode.ApplyStyle("SA_AudioDirectorSingleStreamMode");
            this._singleStreamMode.ApplyStyle("SA_AudioDirectorMultistreamMode");
            if (this._audioDirector != null)
            {
                this._preview.AudioDirectorMode = this._audioDirector.AudioDirectorMode;
            }
        }

        private void AudioDirector_AdvancedOptionsChanged(ImageCheckBox item, bool newState)
        {
            this.ShowAdvancedMultiSteamOptions(this._advancedOptions.Selected, true);
        }

        private void AudioDirector_OnItemStateChanged(ImageCheckBox item, bool newState)
        {
            bool flag = false;
            App current = Application.Current as App;
            try
            {
                if ((item == this._multistreamMode) && this._multistreamMode.Selected)
                {
                    try
                    {
                        if (this._audioDirector.AudioDirectorMode != CxHDAudioAudioDirectorMode.MultiStream)
                        {
                            flag = true;
                            this._singleStreamMode.Selected = false;
                        }
                    }
                    catch (Exception exception)
                    {
                        SmartAudioLog.Log("AudioDirectorPage::AudioDirector_OnItemStateChanged - _audioDirector.AudioDirectorMode == CxHDAudioAudioDirectorMode.MultiStream", Severity.FATALERROR, exception);
                    }
                }
                else if ((item == this._singleStreamMode) && this._singleStreamMode.Selected)
                {
                    try
                    {
                        if (this._audioDirector.AudioDirectorMode != CxHDAudioAudioDirectorMode.SingleStream)
                        {
                            flag = true;
                            this._multistreamMode.Selected = false;
                        }
                    }
                    catch (Exception exception2)
                    {
                        SmartAudioLog.Log("AudioDirectorPage::AudioDirector_OnItemStateChanged - _audioDirector.AudioDirectorMode == CxHDAudioAudioDirectorMode.SingleStream", Severity.FATALERROR, exception2);
                    }
                }
                if (flag && !current.IsDemoMode)
                {
                    if (Cursors.Wait != Application.Current.MainWindow.Cursor)
                    {
                        this.oldCursor = Application.Current.MainWindow.Cursor;
                        Application.Current.MainWindow.Cursor = Cursors.Wait;
                    }
                    PostMessage(FindWindow("Conexant ADR Agent", null), 0x1400, 0, 0);
                    Thread.Sleep((int) (((int) this._afaDelayValue) * 100));
                    if (this._singleStreamMode.Selected)
                    {
                        base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new ChangeAudioDirectorMode(this.OnChangeAudioDirectorMode), CxHDAudioAudioDirectorMode.SingleStream);
                    }
                    else if (this._multistreamMode.Selected)
                    {
                        base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new ChangeAudioDirectorMode(this.OnChangeAudioDirectorMode), CxHDAudioAudioDirectorMode.MultiStream);
                    }
                    this._multistreamMode.IsEnabled = false;
                    this._singleStreamMode.IsEnabled = false;
                    this.ChangeTextColor();
                }
            }
            catch (Exception exception3)
            {
                SmartAudioLog.Log("AudioDirectorPage::AudioDirector_OnItemStateChanged", Severity.FATALERROR, exception3);
            }
        }

        private void AudioDirector_RedirectionScheme(ImageCheckBox item, bool newState)
        {
        }

        private void AudioFactory_OnHeadphoneStatusChanged(CxJackPluginStatus newStatus)
        {
            this._preview.AudioDirectorMode = this._audioDirector.AudioDirectorMode;
        }

        private void ChangeTextColor()
        {
            this._multiStreamModeText.Foreground = this._multistreamMode.Selected ? Brushes.White : this._unselectedForeground;
            this._singleStreamModeText.Foreground = this._singleStreamMode.Selected ? Brushes.White : this._unselectedForeground;
        }

        [DllImport("user32.dll", SetLastError=true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (!this._contentLoaded)
            {
                this._contentLoaded = true;
                Uri resourceLocator = new Uri("/SmartAudio;component/audiodirectorpage.xaml", UriKind.Relative);
                Application.LoadComponent(this, resourceLocator);
            }
        }

        public bool InitializePage(CxHDAudioFactory audioFactory)
        {
            try
            {
                this.AudioDirector = audioFactory.AudioDirector;
                Keyboard.Focus(this._singleStreamMode);
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("AudioDirectorPage::InitializePage()", Severity.FATALERROR, exception);
                throw exception;
            }
            return true;
        }

        public bool Localize() => 
            true;

        private void OnChangeAudioDirectorMode(CxHDAudioAudioDirectorMode mode)
        {
            App current = Application.Current as App;
            long num = this.DateTimeOfLastChanged.ToFileTime();
            long num2 = DateTime.Now.ToFileTime();
            if (!current.IsDemoMode && ((num + ((this._audioDirectorDelayValue * 0x3e8L) * 0x3e8L)) >= num2))
            {
                base.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new ChangeAudioDirectorMode(this.OnChangeAudioDirectorMode), mode);
            }
            else
            {
                try
                {
                    this._audioDirector.AudioDirectorMode = mode;
                    PostMessage(FindWindow("Conexant ADR Agent", null), 0x1400, 1, 0);
                    if (((App) Application.Current).SpeakerConfigType == CxSpeakerConfigType.FiveDotOneSpeakers)
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            Thread.Sleep(500);
                            if (this._audioDirector.AudioDirectorMode == mode)
                            {
                                break;
                            }
                        }
                        this.UpdatedAudioDirectorMode();
                    }
                }
                catch (Exception exception)
                {
                    SmartAudioLog.Log("AudioDirectorPage::OnChangeAudioDirectorMode(): Setting AudioDirectorMode Failed ", Severity.FATALERROR, exception);
                }
            }
        }

        private void OnReEnumerate()
        {
            this.ShowPreview(this._audioDirector.AudioDirectorMode);
            this._audioFactory.add_OnHeadphoneStatusChanged(new _ICxHDAudioFactoryEvents_OnHeadphoneStatusChangedEventHandler(this._audioFactory_OnHeadphoneStatusChanged));
        }

        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr PostMessage(IntPtr hWnd, int msg, int wParam, int IParam);
        public void ResetToClassicMode()
        {
            this._singleStreamMode.Selected = true;
            this.AudioDirector_OnItemStateChanged(this._singleStreamMode, true);
        }

        private void ShowAdvancedMultiSteamOptions(bool bShow, bool animate)
        {
        }

        private void ShowPreview(CxHDAudioAudioDirectorMode mode)
        {
            this._preview.AudioDirectorMode = mode;
            if (mode == CxHDAudioAudioDirectorMode.SingleStream)
            {
                this._classicModeImage.Visibility = Visibility.Visible;
                this._multistreamModeImage.Visibility = Visibility.Hidden;
            }
            else if (mode == CxHDAudioAudioDirectorMode.MultiStream)
            {
                this._classicModeImage.Visibility = Visibility.Hidden;
                this._multistreamModeImage.Visibility = Visibility.Visible;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), DebuggerNonUserCode]
        void IComponentConnector.Connect(int connectionId, object target)
        {
            switch (connectionId)
            {
                case 1:
                    this.image3 = (Image) target;
                    return;

                case 2:
                    this._classicModeImage = (Image) target;
                    return;

                case 3:
                    this._multistreamModeImage = (Image) target;
                    return;

                case 4:
                    this._advancedOptions = (ImageCheckBox) target;
                    return;

                case 5:
                    this._preview = (AudioDirectorPreview) target;
                    return;

                case 6:
                    this._multistreamMode = (ImageCheckBox) target;
                    return;

                case 7:
                    this._singleStreamMode = (ImageCheckBox) target;
                    return;

                case 8:
                    this._singleStreamModeText = (TextBlock) target;
                    return;

                case 9:
                    this._multiStreamModeText = (TextBlock) target;
                    return;
            }
            this._contentLoaded = true;
        }

        public void UpdatedAudioDirectorMode()
        {
            Application.Current.MainWindow.Cursor = this.oldCursor;
            this.ShowPreview(this._audioDirector.AudioDirectorMode);
            this._multistreamMode.IsEnabled = true;
            this._singleStreamMode.IsEnabled = true;
            this.DateTimeOfLastChanged = DateTime.Now;
        }

        public CxHDAudioDirector AudioDirector
        {
            get => 
                this._audioDirector;
            set
            {
                try
                {
                    this._audioDirector = value;
                    this._preview.AudioDirector = value;
                    if (this._audioDirector != null)
                    {
                        App current = Application.Current as App;
                        current.AudioFactory.add_OnHeadphoneStatusChanged(new _ICxHDAudioFactoryEvents_OnHeadphoneStatusChangedEventHandler(this.AudioFactory_OnHeadphoneStatusChanged));
                        if (this._audioDirector.AudioDirectorMode == CxHDAudioAudioDirectorMode.MultiStream)
                        {
                            this._multistreamMode.Selected = true;
                            if (this._audioDirector.RedirectionScheme.HeadphoneToInternalSpeaker || this._audioDirector.RedirectionScheme.InternalSpeakerToHeadphone)
                            {
                                this.ShowAdvancedMultiSteamOptions(true, false);
                            }
                            else
                            {
                                this.ShowAdvancedMultiSteamOptions(false, false);
                            }
                        }
                        else if (this._audioDirector.AudioDirectorMode == CxHDAudioAudioDirectorMode.SingleStream)
                        {
                            this._singleStreamMode.Selected = true;
                            this.ShowAdvancedMultiSteamOptions(false, false);
                        }
                        this.ShowPreview(this._audioDirector.AudioDirectorMode);
                        this.ChangeTextColor();
                    }
                }
                catch (Exception exception)
                {
                    SmartAudioLog.Log("AudioDirectorPage::AudioDirector {Set}", Severity.FATALERROR, exception);
                }
            }
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
                    this._audioFactory.add_OnHeadphoneStatusChanged(new _ICxHDAudioFactoryEvents_OnHeadphoneStatusChangedEventHandler(this._audioFactory_OnHeadphoneStatusChanged));
                }
            }
        }

        public string FriendlyName =>
            "Audio Director Page";

        public bool IsHeadphonePresent =>
            ((App) Application.Current).AudioFactory.DeviceIOConfig.HeadphonePresent;
    }
}

