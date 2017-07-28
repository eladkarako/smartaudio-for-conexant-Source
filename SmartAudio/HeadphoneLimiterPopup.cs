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

    public class HeadphoneLimiterPopup : Window, IComponentConnector
    {
        internal Button _accept;
        internal TextBlock _acceptText;
        internal Button _cancel;
        internal TextBlock _cancelText;
        private bool _contentLoaded;
        internal CheckBox _dontShowPopup;
        internal Image _headPhoneLimiter;
        internal TextBlock _limiterText;

        public HeadphoneLimiterPopup()
        {
            this.InitializeComponent();
            base.Title = Resources.SA_SmartAudio;
            this._limiterText.Text = Resources.SA_GSMARKWARNING;
            this._acceptText.Text = Resources.SA_ACCEPT;
            this._cancelText.Text = Resources.SA_CANCEL;
            base.MouseLeftButtonDown += new MouseButtonEventHandler(this.JackRetaskingPopup_MouseLeftButtonDown);
        }

        private void _accept_Click(object sender, RoutedEventArgs e)
        {
            base.DialogResult = true;
            base.Close();
        }

        private void _cancel_Click(object sender, RoutedEventArgs e)
        {
            base.DialogResult = false;
            base.Close();
        }

        private void _closeBtn_Click(object sender, RoutedEventArgs e)
        {
        }

        private void _fullMode_Click(object sender, RoutedEventArgs e)
        {
            base.Close();
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (!this._contentLoaded)
            {
                this._contentLoaded = true;
                Uri resourceLocator = new Uri("/SmartAudio;component/headphonelimiterpopup.xaml", UriKind.Relative);
                Application.LoadComponent(this, resourceLocator);
            }
        }

        private void JackRetaskingPopup_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            base.DragMove();
        }

        [DebuggerNonUserCode, EditorBrowsable(EditorBrowsableState.Never)]
        void IComponentConnector.Connect(int connectionId, object target)
        {
            switch (connectionId)
            {
                case 1:
                    this._accept = (Button) target;
                    this._accept.Click += new RoutedEventHandler(this._accept_Click);
                    return;

                case 2:
                    this._acceptText = (TextBlock) target;
                    return;

                case 3:
                    this._cancel = (Button) target;
                    this._cancel.Click += new RoutedEventHandler(this._cancel_Click);
                    return;

                case 4:
                    this._cancelText = (TextBlock) target;
                    return;

                case 5:
                    this._headPhoneLimiter = (Image) target;
                    return;

                case 6:
                    this._limiterText = (TextBlock) target;
                    return;

                case 7:
                    this._dontShowPopup = (CheckBox) target;
                    return;
            }
            this._contentLoaded = true;
        }
    }
}

