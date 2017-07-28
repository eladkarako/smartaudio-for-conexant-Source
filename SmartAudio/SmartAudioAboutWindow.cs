namespace SmartAudio
{
    using CxHDAudioAPILib;
    using SmartAudio.Properties;
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Forms;
    using System.Windows.Input;
    using System.Windows.Markup;
    using System.Windows.Shapes;

    public class SmartAudioAboutWindow : Window, IComponentConnector
    {
        internal TextBlock _APIVersion;
        internal System.Windows.Controls.Button _closeBtn;
        internal TextBlock _codecModel;
        internal TextBlock _codecModelText;
        private bool _contentLoaded;
        internal TextBlock _driverVersion;
        internal TextBlock _driverVersionText;
        internal TextBlock _HDAudioAPIVersionText;
        internal Hyperlink _hyperLink;
        internal TextBlock _hyperLinkText;
        internal Border _mainWindow;
        internal TextBlock _smartAudioVersion;
        internal TextBlock _smartAudioVersionText;
        internal Border _titleBar;
        internal Image image1;
        internal Rectangle rectangle1;
        internal StackPanel stackPanel1;
        internal StackPanel stackPanel2;
        internal StackPanel stackPanel3;

        public SmartAudioAboutWindow()
        {
            this.InitializeComponent();
            this._titleBar.MouseLeftButtonDown += new MouseButtonEventHandler(this.Window_MouseLeftButtonDown);
            base.Title = Resources.SA_XAML_SmartAudioII;
            this._codecModelText.Text = Resources.SA_IFPACKAGEID;
            this._driverVersionText.Text = Resources.SA_IFDRIVERVERSION;
            this._smartAudioVersionText.Text = Resources.SA_IFSSVERSION;
            this._HDAudioAPIVersionText.Text = Resources.SA_CxHDAudioAPIVersion;
            this._hyperLink.Inlines.Clear();
            this._hyperLink.Inlines.Add(new System.Windows.Documents.Run(Resources.SA_COPYRIGHT));
        }

        private void _closeBtn_Click(object sender, RoutedEventArgs e)
        {
            base.Close();
        }

        private void _hyperLink_Click(object sender, RoutedEventArgs e)
        {
            Hyperlink hyperlink = (Hyperlink) sender;
            Process.Start(hyperlink.NavigateUri.AbsoluteUri);
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (!this._contentLoaded)
            {
                this._contentLoaded = true;
                Uri resourceLocator = new Uri("/SmartAudio;component/smartaudioaboutwindow.xaml", UriKind.Relative);
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

        [DebuggerNonUserCode, EditorBrowsable(EditorBrowsableState.Never)]
        void IComponentConnector.Connect(int connectionId, object target)
        {
            switch (connectionId)
            {
                case 1:
                    ((SmartAudioAboutWindow) target).MouseLeftButtonDown += new MouseButtonEventHandler(this.Window_MouseLeftButtonDown);
                    return;

                case 2:
                    this._mainWindow = (Border) target;
                    return;

                case 3:
                    this.stackPanel1 = (StackPanel) target;
                    return;

                case 4:
                    this._closeBtn = (System.Windows.Controls.Button) target;
                    this._closeBtn.Click += new RoutedEventHandler(this._closeBtn_Click);
                    return;

                case 5:
                    this.image1 = (Image) target;
                    return;

                case 6:
                    this.rectangle1 = (Rectangle) target;
                    return;

                case 7:
                    this._hyperLinkText = (TextBlock) target;
                    return;

                case 8:
                    this._hyperLink = (Hyperlink) target;
                    this._hyperLink.Click += new RoutedEventHandler(this._hyperLink_Click);
                    return;

                case 9:
                    this.stackPanel2 = (StackPanel) target;
                    return;

                case 10:
                    this._codecModelText = (TextBlock) target;
                    return;

                case 11:
                    this._driverVersionText = (TextBlock) target;
                    return;

                case 12:
                    this._HDAudioAPIVersionText = (TextBlock) target;
                    return;

                case 13:
                    this._smartAudioVersionText = (TextBlock) target;
                    return;

                case 14:
                    this.stackPanel3 = (StackPanel) target;
                    return;

                case 15:
                    this._codecModel = (TextBlock) target;
                    return;

                case 0x10:
                    this._driverVersion = (TextBlock) target;
                    return;

                case 0x11:
                    this._APIVersion = (TextBlock) target;
                    return;

                case 0x12:
                    this._smartAudioVersion = (TextBlock) target;
                    return;

                case 0x13:
                    this._titleBar = (Border) target;
                    return;
            }
            this._contentLoaded = true;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.GetPosition((UIElement) sender);
            base.DragMove();
        }

        public CxHDAudioFactory AudioFactory
        {
            set
            {
                try
                {
                    this._codecModel.Text = value.DeviceIOConfig.PackageID;
                }
                catch (Exception exception)
                {
                    SmartAudioLog.Log("SmartAudioAboutWindow.AudioFactory", Severity.WARNING, exception);
                }
                if (this._codecModel.Text == "")
                {
                    this._codecModel.Visibility = Visibility.Hidden;
                    this._codecModelText.Visibility = Visibility.Hidden;
                }
                this._driverVersion.Text = value.DeviceIOConfig.DriverVersion;
                AssemblyName name = Assembly.GetExecutingAssembly().GetName(false);
                if (name != null)
                {
                    this._smartAudioVersion.Text = name.Version.ToString();
                }
                App current = System.Windows.Application.Current as App;
                this._APIVersion.Text = current.HDAudioAPIVersion;
            }
        }
    }
}

