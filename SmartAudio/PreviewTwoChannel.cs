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

    public class PreviewTwoChannel : UserControl, IHDAudioPreview, IComponentConnector
    {
        private CxHDAudioChannelEnumeratorClass _audioChannelEnumerator;
        internal Slider _balance;
        private ChannelBalance _channelBalance;
        private bool _contentLoaded;
        internal TextBlock _Left;
        private CxHDMasterVolumeControl _masterVolumeControl;
        internal TextBlock _Right;
        internal Image image1;

        public PreviewTwoChannel()
        {
            this.InitializeComponent();
            this._channelBalance = new ChannelBalance();
            this._balance.ValueChanged += new RoutedPropertyChangedEventHandler<double>(this.VolumeBar_ValueChanged);
            this.Localize();
        }

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
                Uri resourceLocator = new Uri("/SmartAudio;component/previewtwochannel.xaml", UriKind.Relative);
                Application.LoadComponent(this, resourceLocator);
            }
        }

        public void Localize()
        {
            this._Left.Text = Resources.SA_LeftChannelShort;
            this._Right.Text = Resources.SA_RightChannelShort;
        }

        public void OnMasterVolumeChanged(double newValue)
        {
        }

        public void OnMasterVolumeChanging(double newValue)
        {
        }

        private void OnRefreshBalance()
        {
            this._balance.ValueChanged -= new RoutedPropertyChangedEventHandler<double>(this.VolumeBar_ValueChanged);
            this._balance.Value = this._channelBalance.Value;
            this._balance.ValueChanged += new RoutedPropertyChangedEventHandler<double>(this.VolumeBar_ValueChanged);
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
                    this._balance = (Slider) target;
                    return;

                case 3:
                    this._Left = (TextBlock) target;
                    return;

                case 4:
                    this._Right = (TextBlock) target;
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
            }
        }
    }
}

