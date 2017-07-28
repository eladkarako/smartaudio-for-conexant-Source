namespace SmartAudio
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Data;
    using System.Windows.Markup;
    using System.Windows.Media;
    using System.Windows.Media.Animation;
    using System.Windows.Media.Media3D;
    using System.Windows.Shapes;
    using System.Windows.Threading;

    public class Preview3DSound : UserControl, IComponentConnector
    {
        internal EllipseGeometry _animFieldGeoGroup;
        internal GeometryGroup _animGeoGroup;
        private bool _contentLoaded;
        internal EllipseGeometry _ElliGeo1;
        internal EllipseGeometry _ElliGeo2;
        internal EllipseGeometry _ElliGeo3;
        internal Image _imageLaptop;
        internal Slider _intensity;
        private bool _isSliderHidden;
        internal Viewport3D _preview3D;
        internal Path animPath;
        internal GradientStop m_BlackStop;
        private int m_index;
        internal Path MyPath2;
        internal AxisAngleRotation3D MyRotation3D;
        internal AxisAngleRotation3D MyRotation3D2;
        internal Border rectangle1;
        private DispatcherTimer timer;

        public Preview3DSound()
        {
            this.InitializeComponent();
            this._intensity.Minimum = 0.0;
            this._intensity.Maximum = 180.0;
            this._intensity.ValueChanged += new RoutedPropertyChangedEventHandler<double>(this._intensity_ValueChanged);
            base.Loaded += new RoutedEventHandler(this.Preview3DSound_Loaded);
            this.m_index = 0;
        }

        private void _intensity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ((PerspectiveCamera) this._preview3D.Camera).FieldOfView = 25.0 + (((180.0 - this._intensity.Value) * 15.0) / 180.0);
            this._preview3D.Opacity = this._intensity.Value / 180.0;
        }

        public void doPreview(bool on)
        {
            if (on)
            {
                this._intensity.IsEnabled = true;
            }
            else
            {
                this._intensity.IsEnabled = false;
            }
        }

        public void initAnim()
        {
            DoubleAnimation animation = new DoubleAnimation {
                Duration = new Duration(TimeSpan.FromMilliseconds(4000.0)),
                AutoReverse = false,
                From = 150.0,
                To = 200.0
            };
            this._ElliGeo1.BeginAnimation(EllipseGeometry.RadiusXProperty, animation);
            this._ElliGeo1.BeginAnimation(EllipseGeometry.RadiusYProperty, animation);
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (!this._contentLoaded)
            {
                this._contentLoaded = true;
                Uri resourceLocator = new Uri("/SmartAudio;component/preview3dsound.xaml", UriKind.Relative);
                Application.LoadComponent(this, resourceLocator);
            }
        }

        private void Preview3DSound_Loaded(object sender, RoutedEventArgs e)
        {
            this._intensity_ValueChanged(null, null);
        }

        [DebuggerNonUserCode, EditorBrowsable(EditorBrowsableState.Never)]
        void IComponentConnector.Connect(int connectionId, object target)
        {
            switch (connectionId)
            {
                case 1:
                    this._preview3D = (Viewport3D) target;
                    return;

                case 2:
                    this.MyRotation3D2 = (AxisAngleRotation3D) target;
                    return;

                case 3:
                    this.MyPath2 = (Path) target;
                    return;

                case 4:
                    this._animFieldGeoGroup = (EllipseGeometry) target;
                    return;

                case 5:
                    this.m_BlackStop = (GradientStop) target;
                    return;

                case 6:
                    this.MyRotation3D = (AxisAngleRotation3D) target;
                    return;

                case 7:
                    this.animPath = (Path) target;
                    return;

                case 8:
                    this._animGeoGroup = (GeometryGroup) target;
                    return;

                case 9:
                    this._ElliGeo1 = (EllipseGeometry) target;
                    return;

                case 10:
                    this._ElliGeo2 = (EllipseGeometry) target;
                    return;

                case 11:
                    this._ElliGeo3 = (EllipseGeometry) target;
                    return;

                case 12:
                    this._imageLaptop = (Image) target;
                    return;

                case 13:
                    this._intensity = (Slider) target;
                    return;

                case 14:
                    this.rectangle1 = (Border) target;
                    return;
            }
            this._contentLoaded = true;
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            this.m_index++;
            this._ElliGeo1.RadiusX = this.m_index * 2;
            this._ElliGeo1.RadiusY = this.m_index * 2;
            this.m_BlackStop.Offset = (this.m_index * 1.0) / 100.0;
            if (this.m_index > 100)
            {
                this.m_index = 0;
            }
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

