namespace SmartAudio
{
    using CxHDAudioAPILib;
    using SmartAudio.Properties;
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Markup;

    public class JackprofileConfigControl : UserControl, ISmartAudioPage, IComponentConnector
    {
        private bool _contentLoaded;
        private CxHDAudioJackConfig _jackConfig;

        public JackprofileConfigControl()
        {
            this.InitializeComponent();
        }

        private void _ioRetaskingScheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void _jackTypeImage_Click(object sender, RoutedEventArgs e)
        {
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (!this._contentLoaded)
            {
                this._contentLoaded = true;
                Uri resourceLocator = new Uri("/SmartAudio;component/jackprofileconfigcontrol.xaml", UriKind.Relative);
                Application.LoadComponent(this, resourceLocator);
            }
        }

        public bool Localize()
        {
            string text1 = Resources.SA_XAML_2_0ChannelSpeakers_Default;
            string text2 = Resources.SA_XAML_2_1ChannelSpeakers;
            string text3 = Resources.SA_XAML_5_1ChannelSurround;
            return true;
        }

        [EditorBrowsable(EditorBrowsableState.Never), DebuggerNonUserCode]
        void IComponentConnector.Connect(int connectionId, object target)
        {
            this._contentLoaded = true;
        }

        public CxHDAudioFactory AudioFactory
        {
            get => 
                null;
            set
            {
            }
        }

        public string FriendlyName =>
            "Jack Retasking";

        public CxHDAudioJackConfig JackConfig
        {
            get => 
                this._jackConfig;
            set
            {
                this._jackConfig = value;
            }
        }
    }
}

