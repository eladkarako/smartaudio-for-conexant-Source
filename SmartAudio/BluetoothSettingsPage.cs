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

    public class BluetoothSettingsPage : UserControl, ISmartAudioPage, IComponentConnector
    {
        private CxHDAudioFactory _audioFactory;
        private CxHDAudioAuxEndPoints _auxEndPoint;
        internal ImageCheckBox _BlueToothEndPoint;
        internal ImageCheckBox _blueToothSwitch;
        private string _BlueToothTootlTipOff;
        private string _BlueToothTootlTipOn;
        internal ImageCheckBox _btArrow;
        private bool _contentLoaded;
        internal ImageCheckBox _pcArrow;
        internal ImageCheckBox _PCEndPoint;

        public BluetoothSettingsPage()
        {
            this.InitializeComponent();
            this.Localize();
            this._blueToothSwitch.OnItemStateChanged += new ItemStateChanged(this._bluetoothRedirect_OnItemStateChanged);
            this._PCEndPoint.MouseDoubleClick += new MouseButtonEventHandler(this._PCEndPoint_MouseDoubleClick);
            this._BlueToothEndPoint.MouseDoubleClick += new MouseButtonEventHandler(this._BTEndPoint_MouseDoubleClick);
            this._blueToothSwitch.ToolTip = this._blueToothSwitch.Selected ? this._BlueToothTootlTipOn : this._BlueToothTootlTipOff;
        }

        private void _bluetoothRedirect_OnItemStateChanged(ImageCheckBox item, bool newState)
        {
            if (item.Selected)
            {
                this._PCEndPoint.Selected = false;
                this._BlueToothEndPoint.Selected = true;
                this._pcArrow.Selected = false;
                this._btArrow.Selected = true;
                this.GlowItem(this._BlueToothEndPoint, true);
                this.GlowItem(this._btArrow, true);
                this.GlowItem(this._pcArrow, false);
            }
            else
            {
                this._pcArrow.Selected = true;
                this._btArrow.Selected = false;
                this._PCEndPoint.Selected = true;
                this._BlueToothEndPoint.Selected = false;
                this.GlowItem(this._BlueToothEndPoint, false);
                this.GlowItem(this._btArrow, false);
                this.GlowItem(this._pcArrow, true);
            }
            this._blueToothSwitch.ToolTip = this._blueToothSwitch.Selected ? this._BlueToothTootlTipOn : this._BlueToothTootlTipOff;
            this.SetBTState(item.Selected);
        }

        private void _BTEndPoint_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this._blueToothSwitch.Selected = true;
            this._BlueToothEndPoint.Selected = true;
            this.GlowItem(this._BlueToothEndPoint, true);
            this.GlowItem(this._btArrow, true);
            this.GlowItem(this._pcArrow, false);
            this._blueToothSwitch.Selected = true;
            this._pcArrow.Selected = false;
            this._btArrow.Selected = true;
            this.SetBTState(true);
        }

        [DebuggerNonUserCode]
        internal Delegate _CreateDelegate(Type delegateType, string handler) => 
            Delegate.CreateDelegate(delegateType, this, handler);

        private void _PCEndPoint_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this._PCEndPoint.Selected = true;
            this._BlueToothEndPoint.Selected = false;
            this.GlowItem(this._BlueToothEndPoint, false);
            this.GlowItem(this._btArrow, false);
            this.GlowItem(this._pcArrow, true);
            this._blueToothSwitch.Selected = false;
            this._pcArrow.Selected = true;
            this._btArrow.Selected = false;
            this.SetBTState(false);
        }

        private void GlowItem(Control control, bool flag)
        {
            if (flag)
            {
                OuterGlowBitmapEffect effect = new OuterGlowBitmapEffect {
                    GlowColor = Colors.Blue
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
                Uri resourceLocator = new Uri("/SmartAudio;component/bluetoothsettingspage.xaml", UriKind.Relative);
                Application.LoadComponent(this, resourceLocator);
            }
        }

        public bool Localize()
        {
            this._BlueToothTootlTipOff = Resources.SA_BlueToothsISOff;
            this._BlueToothTootlTipOn = Resources.SA_BlueToothsISOn;
            this._blueToothSwitch.ToolTip = this._blueToothSwitch.Selected ? this._BlueToothTootlTipOn : this._BlueToothTootlTipOff;
            return true;
        }

        public void ResetToDefault(bool defaultSetting)
        {
            this._blueToothSwitch.Selected = defaultSetting;
            this._bluetoothRedirect_OnItemStateChanged(this._blueToothSwitch, defaultSetting);
        }

        private void SetBTState(bool newState)
        {
            try
            {
                this._auxEndPoint.BlueToothRedirect = newState;
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("BluetoothSettingsPage::SetBTState()", Severity.FATALERROR, exception);
            }
        }

        private void ShowBTState(bool newState)
        {
            if (newState)
            {
                this._PCEndPoint.Selected = false;
                this._BlueToothEndPoint.Selected = true;
                this._pcArrow.Selected = false;
                this._btArrow.Selected = true;
                this.GlowItem(this._BlueToothEndPoint, true);
                this.GlowItem(this._btArrow, true);
                this.GlowItem(this._pcArrow, false);
            }
            else
            {
                this._pcArrow.Selected = true;
                this._btArrow.Selected = false;
                this._PCEndPoint.Selected = true;
                this._BlueToothEndPoint.Selected = false;
                this.GlowItem(this._BlueToothEndPoint, false);
                this.GlowItem(this._btArrow, false);
                this.GlowItem(this._pcArrow, true);
            }
            this._blueToothSwitch.ToolTip = newState ? this._BlueToothTootlTipOn : this._BlueToothTootlTipOff;
        }

        [EditorBrowsable(EditorBrowsableState.Never), DebuggerNonUserCode]
        void IComponentConnector.Connect(int connectionId, object target)
        {
            switch (connectionId)
            {
                case 1:
                    this._PCEndPoint = (ImageCheckBox) target;
                    return;

                case 2:
                    this._BlueToothEndPoint = (ImageCheckBox) target;
                    return;

                case 3:
                    this._blueToothSwitch = (ImageCheckBox) target;
                    return;

                case 4:
                    this._pcArrow = (ImageCheckBox) target;
                    return;

                case 5:
                    this._btArrow = (ImageCheckBox) target;
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
                    newState = this._blueToothSwitch.Selected = this._auxEndPoint.BlueToothRedirect;
                }
                catch (Exception exception)
                {
                    SmartAudioLog.Log("BluetoothSettingsPage::AudioFactory()", Severity.FATALERROR, exception);
                }
                this.ShowBTState(newState);
            }
        }

        public string FriendlyName =>
            "Bluetooth Settings Page";
    }
}

