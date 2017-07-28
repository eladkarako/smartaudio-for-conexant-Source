namespace SmartAudio
{
    using CxHDAudioAPILib;
    using SmartAudio.Properties;
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Markup;
    using System.Windows.Media;

    public class IORetaskingUserControl : UserControl, ISmartAudioPage, IComponentConnector
    {
        internal Border _background;
        private bool _contentLoaded;
        private CxHDAudioConfig _cxHDAudioconfig;
        internal StackPanel _ioRetaskingPanel;
        private bool _isUnSupportedLocationExisted;
        private JackConfigControl _jack1;
        private JackConfigControl _jack2;
        private JackConfigControl _jack3;
        private GroupBox[] _jackGroupBox;
        private bool[][] _locations;
        private const int JackHeight = 0x25;
        private const int JackWidth = 210;
        private const int MaxNumOfJackLocations = 9;
        private const int MaxNumOfRetaskableJacks = 3;

        public event JackPortChangedHandler JackPortChanged;

        public event JackPortUpdateCompletedHandler JackPortUpdateCompleted;

        public event JackPortSelectionChangedHandler JackSelectionChanged;

        public IORetaskingUserControl()
        {
            this.InitializeComponent();
            this._jack1 = new JackConfigControl();
            this._jack1.Margin = new Thickness(5.0, 2.0, 5.0, 5.0);
            this._jack1.Width = 210.0;
            this._jack1.Height = 37.0;
            this._jack2 = new JackConfigControl();
            this._jack2.Margin = new Thickness(5.0, 2.0, 5.0, 5.0);
            this._jack2.Width = 210.0;
            this._jack2.Height = 37.0;
            this._jack3 = new JackConfigControl();
            this._jack3.Margin = new Thickness(5.0, 2.0, 5.0, 5.0);
            this._jack3.Width = 210.0;
            this._jack3.Height = 37.0;
            this._locations = new bool[3][];
            for (int i = 0; i < 3; i++)
            {
                this._locations[i] = new bool[9];
                for (int j = 0; j < 9; j++)
                {
                    this._locations[i][j] = false;
                }
            }
            this._jackGroupBox = new GroupBox[9];
        }

        private void _jack1_JackPortChanged(int selectedJack)
        {
            if (this.JackPortChanged != null)
            {
                this.JackPortChanged(0, selectedJack);
            }
        }

        private void _jack1_JackSelectionChanged(int selectedJack)
        {
            if (this.JackSelectionChanged != null)
            {
                this.JackSelectionChanged(0, selectedJack);
            }
        }

        private void _jack2_JackPortChanged(int selectedJack)
        {
            if (this.JackPortChanged != null)
            {
                this.JackPortChanged(1, selectedJack);
            }
        }

        private void _jack2_JackSelectionChanged(int selectedJack)
        {
            if (this.JackSelectionChanged != null)
            {
                this.JackSelectionChanged(1, selectedJack);
            }
        }

        private void _jack3_JackPortChanged(int selectedJack)
        {
            if (this.JackPortChanged != null)
            {
                this.JackPortChanged(2, selectedJack);
            }
        }

        private void _jack3_JackSelectionChanged(int selectedJack)
        {
            if (this.JackSelectionChanged != null)
            {
                this.JackSelectionChanged(2, selectedJack);
            }
        }

        public void ApplyChange()
        {
            if ((this._jack1 != null) && (this._jack1.Visibility == Visibility.Visible))
            {
                this._jack1.ApplyChange();
            }
            if ((this._jack2 != null) && (this._jack2.Visibility == Visibility.Visible))
            {
                this._jack2.ApplyChange();
            }
            if ((this._jack3 != null) && (this._jack3.Visibility == Visibility.Visible))
            {
                this._jack3.ApplyChange();
            }
        }

        public bool AreJackSelectionsApplied() => 
            ((this._jack1.IsSelectedJackTypeApplied() && this._jack2.IsSelectedJackTypeApplied()) && this._jack3.IsSelectedJackTypeApplied());

        private void CheckLocation(int jackIndex)
        {
            if ((jackIndex >= 1) && (jackIndex <= 3))
            {
                for (int i = 0; i < 9; i++)
                {
                    this._locations[jackIndex - 1][i] = false;
                }
                switch (jackIndex)
                {
                    case 1:
                        this._locations[jackIndex - 1][(int) this._jack1.JackLocation] = true;
                        return;

                    case 2:
                        this._locations[jackIndex - 1][(int) this._jack2.JackLocation] = true;
                        return;

                    case 3:
                        this._locations[jackIndex - 1][(int) this._jack3.JackLocation] = true;
                        return;
                }
            }
        }

        private void CreateGroupBoxes()
        {
            try
            {
                if (this._jack1.SupportedDeviceTypesCount >= 2)
                {
                    this.CheckLocation(1);
                }
                if (this._jack2.SupportedDeviceTypesCount >= 2)
                {
                    this.CheckLocation(2);
                }
                if (this._jack3.SupportedDeviceTypesCount >= 2)
                {
                    this.CheckLocation(3);
                }
                for (int i = 0; i < 9; i++)
                {
                    this._jackGroupBox[i] = null;
                }
                this._isUnSupportedLocationExisted = false;
                for (int j = 0; j < 9; j++)
                {
                    GroupBox element = null;
                    StackPanel panel = null;
                    for (int k = 0; k < 3; k++)
                    {
                        if (this._locations[k][j])
                        {
                            if (j == 8)
                            {
                                if (panel == null)
                                {
                                    panel = new StackPanel {
                                        Orientation = Orientation.Horizontal,
                                        HorizontalAlignment = HorizontalAlignment.Center
                                    };
                                }
                            }
                            else if (element == null)
                            {
                                element = new GroupBox {
                                    Margin = new Thickness(10.0, 5.0, 10.0, 5.0)
                                };
                                object obj2 = base.FindResource("GroupBoxStyle");
                                if (obj2 != null)
                                {
                                    element.Style = (Style) obj2;
                                }
                                TextBlock block = new TextBlock {
                                    Foreground = Brushes.White,
                                    Text = this.GetHeaderText((CxIOJackLocation) j)
                                };
                                HeaderedContentControl control = new HeaderedContentControl {
                                    Content = block
                                };
                                element.Header = control;
                                panel = new StackPanel {
                                    Orientation = Orientation.Horizontal,
                                    HorizontalAlignment = HorizontalAlignment.Center
                                };
                                element.Content = panel;
                            }
                            if (panel != null)
                            {
                                switch (k)
                                {
                                    case 0:
                                    {
                                        this._jack1.JackPortChanged += new JackChangedHandler(this._jack1_JackPortChanged);
                                        this._jack1.JackSelectionChanged += new JackSelectionChangedHandler(this._jack1_JackSelectionChanged);
                                        panel.Children.Add(this._jack1);
                                        continue;
                                    }
                                    case 1:
                                    {
                                        this._jack2.JackPortChanged += new JackChangedHandler(this._jack2_JackPortChanged);
                                        this._jack2.JackSelectionChanged += new JackSelectionChangedHandler(this._jack2_JackSelectionChanged);
                                        panel.Children.Add(this._jack2);
                                        continue;
                                    }
                                    case 2:
                                    {
                                        this._jack3.JackPortChanged += new JackChangedHandler(this._jack3_JackPortChanged);
                                        this._jack3.JackSelectionChanged += new JackSelectionChangedHandler(this._jack3_JackSelectionChanged);
                                        panel.Children.Add(this._jack3);
                                        continue;
                                    }
                                }
                                throw new Exception("The number of retaskable jacks is greater than the maximum number of retaskable jacks " + 3);
                            }
                        }
                    }
                    if (element != null)
                    {
                        this._ioRetaskingPanel.Children.Add(element);
                        this._jackGroupBox[j] = element;
                    }
                    else if (panel != null)
                    {
                        this._ioRetaskingPanel.Children.Add(panel);
                        this._isUnSupportedLocationExisted = true;
                    }
                }
                this.LocalizeGroupBox();
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("IORetaskingUserControl::CreateGroupBoxes", Severity.FATALERROR, exception);
            }
        }

        private string GetHeaderText(CxIOJackLocation location)
        {
            switch (location)
            {
                case CxIOJackLocation.UNKNOWN_LOC:
                    return Resources.SA_NA;

                case CxIOJackLocation.REAR_LOC:
                    return Resources.SA_EAREAR;

                case CxIOJackLocation.FRONT_LOC:
                    return Resources.SA_EAFRONT;

                case CxIOJackLocation.LEFT_LOC:
                    return Resources.SA_LeftChannel;

                case CxIOJackLocation.RIGHT_LOC:
                    return Resources.SA_RightChannel;

                case CxIOJackLocation.TOP_LOC:
                    return Resources.SA_Top;

                case CxIOJackLocation.BOTTOM_LOC:
                    return Resources.SA_Bottom;

                case CxIOJackLocation.SPECIAL_LOC:
                    return Resources.SA_Special;
            }
            return Resources.SA_NA;
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (!this._contentLoaded)
            {
                this._contentLoaded = true;
                Uri resourceLocator = new Uri("/SmartAudio;component/ioretaskingusercontrol.xaml", UriKind.Relative);
                Application.LoadComponent(this, resourceLocator);
            }
        }

        public bool IsJackConfigurationChanged()
        {
            bool flag = false;
            if ((this._jack1 != null) && (this._jack1.Visibility == Visibility.Visible))
            {
                flag |= this._jack1.IsJackConfigurationChanged();
            }
            if ((this._jack2 != null) && (this._jack2.Visibility == Visibility.Visible))
            {
                flag |= this._jack2.IsJackConfigurationChanged();
            }
            if ((this._jack3 != null) && (this._jack3.Visibility == Visibility.Visible))
            {
                flag |= this._jack3.IsJackConfigurationChanged();
            }
            return flag;
        }

        public bool Localize()
        {
            if (this._jack1 != null)
            {
                this._jack1.RefreshTypes();
                this._jack1.RefreshJack();
            }
            if (this._jack2 != null)
            {
                this._jack2.RefreshTypes();
                this._jack2.RefreshJack();
            }
            if (this._jack3 != null)
            {
                this._jack3.RefreshTypes();
                this._jack3.RefreshJack();
            }
            this.LocalizeGroupBox();
            return true;
        }

        private void LocalizeGroupBox()
        {
            for (int i = 0; i < 9; i++)
            {
                if (this._jackGroupBox[i] != null)
                {
                    TextBlock block = new TextBlock {
                        Foreground = Brushes.White,
                        Text = this.GetHeaderText((CxIOJackLocation) i)
                    };
                    HeaderedContentControl control = new HeaderedContentControl {
                        Content = block
                    };
                    this._jackGroupBox[i].Header = control;
                }
            }
        }

        public void OnJackAssignmentChanged(int jack)
        {
            switch (jack)
            {
                case 0:
                    this._jack1.UpdateJack();
                    break;

                case 1:
                    this._jack2.UpdateJack();
                    break;

                case 2:
                    this._jack3.UpdateJack();
                    break;
            }
            if (this.JackPortUpdateCompleted != null)
            {
                this.JackPortUpdateCompleted(jack);
            }
        }

        public void ResetJacks()
        {
            if (this._jack1 != null)
            {
                this._jack1.ResetJack();
            }
            if (this._jack2 != null)
            {
                this._jack2.ResetJack();
            }
            if (this._jack3 != null)
            {
                this._jack3.ResetJack();
            }
        }

        public void SaveJackConfiguration()
        {
            if ((this._jack1 != null) && (this._jack1.Visibility == Visibility.Visible))
            {
                this._jack1.SaveJackConfiguration();
            }
            if ((this._jack2 != null) && (this._jack2.Visibility == Visibility.Visible))
            {
                this._jack2.SaveJackConfiguration();
            }
            if ((this._jack3 != null) && (this._jack3.Visibility == Visibility.Visible))
            {
                this._jack3.SaveJackConfiguration();
            }
        }

        public void SelectPort(int port, bool select)
        {
            switch (port)
            {
                case 0:
                    this._jack1.Select(select);
                    return;

                case 1:
                    this._jack2.Select(select);
                    return;

                case 2:
                    this._jack3.Select(select);
                    return;
            }
        }

        public void SetIsJackDelayed(bool isDelayed)
        {
            if (this._jack1 != null)
            {
                this._jack1.IsDelayed = isDelayed;
            }
            if (this._jack2 != null)
            {
                this._jack2.IsDelayed = isDelayed;
            }
            if (this._jack3 != null)
            {
                this._jack3.IsDelayed = isDelayed;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), DebuggerNonUserCode]
        void IComponentConnector.Connect(int connectionId, object target)
        {
            switch (connectionId)
            {
                case 1:
                    this._background = (Border) target;
                    return;

                case 2:
                    this._ioRetaskingPanel = (StackPanel) target;
                    return;
            }
            this._contentLoaded = true;
        }

        public void UpdateJackConfiguration()
        {
            int num = 0;
            if ((this._jack1 != null) && (this._jack1.Visibility == Visibility.Visible))
            {
                if (this._jack1.GetJackPlugged() == 1)
                {
                    num++;
                }
                else if (this._jack1.GetJackPlugged() == -1)
                {
                    num = -1;
                }
            }
            if ((this._jack2 != null) && (this._jack2.Visibility == Visibility.Visible))
            {
                if (this._jack2.GetJackPlugged() == 1)
                {
                    num++;
                }
                else if (this._jack2.GetJackPlugged() == -1)
                {
                    num = -1;
                }
            }
            if ((this._jack3 != null) && (this._jack3.Visibility == Visibility.Visible))
            {
                if (this._jack3.GetJackPlugged() == 1)
                {
                    num++;
                }
                else if (this._jack3.GetJackPlugged() == -1)
                {
                    num = -1;
                }
            }
            if (num == 0)
            {
                App current = Application.Current as App;
                current.IsJackConfigurationChangedS3_S4 = false;
            }
        }

        public void UpdateJackPorts(int jack, int selectedJack)
        {
            switch (jack)
            {
                case 0:
                    this._jack1.SelectedJackIndex = selectedJack;
                    return;

                case 1:
                    this._jack2.SelectedJackIndex = selectedJack;
                    return;

                case 2:
                    this._jack3.SelectedJackIndex = selectedJack;
                    return;
            }
        }

        public CxHDAudioFactory AudioFactory
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string FriendlyName =>
            "Jack Retasking";

        public CxHDAudioConfig HDAudioConfig
        {
            get => 
                this._cxHDAudioconfig;
            set
            {
                try
                {
                    this._cxHDAudioconfig = value;
                    if (this._cxHDAudioconfig.JackEnumerator.Count >= 3)
                    {
                        if (this._jack1 != null)
                        {
                            this._jack1.JackConfig = this._cxHDAudioconfig.JackEnumerator[1] as CxHDAudioJackConfig;
                            this._jack1.JackIndex = 0;
                        }
                        if (this._jack2 != null)
                        {
                            this._jack2.JackConfig = this._cxHDAudioconfig.JackEnumerator[2] as CxHDAudioJackConfig;
                            this._jack2.JackIndex = 1;
                        }
                        if (this._jack3 != null)
                        {
                            this._jack3.JackConfig = this._cxHDAudioconfig.JackEnumerator[3] as CxHDAudioJackConfig;
                            this._jack3.JackIndex = 2;
                        }
                        this.CreateGroupBoxes();
                        this.Localize();
                        App current = Application.Current as App;
                        current.OnJackChanged = (SmartAudio.OnJackAssignmentChanged) Delegate.Combine(current.OnJackChanged, new SmartAudio.OnJackAssignmentChanged(this.OnJackAssignmentChanged));
                    }
                }
                catch (Exception exception)
                {
                    SmartAudioLog.Log("IORetaskingUserControl::HDAudioConfig {set}", Severity.WARNING, exception);
                }
            }
        }

        public bool IsUnSupportedLocationExisted =>
            this._isUnSupportedLocationExisted;

        public bool ShowBackground
        {
            get
            {
                if (this._background.Visibility != Visibility.Visible)
                {
                    return false;
                }
                return true;
            }
            set
            {
                this._background.Visibility = value ? Visibility.Visible : Visibility.Hidden;
            }
        }
    }
}

