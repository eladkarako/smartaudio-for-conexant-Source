namespace SmartAudio
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Markup;
    using System.Windows.Media;
    using System.Windows.Media.Animation;

    public class AnimationToolbar : UserControl, IComponentConnector
    {
        private bool _contentLoaded;
        private double amplitude = 192.0;
        internal Canvas can;
        internal Image d1;
        internal Image d2;
        internal Image d3;
        internal Image d4;
        internal Image d5;
        internal Image d6;
        internal Image d7;
        internal Image d8;
        internal Image d9;
        internal Grid DocumentRoot;
        private double icon_max = 128.0;
        private double icon_min = 32.0;
        internal BeginStoryboard OnLoaded_BeginStoryboard;
        private double[] pos = new double[] { -118.0, -84.0, -50.0, -16.0, 18.0, 52.0, 86.0, 120.0 };
        private double ratio = 0.0163624617374468;
        private double scale = double.NegativeInfinity;
        private double span = 96.0;
        private ScaleTransform[] sts = new ScaleTransform[8];
        private double trend;
        private double[] widths = new double[8];

        public AnimationToolbar()
        {
            this.InitializeComponent();
            this.Initialize();
        }

        private void calculate(object sender, EventArgs e)
        {
            bool flag = (((Mouse.GetPosition(this.can).X > Canvas.GetLeft(this.can.Children[0])) && (Mouse.GetPosition(this.can).X < ((Canvas.GetLeft(this.can.Children[7]) + (128.0 * this.sts[7].ScaleX)) - 10.0))) && (Mouse.GetPosition(this.can).Y > 40.0)) && (Mouse.GetPosition(this.can).Y < 130.0);
            if (this.trend == 0.0)
            {
                this.trend = flag ? 0.25 : -0.25;
            }
            this.scale += this.trend;
            if ((this.scale < 0.02) || (this.scale > 0.98))
            {
                this.trend = 0.0;
            }
            this.scale = Math.Min(1.0, Math.Max(0.0, this.scale));
            for (int i = 0; i < 8; i++)
            {
                Image element = (Image) this.can.Children[i];
                double num2 = this.pos[i] - Mouse.GetPosition(this.can).X;
                num2 = Math.Min(Math.Max(num2, -this.span), this.span);
                double num3 = this.icon_min + ((this.icon_max - this.icon_min) * Math.Cos(num2 * this.ratio));
                double length = this.pos[i] + ((this.scale * this.amplitude) * Math.Sin(num2 * this.ratio));
                Canvas.SetLeft(element, length);
                if (flag)
                {
                    this.sts[i].ScaleY = num3 / 128.0;
                    this.sts[i].ScaleX = num3 / 128.0;
                }
                else
                {
                    this.sts[i].ScaleX = 0.25;
                    this.sts[i].ScaleY = 0.25;
                }
                element.RenderTransform = this.sts[i];
                this.widths[i] = 128.0 * this.sts[i].ScaleX;
            }
        }

        public void Initialize()
        {
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (!this._contentLoaded)
            {
                this._contentLoaded = true;
                Uri resourceLocator = new Uri("/SmartAudio;component/animationtoolbar.xaml", UriKind.Relative);
                Application.LoadComponent(this, resourceLocator);
            }
        }

        protected override void OnInitialized(EventArgs e)
        {
            this.Initialize();
        }

        [EditorBrowsable(EditorBrowsableState.Never), DebuggerNonUserCode]
        void IComponentConnector.Connect(int connectionId, object target)
        {
            switch (connectionId)
            {
                case 1:
                    this.DocumentRoot = (Grid) target;
                    return;

                case 2:
                    this.OnLoaded_BeginStoryboard = (BeginStoryboard) target;
                    return;

                case 3:
                    this.can = (Canvas) target;
                    return;

                case 4:
                    this.d1 = (Image) target;
                    return;

                case 5:
                    this.d2 = (Image) target;
                    return;

                case 6:
                    this.d3 = (Image) target;
                    return;

                case 7:
                    this.d4 = (Image) target;
                    return;

                case 8:
                    this.d5 = (Image) target;
                    return;

                case 9:
                    this.d6 = (Image) target;
                    return;

                case 10:
                    this.d7 = (Image) target;
                    return;

                case 11:
                    this.d8 = (Image) target;
                    return;

                case 12:
                    this.d9 = (Image) target;
                    return;
            }
            this._contentLoaded = true;
        }
    }
}

