namespace SmartAudio
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Markup;
    using System.Windows.Media;
    using System.Windows.Media.Effects;
    using System.Windows.Media.Imaging;

    public class ImageCheckBox : UserControl, IComponentConnector
    {
        internal StackPanel _checkBoxPanel;
        private bool _contentLoaded;
        internal Image _image;
        internal System.Windows.Controls.Label _label;
        private bool _radioStyle;
        private bool _readOnly;
        private bool _selected;
        private ImageSource _selectedImage;
        private bool _showLabel;
        private ImageSource _unSelectedImage;
        public static readonly DependencyProperty SelectedPropertyDPProperty = DependencyProperty.Register("SelectedImageDP", typeof(ImageSource), typeof(ImageCheckBox));
        public static readonly DependencyProperty UnselectedPropertyDPProperty = DependencyProperty.Register("UnselectedImageDP", typeof(ImageSource), typeof(ImageCheckBox));

        public event ItemStateChanged OnItemStateChanged;

        public event ItemStateChanging OnItemStateChanging;

        public ImageCheckBox()
        {
            base.IsTabStop = true;
            this.InitializeComponent();
            this._image.MouseLeftButtonDown += new MouseButtonEventHandler(this.ImageCheckBox_MouseLeftButtonDown);
            this._label.MouseLeftButtonDown += new MouseButtonEventHandler(this.ImageCheckBox_MouseLeftButtonDown);
            this._radioStyle = false;
            this._readOnly = false;
            base.KeyDown += new KeyEventHandler(this.ImageCheckBox_KeyDown);
            Keyboard.Focus(this._image);
            base.Focusable = true;
            this._image.Focusable = true;
            base.MouseEnter += new MouseEventHandler(this._image_MouseEnter);
            base.MouseLeave += new MouseEventHandler(this._image_MouseLeave);
        }

        private void _image_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            try
            {
                SmartAudioLog.Log("Failed to load image " + e.ErrorException.Message, Severity.FATALERROR, e.ErrorException);
            }
            catch (Exception)
            {
                Trace.WriteLine("Image Failed " + e.ErrorException.Message);
            }
        }

        private void _image_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!this._readOnly)
            {
                this.GlowItem(this._image, true);
            }
        }

        private void _image_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!this._readOnly)
            {
                this.GlowItem(this._image, false);
            }
        }

        public void ApplyStyle(string styleName)
        {
            Style style = (Style) base.FindResource(styleName);
            Setter setter = this.FindProperty(style, "SelectedImageDP");
            Setter setter2 = this.FindProperty(style, "UnselectedImageDP");
            if (setter != null)
            {
                this.SelectedImage = this.GetImage(new Uri(setter.Value.ToString()));
            }
            if (setter2 != null)
            {
                this.UnselectedImage = this.GetImage(new Uri(setter2.Value.ToString()));
            }
            if (this._selected)
            {
                this._image.Source = this.SelectedImage;
            }
            else
            {
                this._image.Source = this.UnselectedImage;
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

        public void GlowImage(bool flag)
        {
            this.GlowItem(this._image, flag);
        }

        private void GlowItem(Image control, bool flag)
        {
            if (control != null)
            {
                if (flag)
                {
                    OuterGlowBitmapEffect effect = new OuterGlowBitmapEffect {
                        GlowColor = Colors.CornflowerBlue
                    };
                    control.BitmapEffect = effect;
                    effect.GlowSize = 5.0;
                }
                else
                {
                    control.BitmapEffect = null;
                }
            }
        }

        private void ImageCheckBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                this.SelectItem();
            }
        }

        private void ImageCheckBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.SelectItem();
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (!this._contentLoaded)
            {
                this._contentLoaded = true;
                Uri resourceLocator = new Uri("/SmartAudio;component/imagecheckbox.xaml", UriKind.Relative);
                Application.LoadComponent(this, resourceLocator);
            }
        }

        public void SelectItem()
        {
            if (!this._readOnly)
            {
                bool selected = this.Selected;
                if (!this._radioStyle)
                {
                    selected = !this.Selected;
                }
                else if (!this.Selected)
                {
                    selected = true;
                }
                if ((this.OnItemStateChanging == null) || this.OnItemStateChanging(this, selected))
                {
                    this.Selected = selected;
                    if (this.OnItemStateChanged != null)
                    {
                        this.OnItemStateChanged(this, this._selected);
                    }
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), DebuggerNonUserCode]
        void IComponentConnector.Connect(int connectionId, object target)
        {
            switch (connectionId)
            {
                case 1:
                    this._checkBoxPanel = (StackPanel) target;
                    return;

                case 2:
                    this._image = (Image) target;
                    this._image.ImageFailed += new EventHandler<ExceptionRoutedEventArgs>(this._image_ImageFailed);
                    return;

                case 3:
                    this._label = (System.Windows.Controls.Label) target;
                    return;
            }
            this._contentLoaded = true;
        }

        public double ImageHeight
        {
            get => 
                this._image.Height;
            set
            {
                this._image.Height = value;
            }
        }

        public double ImageWidth
        {
            get => 
                this._image.Width;
            set
            {
                this._image.Width = value;
            }
        }

        public string Label
        {
            get => 
                ((string) this._label.Content);
            set
            {
                this._label.Content = value;
            }
        }

        public bool RadioStyle
        {
            get => 
                this._radioStyle;
            set
            {
                this._radioStyle = value;
            }
        }

        public bool ReadOnly
        {
            get => 
                this._readOnly;
            set
            {
                this._readOnly = value;
            }
        }

        public bool Selected
        {
            get => 
                this._selected;
            set
            {
                this._selected = value;
                if (this._selected)
                {
                    this._image.Source = this._selectedImage;
                }
                else
                {
                    this._image.Source = this._unSelectedImage;
                }
            }
        }

        public ImageSource SelectedImage
        {
            get => 
                this._selectedImage;
            set
            {
                this._selectedImage = value;
            }
        }

        public ImageSource SelectedImageDP
        {
            get => 
                ((ImageSource) base.GetValue(SelectedPropertyDPProperty));
            set
            {
                base.SetValue(SelectedPropertyDPProperty, value);
                this._selectedImage = value;
            }
        }

        public bool ShowLabel
        {
            get => 
                this._showLabel;
            set
            {
                if (!value)
                {
                    this._checkBoxPanel.Children.Remove(this._label);
                }
                else if (this._checkBoxPanel.Children.IndexOf(this._label) == -1)
                {
                    this._checkBoxPanel.Children.Add(this._label);
                }
                this._showLabel = value;
            }
        }

        public System.Windows.Media.Stretch Stretch
        {
            get => 
                this._image.Stretch;
            set
            {
                this._image.Stretch = value;
            }
        }

        public ImageSource UnselectedImage
        {
            get => 
                this._unSelectedImage;
            set
            {
                this._unSelectedImage = value;
            }
        }

        public ImageSource UnselectedImageDP
        {
            get => 
                ((ImageSource) base.GetValue(UnselectedPropertyDPProperty));
            set
            {
                base.SetValue(UnselectedPropertyDPProperty, value);
                this.UnselectedImage = value;
            }
        }
    }
}

