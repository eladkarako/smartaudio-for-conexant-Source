namespace SmartAudio
{
    using CxHDAudioAPILib;
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Markup;
    using System.Windows.Threading;

    public class PreviewExternalMicrophone : UserControl, IHDAudioPreview, IComponentConnector
    {
        private CxHDAudioChannelEnumeratorClass _audioChannelEnumerator;
        private bool _contentLoaded;
        private CxHDMasterVolumeControl _masterVolumeControl;
        internal Image image1;
        internal FormattedSlider slider;

        public PreviewExternalMicrophone()
        {
            this.InitializeComponent();
            this.slider.ValueChanged += new RoutedPropertyChangedEventHandler<double>(this.VolumeBar_ValueChanged);
        }

        [DebuggerNonUserCode]
        internal Delegate _CreateDelegate(Type delegateType, string handler) => 
            Delegate.CreateDelegate(delegateType, this, handler);

        private void _masterVolumeControl_OnVolumeChanged(double newValue, string context)
        {
            base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new SmartAudio.OnRefreshBalance(this.OnRefreshBalance));
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (!this._contentLoaded)
            {
                this._contentLoaded = true;
                Uri resourceLocator = new Uri("/SmartAudio;component/previewexternalmicrophone.xaml", UriKind.Relative);
                Application.LoadComponent(this, resourceLocator);
            }
        }

        public void InitializeSlider()
        {
            if (this._masterVolumeControl != null)
            {
                double boostMin = 0.0;
                double boostMax = 0.0;
                double boostStep = 0.0;
                this._masterVolumeControl.BoostRange(out boostMin, out boostMax, out boostStep);
                this.slider.Ticks.Clear();
                this.slider.Minimum = boostMin;
                this.slider.Maximum = boostMax;
                if (0.0 != boostStep)
                {
                    double num4 = (boostMax - boostMin) / boostStep;
                    for (int i = 0; i < ((int) num4); i++)
                    {
                        this.slider.Ticks.Add(boostStep * i);
                    }
                }
                this.slider.Value = this._masterVolumeControl.Boost;
            }
        }

        public void Localize()
        {
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
                this.slider.ValueChanged -= new RoutedPropertyChangedEventHandler<double>(this.VolumeBar_ValueChanged);
                this.slider.Value = this._masterVolumeControl.Boost;
                this.slider.ValueChanged += new RoutedPropertyChangedEventHandler<double>(this.VolumeBar_ValueChanged);
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("PreviewExternalMicrophone : OnRefreshBalance - failed", Severity.INFORMATION, exception);
            }
        }

        [DebuggerNonUserCode, EditorBrowsable(EditorBrowsableState.Never)]
        void IComponentConnector.Connect(int connectionId, object target)
        {
            switch (connectionId)
            {
                case 1:
                    this.image1 = (Image) target;
                    return;

                case 2:
                    this.slider = (FormattedSlider) target;
                    return;
            }
            this._contentLoaded = true;
        }

        private void VolumeBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this._masterVolumeControl != null)
            {
                this._masterVolumeControl.Boost = e.NewValue;
                this.slider.Value = this._masterVolumeControl.Boost;
            }
        }

        public CxHDAudioChannelEnumeratorClass AudioChannelEnumerator
        {
            get => 
                this._audioChannelEnumerator;
            set
            {
                this._audioChannelEnumerator = value;
            }
        }

        public CxHDMasterVolumeControl MasterVolume
        {
            get => 
                this._masterVolumeControl;
            set
            {
                this._masterVolumeControl = value;
                double boostMin = 0.0;
                double boostMax = 0.0;
                double boostStep = 0.0;
                this._masterVolumeControl.BoostRange(out boostMin, out boostMax, out boostStep);
                this.slider.Ticks.Clear();
                this.slider.Minimum = boostMin;
                this.slider.Maximum = boostMax;
                if (0.0 != boostStep)
                {
                    double num4;
                    if (1.0 == boostStep)
                    {
                        num4 = 1.0;
                    }
                    else
                    {
                        num4 = (boostMax - boostMin) / boostStep;
                    }
                    for (int i = 0; i < ((int) num4); i++)
                    {
                        this.slider.Ticks.Add(boostStep * i);
                    }
                }
                this.slider.Value = this._masterVolumeControl.Boost;
                this.slider.ValueChanged += new RoutedPropertyChangedEventHandler<double>(this.VolumeBar_ValueChanged);
                this._masterVolumeControl.add_OnVolumeChanged(new _ICxHDAudioVolumeEvents_OnVolumeChangedEventHandler(this._masterVolumeControl_OnVolumeChanged));
            }
        }
    }
}

