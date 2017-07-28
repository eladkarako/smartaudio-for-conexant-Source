namespace SmartAudio
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Forms;
    using System.Windows.Markup;
    using System.Windows.Shapes;

    public class SmartAudioTrayWindow : Window, IComponentConnector
    {
        private bool _contentLoaded;
        internal Rectangle _mainWindow;
        internal Image image1;

        public SmartAudioTrayWindow()
        {
            this.InitializeComponent();
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (!this._contentLoaded)
            {
                this._contentLoaded = true;
                Uri resourceLocator = new Uri("/SmartAudio;component/smartaudiotraywindow.xaml", UriKind.Relative);
                System.Windows.Application.LoadComponent(this, resourceLocator);
            }
        }

        public void ShowWindow()
        {
            base.Visibility = (base.Visibility == Visibility.Hidden) ? Visibility.Visible : Visibility.Hidden;
            if (base.Visibility == Visibility.Visible)
            {
                base.Left = (Screen.PrimaryScreen.WorkingArea.Left + Screen.PrimaryScreen.WorkingArea.Width) - base.Width;
                base.Top = (Screen.PrimaryScreen.WorkingArea.Top + Screen.PrimaryScreen.WorkingArea.Height) - base.Height;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), DebuggerNonUserCode]
        void IComponentConnector.Connect(int connectionId, object target)
        {
            switch (connectionId)
            {
                case 1:
                    this._mainWindow = (Rectangle) target;
                    return;

                case 2:
                    this.image1 = (Image) target;
                    return;
            }
            this._contentLoaded = true;
        }
    }
}

