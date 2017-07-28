namespace SmartAudio
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Markup;
    using System.Windows.Media;
    using System.Windows.Media.Effects;
    using System.Windows.Shapes;

    public class Preview3DSpeaker : UserControl, IComponentConnector
    {
        private readonly List<double> _angleLength;
        internal Canvas _animCanvas;
        private readonly Queue<Path> _bezierPathQ;
        public const int _colorDelta = 5;
        private bool _contentLoaded;
        private readonly List<double> _heightVar;
        internal System.Windows.Controls.Image _imageLaptop;
        internal System.Windows.Controls.Image _imageLeftSpeaker;
        internal System.Windows.Controls.Image _imageLeftSpeakerRL;
        internal System.Windows.Controls.Image _imageRightSpeaker;
        internal System.Windows.Controls.Image _imageRightSpeakerRL;
        internal Slider _intensity;
        private bool _isSliderHidden;
        private Path _leftPath;
        internal Canvas _lrSpeakerCanvas;
        private const int _maxCurves = 100;
        private List<System.Windows.Point> _points;
        private readonly List<double> _pointXOffset;
        private readonly List<double> _pointYOffset;
        private Path _rightPath;
        private readonly List<double> _tickIndex;
        private readonly List<double> _widthVar;
        private double _xLeft;
        private double _xLeft0 = -5.0;
        private double _xLeft1 = -15.0;
        private double _xRight;
        private double _xRight0 = 170.0;
        private double _xRight1 = 180.0;
        private double _yLeft;
        private double _yLeft0 = 20.0;
        private double _yLeft1 = 110.0;
        private double _yRight;
        private double _yRight0 = 20.0;
        private double _yRight1 = 110.0;
        private const int count = 30;
        private int flag = 1;
        internal Grid Grid_1;
        private int index;
        private RadialGradientBrush leftGradientBrush;
        private Random random;
        internal Border rectangle1;
        private RadialGradientBrush rightGradientBrush;
        private double x0;
        private double x1 = 100.0;
        private double y01 = 70.0;
        private double y02 = 160.0;
        private double y11 = 100.0;
        private double y12 = 70.0;

        public Preview3DSpeaker()
        {
            try
            {
                this.InitializeComponent();
                this._bezierPathQ = new Queue<Path>();
                this.random = new Random((DateTime.Now.Minute + DateTime.Now.Second) + DateTime.Now.Millisecond);
                List<double> list = new List<double> { 
                    0.0,
                    0.0,
                    0.0,
                    0.0
                };
                this._tickIndex = list;
                List<double> list2 = new List<double> { 
                    0.0,
                    0.0,
                    0.0,
                    0.0
                };
                this._angleLength = list2;
                List<double> list3 = new List<double> { 
                    0.0,
                    0.0,
                    0.0,
                    0.0
                };
                this._heightVar = list3;
                List<double> list4 = new List<double> { 
                    0.0,
                    0.0,
                    0.0,
                    0.0
                };
                this._widthVar = list4;
                List<double> list5 = new List<double> { 
                    0.0,
                    0.0,
                    0.0,
                    0.0
                };
                this._pointYOffset = list5;
                List<double> list6 = new List<double> { 
                    0.0,
                    0.0,
                    0.0,
                    0.0
                };
                this._pointXOffset = list6;
                this.rightGradientBrush = new RadialGradientBrush();
                this.rightGradientBrush.GradientStops.Add(new GradientStop(Colors.Red, 0.0));
                this.rightGradientBrush.GradientStops.Add(new GradientStop(Colors.OrangeRed, 0.25));
                this.rightGradientBrush.GradientStops.Add(new GradientStop(Colors.Orange, 0.75));
                this.rightGradientBrush.GradientStops.Add(new GradientStop(Colors.Yellow, 1.0));
                this.leftGradientBrush = new RadialGradientBrush();
                this.leftGradientBrush.GradientStops.Add(new GradientStop(Colors.Red, 0.0));
                this.leftGradientBrush.GradientStops.Add(new GradientStop(Colors.OrangeRed, 0.25));
                this.leftGradientBrush.GradientStops.Add(new GradientStop(Colors.Orange, 0.75));
                this.leftGradientBrush.GradientStops.Add(new GradientStop(Colors.Yellow, 1.0));
                this._intensity.Minimum = 0.0;
                this._intensity.Maximum = 180.0;
                this._intensity.ValueChanged += new RoutedPropertyChangedEventHandler<double>(this._intensity_ValueChanged);
                base.Loaded += new RoutedEventHandler(this.Preview3DSpeaker_Loaded);
                base.IsVisibleChanged += new DependencyPropertyChangedEventHandler(this.Preview3DSpeaker_IsVisibleChanged);
                this._intensity_ValueChanged(null, null);
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("Preview3DSpeaker::Preview3DSpeaker()", Severity.FATALERROR, exception);
            }
        }

        private void _intensity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this._bezierPathQ.Clear();
            this._animCanvas.Children.Clear();
            OuterGlowBitmapEffect effect = new OuterGlowBitmapEffect {
                GlowSize = 20.0,
                GlowColor = System.Windows.Media.Color.FromRgb(System.Drawing.Color.Red.R, System.Drawing.Color.Red.G, System.Drawing.Color.Red.B),
                Noise = 0.0,
                Opacity = this._intensity.Value / 180.0
            };
            this._imageLeftSpeaker.BitmapEffect = effect;
            this._imageRightSpeaker.BitmapEffect = effect;
            this._imageLeftSpeakerRL.BitmapEffect = effect;
            this._imageRightSpeakerRL.BitmapEffect = effect;
            this._animCanvas.Opacity = this._intensity.Value / 180.0;
            this.SetUpSpeakers();
            this.OnRefresh3DSpeakerFieldForQueue();
        }

        public void CreatePath()
        {
            double width = this._animCanvas.Width / 2.0;
            double height = this._animCanvas.Height;
            if ((this._leftPath == null) && (this._rightPath == null))
            {
                this._points = new List<System.Windows.Point>();
                this._points.Add(new System.Windows.Point(this.x0, this.y01 + (((1.0 * (this.y02 - this.y01)) * this.index) / 30.0)));
                this._points.Add(new System.Windows.Point(this.x0 + (0.5 * width), 0.5 * height));
                this._points.Add(new System.Windows.Point(this.x0 + (0.8 * width), 0.8 * height));
                this._points.Add(new System.Windows.Point(this.x1, this.y11 + (((1.0 * (this.y12 - this.y11)) * this.index) / 30.0)));
                for (int i = 0; i < this._points.Count; i++)
                {
                    this.ResetPointAnimationParameters(i, width, height);
                }
            }
            else
            {
                this.IncreaseTickCounters();
                this._points[0] = new System.Windows.Point(this.x0, this.y01 + (((1.0 * (this.y02 - this.y01)) * this.index) / 30.0));
                this._points[3] = new System.Windows.Point(this.x1, this.y11 + (((1.0 * (this.y12 - this.y11)) * this.index) / 30.0));
                for (int j = 1; j < (this._points.Count - 1); j++)
                {
                    double a = this._pointXOffset[j] + (this._widthVar[j] * Math.Cos(this._tickIndex[j]));
                    a = (a > 0.0) ? a : -a;
                    double num6 = this._pointYOffset[j] + (this._heightVar[j] * Math.Cos(this._tickIndex[j]));
                    num6 = (num6 > 0.0) ? num6 : -num6;
                    this._points[j] = new System.Windows.Point(Math.Round(a) % Math.Round(width), Math.Round(num6) % Math.Round(height));
                    if (this._tickIndex[j] >= 6.2831853071795862)
                    {
                        this.ResetPointAnimationParameters(j, width, height);
                    }
                }
            }
            this._leftPath = this.CreatePath(this._points[0], this._points[1], this._points[2], this._points[3]);
            System.Windows.Point point4 = this._points[3];
            System.Windows.Point point5 = this._points[3];
            System.Windows.Point point6 = this._points[2];
            System.Windows.Point point7 = this._points[2];
            System.Windows.Point point = new System.Windows.Point((point4.X + point5.X) - point6.X, point7.Y);
            System.Windows.Point point8 = this._points[3];
            System.Windows.Point point9 = this._points[3];
            System.Windows.Point point10 = this._points[1];
            System.Windows.Point point11 = this._points[1];
            System.Windows.Point point2 = new System.Windows.Point((point8.X + point9.X) - point10.X, point11.Y);
            System.Windows.Point point12 = this._points[3];
            System.Windows.Point point13 = this._points[3];
            System.Windows.Point point14 = this._points[0];
            System.Windows.Point point15 = this._points[0];
            System.Windows.Point point3 = new System.Windows.Point((point12.X + point13.X) - point14.X, point15.Y);
            this._rightPath = this.CreatePath(this._points[3], point, point2, point3);
            this._leftPath.Stroke = this.leftGradientBrush;
            this._leftPath.StrokeThickness = 0.5;
            this._leftPath.StrokeDashArray.Add(2.0);
            this._leftPath.StrokeDashArray.Add(2.0);
            this._rightPath.Stroke = this.rightGradientBrush;
            this._rightPath.StrokeThickness = 0.5;
            this._leftPath.StrokeDashArray.Add(2.0);
            this._leftPath.StrokeDashArray.Add(2.0);
        }

        public Path CreatePath(System.Windows.Point point0, System.Windows.Point point1, System.Windows.Point point2, System.Windows.Point point3)
        {
            BezierSegment segment = new BezierSegment {
                Point1 = point1,
                Point2 = point2,
                Point3 = point3
            };
            PathFigure figure = new PathFigure {
                StartPoint = point0,
                Segments = { segment }
            };
            PathGeometry geometry = new PathGeometry {
                Figures = { figure }
            };
            return new Path { Data = geometry };
        }

        public void doPreview(bool on)
        {
            if (on && (base.Visibility == Visibility.Visible))
            {
                this._intensity.IsEnabled = true;
            }
            else
            {
                this._intensity.IsEnabled = false;
            }
        }

        private void dpTimer_Tick(object sender, EventArgs e)
        {
            this.OnRefresh3DSpeakerField();
            CommandManager.InvalidateRequerySuggested();
        }

        private void IncreaseTickCounters()
        {
            for (int i = 0; i < this._points.Count; i++)
            {
                List<double> list;
                int num2;
                (list = this._tickIndex)[num2 = i] = list[num2] + this._angleLength[i];
            }
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (!this._contentLoaded)
            {
                this._contentLoaded = true;
                Uri resourceLocator = new Uri("/SmartAudio;component/preview3dspeaker.xaml", UriKind.Relative);
                Application.LoadComponent(this, resourceLocator);
            }
        }

        private void OnRefresh3DSpeakerField()
        {
            try
            {
                for (int i = 0; i < this._animCanvas.Children.Count; i++)
                {
                    Path path = this._animCanvas.Children[i] as Path;
                    if ((path != null) && (path.Opacity > 0.015))
                    {
                        path.Opacity -= 0.015;
                    }
                }
                Path element = null;
                Path path3 = null;
                while (this._bezierPathQ.Count >= 200)
                {
                    element = this._bezierPathQ.Dequeue();
                    this._animCanvas.Children.Remove(element);
                    path3 = this._bezierPathQ.Dequeue();
                    this._animCanvas.Children.Remove(path3);
                }
                this.CreatePath();
                this._animCanvas.Children.Add(this._leftPath);
                this._bezierPathQ.Enqueue(this._leftPath);
                this._animCanvas.Children.Add(this._rightPath);
                this._bezierPathQ.Enqueue(this._rightPath);
                if (this.flag == 1)
                {
                    this.index++;
                }
                else
                {
                    this.index--;
                }
                if (this.index >= 30)
                {
                    this.flag *= -1;
                }
                else if (this.index <= 0)
                {
                    this.flag *= -1;
                }
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("Preview3DSpeaker::OnRefresh3DSpeakerField()", Severity.WARNING, exception);
            }
        }

        private void OnRefresh3DSpeakerFieldForQueue()
        {
            try
            {
                for (int i = 0; i < this._animCanvas.Children.Count; i++)
                {
                    Path path = this._animCanvas.Children[i] as Path;
                    if ((path != null) && (path.Opacity > 0.3))
                    {
                        path.Opacity -= 0.3;
                    }
                }
                Path element = null;
                Path path3 = null;
                while (this._bezierPathQ.Count >= 200)
                {
                    element = this._bezierPathQ.Dequeue();
                    this._animCanvas.Children.Remove(element);
                    path3 = this._bezierPathQ.Dequeue();
                    this._animCanvas.Children.Remove(path3);
                }
                while (this._bezierPathQ.Count < 200)
                {
                    this.CreatePath();
                    this._animCanvas.Children.Add(this._leftPath);
                    this._bezierPathQ.Enqueue(this._leftPath);
                    this._animCanvas.Children.Add(this._rightPath);
                    this._bezierPathQ.Enqueue(this._rightPath);
                    if (this.flag == 1)
                    {
                        this.index++;
                    }
                    else
                    {
                        this.index--;
                    }
                    if (this.index >= 30)
                    {
                        this.flag *= -1;
                    }
                    else if (this.index <= 0)
                    {
                        this.flag *= -1;
                    }
                }
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("Preview3DSpeaker::OnRefresh3DSpeakerField()", Severity.WARNING, exception);
            }
        }

        private void Preview3DSpeaker_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (base.Visibility != Visibility.Hidden)
            {
                Visibility visibility = base.Visibility;
            }
        }

        private void Preview3DSpeaker_Loaded(object sender, RoutedEventArgs e)
        {
            BitmapEffect bitmapEffect = base.BitmapEffect;
        }

        private void ResetPointAnimationParameters(int index, double width, double height)
        {
            this._tickIndex[index] = 3.1415926535897931 * index;
            this._angleLength[index] = 0.089759790102565518;
            System.Windows.Point point = this._points[index];
            this._widthVar[index] = 0.5 * ((0.35 * width) - point.X);
            if (index == 1)
            {
                System.Windows.Point point2 = this._points[index];
                this._pointXOffset[index] = point2.X + this._widthVar[index];
            }
            else if (index == 2)
            {
                System.Windows.Point point3 = this._points[index];
                this._pointXOffset[index] = point3.X + this._widthVar[index];
            }
            else
            {
                System.Windows.Point point4 = this._points[index];
                this._pointXOffset[index] = point4.X + this._widthVar[index];
            }
            System.Windows.Point point5 = this._points[index];
            this._heightVar[index] = 0.5 * ((0.65 * height) - point5.Y);
            System.Windows.Point point6 = this._points[index];
            this._pointYOffset[index] = point6.Y + this._heightVar[index];
        }

        private void SetUpSpeakers()
        {
            this._xLeft = this._xLeft0 + (((this._xLeft1 - this._xLeft0) * this._intensity.Value) / 180.0);
            this._yLeft = this._yLeft0 + (((this._yLeft1 - this._yLeft0) * this._intensity.Value) / 180.0);
            this._imageLeftSpeaker.SetValue(Canvas.LeftProperty, this._xLeft);
            this._imageLeftSpeaker.SetValue(Canvas.TopProperty, this._yLeft);
            this._imageLeftSpeakerRL.SetValue(Canvas.LeftProperty, this._xLeft);
            this._imageLeftSpeakerRL.SetValue(Canvas.TopProperty, (this._yLeft + this._imageLeftSpeaker.Height) + 20.0);
            this._xRight = this._xRight0 + (((this._xRight1 - this._xRight0) * this._intensity.Value) / 180.0);
            this._yRight = this._yRight0 + (((this._yRight1 - this._yRight0) * this._intensity.Value) / 180.0);
            this._imageRightSpeaker.SetValue(Canvas.LeftProperty, this._xRight);
            this._imageRightSpeaker.SetValue(Canvas.TopProperty, this._yRight);
            this._imageRightSpeakerRL.SetValue(Canvas.LeftProperty, this._xRight);
            this._imageRightSpeakerRL.SetValue(Canvas.TopProperty, (this._yRight + this._imageRightSpeaker.Height) + 20.0);
            this.x0 = Math.Ceiling(this._xLeft) + (this._imageLeftSpeaker.Width / 2.0);
            this.y01 = Math.Ceiling(this._yLeft);
            this.y02 = Math.Floor((double) (this._yLeft + this._imageLeftSpeaker.Height));
        }

        [EditorBrowsable(EditorBrowsableState.Never), DebuggerNonUserCode]
        void IComponentConnector.Connect(int connectionId, object target)
        {
            switch (connectionId)
            {
                case 1:
                    this.Grid_1 = (Grid) target;
                    return;

                case 2:
                    this._animCanvas = (Canvas) target;
                    return;

                case 3:
                    this._lrSpeakerCanvas = (Canvas) target;
                    return;

                case 4:
                    this._imageLeftSpeaker = (System.Windows.Controls.Image) target;
                    return;

                case 5:
                    this._imageLeftSpeakerRL = (System.Windows.Controls.Image) target;
                    return;

                case 6:
                    this._imageRightSpeaker = (System.Windows.Controls.Image) target;
                    return;

                case 7:
                    this._imageRightSpeakerRL = (System.Windows.Controls.Image) target;
                    return;

                case 8:
                    this._imageLaptop = (System.Windows.Controls.Image) target;
                    return;

                case 9:
                    this._intensity = (Slider) target;
                    return;

                case 10:
                    this.rectangle1 = (Border) target;
                    return;
            }
            this._contentLoaded = true;
        }

        public void UpdateSpread()
        {
            BindingOperations.GetBindingExpressionBase(this._intensity, RangeBase.ValueProperty).UpdateTarget();
        }

        public bool IsSliderHidden
        {
            get => 
                this._isSliderHidden;
            set
            {
                this._isSliderHidden = value;
                this._intensity.Visibility = this._isSliderHidden ? Visibility.Hidden : Visibility.Visible;
            }
        }
    }
}

