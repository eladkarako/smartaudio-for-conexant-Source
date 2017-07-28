namespace SmartAudio
{
    using CxHDAudioAPILib;
    using SmartAudio.Properties;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Input;
    using System.Windows.Markup;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    public class JackConfigControl : UserControl, IComponentConnector
    {
        private bool _contentLoaded;
        private bool _isDelayed;
        private bool _isJackPlugged;
        internal Image _jack;
        private CxHDAudioJackConfig _jackConfig;
        private int _jackIndex;
        internal Button _jackTypeImage;
        internal Image _jackTypeImage_int;
        internal ComboBox _jackTypeSelection;
        private List<CxIOJackType> _supportedTypes;
        private int _supportedTypesCnt;
        internal Label _text;
        private BackgroundWorker _workerThread;

        public event JackChangedHandler JackPortChanged;

        public event JackSelectionChangedHandler JackSelectionChanged;

        public JackConfigControl()
        {
            this.InitializeComponent();
            this._supportedTypes = new List<CxIOJackType>();
            this._workerThread = new BackgroundWorker();
            this._workerThread.DoWork += new DoWorkEventHandler(this._workerThread_DoWork);
            this._workerThread.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this._workerThread_RunWorkerCompleted);
            this._jackTypeSelection.DataContext = this;
            this._jackIndex = 0;
        }

        private void _jackTypeImage_Click(object sender, RoutedEventArgs e)
        {
            this._jackTypeSelection.Visibility = Visibility.Visible;
        }

        private void _jackTypeSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool flag = true;
            try
            {
                if (Mouse.OverrideCursor == null)
                {
                    Mouse.OverrideCursor = Cursors.Wait;
                    flag = false;
                }
                if ((this._jackTypeSelection.SelectedIndex >= 0) && (this._jackTypeSelection.SelectedIndex < this._jackTypeSelection.Items.Count))
                {
                    for (int i = 0; i < this._jackTypeSelection.Items.Count; i++)
                    {
                        ((ListBoxItem) this._jackTypeSelection.Items[i]).IsSelected = this._jackTypeSelection.SelectedIndex == i;
                    }
                }
                if (this.JackSelectionChanged != null)
                {
                    this.JackSelectionChanged(this._jackTypeSelection.SelectedIndex);
                }
                if ((this.JackPortChanged != null) && !this._isDelayed)
                {
                    this.JackPortChanged(this._jackTypeSelection.SelectedIndex);
                }
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("JackConfigControl::_jackTypeSelection_SelectionChanged", Severity.WARNING, exception);
            }
            finally
            {
                if (!flag)
                {
                    Mouse.OverrideCursor = null;
                }
            }
        }

        private void _workerThread_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                this.UpdateJackType((CxIOJackType) e.Argument);
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("JackConfigControl::_workerThread_DoWork", Severity.WARNING, exception);
            }
        }

        private void _workerThread_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            string label = "";
            string image = "";
            if (this.GetImageAndLabel(this._jackConfig.JackType, ref image, ref label))
            {
                this._jackTypeImage_int.Source = new BitmapImage(new Uri(image));
                this._text.Content = label;
                this._jackTypeImage.Visibility = Visibility.Hidden;
            }
            App current = Application.Current as App;
            if (current.OnJackChanged != null)
            {
                current.OnJackChanged(this._jackIndex);
            }
        }

        public void ApplyChange()
        {
            if (this._jackTypeSelection.SelectedIndex != this.SelectedJack)
            {
                this.SelectedJack = this._jackTypeSelection.SelectedIndex;
            }
        }

        public void DoWork(object argument)
        {
            this.UpdateJackType((CxIOJackType) argument);
        }

        public bool GetImageAndLabel(CxIOJackType jackType, ref string image, ref string label)
        {
            switch (jackType)
            {
                case CxIOJackType.HeadPhoneJack:
                    image = "pack://application:,,,/Resources/Images/AudioJacks/Jack-Retasking-Menu-Headphone.png";
                    label = Resources.SA_CHeadPhones;
                    return true;

                case CxIOJackType.MicophoneJack:
                    image = "pack://application:,,,/Resources/Images/AudioJacks/Jack-Retasking-Menu-Mic-V2.png";
                    label = Resources.SA_Microphone;
                    return true;

                case CxIOJackType.FrontSpeakers:
                    image = "pack://application:,,,/Resources/Images/AudioJacks/Jack-Retasking-Menu-Front-SPK.png";
                    label = Resources.SA_FrontSpeakers;
                    return true;

                case CxIOJackType.CenterSpeakers:
                    image = "pack://application:,,,/Resources/Images/AudioJacks/Jack-Retasking-Menu-CNTR-SPK.png";
                    label = Resources.SA_CenterSpeakers;
                    return true;

                case CxIOJackType.RearSpeakers:
                    image = "pack://application:,,,/Resources/Images/AudioJacks/Jack-Retasking-Menu-Rear-SPK.png";
                    label = Resources.SA_RearSpeakers;
                    return true;

                case CxIOJackType.LineIN:
                    image = "pack://application:,,,/Resources/Images/AudioJacks/Jack-Retasking-Menu-line-IN.png";
                    label = Resources.SA_LineIn;
                    return true;

                case CxIOJackType.LineOut:
                    image = "pack://application:,,,/Resources/Images/AudioJacks/Jack-Retasking-Menu-line-Out.png";
                    label = Resources.SA_LineOut;
                    return true;
            }
            return false;
        }

        public int GetJackPlugged()
        {
            int num = -1;
            try
            {
                if (this._jackConfig.IsJackPlugged())
                {
                    return 1;
                }
                num = 0;
            }
            catch
            {
                SmartAudioLog.Log("JackConfigControl::IsJackConfigurationChanged, IsJackPlugged is not supported by the driver.");
            }
            return num;
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (!this._contentLoaded)
            {
                this._contentLoaded = true;
                Uri resourceLocator = new Uri("/SmartAudio;component/jackconfigcontrol.xaml", UriKind.Relative);
                Application.LoadComponent(this, resourceLocator);
            }
        }

        public bool IsJackConfigurationChanged()
        {
            bool flag = false;
            try
            {
                flag = this._isJackPlugged != this._jackConfig.IsJackPlugged();
            }
            catch
            {
                SmartAudioLog.Log("JackConfigControl::IsJackConfigurationChanged, IsJackPlugged is not supported by the driver.");
            }
            return flag;
        }

        public bool IsSelectedJackTypeApplied() => 
            (this.SelectedJack == this._jackTypeSelection.SelectedIndex);

        public void RefreshJack()
        {
            try
            {
                try
                {
                    if (this._jackConfig == null)
                    {
                        return;
                    }
                    CxIOJackType jackType = this._jackConfig.JackType;
                    string image = "";
                    string label = "";
                    if (this.GetImageAndLabel(jackType, ref image, ref label))
                    {
                        this._jackTypeImage_int.Source = new BitmapImage(new Uri(image));
                        this._text.Content = label;
                        this._jackTypeImage.Visibility = Visibility.Hidden;
                    }
                }
                catch (Exception exception)
                {
                    SmartAudioLog.Log("JackConfigControl::_jackTypeSelection_SelectionChanged", Severity.WARNING, exception);
                }
                try
                {
                    switch (this._jackConfig.JackColor)
                    {
                        case CxIOJackColors.Black_PORT:
                            this._jack.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/blackjack.png"));
                            break;

                        case CxIOJackColors.GREY_PORT:
                            this._jack.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/greyjack.png"));
                            break;

                        case CxIOJackColors.BLUE_PORT:
                            this._jack.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/bluejack.png"));
                            break;

                        case CxIOJackColors.GREEN_PORT:
                            this._jack.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/greenjack.png"));
                            break;

                        case CxIOJackColors.ORANGE_PORT:
                            this._jack.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/orangejack.png"));
                            break;

                        case CxIOJackColors.PINK_PORT:
                            this._jack.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/pinkjack.png"));
                            break;

                        case CxIOJackColors.WHITE_PORT:
                            this._jack.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/whitejack.png"));
                            break;

                        default:
                            this._jack.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/blackjack.png"));
                            break;
                    }
                    this._jack.Width = this._jack.Source.Width;
                    this._jack.Height = this._jack.Source.Height;
                }
                catch (Exception exception2)
                {
                    SmartAudioLog.Log("JackConfigControl::_jackTypeSelection_SelectionChanged", Severity.WARNING, exception2);
                    this._jack.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/greenjack.png"));
                }
            }
            catch (Exception exception3)
            {
                SmartAudioLog.Log("JackConfigControl::InitializeJack() Failed to get jack color ", Severity.FATALERROR, exception3);
            }
        }

        public void RefreshTypes()
        {
            for (int i = 0; i < this._supportedTypes.Count; i++)
            {
                StackPanel content = (StackPanel) ((ListBoxItem) this._jackTypeSelection.Items[i]).Content;
                string label = "";
                string image = "";
                if (this.GetImageAndLabel(this._supportedTypes[i], ref image, ref label))
                {
                    ((Label) content.Children[1]).Content = " " + label;
                    ((Image) content.Children[0]).Source = new BitmapImage(new Uri(image));
                    ((ListBoxItem) this._jackTypeSelection.Items[i]).Tag = this._supportedTypes[i];
                }
            }
        }

        public void ResetJack()
        {
            this.RefreshJack();
            this._jackTypeSelection.SelectionChanged -= new SelectionChangedEventHandler(this._jackTypeSelection_SelectionChanged);
            for (int i = 0; i < this._jackTypeSelection.Items.Count; i++)
            {
                if (((Label) ((StackPanel) ((ListBoxItem) this._jackTypeSelection.Items[i]).Content).Children[1]).Content.ToString() == (" " + this._text.Content.ToString()))
                {
                    this._jackTypeSelection.SelectedIndex = i;
                    ((ListBoxItem) this._jackTypeSelection.Items[i]).IsSelected = true;
                }
                else
                {
                    ((ListBoxItem) this._jackTypeSelection.Items[i]).IsSelected = false;
                }
            }
            this._jackTypeSelection.GetBindingExpression(Selector.SelectedIndexProperty).UpdateTarget();
            this._jackTypeSelection.SelectionChanged += new SelectionChangedEventHandler(this._jackTypeSelection_SelectionChanged);
        }

        public void SaveJackConfiguration()
        {
            try
            {
                this._isJackPlugged = this._jackConfig.IsJackPlugged();
            }
            catch
            {
                SmartAudioLog.Log("JackConfigControl::SaveJackConfiguration, IsJackPlugged is not supported by the driver.");
            }
        }

        public void Select(bool bSelect)
        {
            if (bSelect)
            {
                this._jackTypeSelection.Visibility = Visibility.Visible;
                this._jackTypeSelection.IsDropDownOpen = true;
            }
            else
            {
                this._jackTypeSelection.IsDropDownOpen = false;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), DebuggerNonUserCode]
        void IComponentConnector.Connect(int connectionId, object target)
        {
            switch (connectionId)
            {
                case 1:
                    this._jackTypeImage = (Button) target;
                    this._jackTypeImage.Click += new RoutedEventHandler(this._jackTypeImage_Click);
                    return;

                case 2:
                    this._jackTypeImage_int = (Image) target;
                    return;

                case 3:
                    this._text = (Label) target;
                    return;

                case 4:
                    this._jackTypeSelection = (ComboBox) target;
                    return;

                case 5:
                    this._jack = (Image) target;
                    return;
            }
            this._contentLoaded = true;
        }

        public void UpdateJack()
        {
            this.RefreshJack();
            this._jackTypeSelection.SelectionChanged -= new SelectionChangedEventHandler(this._jackTypeSelection_SelectionChanged);
            for (int i = 0; i < this._jackTypeSelection.Items.Count; i++)
            {
                if (((Label) ((StackPanel) ((ListBoxItem) this._jackTypeSelection.Items[i]).Content).Children[1]).Content.ToString() == (" " + this._text.Content.ToString()))
                {
                    this._jackTypeSelection.SelectedIndex = i;
                    ((ListBoxItem) this._jackTypeSelection.Items[i]).IsSelected = true;
                }
                else
                {
                    ((ListBoxItem) this._jackTypeSelection.Items[i]).IsSelected = false;
                }
            }
            this._jackTypeSelection.SelectionChanged += new SelectionChangedEventHandler(this._jackTypeSelection_SelectionChanged);
        }

        public void UpdateJackType(CxIOJackType jackType)
        {
            App current = Application.Current as App;
            Monitor.Enter(current);
            if (this._jackConfig.JackType != jackType)
            {
                bool flag = true;
                if (((current.AudioFactory.HostOperatingSystem == CxHostOperatingSystemType.WindowsXP) || (current.AudioFactory.HostOperatingSystem == CxHostOperatingSystemType.WindowsXP64)) || ((current.AudioFactory.HostOperatingSystem == CxHostOperatingSystemType.Windows2000) || (current.AudioFactory.HostOperatingSystem == CxHostOperatingSystemType.UnknownOS)))
                {
                    flag = false;
                }
                else if (current.AudioFactory.AudioDirector.AudioDirectorMode == CxHDAudioAudioDirectorMode.MultiStream)
                {
                    flag = false;
                }
                else if (!current.Settings.INISettings.IsSetDefEndPointAfterJackRetaskEnabled)
                {
                    flag = false;
                }
                this._jackConfig.set_SetJackType(jackType, flag);
            }
            current.AudioFactory.RefreshHeadphoneStatus();
            Monitor.Exit(current);
        }

        public bool IsDelayed
        {
            get => 
                this._isDelayed;
            set
            {
                this._isDelayed = value;
            }
        }

        public CxHDAudioJackConfig JackConfig
        {
            get => 
                this._jackConfig;
            set
            {
                try
                {
                    this._jackConfig = value;
                    if (this._jackConfig != null)
                    {
                        if (this._jackConfig.get_IsDeviceTypeSupported(CxIOJackType.HeadPhoneJack))
                        {
                            this._supportedTypes.Add(CxIOJackType.HeadPhoneJack);
                        }
                        if (this._jackConfig.get_IsDeviceTypeSupported(CxIOJackType.MicophoneJack))
                        {
                            this._supportedTypes.Add(CxIOJackType.MicophoneJack);
                        }
                        if (this._jackConfig.get_IsDeviceTypeSupported(CxIOJackType.LineIN))
                        {
                            this._supportedTypes.Add(CxIOJackType.LineIN);
                        }
                        if (this._jackConfig.get_IsDeviceTypeSupported(CxIOJackType.LineOut))
                        {
                            this._supportedTypes.Add(CxIOJackType.LineOut);
                        }
                        this._supportedTypesCnt = this._supportedTypes.Count;
                        for (int i = 0; i < this._supportedTypes.Count; i++)
                        {
                            StackPanel panel = new StackPanel {
                                Orientation = Orientation.Horizontal
                            };
                            ListBoxItem newItem = new ListBoxItem {
                                Content = panel
                            };
                            Image element = new Image();
                            element.Height = element.Width = 30.0;
                            element.Stretch = Stretch.Fill;
                            panel.Children.Add(element);
                            Label label = new Label {
                                Foreground = Brushes.White
                            };
                            panel.Children.Add(label);
                            this._jackTypeSelection.Items.Add(newItem);
                            StackPanel content = (StackPanel) ((ListBoxItem) this._jackTypeSelection.Items[i]).Content;
                            string str = "";
                            string image = "";
                            if (this.GetImageAndLabel(this._supportedTypes[i], ref image, ref str))
                            {
                                ((Label) content.Children[1]).Content = " " + str;
                                ((Image) content.Children[0]).Source = new BitmapImage(new Uri(image));
                                ((ListBoxItem) this._jackTypeSelection.Items[i]).Tag = this._supportedTypes[i];
                            }
                        }
                        this._jackTypeSelection.SelectionChanged -= new SelectionChangedEventHandler(this._jackTypeSelection_SelectionChanged);
                        this.RefreshJack();
                        this._jackTypeSelection.GetBindingExpression(Selector.SelectedIndexProperty).UpdateTarget();
                        this._jackTypeSelection.SelectionChanged += new SelectionChangedEventHandler(this._jackTypeSelection_SelectionChanged);
                    }
                }
                catch (Exception exception)
                {
                    SmartAudioLog.Log("JackConfigControl::JackConfig {set}", Severity.WARNING, exception);
                }
            }
        }

        public int JackIndex
        {
            get => 
                this._jackIndex;
            set
            {
                this._jackIndex = value;
            }
        }

        public CxIOJackLocation JackLocation =>
            this._jackConfig?.JackLocation;

        public int SelectedJack
        {
            get
            {
                if (this._jackConfig != null)
                {
                    CxIOJackType jackType = this._jackConfig.JackType;
                    for (int i = 0; i < this._supportedTypes.Count; i++)
                    {
                        if (((CxIOJackType) this._supportedTypes[i]) == jackType)
                        {
                            return i;
                        }
                    }
                }
                return -1;
            }
            set
            {
                try
                {
                    if (((this._jackConfig != null) && !this._isDelayed) && ((((value >= 0) && (value < this._supportedTypes.Count)) && ((this._jackTypeSelection.Visibility == Visibility.Visible) && (this._jackTypeSelection.SelectedIndex != -1))) && (this._jackConfig.JackType != ((CxIOJackType) this._supportedTypes[value]))))
                    {
                        Mouse.OverrideCursor = Cursors.Wait;
                        this._workerThread.RunWorkerAsync(this._supportedTypes[value]);
                    }
                }
                catch (Exception exception)
                {
                    SmartAudioLog.Log("JackConfigControl::SelectedJack {set}", Severity.WARNING, exception);
                }
            }
        }

        public int SelectedJackIndex
        {
            set
            {
                if ((value >= 0) && (value < this._jackTypeSelection.Items.Count))
                {
                    this._jackTypeSelection.SelectedIndex = value;
                    this._workerThread.RunWorkerAsync(this._supportedTypes[value]);
                }
            }
        }

        public int SupportedDeviceTypesCount =>
            this._supportedTypesCnt;
    }
}

