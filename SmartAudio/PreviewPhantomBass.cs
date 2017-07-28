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
    using System.Windows.Media.Media3D;

    public class PreviewPhantomBass : UserControl, IComponentConnector
    {
        internal PerspectiveCamera _camera1;
        internal PerspectiveCamera _camera2;
        internal PerspectiveCamera _camera3;
        internal PerspectiveCamera _camera4;
        private bool _contentLoaded;
        internal Image _imageLaptop;
        internal Slider _intensity;
        private bool _isSliderHidden;
        internal Viewport3D _preview3D;
        internal Viewport3D _preview3D_2;
        internal Viewport3D _preview3D_3;
        internal Viewport3D _preview3D_4;
        internal AxisAngleRotation3D MyRotation3D;
        internal Border rectangle1;

        public PreviewPhantomBass()
        {
            this.InitializeComponent();
            base.Loaded += new RoutedEventHandler(this.PreviewPhantomBass_Loaded);
        }

        private void _intensity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double num = 120.0;
            double num2 = 80.0;
            double num3 = (this._intensity.Value * 0.4) + 80.0;
            this._camera1.FieldOfView = (num + num2) - num3;
            this._camera2.FieldOfView = this._camera1.FieldOfView + 12.0;
            this._camera3.FieldOfView = this._camera1.FieldOfView + 34.0;
            this._camera4.FieldOfView = this._camera1.FieldOfView + 48.0;
            double num4 = -1.1;
            double num5 = 0.085;
            this._camera1.Position = new Point3D(-1.0 + (((num4 + 0.9) * (num3 - num2)) / (num - num2)), (num5 * (num3 - num2)) / (num - num2), this._camera1.Position.Z);
            this._camera2.Position = new Point3D(this._camera1.Position.X + 0.05, this._camera1.Position.Y + 7E-05, this._camera1.Position.Z);
            this._camera3.Position = new Point3D(this._camera1.Position.X + 0.1, this._camera2.Position.Y + 7E-05, this._camera2.Position.Z);
            this._camera4.Position = new Point3D(this._camera1.Position.X + 0.2, this._camera2.Position.Y + 7E-05, this._camera2.Position.Z);
            this._preview3D.Opacity = (num3 - num2) / (num - num2);
            this._preview3D_2.Opacity = this._preview3D.Opacity * 0.8;
            this._preview3D_3.Opacity = this._preview3D.Opacity * 0.5;
            this._preview3D_4.Opacity = this._preview3D.Opacity * 0.3;
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

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (!this._contentLoaded)
            {
                this._contentLoaded = true;
                Uri resourceLocator = new Uri("/SmartAudio;component/previewphantombass.xaml", UriKind.Relative);
                Application.LoadComponent(this, resourceLocator);
            }
        }

        private void PreviewPhantomBass_Loaded(object sender, RoutedEventArgs e)
        {
            this._intensity_ValueChanged(null, null);
        }

        [EditorBrowsable(EditorBrowsableState.Never), DebuggerNonUserCode]
        void IComponentConnector.Connect(int connectionId, object target)
        {
            switch (connectionId)
            {
                case 1:
                    this._imageLaptop = (Image) target;
                    return;

                case 2:
                    this._preview3D_4 = (Viewport3D) target;
                    return;

                case 3:
                    this._camera4 = (PerspectiveCamera) target;
                    return;

                case 4:
                    this._preview3D_3 = (Viewport3D) target;
                    return;

                case 5:
                    this._camera3 = (PerspectiveCamera) target;
                    return;

                case 6:
                    this._preview3D_2 = (Viewport3D) target;
                    return;

                case 7:
                    this._camera2 = (PerspectiveCamera) target;
                    return;

                case 8:
                    this._preview3D = (Viewport3D) target;
                    return;

                case 9:
                    this._camera1 = (PerspectiveCamera) target;
                    return;

                case 10:
                    this.MyRotation3D = (AxisAngleRotation3D) target;
                    return;

                case 11:
                    this._intensity = (Slider) target;
                    this._intensity.ValueChanged += new RoutedPropertyChangedEventHandler<double>(this._intensity_ValueChanged);
                    return;

                case 12:
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

