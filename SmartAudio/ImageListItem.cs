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

    public class ImageListItem : UserControl, IComponentConnector
    {
        internal StackPanel _checkBoxPanel;
        private bool _contentLoaded;
        internal Image _image;
        private bool _isEnabled;
        internal System.Windows.Controls.Label _label;
        private bool _selected;
        private ImageSource _selectedImage;
        private bool _showLabel;
        private ImageSource _unSelectedImage;

        public ImageListItem()
        {
            this.InitializeComponent();
            base.KeyDown += new KeyEventHandler(this.ImageListItem_KeyDown);
            this._isEnabled = true;
        }

        private void ImageListItem_KeyDown(object sender, KeyEventArgs e)
        {
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (!this._contentLoaded)
            {
                this._contentLoaded = true;
                Uri resourceLocator = new Uri("/SmartAudio;component/imagelistitem.xaml", UriKind.Relative);
                Application.LoadComponent(this, resourceLocator);
            }
        }

        [DebuggerNonUserCode, EditorBrowsable(EditorBrowsableState.Never)]
        void IComponentConnector.Connect(int connectionId, object target)
        {
            switch (connectionId)
            {
                case 1:
                    this._checkBoxPanel = (StackPanel) target;
                    return;

                case 2:
                    this._image = (Image) target;
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

        public bool SAIsEnabled
        {
            get => 
                this._isEnabled;
            set
            {
                this._isEnabled = value;
            }
        }

        public bool Selected
        {
            get => 
                this._selected;
            set
            {
                if (this._isEnabled)
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

        public ImageSource UnselectedImage
        {
            get => 
                this._unSelectedImage;
            set
            {
                this._unSelectedImage = value;
            }
        }
    }
}

