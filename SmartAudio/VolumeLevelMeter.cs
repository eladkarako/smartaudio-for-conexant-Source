namespace SmartAudio
{
    using CxHDAudioAPILib;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.Remoting.Messaging;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Markup;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using System.Windows.Threading;

    public class VolumeLevelMeter : UserControl, IComponentConnector
    {
        internal Image _audioOutputPanelBack;
        private List<Brush> _brushes;
        private bool _contentLoaded;
        internal StackPanel _energyBarsPanel;
        internal Image _glassLook;
        private bool _isMuted;
        private CxHDMasterVolumeControl _masterVolumeControl;
        private double _maxRange;
        private double _maxValue;
        private bool _meterDisabled;
        private double _minRange = -180.0;
        private double _minValue;
        private int _numberOfBars;
        private List<ValueSliderMapper> _progressBars;
        private Timer _timer;
        private Timer _timerPeaks;
        internal Border shine;

        public VolumeLevelMeter()
        {
            this.InitializeComponent();
            this._progressBars = new List<ValueSliderMapper>();
            this._brushes = new List<Brush>();
            this._brushes.Add(new SolidColorBrush((Color) ColorConverter.ConvertFromString("#7983a3")));
            this._brushes.Add(new SolidColorBrush((Color) ColorConverter.ConvertFromString("#b7c6e5")));
            this._brushes.Add(new SolidColorBrush((Color) ColorConverter.ConvertFromString("#8f9fd1")));
            this._brushes.Add(new SolidColorBrush((Color) ColorConverter.ConvertFromString("#7584BF")));
            this._brushes.Add(new SolidColorBrush((Color) ColorConverter.ConvertFromString("#8595ca")));
            this._brushes.Add(new SolidColorBrush((Color) ColorConverter.ConvertFromString("#7c8bc4")));
            this._brushes.Add(new SolidColorBrush((Color) ColorConverter.ConvertFromString("#7685c0")));
            this._brushes.Add(new SolidColorBrush((Color) ColorConverter.ConvertFromString("#6d7bb9")));
            this._brushes.Add(new SolidColorBrush((Color) ColorConverter.ConvertFromString("#6775b5")));
            this._brushes.Add(new SolidColorBrush((Color) ColorConverter.ConvertFromString("#5d6cb0")));
            this._brushes.Add(new SolidColorBrush((Color) ColorConverter.ConvertFromString("#5563ad")));
            this._brushes.Add(new SolidColorBrush((Color) ColorConverter.ConvertFromString("#4d5ea7")));
            this._brushes.Add(new SolidColorBrush((Color) ColorConverter.ConvertFromString("#4656a3")));
            this._brushes.Add(new SolidColorBrush((Color) ColorConverter.ConvertFromString("#3f509f")));
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            if (this._masterVolumeControl != null)
            {
                base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new RefreshVolumeLevelMeter(this.Refresh));
            }
        }

        protected void CallbackMethod(IAsyncResult ar)
        {
            try
            {
                AsyncResult result = (AsyncResult) ar;
                CxSpectrumDataState returnValue = ((AsyncPeakMeterHandler) result.AsyncDelegate).EndInvoke(ar);
                this.UpdateSpectrumAnalyzer(returnValue);
            }
            catch (Exception exception)
            {
                string text1 = "Error: " + exception.Message;
                this.UpdateSpectrumAnalyzer(CxSpectrumDataState.NoDataDetected);
            }
        }

        protected bool CannotStartVolumeLevelMeter()
        {
            bool flag = false;
            App current = Application.Current as App;
            if ((current.IsRunningInBackground || current.IsRunningSliently) || ((current.ActivePage != "MainAudioPage") || current.IsRunningMinimized))
            {
                flag = true;
                bool isRunningInBackground = current.IsRunningInBackground;
                bool isRunningSliently = current.IsRunningSliently;
                bool flag3 = current.ActivePage != "MainAudioPage";
                bool isRunningMinimized = current.IsRunningMinimized;
            }
            return flag;
        }

        protected void FallOffCallback(object thisObject)
        {
            Application current = Application.Current;
            if (this.CannotStartVolumeLevelMeter())
            {
                this._timerPeaks.Change(-1, -1);
            }
            else
            {
                try
                {
                    base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new RefreshVolumeLevelMeter(this.RefreshFallOff));
                }
                catch (Exception exception)
                {
                    SmartAudioLog.Log("VolumeLevelMeter.FallOffCallback ", Severity.WARNING, exception);
                }
            }
        }

        public void Initialize()
        {
            this._numberOfBars = 0;
            this._progressBars = new List<ValueSliderMapper>();
            this.SetBars(3);
        }

        public void Initialize(double minValue, double maxValue, int numberOfBars)
        {
            App current = Application.Current as App;
            this._numberOfBars = 0x10;
            if (current.IsDemoMode)
            {
                this._minValue = 0.0;
                this._maxValue = 100.0;
            }
            else
            {
                this._minValue = this._minRange;
                this._maxValue = this._maxRange;
            }
            this.SetBars(this._numberOfBars);
            for (int i = 0; i < this._progressBars.Count; i++)
            {
                this._progressBars[i].PeakMeterBAR.Maximum = this._maxValue;
                this._progressBars[i].PeakMeterBAR.Minimum = this._minValue;
                this._progressBars[i].PeakMeterBAR.Value = this._minValue;
                this._progressBars[i].PeakMeterBAR.Initialize(this._minValue, this._maxValue + (this._minValue / 2.0), this._maxValue);
            }
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (!this._contentLoaded)
            {
                this._contentLoaded = true;
                Uri resourceLocator = new Uri("/SmartAudio;component/volumelevelmeter.xaml", UriKind.Relative);
                Application.LoadComponent(this, resourceLocator);
            }
        }

        private CxSpectrumDataState ReadPeakMeterData()
        {
            App current = Application.Current as App;
            CxSpectrumDataState noDataDetected = CxSpectrumDataState.NoDataDetected;
            if (!current.IsDemoMode)
            {
                try
                {
                    noDataDetected = this._masterVolumeControl.RefreshPeakValues();
                }
                catch (Exception exception)
                {
                    SmartAudioLog.Log("VolumeLevelMeter.ReadPeakMeterData", Severity.FATALERROR, exception);
                }
            }
            return noDataDetected;
        }

        public void Refresh()
        {
            new AsyncPeakMeterHandler(this.ReadPeakMeterData).BeginInvoke(new AsyncCallback(this.CallbackMethod), null);
        }

        public void RefreshFallOff()
        {
            for (int i = 0; i < this._progressBars.Count; i++)
            {
                this._progressBars[i].PeakMeterBAR.UpdateFallOff();
            }
        }

        public void Reset()
        {
            for (int i = 0; i < this._progressBars.Count; i++)
            {
                this._progressBars[i].PeakMeterBAR.Value = this._minValue;
            }
        }

        public void SetBars(int numberOfBars)
        {
            this._progressBars.Clear();
            this._energyBarsPanel.Children.Clear();
            double num = (base.Width - (5 * numberOfBars)) / ((double) numberOfBars);
            double num2 = (this._maxValue - this._minValue) / ((double) numberOfBars);
            for (int i = 0; i < numberOfBars; i++)
            {
                PeakMeterBAR element = new PeakMeterBAR(this._brushes);
                Rectangle rectangle = new Rectangle {
                    Width = 4.0,
                    Height = this._energyBarsPanel.Height
                };
                this._energyBarsPanel.Children.Add(rectangle);
                element.Background = Brushes.Transparent;
                element.Width = num;
                element.Height = base.Height;
                element.Minimum = this._minValue;
                element.Maximum = this._maxValue;
                this._energyBarsPanel.Children.Add(element);
                this._progressBars.Add(new ValueSliderMapper(num2 * (i + 1), element));
                element.Value = 50.0;
            }
        }

        [DebuggerNonUserCode, EditorBrowsable(EditorBrowsableState.Never)]
        void IComponentConnector.Connect(int connectionId, object target)
        {
            switch (connectionId)
            {
                case 1:
                    this._audioOutputPanelBack = (Image) target;
                    return;

                case 2:
                    this._energyBarsPanel = (StackPanel) target;
                    return;

                case 3:
                    this.shine = (Border) target;
                    return;

                case 4:
                    this._glassLook = (Image) target;
                    return;
            }
            this._contentLoaded = true;
        }

        protected void TimerCallback(object thisObject)
        {
            Application current = Application.Current;
            if (this.CannotStartVolumeLevelMeter())
            {
                this._timer.Change(-1, -1);
            }
            else
            {
                try
                {
                    base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new RefreshVolumeLevelMeter(this.Refresh));
                }
                catch (Exception exception)
                {
                    SmartAudioLog.Log("VolumeLevelMeter.TimerCallback ", Severity.WARNING, exception);
                }
            }
        }

        public void UpdateSpectrumAnalyzer(CxSpectrumDataState returnValue)
        {
            UpdateSpectrumBars method = new UpdateSpectrumBars(this.UpdateUIIndicators);
            base.Dispatcher.Invoke(DispatcherPriority.Normal, method, returnValue);
        }

        public void UpdateUIIndicators(CxSpectrumDataState spectrumDataState)
        {
            Monitor.Enter(this._progressBars);
            Monitor.Enter(this._masterVolumeControl.PeakValues);
            if ((spectrumDataState == CxSpectrumDataState.ValidSpectumData) && !this._isMuted)
            {
                CxHDAudioPeakValues peakValues = this._masterVolumeControl.PeakValues;
                double d = 0.0;
                for (int i = 0; i < this._progressBars.Count; i++)
                {
                    try
                    {
                        d = peakValues.GetPeakValueEx(PeakValueChannel.AllChannels, (CxHDAudioPeakFrequencies) i);
                        if (!double.IsInfinity(d))
                        {
                            this._progressBars[i].PeakMeterBAR.Value = d;
                        }
                        else
                        {
                            this._progressBars[i].PeakMeterBAR.Value = this._minValue;
                        }
                    }
                    catch (Exception exception)
                    {
                        SmartAudioLog.Log("VolumeLevelMeter.UpdateUIIndicators", Severity.INFORMATION, exception);
                        this._progressBars[i].PeakMeterBAR.Value = this._minValue;
                    }
                }
            }
            else
            {
                for (int j = 0; j < this._progressBars.Count; j++)
                {
                    try
                    {
                        this._progressBars[j].PeakMeterBAR.Value = this._minValue;
                    }
                    catch (Exception exception2)
                    {
                        SmartAudioLog.Log("VolumeLevelMeter.UpdateUIIndicators.PeakMeterBAR.Value", Severity.FATALERROR, exception2);
                    }
                }
                return;
            }
            Monitor.Exit(this._progressBars);
            Monitor.Exit(this._masterVolumeControl.PeakValues);
        }

        public bool IsMeterDisabled
        {
            get => 
                this._meterDisabled;
            set
            {
                this._meterDisabled = value;
            }
        }

        public bool IsMuted
        {
            get => 
                this._isMuted;
            set
            {
                this._isMuted = value;
            }
        }

        public CxHDMasterVolumeControl MasterVolumeControl
        {
            get => 
                this._masterVolumeControl;
            set
            {
                this._masterVolumeControl = value;
            }
        }

        public bool Simulate
        {
            set
            {
                App current = Application.Current as App;
                if (!current.IsDemoMode)
                {
                    if (this._timer == null)
                    {
                        SmartAudioLog.Log("VolumeLevelMeter : starting the timer and timer = null", new object[] { Severity.INFORMATION });
                        System.Threading.TimerCallback callback = new System.Threading.TimerCallback(this.TimerCallback);
                        System.Threading.TimerCallback callback2 = new System.Threading.TimerCallback(this.FallOffCallback);
                        this._timer = new Timer(callback, this, -1, -1);
                        this._timerPeaks = new Timer(callback2, this, -1, -1);
                    }
                    if (value)
                    {
                        SmartAudioLog.Log("VolumeLevelMeter : starting the timer, timer != null and value = true", new object[] { Severity.INFORMATION });
                        this._timer.Change(30, 30);
                        this._timerPeaks.Change(40, 40);
                    }
                    else
                    {
                        SmartAudioLog.Log("VolumeLevelMeter : starting the timer, timer != null and value = false", new object[] { Severity.INFORMATION });
                        this._timer.Change(-1, -1);
                        this._timerPeaks.Change(-1, -1);
                    }
                }
            }
        }
    }
}

