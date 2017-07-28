namespace SmartAudio
{
    using CxHDAudioAPILib;
    using Microsoft.Win32;
    using SmartAudio.Core;
    using SmartAudio.Properties;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Forms;
    using System.Windows.Input;
    using System.Windows.Markup;
    using System.Windows.Media.Animation;
    using System.Windows.Threading;

    public class MainWindow : Window, ISmartAudioPage, IComponentConnector
    {
        internal Border _background;
        internal StackPanel _bigToolbar;
        internal ColumnDefinition _captionCol;
        internal System.Windows.Controls.Button _closeBtn;
        internal System.Windows.Controls.Button _closeBtnLarge;
        internal Grid _containerGrid;
        private bool _contentLoaded;
        internal ContentControl _controlFrame;
        private string _currentlyActivePage;
        private double _currentScaleResolution = 1.0;
        private System.Windows.Controls.MenuItem _disableJackMenu;
        internal System.Windows.Controls.Button _dolby;
        internal System.Windows.Controls.Button _helpButton;
        internal System.Windows.Controls.Button _helpButtonLarge;
        private bool _jackConfigVisible;
        private IORetaskingUserControl _jackReassignment;
        private RowDefinition _jackretaskingPane;
        private System.Windows.Controls.MenuItem _languageMenu;
        internal MainWindow _mainContainer;
        private System.Windows.Controls.ContextMenu _mainMenu;
        internal System.Windows.Controls.ListBox _mainTabButtonBar;
        internal System.Windows.Controls.Button _maxxAudio;
        internal Image _menuBar;
        internal System.Windows.Controls.Button _minimizeButton;
        internal System.Windows.Controls.Button _minimizeButtonLarge;
        private MonitorReg _monitor;
        internal Grid _outerGrid;
        private Dictionary<string, object> _pagesList;
        private double _primaryScreenHeight = SystemParameters.PrimaryScreenHeight;
        private double _primaryScreenWidth = SystemParameters.PrimaryScreenWidth;
        private System.Windows.Controls.MenuItem _resetMenu;
        internal System.Windows.Controls.Button _retask_jack;
        private ScrollViewer _scrollViewer;
        private string _selectedSkin = "Default";
        private System.Windows.Controls.MenuItem _settingsMenu;
        private System.Windows.Controls.MenuItem _skinsMenu;
        internal System.Windows.Controls.Button _SRS;
        private System.Windows.Controls.MenuItem _systemTrayMenu;
        internal System.Windows.Controls.ListBox _tabButtonBarStatic;
        internal SplitButton _taskBarMenu;
        internal SplitButton _taskBarMenuBig;
        internal Border _titleBar;
        internal ColumnDefinition _toolBarCol;
        internal StackPanel _toolBarPanel;
        internal Image _toolbarSeperator;
        private BackgroundWorker _workerThread;
        private int DefaultScrollWindowHeight = 0x2b8;
        private int DefaultScrollWindowWidth = 850;
        private int DefaultWindowHeight = 0x2ae;
        private int DefaultWindowWidth = 0x312;
        private Form helpForm;
        private ThirdPartyWindow hiddenWindow;
        public static MainWindow theCurrent;

        public event OnAltF4CloseHandler OnAltF4Close;

        public MainWindow()
        {
            try
            {
                this.InitializeComponent();
                Workaround110052078705416.ApplyWorkaround110052078705416(this);
                base.Closing += new CancelEventHandler(this.MainWindow_Closing);
                SystemEvents.DisplaySettingsChanged += new EventHandler(this.SystemEvents_DisplaySettingsChanged);
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("MainWindow::MainWindow() Error ", Severity.FATALERROR, exception);
                System.Windows.MessageBox.Show(Resources.SA_FailedToInitialize, Resources.SA_SmartAudio);
                System.Windows.Application.Current.Shutdown();
            }
            App current = System.Windows.Application.Current as App;
            if (current.Settings.ShowJackpopup && (this._jackReassignment != null))
            {
                this._jackReassignment.JackPortChanged += new JackPortChangedHandler(this._jackReassignment_JackPortChanged);
            }
            theCurrent = this;
            this.helpForm = new Form();
            this.helpForm.Visible = false;
            this.helpForm.CreateControl();
            base.Loaded += new RoutedEventHandler(this.MainWindow_Loaded);
            base.Activated += new EventHandler(this.MainWindow_Activated);
            base.IsEnabledChanged += new DependencyPropertyChangedEventHandler(this.MainWindow_IsEnabledChanged);
            if (current.Settings.INISettings.IsScrollBarsEnabled)
            {
                this._scrollViewer = new ScrollViewer();
                this._scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                this._scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                base.Content = null;
                base.Content = this._scrollViewer;
                this._scrollViewer.Content = this._outerGrid;
                this.DefaultWindowWidth = this.DefaultScrollWindowWidth;
                this.DefaultWindowHeight = this.DefaultScrollWindowHeight;
                base.Width = this.DefaultScrollWindowWidth;
                base.Height = this.DefaultScrollWindowHeight;
                this._jackretaskingPane = this._outerGrid.RowDefinitions[1];
                this._outerGrid.MinHeight -= 110.0;
                this._outerGrid.Height -= 110.0;
                this._outerGrid.RowDefinitions.Remove(this._outerGrid.RowDefinitions[1]);
                this.DefaultWindowHeight -= 0x60;
            }
            if (current.Settings.INISettings.IsUseBigToolbarButton)
            {
                this._bigToolbar.Visibility = Visibility.Visible;
                this._toolBarPanel.Visibility = Visibility.Hidden;
                this._captionCol.Width = new GridLength(638.0);
                this._toolBarCol.Width = new GridLength(144.0, GridUnitType.Star);
            }
            else
            {
                this._bigToolbar.Visibility = Visibility.Hidden;
                this._taskBarMenu.Visibility = Visibility.Visible;
            }
            if (current.Settings.INISettings.IsJackRetaskingEnabled && current.IsJackRetaskable)
            {
                this._jackReassignment = new IORetaskingUserControl();
                this._jackReassignment.Visibility = Visibility.Hidden;
                this._jackReassignment.Opacity = 1.0;
                this._jackReassignment.Margin = new Thickness(0.0, 0.0, 0.0, 3.0);
                Grid.SetRow(this._jackReassignment, 1);
                this._outerGrid.Children.Add(this._jackReassignment);
            }
            current.AudioFactory.add_OnModeChanged(new _ICxHDAudioFactoryEvents_OnModeChangedEventHandler(this.AudioFactory_OnModeChanged));
            if (current.Settings.INISettings.IsScrollBarsEnabled)
            {
                this._currentScaleResolution = current.AdjustWindowSize(null, this, (double) this.DefaultWindowWidth, (double) this.DefaultWindowHeight, this._currentScaleResolution, false);
            }
            else
            {
                this._currentScaleResolution = current.AdjustWindowSize(this._outerGrid, this, (double) this.DefaultWindowWidth, (double) this.DefaultWindowHeight, this._currentScaleResolution, false);
            }
            this.MonitorRegChange();
            base.StateChanged += new EventHandler(this.MainWindow_StateChanged);
            base.UpdateLayout();
        }

        private void _audioDirectorBtn_Click(object sender, RoutedEventArgs e)
        {
            this.ShowPage("AudioDirector");
        }

        private void _closeBtn_Click(object sender, RoutedEventArgs e)
        {
            App current = System.Windows.Application.Current as App;
            if (current.Settings.RunOnSystemTray)
            {
                base.Visibility = Visibility.Hidden;
            }
            else
            {
                this._monitor.Dispose();
                current.CloseSystemTray();
                System.Windows.Application.Current.Shutdown();
            }
        }

        [DebuggerNonUserCode]
        internal Delegate _CreateDelegate(System.Type delegateType, string handler) => 
            Delegate.CreateDelegate(delegateType, this, handler);

        private void _dolbyBtn_Click(object sender, RoutedEventArgs e)
        {
            App current = System.Windows.Application.Current as App;
            if (current.AudioFactory != null)
            {
                current.AudioFactory.LaunchDolbyApp();
            }
        }

        private void _hdmiBtn_Click(object sender, RoutedEventArgs e)
        {
            this.ShowPage("AuxiliaryEndPoint");
        }

        private void _helpButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application current = System.Windows.Application.Current;
            try
            {
                Help.ShowHelpIndex(this.helpForm, this.GetHelpFile());
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("MainWindow::_helpButton_Click(): Failed to launch  help", Severity.FATALERROR, exception);
            }
        }

        private void _jackReassignment_JackPortChanged(int jack)
        {
        }

        private void _jackReassignment_JackPortChanged(int jack, int selectedJack)
        {
            App.theCurrentApp.UpdateJackPorts(jack, selectedJack);
        }

        private void _mainAudioBtn_Click(object sender, RoutedEventArgs e)
        {
            this.ShowPage("MainAudioPage");
        }

        private void _mainAudioBtn_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Image image = sender as Image;
            if (image != null)
            {
                DoubleAnimation animation = new DoubleAnimation {
                    Duration = new Duration(TimeSpan.FromMilliseconds(20.0)),
                    From = 60.0,
                    To = 75.0,
                    AutoReverse = false
                };
                image.BeginAnimation(FrameworkElement.WidthProperty, animation);
                image.BeginAnimation(FrameworkElement.HeightProperty, animation);
            }
        }

        private void _mainAudioBtn_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Image image = sender as Image;
            if (image != null)
            {
                DoubleAnimation animation = new DoubleAnimation {
                    Duration = new Duration(TimeSpan.FromMilliseconds(20.0)),
                    From = 75.0,
                    To = 60.0,
                    AutoReverse = false
                };
                image.BeginAnimation(FrameworkElement.WidthProperty, animation);
                image.BeginAnimation(FrameworkElement.HeightProperty, animation);
            }
        }

        private void _mainContainer_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                App current = System.Windows.Application.Current as App;
                if (current.AudioFactory != null)
                {
                    current.RunOnSystemTray();
                    this._jackConfigVisible = false;
                    this.CreateTabPages();
                    this.Localize();
                    if (!current.IsResumeFromS3_S4)
                    {
                        this.ShowPage("MainAudioPage");
                    }
                    this._mainMenu = new System.Windows.Controls.ContextMenu();
                    this._taskBarMenu.ContextMenu = this._mainMenu;
                    if (this._jackReassignment != null)
                    {
                        this._jackReassignment.HDAudioConfig = current.AudioFactory.DeviceIOConfig;
                    }
                    SystemEvents.UserPreferenceChanged += new UserPreferenceChangedEventHandler(this.SystemEvents_UserPreferenceChanged);
                    if (current.IsRunningInBackground)
                    {
                        base.Visibility = Visibility.Hidden;
                    }
                    else if (current.IsRunningSliently)
                    {
                        base.Visibility = Visibility.Hidden;
                    }
                    if (current.IsResumeFromS3_S4)
                    {
                        base.Visibility = Visibility.Hidden;
                    }
                }
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("MainWindow::_mainContainer_Loaded() Error ", Severity.FATALERROR, exception);
                System.Windows.MessageBox.Show(Resources.SA_FailedToInitialize, Resources.SA_SmartAudio);
            }
        }

        private void _mainMenu_Opened(object sender, RoutedEventArgs e)
        {
            this.RefreshLanguageMenu();
        }

        private void _maxxAudio_Click(object sender, RoutedEventArgs e)
        {
            App current = System.Windows.Application.Current as App;
            if (current.AudioFactory != null)
            {
                current.AudioFactory.LaunchMaxxApp();
            }
        }

        private void _minimizeBtn_Click(object sender, RoutedEventArgs e)
        {
            App current = System.Windows.Application.Current as App;
            current.IsRunningMinimized = true;
            base.WindowState = WindowState.Minimized;
        }

        private void _minimizeButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }

        private void _restoreBtn_Click(object sender, RoutedEventArgs e)
        {
            base.WindowState = WindowState.Normal;
        }

        private void _retask_jack_Click(object sender, RoutedEventArgs e)
        {
            DoubleAnimation animation = new DoubleAnimation();
            if (this._jackConfigVisible)
            {
                animation.From = 1.0;
                animation.To = 0.0;
            }
            else
            {
                animation.From = 0.0;
                animation.To = 1.0;
            }
            this._jackConfigVisible = !this._jackConfigVisible;
            App current = System.Windows.Application.Current as App;
            if (current.Settings.INISettings.IsScrollBarsEnabled)
            {
                if (this._jackConfigVisible)
                {
                    this._outerGrid.MinHeight += 110.0;
                    this._outerGrid.Height += 110.0;
                    this._outerGrid.RowDefinitions.Add(this._jackretaskingPane);
                    this.DefaultWindowHeight += 0x60;
                    base.Height += 96.0;
                }
                else
                {
                    this._outerGrid.MinHeight -= 110.0;
                    this._outerGrid.Height -= 110.0;
                    this._outerGrid.RowDefinitions.Remove(this._jackretaskingPane);
                    this.DefaultWindowHeight -= 0x60;
                    base.Height -= 96.0;
                }
                this._currentScaleResolution = current.AdjustWindowSize(null, this, (double) this.DefaultWindowWidth, (double) this.DefaultWindowHeight, this._currentScaleResolution, false);
            }
            else
            {
                this._currentScaleResolution = current.AdjustWindowSize(this._outerGrid, this, (double) this.DefaultWindowWidth, (double) this.DefaultWindowHeight, this._currentScaleResolution, false);
            }
            animation.Duration = new Duration(TimeSpan.FromSeconds(1.0));
            animation.AutoReverse = false;
            this._jackReassignment.BeginAnimation(UIElement.OpacityProperty, animation);
            this._jackReassignment.Visibility = this._jackConfigVisible ? Visibility.Visible : Visibility.Hidden;
        }

        private void _skins_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.MenuItem originalSource = e.OriginalSource as System.Windows.Controls.MenuItem;
            App current = System.Windows.Application.Current as App;
            try
            {
                this._selectedSkin = (string) originalSource.Header;
                for (int i = 0; (this._skinsMenu != null) && (i < this._skinsMenu.Items.Count); i++)
                {
                    System.Windows.Controls.MenuItem item2 = (System.Windows.Controls.MenuItem) this._skinsMenu.Items[i];
                    if (item2.Header.ToString().Trim().ToUpper() == this._selectedSkin)
                    {
                        item2.IsCheckable = item2.IsChecked = true;
                    }
                    else
                    {
                        item2.IsCheckable = false;
                    }
                }
                current.LoadSkin((Guid) originalSource.Tag);
                current.Settings.SelectedSkin = (Guid) originalSource.Tag;
                current.SelectDefaultSkin();
                if (this._skinsMenu != null)
                {
                    this._skinsMenu.Items.Clear();
                }
                this.CreateContextMenu();
                current.Settings.Save();
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("MainWindow::_skins_Click(): Failed to load Sking ", Severity.FATALERROR, exception);
            }
        }

        private void _SRS_Click(object sender, RoutedEventArgs e)
        {
            App current = System.Windows.Application.Current as App;
            if (current.AudioFactory != null)
            {
                current.AudioFactory.LaunchSRSApp();
            }
        }

        private void _tabButtonBarStatic_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this._mainTabButtonBar.SelectedIndex != -1)
            {
                switch (((string) ((ListBoxItem) this._mainTabButtonBar.SelectedItem).Tag))
                {
                    case "AudioMixer":
                        this.ShowPage("MainAudioPage");
                        return;

                    case "VoiceEffects":
                        this.ShowPage("VoiceSettingsPage");
                        return;

                    case "AudioDirector":
                        this.ShowPage("AudioDirector");
                        return;

                    case "HDMISwitch":
                        this.ShowPage("AuxiliaryEndPoint");
                        return;

                    case "BTRedirection":
                        this.ShowPage("Bluetooth");
                        return;

                    case "Equalizer":
                        this.ShowPage("GraphicEqualizers");
                        return;

                    case "SpeakerSetup":
                        this.ShowPage("SpeakerSetup");
                        return;
                }
            }
        }

        private void _taskBarMenu_Click(object sender, RoutedEventArgs e)
        {
            if (this._mainMenu.Items.Count == 0)
            {
                this.CreateContextMenu();
            }
            this._mainMenu.PlacementTarget = this._taskBarMenu;
            this._mainMenu.Placement = PlacementMode.Bottom;
            this._mainMenu.IsOpen = true;
        }

        private void _voipBtn_Click(object sender, RoutedEventArgs e)
        {
            this.ShowPage("VoiceSettingsPage");
        }

        private void _workerThread_DoWork(object sender, DoWorkEventArgs e)
        {
            base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new OnAltF4CloseHandler(this.MainWindow_OnAltF4Close));
        }

        private void AboutItem_Click(object sender, RoutedEventArgs e)
        {
            App current = System.Windows.Application.Current as App;
            new SmartAudioAboutWindow { 
                AudioFactory = current.AudioFactory,
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            }.ShowDialog();
        }

        private void ApplySkin(Uri skinUri)
        {
            try
            {
                (System.Windows.Application.Current as App).ApplySkin(skinUri);
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("MainWindow::ApplySkin() Error ", Severity.FATALERROR, exception);
            }
        }

        private void AudioFactory_OnModeChanged(CxOneKeyMode newMode)
        {
            base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new RefreshPagesForMOdeChange(this.RefreshPages), newMode);
        }

        private void blueToothBtn_Click(object sender, RoutedEventArgs e)
        {
            this.ShowPage("Bluetooth");
        }

        private void CreateContextMenu()
        {
            App current = System.Windows.Application.Current as App;
            this._mainMenu.Items.Clear();
            System.Windows.Controls.MenuItem newItem = new System.Windows.Controls.MenuItem();
            this._settingsMenu = newItem;
            newItem.Header = Resources.SA_Settings;
            this._mainMenu.Items.Add(newItem);
            this._mainMenu.Items.Add(new Separator());
            this._mainMenu.Opened += new RoutedEventHandler(this._mainMenu_Opened);
            System.Windows.Controls.MenuItem item2 = new System.Windows.Controls.MenuItem();
            this._resetMenu = item2;
            item2.Header = Resources.SA_BTNRESET;
            item2.Click += new RoutedEventHandler(this.ResetItem_Click);
            this._mainMenu.Items.Add(item2);
            item2 = new System.Windows.Controls.MenuItem {
                Header = Resources.SA_About
            };
            item2.Click += new RoutedEventHandler(this.AboutItem_Click);
            this._mainMenu.Items.Add(item2);
            if (current.Settings.INISettings.EnableSkinMenu)
            {
                this.EnumerateSkins();
            }
            item2 = new System.Windows.Controls.MenuItem {
                Header = Resources.SA_LANGUAGE
            };
            this._languageMenu = item2;
            if ((current.AvailableLanguages != null) && (current.AvailableLanguages.Count > 1))
            {
                newItem.Items.Add(item2);
                newItem.Items.Add(new Separator());
            }
            item2 = new System.Windows.Controls.MenuItem {
                Header = Resources.SA_RunInSystemTray,
                IsCheckable = true
            };
            if (this.IsRunInTaskbarOptionDisabled())
            {
                item2.IsEnabled = false;
            }
            this._systemTrayMenu = item2;
            item2.IsChecked = current.Settings.RunOnSystemTray;
            item2.Checked += new RoutedEventHandler(this.RunOnSystemTray_Checked);
            item2.Unchecked += new RoutedEventHandler(this.RunOnSystemTray_Checked);
            current.AudioFactory.DeviceIOConfig.InitRunInTaskbarSetting = false;
            newItem.Items.Add(item2);
            if (current.Settings.INISettings.IsJackRetaskingEnabled && current.IsJackRetaskable)
            {
                item2 = new System.Windows.Controls.MenuItem();
                this._disableJackMenu = item2;
                item2.Header = Resources.SA_DEVLINEOUT;
                item2.IsCheckable = true;
                item2.IsChecked = !current.Settings.ShowJackpopup;
                item2.Checked += new RoutedEventHandler(this.jackPopup_Checked);
                item2.Unchecked += new RoutedEventHandler(this.jackPopup_Checked);
                newItem.Items.Add(item2);
            }
            else
            {
                this._disableJackMenu = null;
            }
        }

        private void CreateTabPages()
        {
            App current = System.Windows.Application.Current as App;
            int count = this._tabButtonBarStatic.Items.Count;
            int num2 = 0x5f;
            try
            {
                this._pagesList = new Dictionary<string, object>();
                this._pagesList.Add("MainAudioPage", new MainAudioControlPage());
                if (this.IsBeamFormingEnabled())
                {
                    this._pagesList.Add("VoiceSettingsPage", new VoiceEffectsPage());
                }
                else
                {
                    this._pagesList.Add("VoiceSettingsPage", new VoiceEffectsPageNoBeamForming());
                }
                if ((current.AudioFactory.HostOperatingSystem == CxHostOperatingSystemType.WindowsVista) || (current.AudioFactory.HostOperatingSystem == CxHostOperatingSystemType.Windows7))
                {
                    this._pagesList.Add("AudioDirector", new AudioDirectorPage());
                    this._pagesList.Add("AuxiliaryEndPoint", new HDMISettingsPage());
                    this._pagesList.Add("Bluetooth", new BluetoothSettingsPage());
                }
                this._pagesList.Add("GraphicEqualizers", new GraphicEqualizersPage());
                foreach (object obj2 in this._pagesList.Values)
                {
                    ISmartAudioPage page = (ISmartAudioPage) obj2;
                    try
                    {
                        if (page != null)
                        {
                            page.AudioFactory = current.AudioFactory;
                        }
                    }
                    catch (Exception exception)
                    {
                        SmartAudioLog.Log("Failed to initialize page '" + page.FriendlyName, Severity.FATALERROR, exception);
                        System.Windows.MessageBox.Show(Resources.SA_FailedToInitialize, Resources.SA_SmartAudio);
                        current.CloseSystemTray();
                        System.Windows.Application.Current.Shutdown();
                    }
                }
            }
            catch (Exception exception2)
            {
                SmartAudioLog.Log("MainWindow::CreateTabPages()", Severity.FATALERROR, exception2);
                System.Windows.MessageBox.Show(Resources.SA_FailedToInitialize, Resources.SA_SmartAudio);
                current.CloseSystemTray();
                System.Windows.Application.Current.Shutdown();
            }
            if ((current.AudioFactory.HostOperatingSystem != CxHostOperatingSystemType.WindowsVista) && (current.AudioFactory.HostOperatingSystem != CxHostOperatingSystemType.Windows7))
            {
                this.RemoveToolBarIcon(this._mainTabButtonBar, "AudioDirector");
                this.RemoveToolBarIcon(this._mainTabButtonBar, "HDMISwitch");
                this.RemoveToolBarIcon(this._mainTabButtonBar, "BTRedirection");
                this.RemoveToolBarIcon(this._tabButtonBarStatic, "Dolby");
                this.RemoveToolBarIcon(this._tabButtonBarStatic, "SRS");
            }
            else if (!current.IsDemoMode)
            {
                if (current.Settings.INISettings.IsAudioRedirectorDisabled)
                {
                    this.RemoveToolBarIcon(this._mainTabButtonBar, "AudioDirector");
                }
                if (!this.IsDolbyEnabled)
                {
                    this.RemoveToolBarIcon(this._tabButtonBarStatic, "Dolby");
                }
                if (!this.IsSRSEnabled)
                {
                    this.RemoveToolBarIcon(this._tabButtonBarStatic, "SRS");
                }
                if (!current.Settings.INFSettings.BluetoothAvailable)
                {
                    this.RemoveToolBarIcon(this._mainTabButtonBar, "BTRedirection");
                }
                if (!current.Settings.INFSettings.HDMIAvailable)
                {
                    this.RemoveToolBarIcon(this._mainTabButtonBar, "HDMISwitch");
                }
                if (current.Settings.INISettings.IsVPAPageDisabled)
                {
                    this.RemoveToolBarIcon(this._mainTabButtonBar, "VoiceEffects");
                }
            }
            if (!this.IsJackRetaskingEnabled)
            {
                this.RemoveToolBarIcon(this._tabButtonBarStatic, "JackRetasking");
            }
            if (!current.Settings.INFSettings.MaxxAudioAvailable)
            {
                this.RemoveToolBarIcon(this._tabButtonBarStatic, "MaxxAudio");
            }
            MainAudioControlPage page2 = this._pagesList["MainAudioPage"] as MainAudioControlPage;
            try
            {
                if (page2 != null)
                {
                    if (current.IsMultiChannelSupported)
                    {
                        ISmartAudioPage page3 = new SpeakerSetupPage();
                        if (page3 != null)
                        {
                            this._pagesList.Add("SpeakerSetup", page3);
                            page3.AudioFactory = current.AudioFactory;
                            MainAudioControlPage page4 = this._pagesList["MainAudioPage"] as MainAudioControlPage;
                            page4.OnSpeakerConfigChange += new MainAudioControlPage.OnSpeakerConfigChangeHandler(this.mainAudioPage_OnSpeakerConfigChange);
                        }
                    }
                    else
                    {
                        this.RemoveToolBarIcon(this._mainTabButtonBar, "SpeakerSetup");
                    }
                }
            }
            catch (Exception exception3)
            {
                SmartAudioLog.Log("Failed to initialize the Speaker Setup page ", Severity.FATALERROR, exception3);
            }
            if (this._tabButtonBarStatic.Items.Count < 3)
            {
                if (this._tabButtonBarStatic.Items.Count == 0)
                {
                    this._toolbarSeperator.Visibility = Visibility.Hidden;
                    this._tabButtonBarStatic.Visibility = Visibility.Hidden;
                }
                else
                {
                    Thickness thickness = new Thickness(this._toolbarSeperator.Margin.Left, this._toolbarSeperator.Margin.Top, this._toolbarSeperator.Margin.Right, this._toolbarSeperator.Margin.Bottom);
                    thickness.Left += (count - this._tabButtonBarStatic.Items.Count) * 70;
                    this._toolbarSeperator.Margin = thickness;
                    this._tabButtonBarStatic.Width = num2 * this._tabButtonBarStatic.Items.Count;
                }
            }
            this.UpdateGUIForOneKeyFeature();
            object obj3 = theCurrent.GetPage("VoiceSettingsPage");
            if ((obj3 != null) && (page2 != null))
            {
                if (this.IsBeamFormingEnabled())
                {
                    ((VoiceEffectsPage) obj3).RefreshVOIPControl(page2.HPPlugInStatus && page2.IsExternalMicPluggedIn());
                }
                else
                {
                    ((VoiceEffectsPageNoBeamForming) obj3).RefreshVOIPControl(page2.HPPlugInStatus && page2.IsExternalMicPluggedIn());
                }
            }
        }

        private void DisplayToolBarIcon(System.Windows.Controls.ListBox listBox, string tagName, bool isVisiable)
        {
            for (int i = 0; i < listBox.Items.Count; i++)
            {
                ListBoxItem item = (ListBoxItem) listBox.Items[i];
                if (item.Tag.ToString().CompareTo(tagName) == 0)
                {
                    item.Visibility = isVisiable ? Visibility.Visible : Visibility.Hidden;
                    return;
                }
            }
        }

        private void EnumerateSkins()
        {
            App current = System.Windows.Application.Current as App;
            System.Windows.Controls.MenuItem newItem = new System.Windows.Controls.MenuItem {
                Header = Resources.SA_Skin
            };
            this._skinsMenu = newItem;
            if (this._skinsMenu.Items.Count == 0)
            {
                foreach (SkinDescriptor descriptor in current.Skins)
                {
                    System.Windows.Controls.MenuItem item2 = new System.Windows.Controls.MenuItem {
                        Header = descriptor.SkinName,
                        Tag = descriptor.UniqueID
                    };
                    item2.Click += new RoutedEventHandler(this._skins_Click);
                    newItem.Items.Add(item2);
                    if (descriptor.SelectAsDefault)
                    {
                        item2.IsCheckable = true;
                        item2.IsChecked = true;
                    }
                }
            }
            if (newItem.Items.Count == 0)
            {
                SmartAudioLog.Log("EnumerateSkins() did not find any skins");
                System.Windows.MessageBox.Show(Resources.SA_FailedToInitialize, Resources.SA_SmartAudio);
                current.CloseSystemTray();
                System.Windows.Application.Current.Shutdown();
            }
            if (newItem.Items.Count > 1)
            {
                this._settingsMenu.Items.Add(newItem);
            }
        }

        private void equalizer_jack_Click(object sender, RoutedEventArgs e)
        {
            this.ShowPage("GraphicEqualizers");
        }

        ~MainWindow()
        {
        }

        public int GetBFSetting()
        {
            bool beamForming = false;
            object obj2 = this._pagesList["VoiceSettingsPage"];
            if (obj2 != null)
            {
                VoiceEffectsPage page = obj2 as VoiceEffectsPage;
                if (page != null)
                {
                    beamForming = page.GetBeamForming();
                }
            }
            if (!beamForming)
            {
                return 0;
            }
            return 1;
        }

        private string GetHelpFile()
        {
            App current = System.Windows.Application.Current as App;
            string mappedName = SALocalization.GetMappedName(current.CurrentLocale.ToString().ToUpper());
            FileInfo info = new FileInfo(base.GetType().Assembly.Location);
            return string.Concat(new object[] { info.Directory, @"\", mappedName, @"\", SALocalization.LocalizationTable[mappedName.ToString().ToUpper()].HelpFileName });
        }

        public object GetPage(string name)
        {
            try
            {
                return this._pagesList[name];
            }
            catch (KeyNotFoundException exception)
            {
                SmartAudioLog.Log("MainWindow.GetPage", Severity.FATALERROR, exception);
                return null;
            }
        }

        private void HelpItem_Click(object sender, RoutedEventArgs e)
        {
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (!this._contentLoaded)
            {
                this._contentLoaded = true;
                Uri resourceLocator = new Uri("/SmartAudio;component/mainwindow.xaml", UriKind.Relative);
                System.Windows.Application.LoadComponent(this, resourceLocator);
            }
        }

        public bool IsBeamFormingEnabled()
        {
            App current = System.Windows.Application.Current as App;
            if (current.AudioFactory == null)
            {
                return false;
            }
            return current.Settings.INFSettings.BeamFormingEnabled;
        }

        public bool IsBeamFormingSupported()
        {
            bool flag = false;
            object obj2 = this._pagesList["VoiceSettingsPage"];
            if ((obj2 != null) && (obj2 is VoiceEffectsPage))
            {
                flag = true;
            }
            return flag;
        }

        private bool IsRunInSystemTrayEnabled()
        {
            App current = System.Windows.Application.Current as App;
            if (current.AudioFactory == null)
            {
                return false;
            }
            return current.Settings.INISettings.RunInSystemTrayEnabled;
        }

        private bool IsRunInTaskbarOptionDisabled()
        {
            bool runInTaskBarOptionDisabled = false;
            try
            {
                App current = System.Windows.Application.Current as App;
                if (current.AudioFactory != null)
                {
                    runInTaskBarOptionDisabled = current.Settings.INISettings.RunInTaskBarOptionDisabled;
                }
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("MainWindow::IsRunInTaskbarOptiondisabled{} ", Severity.FATALERROR, exception);
            }
            return runInTaskBarOptionDisabled;
        }

        private void jackPopup_Checked(object sender, RoutedEventArgs e)
        {
            App current = System.Windows.Application.Current as App;
            System.Windows.Controls.MenuItem originalSource = e.OriginalSource as System.Windows.Controls.MenuItem;
            current.Settings.ShowJackpopup = !originalSource.IsChecked;
            if (current.Settings.ShowJackpopup)
            {
                this._jackReassignment.JackPortChanged += new JackPortChangedHandler(this._jackReassignment_JackPortChanged);
            }
            else
            {
                this._jackReassignment.JackPortChanged -= new JackPortChangedHandler(this._jackReassignment_JackPortChanged);
            }
            current.Settings.Save();
        }

        private void JackpopupItem_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application current = System.Windows.Application.Current;
        }

        private void languageItem_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.MenuItem originalSource = e.OriginalSource as System.Windows.Controls.MenuItem;
            App current = System.Windows.Application.Current as App;
            string tag = (string) originalSource.Tag;
            if (tag.StartsWith("zh"))
            {
                tag = current.GetSpecificLocale(tag);
                if (tag == null)
                {
                    tag = (string) originalSource.Tag;
                }
            }
            current.SetLocale(tag, false);
            this.Localize();
            current.Settings.SelectedLocale = tag;
            if (tag.CompareTo(current.CurrentOSCulture) != 0)
            {
                current.Settings.OverrideCurrentLocale = true;
            }
            else
            {
                current.Settings.OverrideCurrentLocale = false;
            }
            this.RefreshLanguageMenu();
            current.Settings.Save();
        }

        public bool Localize()
        {
            App current = System.Windows.Application.Current as App;
            if (!current.Settings.INISettings.IsLayeredWindowEnabled)
            {
                base.Title = Resources.SA_XAML_SmartAudioII;
            }
            this.LocalizeToolBar();
            foreach (object obj2 in this._pagesList.Values)
            {
                ISmartAudioPage page = obj2 as ISmartAudioPage;
                if (page != null)
                {
                    page.Localize();
                }
            }
            ISmartAudioPage page2 = this._jackReassignment;
            if (page2 != null)
            {
                page2.Localize();
            }
            if (current.SystemTray != null)
            {
                current.SystemTray.RefreshContextMenu();
            }
            if (current.JackRetaskingPopup != null)
            {
                current.JackRetaskingPopup.Localize();
            }
            if (this._languageMenu != null)
            {
                this._languageMenu.Items.Clear();
            }
            if (this._skinsMenu != null)
            {
                this._skinsMenu.Items.Clear();
            }
            if (this._mainMenu != null)
            {
                this.CreateContextMenu();
            }
            return true;
        }

        private void LocalizeToolBar()
        {
            foreach (ListBoxItem item in (IEnumerable) this._mainTabButtonBar.Items)
            {
                switch (((string) item.Tag))
                {
                    case "AudioMixer":
                        item.ToolTip = Resources.SA_XAML_VolumeMixer;
                        break;

                    case "VoiceEffects":
                        item.ToolTip = Resources.SA_XAML_VoiceEffects;
                        break;

                    case "AudioDirector":
                        item.ToolTip = Resources.SA_XAML_AudioDirector;
                        break;

                    case "HDMISwitch":
                        item.ToolTip = Resources.SA_XAML_HDMISwitch;
                        break;

                    case "BTRedirection":
                        item.ToolTip = Resources.SA_XAML_BluetoothRedirection;
                        break;

                    case "SpeakerSetup":
                        item.ToolTip = Resources.SA_SpeakerSetup;
                        break;

                    case "Equalizer":
                        item.ToolTip = Resources.SA_XAML_SmartEQ_3DSettings;
                        break;
                }
            }
            foreach (ListBoxItem item2 in (IEnumerable) this._tabButtonBarStatic.Items)
            {
                string str4 = (string) item2.Tag;
                if (str4 != null)
                {
                    if (str4 == "JackRetasking")
                    {
                        item2.ToolTip = Resources.SA_XAML_RetaskAudioJacks;
                    }
                    else if (str4 == "Dolby")
                    {
                        goto Label_01B4;
                    }
                }
                continue;
            Label_01B4:
                item2.ToolTip = Resources.SA_XAML_LaunchDolbyControlCenter;
            }
        }

        private void mainAudioPage_OnSpeakerConfigChange(CxSpeakerConfigType configurationType)
        {
            SpeakerSetupPage page = this._pagesList["SpeakerSetup"] as SpeakerSetupPage;
            if (page != null)
            {
                page.InitPreview();
                page.SpeakerConfiguration = configurationType;
            }
        }

        private void MainWindow_Activated(object sender, EventArgs e)
        {
            App current = System.Windows.Application.Current as App;
            current.IsRunningMinimized = false;
            this.StartVolumeLevel();
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            this._workerThread = new BackgroundWorker();
            this._workerThread.DoWork += new DoWorkEventHandler(this._workerThread_DoWork);
            this._workerThread.RunWorkerAsync(null);
        }

        private void MainWindow_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            this._jackReassignment.IsEnabled = (bool) e.NewValue;
        }

        private void MainWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.F1)
            {
                object obj2 = this._pagesList["GraphicEqualizers"];
                if (obj2 != null)
                {
                    ((GraphicEqualizersPage) obj2).TogglePhantomBass();
                }
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.hiddenWindow = new ThirdPartyWindow("SmartAudio");
            this.hiddenWindow.CallerWindow = this;
        }

        private void MainWindow_OnAltF4Close()
        {
            App current = System.Windows.Application.Current as App;
            if (current.Settings.RunOnSystemTray)
            {
                base.Visibility = Visibility.Hidden;
            }
            else
            {
                this._monitor.Dispose();
                current.CloseSystemTray();
                System.Windows.Application.Current.Shutdown();
            }
        }

        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            App current = System.Windows.Application.Current as App;
            if (current.Settings.INISettings.IsLayeredWindowEnabled)
            {
                if ((base.WindowState == WindowState.Normal) || (base.WindowState == WindowState.Maximized))
                {
                    base.Title = "";
                }
                else
                {
                    base.Title = Resources.SA_XAML_SmartAudioII;
                }
            }
        }

        private bool ModifyRegistryRunOnsystemTray(bool IsRunOnSystemTrayEnabled)
        {
            bool flag = false;
            try
            {
                App current = System.Windows.Application.Current as App;
                if (current.AudioFactory == null)
                {
                    return flag;
                }
                if (IsRunOnSystemTrayEnabled)
                {
                    current.AudioFactory.DeviceIOConfig.UpdateRunInTaskbarSetting = true;
                    return flag;
                }
                current.AudioFactory.DeviceIOConfig.UpdateRunInTaskbarSetting = false;
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("MainWindow::ModifyRegistryRunOnsystemTray{} ", Severity.FATALERROR, exception);
            }
            return flag;
        }

        private void MonitorRegChange()
        {
            this._monitor = new MonitorReg();
            this._monitor.RegChanged += new EventHandler(this.OnRegChanged);
            this._monitor.StartMonitoring();
        }

        public void OnNewSkinApplied()
        {
            App current = System.Windows.Application.Current as App;
            try
            {
                if ((current.AudioFactory.HostOperatingSystem == CxHostOperatingSystemType.WindowsVista) || (current.AudioFactory.HostOperatingSystem == CxHostOperatingSystemType.Windows7))
                {
                    AudioDirectorPage page = (AudioDirectorPage) this._pagesList["AudioDirector"];
                    if (page != null)
                    {
                        page.ApplyNewStyle();
                    }
                }
                if (this.IsBeamFormingEnabled())
                {
                    ((VoiceEffectsPage) this._pagesList["VoiceSettingsPage"]).ApplyNewStyle();
                }
                else
                {
                    ((VoiceEffectsPageNoBeamForming) this._pagesList["VoiceSettingsPage"]).ApplyNewStyle();
                }
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("OnNewSkinApplied(): Falied to apply skins to pages", Severity.FATALERROR, exception);
            }
        }

        private void OnRegChanged(object sender, EventArgs e)
        {
            base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new OnRefresh_EQ_VOIP_Pages(this.Refresh_EQ_VOIP_Pages));
        }

        private void Refresh_EQ_VOIP_Pages()
        {
            object obj2 = this._pagesList["GraphicEqualizers"];
            if (obj2 != null)
            {
                ((GraphicEqualizersPage) obj2).SelectCurrentProfile();
            }
            object obj3 = this._pagesList["VoiceSettingsPage"];
            if (obj3 != null)
            {
                if (this.IsBeamFormingEnabled())
                {
                    ((VoiceEffectsPage) obj3).ShowCurrentSettings();
                }
                else
                {
                    ((VoiceEffectsPageNoBeamForming) obj3).ShowCurrentSettings();
                }
            }
        }

        private void RefreshLanguageMenu()
        {
            this._languageMenu.Items.Clear();
            App current = System.Windows.Application.Current as App;
            string currentLocale = current.CurrentLocale;
            string str2 = currentLocale;
            if (str2 != null)
            {
                if ((str2 == "zh-CN") || (str2 == "zh-SG"))
                {
                    currentLocale = "zh-Hans";
                }
                else if (((str2 == "zh-TW") || (str2 == "zh-HK")) || (str2 == "zh-MO"))
                {
                    currentLocale = "zh-Hant";
                }
            }
            if ((current.AvailableLanguages != null) && (current.AvailableLanguages.Count > 1))
            {
                foreach (LocaleInfo info in current.AvailableLanguages)
                {
                    System.Windows.Controls.MenuItem newItem = new System.Windows.Controls.MenuItem {
                        Header = info.Translation
                    };
                    if (info.LocaleName == currentLocale)
                    {
                        newItem.IsCheckable = true;
                        newItem.IsChecked = true;
                    }
                    newItem.Tag = info.LocaleName;
                    newItem.Click += new RoutedEventHandler(this.languageItem_Click);
                    this._languageMenu.Items.Add(newItem);
                }
            }
        }

        public void RefreshPageIconSelection()
        {
            string str;
            App current = System.Windows.Application.Current as App;
            switch (current.ActivePage)
            {
                case "MainAudioPage":
                    str = "AudioMixer";
                    break;

                case "VoiceSettingsPage":
                    str = "VoiceEffects";
                    break;

                case "AudioDirector":
                    str = "AudioDirector";
                    break;

                case "AuxiliaryEndPoint":
                    str = "HDMISwitch";
                    break;

                case "Bluetooth":
                    str = "BTRedirection";
                    break;

                case "GraphicEqualizers":
                    str = "Equalizer";
                    break;

                case "SpeakerSetup":
                    str = "SpeakerSetup";
                    break;

                default:
                    str = "AudioMixer";
                    break;
            }
            for (int i = 0; i < this._mainTabButtonBar.Items.Count; i++)
            {
                if (((string) ((ListBoxItem) this._mainTabButtonBar.Items[i]).Tag) == str)
                {
                    ((ListBoxItem) this._mainTabButtonBar.Items[i]).IsSelected = true;
                }
            }
        }

        private void RefreshPages(CxOneKeyMode newMode)
        {
            try
            {
                App current = System.Windows.Application.Current as App;
                if ((newMode == CxOneKeyMode.NORMAL_MODE) && current.Settings.INISettings.EQPageEnabled)
                {
                    this.DisplayToolBarIcon(this._mainTabButtonBar, "Equalizer", true);
                    try
                    {
                        object obj2 = this._pagesList["GraphicEqualizers"];
                        if (obj2 != null)
                        {
                            ((GraphicEqualizersPage) obj2).SelectCurrentProfile();
                            ((GraphicEqualizersPage) obj2).Refresh3D();
                        }
                    }
                    catch (KeyNotFoundException exception)
                    {
                        SmartAudioLog.Log("Failed to reset the EQ page.", Severity.WARNING, exception);
                    }
                }
                else
                {
                    this.DisplayToolBarIcon(this._mainTabButtonBar, "Equalizer", false);
                    if (this._currentlyActivePage == "GraphicEqualizers")
                    {
                        this.ShowPage("MainAudioPage");
                        this._mainTabButtonBar.SelectedIndex = 0;
                    }
                }
            }
            catch (KeyNotFoundException exception2)
            {
                SmartAudioLog.Log("Failed to reset the EQ page.", Severity.WARNING, exception2);
            }
        }

        private void RemoveToolBarIcon(System.Windows.Controls.ListBox listBox, string tagName)
        {
            for (int i = 0; i < listBox.Items.Count; i++)
            {
                ListBoxItem item = (ListBoxItem) listBox.Items[i];
                if (item.Tag.ToString().CompareTo(tagName) == 0)
                {
                    listBox.Items.RemoveAt(i);
                    return;
                }
            }
        }

        private void ResetItem_Click(object sender, RoutedEventArgs e)
        {
            this.ResetToDefault();
        }

        private void ResetToDefault()
        {
            App current = System.Windows.Application.Current as App;
            if (current.AudioFactory != null)
            {
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                bool isDriverSupportReset = false;
                bool isEnableClassicMode = false;
                bool isHptoSpeakerRedirect = false;
                bool isSpeakerToHPRedirect = false;
                bool isEnableBeamforming = false;
                bool isEnableSpeakerNR = false;
                bool isEnableAEC = false;
                bool isEnableMicNR = false;
                bool isEnableHPClone = false;
                bool isDisableSAJackNotify = false;
                bool isEnableMusicNR = false;
                current.AudioFactory.IsDriverSupportResetFeature(out isDriverSupportReset, out isEnableClassicMode, out isHptoSpeakerRedirect, out isSpeakerToHPRedirect, out isEnableBeamforming, out isEnableSpeakerNR, out isEnableAEC, out isEnableMicNR, out isEnableHPClone, out isDisableSAJackNotify, out isEnableMusicNR);
                if (isDriverSupportReset)
                {
                    current.Settings.SelectedSkin = Guid.Empty;
                    current.SelectDefaultSkin();
                    current.ApplySkin();
                    if (this._skinsMenu != null)
                    {
                        this._skinsMenu.Items.Clear();
                    }
                    try
                    {
                        object obj2 = this._pagesList["Bluetooth"];
                        if (obj2 != null)
                        {
                            ((BluetoothSettingsPage) obj2).ResetToDefault(false);
                        }
                    }
                    catch (KeyNotFoundException exception)
                    {
                        SmartAudioLog.Log("Failed to reset the bluetooth page.", Severity.WARNING, exception);
                    }
                    Thread.Sleep(0x7d0);
                    try
                    {
                        object obj3 = this._pagesList["AuxiliaryEndPoint"];
                        if (obj3 != null)
                        {
                            ((HDMISettingsPage) obj3).ResetToDefault(false);
                        }
                    }
                    catch (KeyNotFoundException exception2)
                    {
                        SmartAudioLog.Log("Failed to reset the HDMI page.", Severity.WARNING, exception2);
                    }
                    try
                    {
                        object obj4 = this._pagesList["AudioDirector"];
                        if (obj4 != null)
                        {
                            CxHDAudioAudioDirectorMode audioDirectorMode = ((AudioDirectorPage) obj4).AudioDirector.AudioDirectorMode;
                            if (isEnableClassicMode && (audioDirectorMode != CxHDAudioAudioDirectorMode.SingleStream))
                            {
                                ((AudioDirectorPage) obj4).ResetToClassicMode();
                            }
                        }
                    }
                    catch (Exception exception3)
                    {
                        SmartAudioLog.Log("Failed to reset to the classmic mode.", Severity.WARNING, exception3);
                    }
                    try
                    {
                        object obj5 = this._pagesList["MainAudioPage"];
                        if (obj5 != null)
                        {
                            ((MainAudioControlPage) obj5).ResetHeadphone();
                        }
                    }
                    catch (Exception exception4)
                    {
                        SmartAudioLog.Log("Failed to reset the Headphone status", Severity.WARNING, exception4);
                    }
                    try
                    {
                        if (this.IsBeamFormingEnabled())
                        {
                            object obj6 = this._pagesList["VoiceSettingsPage"];
                            if (obj6 != null)
                            {
                                ((VoiceEffectsPage) obj6).ResetToDefault(isEnableBeamforming, isEnableMicNR, isEnableSpeakerNR, isEnableAEC);
                            }
                        }
                        else
                        {
                            object obj7 = this._pagesList["VoiceSettingsPage"];
                            if (obj7 != null)
                            {
                                ((VoiceEffectsPageNoBeamForming) obj7).ResetToDefault(isEnableBeamforming, isEnableMicNR, isEnableSpeakerNR, isEnableAEC);
                            }
                        }
                    }
                    catch (KeyNotFoundException exception5)
                    {
                        SmartAudioLog.Log("Failed to reset beamforming", Severity.WARNING, exception5);
                    }
                    if (this._disableJackMenu != null)
                    {
                        current.Settings.ShowJackpopup = !isDisableSAJackNotify;
                        this._disableJackMenu.IsChecked = isDisableSAJackNotify;
                        if (current.Settings.ShowJackpopup && (this._jackReassignment != null))
                        {
                            this._jackReassignment.JackPortChanged += new JackPortChangedHandler(this._jackReassignment_JackPortChanged);
                        }
                        else
                        {
                            this._jackReassignment.JackPortChanged -= new JackPortChangedHandler(this._jackReassignment_JackPortChanged);
                        }
                    }
                    if (this._jackReassignment != null)
                    {
                        try
                        {
                            current.AudioFactory.ResetJackPorts();
                            this._jackReassignment.ResetJacks();
                            App.theCurrentApp.ResetJackPopup();
                        }
                        catch (Exception exception6)
                        {
                            SmartAudioLog.Log("Failed to reset the jack reassignment panel.", Severity.WARNING, exception6);
                        }
                    }
                    current.Settings.OverrideCurrentLocale = false;
                    current.ResetLocale();
                    current.Settings.SelectedLocale = CultureInfo.CurrentUICulture.Name;
                    this.Localize();
                    this.RefreshLanguageMenu();
                    if (this._systemTrayMenu != null)
                    {
                        if (this.IsRunInSystemTrayEnabled())
                        {
                            current.Settings.RunOnSystemTray = true;
                            this._systemTrayMenu.IsChecked = true;
                        }
                        else
                        {
                            current.Settings.RunOnSystemTray = false;
                            this._systemTrayMenu.IsChecked = false;
                        }
                        this.RunOnSystemTray_Checked(null, null);
                    }
                    try
                    {
                        object obj8 = this._pagesList["GraphicEqualizers"];
                        if (obj8 != null)
                        {
                            ((GraphicEqualizersPage) obj8).Reset();
                        }
                    }
                    catch (KeyNotFoundException exception7)
                    {
                        SmartAudioLog.Log("Failed to reset the EQ page.", Severity.WARNING, exception7);
                    }
                    Mouse.OverrideCursor = null;
                    this.CreateContextMenu();
                }
            }
        }

        private void RunOnSystemTray_Checked(object sender, RoutedEventArgs e)
        {
            App current = System.Windows.Application.Current as App;
            current.Settings.RunOnSystemTray = this._systemTrayMenu.IsChecked;
            current.Settings.Save();
            if (current.Settings.RunOnSystemTray)
            {
                current.RunOnSystemTray();
                this.ModifyRegistryRunOnsystemTray(true);
            }
            else
            {
                current.CloseSystemTray();
                this.ModifyRegistryRunOnsystemTray(false);
            }
        }

        public void SaveActivePage()
        {
            App current = System.Windows.Application.Current as App;
            if (current != null)
            {
                current.ActivePage = this._currentlyActivePage;
            }
        }

        private void SelectButton(int selectionIndex, bool select)
        {
            double height = 0.0;
            double num2 = 0.0;
            double width = 0.0;
            double num4 = 0.0;
            System.Windows.Controls.Button content = (System.Windows.Controls.Button) ((ListBoxItem) this._mainTabButtonBar.SelectedItem).Content;
            Image image = (Image) content.Content;
            if (select)
            {
                height = content.Height;
                num2 = 80.0;
                width = content.Width;
                num4 = 80.0;
            }
            else
            {
                height = 80.0;
                num2 = content.Height;
                width = 80.0;
                num4 = content.Width;
            }
            DoubleAnimation animation = new DoubleAnimation {
                From = new double?(height),
                To = new double?(num2),
                Duration = new Duration(TimeSpan.FromMilliseconds(100.0)),
                AutoReverse = true
            };
            content.BeginAnimation(FrameworkElement.HeightProperty, animation);
            animation = new DoubleAnimation {
                From = new double?(height),
                To = new double?(num2),
                Duration = new Duration(TimeSpan.FromMilliseconds(100.0)),
                AutoReverse = true
            };
            image.BeginAnimation(FrameworkElement.HeightProperty, animation);
            animation = new DoubleAnimation {
                From = new double?(width),
                To = new double?(num4),
                Duration = new Duration(TimeSpan.FromMilliseconds(100.0)),
                AutoReverse = true
            };
            content.BeginAnimation(FrameworkElement.WidthProperty, animation);
            animation = new DoubleAnimation {
                From = new double?(width),
                To = new double?(num4),
                Duration = new Duration(TimeSpan.FromMilliseconds(100.0)),
                AutoReverse = true
            };
            image.BeginAnimation(FrameworkElement.WidthProperty, animation);
        }

        public bool SetBFSetting(int BFSetting)
        {
            bool flag = false;
            object obj2 = this._pagesList["VoiceSettingsPage"];
            if (obj2 != null)
            {
                VoiceEffectsPage page = obj2 as VoiceEffectsPage;
                if (page != null)
                {
                    flag = page.SetBeamForming(BFSetting);
                }
            }
            return flag;
        }

        public void ShowJackPanel()
        {
            if (!this._jackConfigVisible)
            {
                this._jackConfigVisible = true;
                DoubleAnimation animation = new DoubleAnimation {
                    From = 0.0,
                    To = 1.0,
                    Duration = new Duration(TimeSpan.FromSeconds(1.0)),
                    AutoReverse = false
                };
                this._jackReassignment.BeginAnimation(UIElement.OpacityProperty, animation);
                this._jackReassignment.Visibility = Visibility.Visible;
            }
        }

        public void ShowLastActivePage()
        {
            App current = System.Windows.Application.Current as App;
            if (current != null)
            {
                this.ShowPage(current.ActivePage);
            }
        }

        private void ShowPage(string name)
        {
            this.StartVolumeLevel();
            this._currentlyActivePage = name;
            this.SaveActivePage();
            this._controlFrame.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Stretch;
            this._controlFrame.VerticalContentAlignment = VerticalAlignment.Stretch;
            if (this._controlFrame.Content != null)
            {
                ((System.Windows.Controls.UserControl) this._controlFrame.Content).Visibility = Visibility.Hidden;
            }
            this._controlFrame.Content = this._pagesList[name];
            ((System.Windows.Controls.UserControl) this._pagesList[name]).Visibility = Visibility.Visible;
            ((System.Windows.Controls.UserControl) this._pagesList[name]).Focusable = true;
            ((System.Windows.Controls.UserControl) this._pagesList[name]).Focus();
        }

        private void speakerSetup_Click(object sender, RoutedEventArgs e)
        {
            this.ShowPage("SpeakerSetup");
        }

        public void StartVolumeLevel()
        {
            App current = System.Windows.Application.Current as App;
            if (!current.IsDemoMode)
            {
                try
                {
                    object obj2 = this._pagesList["MainAudioPage"];
                    if ((obj2 != null) && !((MainAudioControlPage) obj2)._volumeLevelMeter.IsMeterDisabled)
                    {
                        ((MainAudioControlPage) obj2)._volumeLevelMeter.Simulate = true;
                    }
                }
                catch (Exception exception)
                {
                    SmartAudioLog.Log("Failed to start the Volume Meter", Severity.WARNING, exception);
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), DebuggerNonUserCode]
        void IComponentConnector.Connect(int connectionId, object target)
        {
            switch (connectionId)
            {
                case 1:
                    this._mainContainer = (MainWindow) target;
                    this._mainContainer.MouseLeftButtonDown += new MouseButtonEventHandler(this.Window1_MouseLeftButtonDown);
                    this._mainContainer.Loaded += new RoutedEventHandler(this._mainContainer_Loaded);
                    return;

                case 2:
                    this._outerGrid = (Grid) target;
                    return;

                case 3:
                    this._background = (Border) target;
                    return;

                case 4:
                    this._containerGrid = (Grid) target;
                    return;

                case 5:
                    this._mainTabButtonBar = (System.Windows.Controls.ListBox) target;
                    this._mainTabButtonBar.SelectionChanged += new SelectionChangedEventHandler(this._tabButtonBarStatic_SelectionChanged);
                    return;

                case 6:
                    ((Image) target).MouseEnter += new System.Windows.Input.MouseEventHandler(this._mainAudioBtn_MouseEnter);
                    ((Image) target).MouseLeave += new System.Windows.Input.MouseEventHandler(this._mainAudioBtn_MouseLeave);
                    return;

                case 7:
                    ((Image) target).MouseEnter += new System.Windows.Input.MouseEventHandler(this._mainAudioBtn_MouseEnter);
                    ((Image) target).MouseLeave += new System.Windows.Input.MouseEventHandler(this._mainAudioBtn_MouseLeave);
                    return;

                case 8:
                    ((Image) target).MouseEnter += new System.Windows.Input.MouseEventHandler(this._mainAudioBtn_MouseEnter);
                    ((Image) target).MouseLeave += new System.Windows.Input.MouseEventHandler(this._mainAudioBtn_MouseLeave);
                    return;

                case 9:
                    ((Image) target).MouseEnter += new System.Windows.Input.MouseEventHandler(this._mainAudioBtn_MouseEnter);
                    ((Image) target).MouseLeave += new System.Windows.Input.MouseEventHandler(this._mainAudioBtn_MouseLeave);
                    return;

                case 10:
                    ((Image) target).MouseEnter += new System.Windows.Input.MouseEventHandler(this._mainAudioBtn_MouseEnter);
                    ((Image) target).MouseLeave += new System.Windows.Input.MouseEventHandler(this._mainAudioBtn_MouseLeave);
                    return;

                case 11:
                    ((Image) target).MouseEnter += new System.Windows.Input.MouseEventHandler(this._mainAudioBtn_MouseEnter);
                    ((Image) target).MouseLeave += new System.Windows.Input.MouseEventHandler(this._mainAudioBtn_MouseLeave);
                    return;

                case 12:
                    ((Image) target).MouseEnter += new System.Windows.Input.MouseEventHandler(this._mainAudioBtn_MouseEnter);
                    ((Image) target).MouseLeave += new System.Windows.Input.MouseEventHandler(this._mainAudioBtn_MouseLeave);
                    return;

                case 13:
                    this._toolbarSeperator = (Image) target;
                    return;

                case 14:
                    this._tabButtonBarStatic = (System.Windows.Controls.ListBox) target;
                    return;

                case 15:
                    this._maxxAudio = (System.Windows.Controls.Button) target;
                    this._maxxAudio.Click += new RoutedEventHandler(this._maxxAudio_Click);
                    return;

                case 0x10:
                    this._dolby = (System.Windows.Controls.Button) target;
                    this._dolby.Click += new RoutedEventHandler(this._dolbyBtn_Click);
                    return;

                case 0x11:
                    this._SRS = (System.Windows.Controls.Button) target;
                    this._SRS.Click += new RoutedEventHandler(this._SRS_Click);
                    return;

                case 0x12:
                    this._retask_jack = (System.Windows.Controls.Button) target;
                    this._retask_jack.Click += new RoutedEventHandler(this._retask_jack_Click);
                    return;

                case 0x13:
                    this._titleBar = (Border) target;
                    return;

                case 20:
                    this._captionCol = (ColumnDefinition) target;
                    return;

                case 0x15:
                    this._toolBarCol = (ColumnDefinition) target;
                    return;

                case 0x16:
                    this._toolBarPanel = (StackPanel) target;
                    return;

                case 0x17:
                    this._taskBarMenu = (SplitButton) target;
                    return;

                case 0x18:
                    this._menuBar = (Image) target;
                    return;

                case 0x19:
                    this._helpButton = (System.Windows.Controls.Button) target;
                    this._helpButton.Click += new RoutedEventHandler(this._helpButton_Click);
                    return;

                case 0x1a:
                    this._minimizeButton = (System.Windows.Controls.Button) target;
                    this._minimizeButton.Click += new RoutedEventHandler(this._minimizeBtn_Click);
                    return;

                case 0x1b:
                    this._closeBtn = (System.Windows.Controls.Button) target;
                    this._closeBtn.Click += new RoutedEventHandler(this._closeBtn_Click);
                    return;

                case 0x1c:
                    this._bigToolbar = (StackPanel) target;
                    return;

                case 0x1d:
                    this._taskBarMenuBig = (SplitButton) target;
                    return;

                case 30:
                    this._helpButtonLarge = (System.Windows.Controls.Button) target;
                    this._helpButtonLarge.Click += new RoutedEventHandler(this._helpButton_Click);
                    return;

                case 0x1f:
                    this._minimizeButtonLarge = (System.Windows.Controls.Button) target;
                    this._minimizeButtonLarge.Click += new RoutedEventHandler(this._minimizeBtn_Click);
                    return;

                case 0x20:
                    this._closeBtnLarge = (System.Windows.Controls.Button) target;
                    this._closeBtnLarge.Click += new RoutedEventHandler(this._closeBtn_Click);
                    return;

                case 0x21:
                    this._controlFrame = (ContentControl) target;
                    return;
            }
            this._contentLoaded = true;
        }

        private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
        {
            double primaryScreenWidth = SystemParameters.PrimaryScreenWidth;
            double primaryScreenHeight = SystemParameters.PrimaryScreenHeight;
            if ((primaryScreenWidth != this._primaryScreenWidth) || (primaryScreenHeight != this._primaryScreenHeight))
            {
                App current = System.Windows.Application.Current as App;
                if (current.Settings.INISettings.IsScrollBarsEnabled)
                {
                    this._currentScaleResolution = current.AdjustWindowSize(null, this, (double) this.DefaultWindowWidth, (double) this.DefaultWindowHeight, this._currentScaleResolution, true);
                }
                else
                {
                    this._currentScaleResolution = current.AdjustWindowSize(this._outerGrid, this, (double) this.DefaultWindowWidth, (double) this.DefaultWindowHeight, this._currentScaleResolution, true);
                }
            }
            this._primaryScreenWidth = SystemParameters.PrimaryScreenWidth;
            this._primaryScreenHeight = SystemParameters.PrimaryScreenHeight;
        }

        private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if ((e.Category == UserPreferenceCategory.Locale) && SALocalization.LocalInfoExits(CultureInfo.CurrentUICulture.Name))
            {
                (System.Windows.Application.Current as App).SetLocale(CultureInfo.CurrentUICulture.Name, true);
            }
        }

        private void TabButtonBar_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < this._mainTabButtonBar.Items.Count; i++)
            {
                if (((System.Windows.Controls.Button) ((ListBoxItem) this._mainTabButtonBar.Items[i]).Content) == e.OriginalSource)
                {
                    ((ListBoxItem) this._mainTabButtonBar.Items[i]).IsSelected = true;
                }
            }
            this._tabButtonBarStatic.SelectedIndex = -1;
        }

        private void TabStaticButtonBar_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < this._tabButtonBarStatic.Items.Count; i++)
            {
                if (((System.Windows.Controls.Button) ((ListBoxItem) this._tabButtonBarStatic.Items[i]).Content) == e.OriginalSource)
                {
                    ((ListBoxItem) this._tabButtonBarStatic.Items[i]).IsSelected = true;
                }
            }
            this._mainTabButtonBar.SelectedIndex = -1;
        }

        private void UpdateGUIForOneKeyFeature()
        {
            App current = System.Windows.Application.Current as App;
            if (!current.IsDemoMode)
            {
                if (current.Settings.INISettings.EQPageEnabled)
                {
                    CxOneKeyMode currentOneKeyMode = current.AudioFactory.GetCurrentOneKeyMode();
                    this.RefreshPages((currentOneKeyMode == CxOneKeyMode.UNKNOWN_MODE) ? CxOneKeyMode.NORMAL_MODE : currentOneKeyMode);
                }
                else
                {
                    this.RefreshPages(CxOneKeyMode.UNKNOWN_MODE);
                }
            }
        }

        public void UpdateJackPorts(int jack, int selectedJack)
        {
            this._jackReassignment.UpdateJackPorts(jack, selectedJack);
        }

        private void Window1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point position = e.GetPosition(this._titleBar);
            Rect rect = new Rect(0.0, 0.0, this._titleBar.ActualWidth, this._titleBar.ActualHeight - 2.0);
            if (((position.X >= rect.X) && (position.X <= rect.Right)) && ((position.Y >= rect.Y) && (position.Y <= rect.Bottom)))
            {
                base.DragMove();
            }
        }

        private static IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) => 
            IntPtr.Zero;

        public CxHDAudioFactory AudioFactory
        {
            get
            {
                App current = System.Windows.Application.Current as App;
                return current.AudioFactory;
            }
            set
            {
            }
        }

        public string FriendlyName =>
            Resources.SA_About;

        private bool IsDolbyEnabled
        {
            get
            {
                bool flag = false;
                try
                {
                    App current = System.Windows.Application.Current as App;
                    bool flag2 = current.AudioFactory.IsDolbyAppExisted();
                    if (!flag2)
                    {
                        SmartAudioLog.Log("MainWindow::IsDolbyAppExisted() return false.", new object[] { Severity.WARNING });
                    }
                    bool isDolbyEnabled = current.AudioFactory.IsDolbyEnabled;
                    if (!isDolbyEnabled)
                    {
                        SmartAudioLog.Log("MainWindow::isDolbyEnabled return false.", new object[] { Severity.WARNING });
                    }
                    flag = (flag2 && isDolbyEnabled) && current.Settings.INISettings.DolbyEnabled;
                }
                catch (Exception exception)
                {
                    SmartAudioLog.Log("MainWindow::IsDolbyEnabled() ", Severity.FATALERROR, exception);
                }
                return flag;
            }
        }

        private bool IsJackRetaskingEnabled
        {
            get
            {
                App current = System.Windows.Application.Current as App;
                return (current.Settings.INISettings.IsJackRetaskingEnabled && current.IsJackRetaskable);
            }
        }

        private bool IsSRSEnabled
        {
            get
            {
                bool flag = false;
                try
                {
                    App current = System.Windows.Application.Current as App;
                    bool flag2 = current.AudioFactory.IsSRSAppExisted();
                    if (!flag2)
                    {
                        SmartAudioLog.Log("MainWindow::IsSRSAppExisted() return false.", new object[] { Severity.WARNING });
                    }
                    bool sRSSupported = current.AudioFactory.DeviceIOConfig.GetSRSSupported();
                    if (!sRSSupported)
                    {
                        SmartAudioLog.Log("MainWindow::GetSRSSupported() return false.", new object[] { Severity.WARNING });
                    }
                    flag = (flag2 && sRSSupported) && current.Settings.INISettings.SRSEnabled;
                }
                catch (Exception exception)
                {
                    SmartAudioLog.Log("MainWindow::IsSRSEnabled() ", Severity.FATALERROR, exception);
                }
                return flag;
            }
        }

        public delegate void OnAltF4CloseHandler();

        public delegate void OnRefresh_EQ_VOIP_Pages();

        private delegate void RefreshPagesForMOdeChange(CxOneKeyMode newMode);
    }
}

