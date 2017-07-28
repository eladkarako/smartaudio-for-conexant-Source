namespace SmartAudio
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Markup;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    public class LEDUserControl : UserControl, IComponentConnector
    {
        private bool _contentLoaded;
        internal Image _LEDImage;
        private ImageSource _LEDOffImage;
        private ImageSource _LEDOnImage;
        private bool _state;
        public static readonly DependencyProperty OffStateImagePropertyDPProperty = DependencyProperty.Register("OffStateImageDP", typeof(ImageSource), typeof(LEDUserControl));
        public static readonly DependencyProperty OnStateImagePropertyDPProperty = DependencyProperty.Register("OnStateImageDP", typeof(ImageSource), typeof(LEDUserControl));
        public static readonly DependencyProperty StateProperty = DependencyProperty.Register("State", typeof(bool), typeof(LEDUserControl), new PropertyMetadata(false));

        public LEDUserControl()
        {
            this.InitializeComponent();
            this._LEDOnImage = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/Small-LED-ON.png", UriKind.RelativeOrAbsolute));
            this._LEDOffImage = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/Small-LED-OFF.png", UriKind.RelativeOrAbsolute));
        }

        public void ApplyStyle(string styleName)
        {
            Style style = (Style) base.FindResource(styleName);
            Setter setter = this.FindProperty(style, "OnStateImageDP");
            Setter setter2 = this.FindProperty(style, "OffStateImageDP");
            if (setter != null)
            {
                this.OnStateImage = this.GetImage(new Uri(setter.Value.ToString()));
            }
            if (setter2 != null)
            {
                this.OffStateImage = this.GetImage(new Uri(setter2.Value.ToString()));
            }
            if (this._state)
            {
                this._LEDImage.Source = this.OnStateImage;
            }
            else
            {
                this._LEDImage.Source = this.OffStateImage;
            }
        }

        public Setter FindProperty(Style style, string property)
        {
            foreach (Setter setter in style.Setters)
            {
                if (setter.Property.ToString() == property)
                {
                    return setter;
                }
            }
            return null;
        }

        public ImageSource GetImage(Uri imageURL)
        {
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.UriSource = imageURL;
            image.EndInit();
            return image;
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (!this._contentLoaded)
            {
                this._contentLoaded = true;
                Uri resourceLocator = new Uri("/SmartAudio;component/ledusercontrol.xaml", UriKind.Relative);
                Application.LoadComponent(this, resourceLocator);
            }
        }

        [DebuggerNonUserCode, EditorBrowsable(EditorBrowsableState.Never)]
        void IComponentConnector.Connect(int connectionId, object target)
        {
            if (connectionId == 1)
            {
                this._LEDImage = (Image) target;
            }
            else
            {
                this._contentLoaded = true;
            }
        }

        public ImageSource OffStateImage
        {
            get => 
                this._LEDOffImage;
            set
            {
                this._LEDOffImage = value;
            }
        }

        public ImageSource OffStateImageDP
        {
            get => 
                ((ImageSource) base.GetValue(OffStateImagePropertyDPProperty));
            set
            {
                base.SetValue(OffStateImagePropertyDPProperty, value);
                this.OffStateImage = value;
            }
        }

        public ImageSource OnStateImage
        {
            get => 
                this._LEDOnImage;
            set
            {
                this._LEDOnImage = value;
            }
        }

        public ImageSource OnStateImageDP
        {
            get => 
                ((ImageSource) base.GetValue(OnStateImagePropertyDPProperty));
            set
            {
                base.SetValue(OnStateImagePropertyDPProperty, value);
                this.OnStateImage = value;
            }
        }

        public bool State
        {
            get => 
                this._state;
            set
            {
                this._LEDImage.Source = value ? this._LEDOnImage : this._LEDOffImage;
                this._state = value;
            }
        }
    }
}

