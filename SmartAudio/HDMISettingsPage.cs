namespace SmartAudio
{
    using CxHDAudioAPILib;
    using SmartAudio.Properties;
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Markup;
    using System.Windows.Media;
    using System.Windows.Media.Effects;

    public class HDMISettingsPage : UserControl, ISmartAudioPage, IComponentConnector
    {
        private CxHDAudioFactory _audioFactory;
        private CxHDAudioAuxEndPoints _auxEndPoint;
        private bool _contentLoaded;
        internal ImageCheckBox _HDMIEndPoint;
        internal ImageCheckBox _hdmiSwitch;
        private string _HDMITootlTipOff;
        private string _HDMITootlTipOn;
        internal ImageCheckBox _PCEndPoint;
        internal ImageCheckBox _PCSelected;
        internal ImageCheckBox _TVConnection;
        internal ImageCheckBox _TVSelected;

        public HDMISettingsPage()
        {
            this.InitializeComponent();
            this.Localize();
            this._hdmiSwitch.OnItemStateChanged += new ItemStateChanged(this._hdmiSwitch_OnItemStateChanged);
            this._PCEndPoint.MouseDoubleClick += new MouseButtonEventHandler(this._PCEndPoint_MouseDoubleClick);
            this._HDMIEndPoint.MouseDoubleClick += new MouseButtonEventHandler(this._HDMIEndPoint_MouseDoubleClick);
            this._hdmiSwitch.ToolTip = this._hdmiSwitch.Selected ? this._HDMITootlTipOn : this._HDMITootlTipOff;
            this._TVConnection.ReadOnly = true;
        }

        [DebuggerNonUserCode]
        internal Delegate _CreateDelegate(Type delegateType, string handler) => 
            Delegate.CreateDelegate(delegateType, this, handler);

        private void _HDMIEndPoint_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this._hdmiSwitch.Selected = true;
            this._TVSelected.Selected = true;
            this._PCEndPoint.Selected = false;
            this._HDMIEndPoint.Selected = true;
            this._PCSelected.Selected = false;
            this.GlowItem(this._TVConnection, true);
            this.GlowItem(this._HDMIEndPoint, true);
            this._hdmiSwitch.Selected = true;
            this.SetHDMIState(true);
        }

        private void _hdmiSwitch_OnItemStateChanged(ImageCheckBox item, bool newState)
        {
            this.ShowHDMIState(item.Selected);
            this.SetHDMIState(item.Selected);
        }

        private void _PCEndPoint_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this._PCEndPoint.Selected = true;
            this._TVSelected.Selected = false;
            this._HDMIEndPoint.Selected = false;
            this._PCSelected.Selected = true;
            this.GlowItem(this._HDMIEndPoint, false);
            this.GlowItem(this._TVConnection, false);
            this._hdmiSwitch.Selected = false;
            this.SetHDMIState(false);
        }

        private void GlowItem(Control control, bool flag)
        {
            if (flag)
            {
                OuterGlowBitmapEffect effect = new OuterGlowBitmapEffect {
                    GlowColor = Colors.CornflowerBlue
                };
                control.BitmapEffect = effect;
                effect.GlowSize = 3.0;
            }
            else
            {
                control.BitmapEffect = null;
            }
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (!this._contentLoaded)
            {
                this._contentLoaded = true;
                Uri resourceLocator = new Uri("/SmartAudio;component/hdmisettingspage.xaml", UriKind.Relative);
                Application.LoadComponent(this, resourceLocator);
            }
        }

        public bool Localize()
        {
            this._HDMITootlTipOff = Resources.SA_HDMIISOff;
            this._HDMITootlTipOn = Resources.SA_HDMIISOn;
            this._hdmiSwitch.ToolTip = this._hdmiSwitch.Selected ? this._HDMITootlTipOn : this._HDMITootlTipOff;
            return true;
        }

        public void ResetToDefault(bool defaultSetting)
        {
            this._hdmiSwitch.Selected = defaultSetting;
            this._hdmiSwitch_OnItemStateChanged(this._hdmiSwitch, defaultSetting);
        }

        private void SetHDMIState(bool newState)
        {
            try
            {
                this._auxEndPoint.HDMISwitch = newState;
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("HDMISettingsPage::_hdmiSwitch_OnItemStateChanged()", Severity.FATALERROR, exception);
            }
        }

        private void ShowHDMIState(bool newState)
        {
            if (newState)
            {
                this._TVSelected.Selected = true;
                this._PCEndPoint.Selected = false;
                this._HDMIEndPoint.Selected = true;
                this._PCSelected.Selected = false;
                this.GlowItem(this._TVConnection, true);
                this.GlowItem(this._HDMIEndPoint, true);
            }
            else
            {
                this._PCEndPoint.Selected = true;
                this._TVSelected.Selected = false;
                this._HDMIEndPoint.Selected = false;
                this._PCSelected.Selected = true;
                this.GlowItem(this._HDMIEndPoint, false);
                this.GlowItem(this._TVConnection, false);
            }
            this._hdmiSwitch.ToolTip = newState ? this._HDMITootlTipOn : this._HDMITootlTipOff;
        }

        [DebuggerNonUserCode, EditorBrowsable(EditorBrowsableState.Never)]
        void IComponentConnector.Connect(int connectionId, object target)
        {
            switch (connectionId)
            {
                case 1:
                    this._PCEndPoint = (ImageCheckBox) target;
                    return;

                case 2:
                    this._TVConnection = (ImageCheckBox) target;
                    return;

                case 3:
                    this._HDMIEndPoint = (ImageCheckBox) target;
                    return;

                case 4:
                    this._hdmiSwitch = (ImageCheckBox) target;
                    return;

                case 5:
                    this._PCSelected = (ImageCheckBox) target;
                    return;

                case 6:
                    this._TVSelected = (ImageCheckBox) target;
                    return;
            }
            this._contentLoaded = true;
        }

        public CxHDAudioFactory AudioFactory
        {
            get => 
                this._audioFactory;
            set
            {
                this._audioFactory = value;
                this._auxEndPoint = value.AuxiliaryEndPoints;
                bool newState = false;
                try
                {
                    newState = this._hdmiSwitch.Selected = this._auxEndPoint.HDMISwitch;
                }
                catch (Exception exception)
                {
                    SmartAudioLog.Log("HDMISettingsPage::AudioFactory()", Severity.FATALERROR, exception);
                }
                this.ShowHDMIState(newState);
            }
        }

        public string FriendlyName =>
            "HDMI Settings Page";
    }
}

