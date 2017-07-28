namespace SmartAudio
{
    using CxHDAudioAPILib;
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Markup;

    public class PreviewMixer : UserControl, IHDAudioPreview, IComponentConnector
    {
        private CxHDAudioChannelEnumeratorClass _audioChannelEnumerator;
        private bool _contentLoaded;
        private CxHDMasterVolumeControl _masterVolumeControl;
        internal Image image1;
        internal Slider slider;

        public PreviewMixer()
        {
            this.InitializeComponent();
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (!this._contentLoaded)
            {
                this._contentLoaded = true;
                Uri resourceLocator = new Uri("/SmartAudio;component/previewmixer.xaml", UriKind.Relative);
                Application.LoadComponent(this, resourceLocator);
            }
        }

        public void Localize()
        {
        }

        public void OnMasterVolumeChanged(double newValue)
        {
        }

        public void OnMasterVolumeChanging(double newValue)
        {
        }

        [EditorBrowsable(EditorBrowsableState.Never), DebuggerNonUserCode]
        void IComponentConnector.Connect(int connectionId, object target)
        {
            switch (connectionId)
            {
                case 1:
                    this.image1 = (Image) target;
                    return;

                case 2:
                    this.slider = (Slider) target;
                    return;
            }
            this._contentLoaded = true;
        }

        public CxHDAudioChannelEnumeratorClass AudioChannelEnumerator
        {
            get => 
                this._audioChannelEnumerator;
            set
            {
                this._audioChannelEnumerator = value;
            }
        }

        public CxHDMasterVolumeControl MasterVolume
        {
            get => 
                this._masterVolumeControl;
            set
            {
                this._masterVolumeControl = value;
                this.slider.Ticks.Clear();
                this.slider.Ticks.Add((this.slider.Maximum - this.slider.Minimum) / 2.0);
                this.slider.Visibility = Visibility.Hidden;
            }
        }
    }
}

