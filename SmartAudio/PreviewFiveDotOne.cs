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
    using System.Windows.Media.Animation;

    public class PreviewFiveDotOne : UserControl, IHDAudioPreview, IComponentConnector
    {
        private CxHDAudioChannelEnumeratorClass _audioChannelEnumerator;
        internal PreviewVolumeBar _centerSpeaker;
        private bool _contentLoaded;
        private MultiChannelBalance _frontChannels;
        internal PreviewVolumeBar _frontVolumeBar;
        internal TextBlock _LeftCenter;
        internal TextBlock _LeftFront;
        internal TextBlock _LeftSubWoofer;
        private CxHDMasterVolumeControl _masterVolumeControl;
        private MultiChannelBalance _rearChannels;
        internal PreviewVolumeBar _rearVolumeBar;
        internal TextBlock _RightCenter;
        internal TextBlock _RightFront;
        internal TextBlock _RightSubWoofer;
        internal PreviewVolumeBar _subWoofer;
        internal Image image1;

        public PreviewFiveDotOne()
        {
            this.InitializeComponent();
            this._frontChannels = new MultiChannelBalance();
            this._rearChannels = new MultiChannelBalance();
            this._rearVolumeBar.VolumeBar.ValueChanged += new RoutedPropertyChangedEventHandler<double>(this.Rear_VolumeBar_ValueChanged);
            this._frontVolumeBar.VolumeBar.ValueChanged += new RoutedPropertyChangedEventHandler<double>(this.Front_VolumeBar_ValueChanged);
            this._frontVolumeBar.MouseEnter += new MouseEventHandler(this._frontVolumeBar_MouseEnter);
            this._frontVolumeBar.MouseLeave += new MouseEventHandler(this._frontVolumeBar_MouseLeave);
            this._subWoofer.MouseEnter += new MouseEventHandler(this._subWoofer_MouseEnter);
            this._subWoofer.MouseLeave += new MouseEventHandler(this._subWoofer_MouseLeave);
            this._centerSpeaker.MouseEnter += new MouseEventHandler(this._centerSpeaker_MouseEnter);
            this._centerSpeaker.MouseLeave += new MouseEventHandler(this._centerSpeaker_MouseLeave);
            this.Localize();
        }

        private void _centerSpeaker_MouseEnter(object sender, MouseEventArgs e)
        {
            this.AnimateOpacity(this._LeftCenter, 0.0, 1.0);
            this.AnimateOpacity(this._RightCenter, 0.0, 1.0);
        }

        private void _centerSpeaker_MouseLeave(object sender, MouseEventArgs e)
        {
            this.AnimateOpacity(this._LeftCenter, 1.0, 0.0);
            this.AnimateOpacity(this._RightCenter, 1.0, 0.0);
        }

        [DebuggerNonUserCode]
        internal Delegate _CreateDelegate(Type delegateType, string handler) => 
            Delegate.CreateDelegate(delegateType, this, handler);

        private void _frontVolumeBar_MouseEnter(object sender, MouseEventArgs e)
        {
            this.AnimateOpacity(this._LeftFront, 0.0, 1.0);
            this.AnimateOpacity(this._RightFront, 0.0, 1.0);
        }

        private void _frontVolumeBar_MouseLeave(object sender, MouseEventArgs e)
        {
            this.AnimateOpacity(this._LeftFront, 1.0, 0.0);
            this.AnimateOpacity(this._RightFront, 1.0, 0.0);
        }

        private void _subWoofer_MouseEnter(object sender, MouseEventArgs e)
        {
            this.AnimateOpacity(this._LeftSubWoofer, 0.0, 1.0);
            this.AnimateOpacity(this._RightSubWoofer, 0.0, 1.0);
        }

        private void _subWoofer_MouseLeave(object sender, MouseEventArgs e)
        {
            this.AnimateOpacity(this._LeftSubWoofer, 1.0, 0.0);
            this.AnimateOpacity(this._RightSubWoofer, 1.0, 0.0);
        }

        private void AnimateOpacity(TextBlock tb, double from, double to)
        {
            DoubleAnimation animation = new DoubleAnimation {
                From = new double?(from),
                To = new double?(to),
                Duration = new Duration(TimeSpan.FromMilliseconds(500.0)),
                AutoReverse = false
            };
            tb.BeginAnimation(UIElement.OpacityProperty, animation);
        }

        private void Front_VolumeBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this._frontChannels.Value = e.NewValue;
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (!this._contentLoaded)
            {
                this._contentLoaded = true;
                Uri resourceLocator = new Uri("/SmartAudio;component/previewfivedotone.xaml", UriKind.Relative);
                Application.LoadComponent(this, resourceLocator);
            }
        }

        public void Localize()
        {
            this._LeftFront.Text = Resources.SA_LeftChannelShort;
            this._RightFront.Text = Resources.SA_RightChannelShort;
            this._LeftSubWoofer.Text = Resources.SA_LeftSurroundShort;
            this._RightSubWoofer.Text = Resources.SA_RightSurroundShort;
            this._LeftCenter.Text = Resources.SA_LeftBackShort;
            this._RightCenter.Text = Resources.SA_RightBackShort;
        }

        public void OnMasterVolumeChanged(double newValue)
        {
            this._rearVolumeBar.HideVolumeBar(true, false);
            this._centerSpeaker.HideVolumeBar(true, false);
        }

        public void OnMasterVolumeChanging(double newValue)
        {
            this._rearVolumeBar.HideVolumeBar(true, true);
            this._centerSpeaker.HideVolumeBar(true, true);
            this._frontVolumeBar.VolumeBar.Value = newValue;
            this._centerSpeaker.VolumeBar.Value = newValue;
        }

        private void Rear_VolumeBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this._rearVolumeBar.VolumeBar.Value = e.NewValue;
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
                    this._rearVolumeBar = (PreviewVolumeBar) target;
                    return;

                case 3:
                    this._subWoofer = (PreviewVolumeBar) target;
                    return;

                case 4:
                    this._LeftSubWoofer = (TextBlock) target;
                    return;

                case 5:
                    this._RightSubWoofer = (TextBlock) target;
                    return;

                case 6:
                    this._frontVolumeBar = (PreviewVolumeBar) target;
                    return;

                case 7:
                    this._LeftFront = (TextBlock) target;
                    return;

                case 8:
                    this._RightFront = (TextBlock) target;
                    return;

                case 9:
                    this._centerSpeaker = (PreviewVolumeBar) target;
                    return;

                case 10:
                    this._LeftCenter = (TextBlock) target;
                    return;

                case 11:
                    this._RightCenter = (TextBlock) target;
                    return;
            }
            this._contentLoaded = true;
        }

        public CxHDAudioChannelEnumeratorClass AudioChannelEnumerator
        {
            get => 
                this._audioChannelEnumerator;
            set
            {
                this._audioChannelEnumerator = value;
                if (this._audioChannelEnumerator.Count == 6)
                {
                    this._frontChannels.AddChannel((CxHDAudioChannel) this._audioChannelEnumerator[1], ChannelType.LeftChannel);
                    this._frontChannels.AddChannel((CxHDAudioChannel) this._audioChannelEnumerator[2], ChannelType.RightChannel);
                    this._frontChannels.AddChannel((CxHDAudioChannel) this._audioChannelEnumerator[6], ChannelType.RightChannel);
                    this._frontChannels.AddChannel((CxHDAudioChannel) this._audioChannelEnumerator[5], ChannelType.LeftChannel);
                }
            }
        }

        public CxHDMasterVolumeControl MasterVolume
        {
            get => 
                this._masterVolumeControl;
            set
            {
                this._masterVolumeControl = value;
                this._frontChannels.MasterVolumeControlClass = value;
                this._frontVolumeBar.ChannelBalance = this._frontChannels;
            }
        }
    }
}

