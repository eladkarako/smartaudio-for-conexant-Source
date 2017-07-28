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

    public class PreviewTwoDotOne : UserControl, IHDAudioPreview, IComponentConnector
    {
        private CxHDAudioChannelEnumeratorClass _audioChannelEnumerator;
        internal Slider _centerSpeaker;
        private bool _contentLoaded;
        internal TextBlock _LeftCenter;
        internal Slider _leftSpeaker;
        private CxHDMasterVolumeControl _masterVolumeControl;
        internal TextBlock _RightCenter;
        internal Slider _rightSpeaker;
        internal Image image1;

        public PreviewTwoDotOne()
        {
            this.InitializeComponent();
            this.Localize();
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (!this._contentLoaded)
            {
                this._contentLoaded = true;
                Uri resourceLocator = new Uri("/SmartAudio;component/previewtwodotone.xaml", UriKind.Relative);
                Application.LoadComponent(this, resourceLocator);
            }
        }

        public void Localize()
        {
            this._LeftCenter.Text = Resources.SA_LeftChannelShort;
            this._RightCenter.Text = Resources.SA_RightChannelShort;
        }

        public void OnMasterVolumeChanged(double newValue)
        {
        }

        public void OnMasterVolumeChanging(double newValue)
        {
            this._leftSpeaker.Value = newValue;
            this._rightSpeaker.Value = newValue;
            this._centerSpeaker.Value = newValue;
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
                    this._centerSpeaker = (Slider) target;
                    return;

                case 3:
                    this._LeftCenter = (TextBlock) target;
                    return;

                case 4:
                    this._RightCenter = (TextBlock) target;
                    return;

                case 5:
                    this._leftSpeaker = (Slider) target;
                    return;

                case 6:
                    this._rightSpeaker = (Slider) target;
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
            }
        }
    }
}

