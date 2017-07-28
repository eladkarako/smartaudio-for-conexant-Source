namespace SmartAudio
{
    using SmartAudio.Properties;
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Markup;
    using System.Windows.Media;
    using System.Windows.Shapes;

    public class Dot : UserControl, IComponentConnector
    {
        public Point _centerPoint;
        private bool _contentLoaded;
        internal ImageBrush _imageBrush;
        private static Rect _rectArea = new Rect(115.0, 90.0, 180.0, 150.0);
        internal RectangleGeometry _rectGeo;
        public static readonly DependencyProperty CenterProperty = DependencyProperty.Register("Center", typeof(Point), typeof(Dot), new PropertyMetadata(new Point(), new PropertyChangedCallback(Dot.OnCenterChanged)));
        internal Path Head;
        public AutoAdjust OnAutoAdjust;

        public Dot()
        {
            this.InitializeComponent();
            this.LocalizeToolTip();
            base.MouseDoubleClick += new MouseButtonEventHandler(this.Dot_MouseDoubleClick);
        }

        public void AutoAdjustToCenter()
        {
            this.Center = this._centerPoint;
        }

        private void Dot_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.AutoAdjustToCenter();
            if (this.OnAutoAdjust != null)
            {
                this.OnAutoAdjust();
            }
        }

        public void ForceToCenter()
        {
            this.Center = this._centerPoint;
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (!this._contentLoaded)
            {
                this._contentLoaded = true;
                Uri resourceLocator = new Uri("/SmartAudio;component/dot.xaml", UriKind.Relative);
                Application.LoadComponent(this, resourceLocator);
            }
        }

        public void LocalizeToolTip()
        {
            this.Head.ToolTip = Resources.SA_TTSpeakerHead;
        }

        private static void OnCenterChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            Point newValue = (Point) args.NewValue;
            Point point3 = (Point) args.NewValue;
            (obj as Dot)._rectGeo.Rect = new Rect(newValue.X - ((obj as Dot)._rectGeo.Rect.Width / 2.0), point3.Y - ((obj as Dot)._rectGeo.Rect.Height / 2.0), (obj as Dot)._rectGeo.Rect.Width, (obj as Dot)._rectGeo.Rect.Height);
            Point point = (Point) args.NewValue;
            double width = (((0.5 * (point.Y - _rectArea.Y)) / _rectArea.Height) + 0.5) * 100.0;
            double height = 0.84 * width;
            Rect rect = new Rect(point.X - (width / 2.0), point.Y - (height / 2.0), width, height);
            (obj as Dot)._rectGeo.Rect = rect;
        }

        [DebuggerNonUserCode, EditorBrowsable(EditorBrowsableState.Never)]
        void IComponentConnector.Connect(int connectionId, object target)
        {
            switch (connectionId)
            {
                case 1:
                    this.Head = (Path) target;
                    return;

                case 2:
                    this._imageBrush = (ImageBrush) target;
                    return;

                case 3:
                    this._rectGeo = (RectangleGeometry) target;
                    return;
            }
            this._contentLoaded = true;
        }

        public Point Center
        {
            get => 
                ((Point) base.GetValue(CenterProperty));
            set
            {
                base.SetValue(CenterProperty, value);
            }
        }

        public Point CenterPoint
        {
            get => 
                this._centerPoint;
            set
            {
                this._centerPoint = value;
                this.Center = value;
            }
        }

        public ImageSource DotImage
        {
            get => 
                this._imageBrush.ImageSource;
            set
            {
                this._imageBrush.ImageSource = value;
            }
        }

        public static Rect RectArea
        {
            get => 
                _rectArea;
            set
            {
                _rectArea = value;
            }
        }

        public RectangleGeometry RectGeo
        {
            get => 
                this._rectGeo;
            set
            {
                this._rectGeo = value;
            }
        }
    }
}

