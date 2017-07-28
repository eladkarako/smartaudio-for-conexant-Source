namespace SmartAudio
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Markup;
    using System.Windows.Media.Animation;

    public class PreviewVolumeBar : UserControl, IComponentConnector
    {
        private IChannelBalance _channelBalance;
        private bool _contentLoaded;
        internal Canvas _mainPanel;
        internal Slider _volumeBarSlider;

        public PreviewVolumeBar()
        {
            this.InitializeComponent();
            this._mainPanel.MouseEnter += new MouseEventHandler(this._mainPanel_MouseEnter);
            this._mainPanel.MouseLeave += new MouseEventHandler(this._mainPanel_MouseLeave);
            this._volumeBarSlider.Opacity = 0.0;
            this._volumeBarSlider.Minimum = 0.0;
            this._volumeBarSlider.Maximum = 100.0;
        }

        private void _mainPanel_MouseEnter(object sender, MouseEventArgs e)
        {
            this.AnimateOpacity(this._volumeBarSlider, 0.0, 1.0);
        }

        private void _mainPanel_MouseLeave(object sender, MouseEventArgs e)
        {
            this.AnimateOpacity(this._volumeBarSlider, 1.0, 0.0);
        }

        private void AnimateOpacity(Slider slider, double from, double to)
        {
            DoubleAnimation animation = new DoubleAnimation {
                From = new double?(from),
                To = new double?(to),
                Duration = new Duration(TimeSpan.FromMilliseconds(500.0)),
                AutoReverse = false
            };
            slider.BeginAnimation(UIElement.OpacityProperty, animation);
        }

        public void HideVolumeBar(bool animate, bool show)
        {
            if ((show || (this._volumeBarSlider.Opacity != 0.0)) && (!show || (this._volumeBarSlider.Opacity != 1.0)))
            {
                if (!animate)
                {
                    if (!show)
                    {
                        this._volumeBarSlider.Opacity = 0.0;
                    }
                    else
                    {
                        this._volumeBarSlider.Opacity = 1.0;
                    }
                }
                else if (!show)
                {
                    this.AnimateOpacity(this._volumeBarSlider, 1.0, 0.0);
                }
                else
                {
                    this.AnimateOpacity(this._volumeBarSlider, 0.0, 1.0);
                }
            }
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (!this._contentLoaded)
            {
                this._contentLoaded = true;
                Uri resourceLocator = new Uri("/SmartAudio;component/previewvolumebar.xaml", UriKind.Relative);
                Application.LoadComponent(this, resourceLocator);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), DebuggerNonUserCode]
        void IComponentConnector.Connect(int connectionId, object target)
        {
            switch (connectionId)
            {
                case 1:
                    this._mainPanel = (Canvas) target;
                    return;

                case 2:
                    this._volumeBarSlider = (Slider) target;
                    return;
            }
            this._contentLoaded = true;
        }

        public IChannelBalance ChannelBalance
        {
            get => 
                this._channelBalance;
            set
            {
                this._channelBalance = value;
                if (this._channelBalance != null)
                {
                    this._volumeBarSlider.Minimum = this._channelBalance.Minimum;
                    this._volumeBarSlider.Maximum = this._channelBalance.Maximum;
                    this._volumeBarSlider.Value = this._channelBalance.Value;
                    this._volumeBarSlider.Ticks.Clear();
                    this._volumeBarSlider.Ticks.Add(this._volumeBarSlider.Minimum);
                    this._volumeBarSlider.Ticks.Add(Math.Ceiling((double) ((this._volumeBarSlider.Maximum - this._volumeBarSlider.Minimum) / 2.0)));
                    this._volumeBarSlider.Ticks.Add(this._volumeBarSlider.Maximum);
                }
            }
        }

        public Slider VolumeBar =>
            this._volumeBarSlider;

        public double VolumebarHeight
        {
            get => 
                this._volumeBarSlider.Height;
            set
            {
                if (value == -1.0)
                {
                    this._volumeBarSlider.Height = this._mainPanel.Height;
                }
                else
                {
                    this._volumeBarSlider.Height = value;
                }
            }
        }

        public HorizontalAlignment VolumeBarHorizontalAlignment
        {
            get => 
                this._volumeBarSlider.HorizontalAlignment;
            set
            {
                this._volumeBarSlider.HorizontalAlignment = value;
            }
        }

        public Orientation VolumeBarOrientation
        {
            get => 
                this._volumeBarSlider.Orientation;
            set
            {
                this._volumeBarSlider.Orientation = value;
            }
        }

        public VerticalAlignment VolumebarVerticalAlignment
        {
            get => 
                this._volumeBarSlider.VerticalAlignment;
            set
            {
                this._volumeBarSlider.VerticalAlignment = value;
            }
        }

        public double VolumebarWidth
        {
            get => 
                this._volumeBarSlider.Width;
            set
            {
                if (value == -1.0)
                {
                    this._volumeBarSlider.Width = this._mainPanel.Width;
                }
                else
                {
                    this._volumeBarSlider.Width = value;
                }
            }
        }
    }
}

