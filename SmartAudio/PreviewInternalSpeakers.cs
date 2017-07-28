namespace SmartAudio
{
    using CxHDAudioAPILib;
    using SmartAudio.Properties;
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Markup;
    using System.Windows.Threading;

    public class PreviewInternalSpeakers : UserControl, IHDAudioPreview, IComponentConnector
    {
        private CxHDAudioAGC _audioAGC;
        private CxHDAudioChannelEnumeratorClass _audioChannelEnumerator;
        internal Slider _balance;
        private ChannelBalance _channelBalance;
        private bool _contentLoaded;
        internal Image _imageLaptop;
        internal TextBlock _Left;
        private CxHDMasterVolumeControl _masterVolumeControl;
        internal ImageCheckBox _muteButton;
        internal TextBlock _Right;

        public PreviewInternalSpeakers()
        {
            this.InitializeComponent();
            this._balance.ValueChanged += new RoutedPropertyChangedEventHandler<double>(this.VolumeBar_ValueChanged);
            this._channelBalance = new ChannelBalance();
            this.Localize();
        }

        [DebuggerNonUserCode]
        internal Delegate _CreateDelegate(Type delegateType, string handler) => 
            Delegate.CreateDelegate(delegateType, this, handler);

        private void _masterVolumeControl_OnMuted(int bValue, string context)
        {
            if (this._audioAGC != null)
            {
                base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new OnMuteChanged(this.OnDeviceMuteStateChanged));
            }
        }

        private void _masterVolumeControl_OnVolumeChanged(double newValue, string context)
        {
            base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new SmartAudio.OnRefreshBalance(this.OnRefreshBalance));
        }

        private void _muteButton_OnItemStateChanged(ImageCheckBox item, bool newState)
        {
            if (this._audioAGC != null)
            {
                this._audioAGC.SetEnabled(newState);
            }
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (!this._contentLoaded)
            {
                this._contentLoaded = true;
                Uri resourceLocator = new Uri("/SmartAudio;component/previewinternalspeakers.xaml", UriKind.Relative);
                Application.LoadComponent(this, resourceLocator);
            }
        }

        public bool IsInternalSpeakerMuted()
        {
            if (this._audioAGC == null)
            {
                return false;
            }
            bool enabled = false;
            try
            {
                enabled = this._audioAGC.GetEnabled();
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("IsInternalSpeakerMuted(): Mute option not implemented", Severity.INFORMATION, exception);
            }
            return enabled;
        }

        public void Localize()
        {
            this._Left.Text = Resources.SA_LeftChannelShort;
            this._Right.Text = Resources.SA_RightChannelShort;
            this._muteButton.ToolTip = this.IsInternalSpeakerMuted() ? Resources.SA_UnmuteInternalSpeaker : Resources.SA_MuteInternalSpeaker;
        }

        private void OnDeviceMuteStateChanged()
        {
            if (this._muteButton.Visibility != Visibility.Hidden)
            {
                bool enabled = false;
                try
                {
                    enabled = this._audioAGC.GetEnabled();
                }
                catch (Exception exception)
                {
                    SmartAudioLog.Log("OnDeviceMuteStateChanged(): Mute option not implemented", Severity.INFORMATION, exception);
                    return;
                }
                this._muteButton.Selected = enabled;
                this._muteButton.ToolTip = enabled ? Resources.SA_UnmuteInternalSpeaker : Resources.SA_MuteInternalSpeaker;
            }
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
                this._balance.ValueChanged -= new RoutedPropertyChangedEventHandler<double>(this.VolumeBar_ValueChanged);
                this._balance.Value = this._channelBalance.Value;
                this._balance.ValueChanged += new RoutedPropertyChangedEventHandler<double>(this.VolumeBar_ValueChanged);
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("PreviewInternalSpeakers : OnRefreshBalance - failed", Severity.INFORMATION, exception);
            }
        }

        [DebuggerNonUserCode, EditorBrowsable(EditorBrowsableState.Never)]
        void IComponentConnector.Connect(int connectionId, object target)
        {
            switch (connectionId)
            {
                case 1:
                    this._imageLaptop = (Image) target;
                    return;

                case 2:
                    this._balance = (Slider) target;
                    return;

                case 3:
                    this._Left = (TextBlock) target;
                    return;

                case 4:
                    this._Right = (TextBlock) target;
                    return;

                case 5:
                    this._muteButton = (ImageCheckBox) target;
                    return;
            }
            this._contentLoaded = true;
        }

        private void VolumeBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Math.Abs((double) (this._channelBalance.Value - e.NewValue)) > 1E-10)
            {
                this._channelBalance.Value = e.NewValue;
            }
        }

        public CxHDAudioAGC AudioAGC
        {
            get => 
                this._audioAGC;
            set
            {
                this._audioAGC = value;
                if (this._audioAGC != null)
                {
                    this._muteButton.Visibility = Visibility.Visible;
                    this._muteButton.Selected = this._audioAGC.GetEnabled();
                }
                else
                {
                    this._muteButton.Visibility = Visibility.Hidden;
                }
            }
        }

        public CxHDAudioChannelEnumeratorClass AudioChannelEnumerator
        {
            get => 
                this._audioChannelEnumerator;
            set
            {
                this._audioChannelEnumerator = value;
                this._channelBalance.AudioChannelEmulator = this._audioChannelEnumerator;
            }
        }

        public CxHDMasterVolumeControl MasterVolume
        {
            get => 
                this._masterVolumeControl;
            set
            {
                this._masterVolumeControl = value;
                this._channelBalance.MasterVolumeControlClass = value;
                this._balance.Minimum = this._channelBalance.Minimum;
                this._balance.Maximum = this._channelBalance.Maximum;
                this._balance.Value = this._channelBalance.Value;
                this._balance.Ticks.Clear();
                this._balance.Ticks.Add((this._balance.Maximum - this._balance.Minimum) / 2.0);
                this._masterVolumeControl.add_OnVolumeChanged(new _ICxHDAudioVolumeEvents_OnVolumeChangedEventHandler(this._masterVolumeControl_OnVolumeChanged));
                this._masterVolumeControl.add_OnMuted(new _ICxHDAudioVolumeEvents_OnMutedEventHandler(this._masterVolumeControl_OnMuted));
            }
        }
    }
}

