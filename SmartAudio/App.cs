namespace SmartAudio
{
    using CxHDAudioAPILib;
    using SmartAudio.Core;
    using SmartAudio.Properties;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Resources;
    using System.Security.Permissions;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Threading;

    public class App : Application
    {
        private CxHDAudioFactoryClass _audioFactory;
        private List<LocaleInfo> _availableLanguages;
        private string _currentAtivePage = "MainAudioPage";
        private string _currentLocale = "en-US";
        private string _currentOSCulture = "en-US";
        private string _defaultLocale = "en-US";
        private bool _demoMode;
        private string _HDAudioAPIVersion;
        public bool _isJackConfigurationChangedS3_S4;
        private bool _isMultiChannelSupported;
        private bool _isResumeFromS3_S4;
        private bool _isRunningInBackground;
        private bool _isRunningMinimized;
        private bool _isRunningSliently;
        private SmartAudio.JackRetaskingPopup _jackRetaskingPopup;
        private int _maxNumOfSupportedChannels;
        private SmartAudioSettings _settings = SmartAudioSettings.CreateSettings();
        private List<SkinDescriptor> _skins;
        private CxHDAudioEndPoint _SPAudioEndPoint;
        private CxSpeakerConfigType _speakerConfigType;
        private string _SPToolTip;
        private SmartAudioStorage _storage = SmartAudioStorage.CreateStorage();
        private SmartAudio.SystemTray _systemTray;
        private const string ApplicationName = "SMARTAUDIO.EXE";
        private double currentScreenHeight;
        private double currentScreenWidth;
        private string DefaultSkinGUID = "7DD902B1-50E4-4ec4-AFD2-E349DA0FE1CF";
        public OnJackAssignmentChanged OnJackChanged;
        public OnNewSkinApplied OnNewStyleApplied;
        public OnRenumerated OnRenumeratedEvent;
        public SmartAudio.OnSessionSwitch OnSessionSwitch;
        public SmartAudio.OnTaskbarCreated OnTaskbarCreated;
        public static App theCurrentApp;

        public App()
        {
            theCurrentApp = this;
        }

        private void _audioFactory_OnActivateMainWindow()
        {
            base.MainWindow.Activate();
        }

        private void _audioFactory_OnDriverUnload(double newValue)
        {
            base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new ShutdownApplication(this.OnShutdownApplication));
        }

        private void _audioFactory_OnSessionSwitch()
        {
            SmartAudioLog.Log("_audioFactory_OnSessionSwitch: session switch detected", new object[] { Severity.INFORMATION });
            this._audioFactory.remove_OnSessionSwitch(new _ICxHDAudioFactoryEvents_OnSessionSwitchEventHandler(this._audioFactory_OnSessionSwitch));
            this.loadAndInitAllParams();
            this._audioFactory.add_OnSessionSwitch(new _ICxHDAudioFactoryEvents_OnSessionSwitchEventHandler(this._audioFactory_OnSessionSwitch));
        }

        private void _audioFactory_OnTaskbarCreated()
        {
            SmartAudioLog.Log("OnTaskbarCreated happened", new object[] { Severity.INFORMATION });
            App current = Application.Current as App;
            if (current.IsRunningInBackground)
            {
                this._systemTray.AddTrayIcon();
            }
        }

        private void _audioFactory_OnUninstallClose()
        {
            base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new ShutdownApplication(this.OnShutdownApplication));
        }

        public void Activate()
        {
            if (((this._systemTray != null) && this.Settings.RunOnSystemTray) && ((base.MainWindow == null) || (base.MainWindow.Visibility != Visibility.Visible)))
            {
                this._systemTray.ShowMainWindow();
            }
            else if (base.MainWindow != null)
            {
                base.MainWindow.Visibility = Visibility.Visible;
                base.MainWindow.WindowState = WindowState.Normal;
                base.MainWindow.Activate();
            }
            if ((this._jackRetaskingPopup != null) && (this._jackRetaskingPopup.Visibility == Visibility.Visible))
            {
                this._jackRetaskingPopup.Visibility = Visibility.Hidden;
            }
        }

        public void AddSkinAssembly(Assembly assembly, string skinFileName)
        {
            if (assembly != null)
            {
                object[] customAttributes = assembly.GetCustomAttributes(typeof(RegisterSkinAssembly), false);
                if ((customAttributes != null) && (customAttributes.Length != 0))
                {
                    SkinDescriptor item = new SkinDescriptor((RegisterSkinAssembly) customAttributes[0], skinFileName);
                    this._skins.Add(item);
                }
            }
        }

        public double AdjustWindowSize(Grid grid, Window window, double defaultWidth, double defaultHeight, double previousRatio, bool resolutionChanged)
        {
            double scaleX = 1.0;
            double width = window.Width;
            double height = window.Height;
            MainWindow window2 = window as MainWindow;
            try
            {
                double primaryScreenWidth = SystemParameters.PrimaryScreenWidth;
                double primaryScreenHeight = SystemParameters.PrimaryScreenHeight;
                try
                {
                    if (resolutionChanged)
                    {
                        new StatusWindow(500).ShowDialog();
                    }
                }
                catch (Exception)
                {
                }
                SmartAudioLog.Log(string.Concat(new object[] { "App::AdjustWindowSize: PrimaryScreenWidth  = ", primaryScreenWidth, " PrimaryScreenHeight = ", primaryScreenHeight }), new object[] { Severity.INFORMATION });
                App current = Application.Current as App;
                if ((current.currentScreenWidth == 0.0) || (current.currentScreenHeight == 0.0))
                {
                    window.Width = defaultWidth;
                    window.Height = defaultHeight;
                }
                double num6 = defaultWidth;
                double num7 = defaultHeight;
                double num8 = 0.8;
                double num9 = defaultWidth / num8;
                double num10 = defaultHeight / num8;
                SmartAudioLog.Log(string.Concat(new object[] { "App::AdjustWindowSize: DesiredWidth  = ", num9, " DesiredHeight = ", num10 }), new object[] { Severity.INFORMATION });
                if ((primaryScreenWidth < num9) || (primaryScreenHeight < num10))
                {
                    double num11 = primaryScreenWidth / num6;
                    double num12 = primaryScreenHeight / num7;
                    scaleX = (num11 < num12) ? num11 : num12;
                    scaleX *= num8;
                    if (window2 != null)
                    {
                        SmartAudioLog.Log("App::AdjustWindowSize: the screen ratio " + scaleX + " is applied to the window. ", new object[] { Severity.INFORMATION });
                    }
                }
                if ((1.0 - scaleX) > 1E-06)
                {
                    window.Width = num6;
                    window.Height = num7;
                    window.UpdateLayout();
                    if (grid != null)
                    {
                        grid.LayoutTransform = null;
                    }
                    window.LayoutTransform = null;
                    window.UpdateLayout();
                    window.LayoutTransform = new ScaleTransform(scaleX, scaleX);
                    width = num6 * scaleX;
                    height = num7 * scaleX;
                    window.Width = width;
                    window.Height = height;
                    window.UpdateLayout();
                    ScaleTransform transform = null;
                    if (grid != null)
                    {
                        transform = new ScaleTransform(scaleX, scaleX);
                        grid.LayoutTransform = transform;
                        grid.UpdateLayout();
                    }
                    window.UpdateLayout();
                    SmartAudioLog.Log(string.Concat(new object[] { "[Shriking] New Size: ***Resizing Window*** window.Width after  = ", window.Width, " window.Height after = ", window.Height, "Scale Transform ", (transform != null) ? string.Concat(new object[] { "Scale-X ", transform.ScaleX, " Scale-y ", transform.ScaleY }) : "" }), new object[] { Severity.INFORMATION });
                }
                else if ((previousRatio < 1.0) || ((num9 >= defaultWidth) && (num10 >= defaultHeight)))
                {
                    if (base.MainWindow != null)
                    {
                        SmartAudioLog.Log(string.Concat(new object[] { "[Restore]***Restoring to original Window Size*** window.Width ***before***  = ", window.Width, " window.Height before = ", window.Height }), new object[] { Severity.INFORMATION });
                    }
                    window.Width = width = num6;
                    window.Height = height = num7;
                    window.UpdateLayout();
                    scaleX = 1.0;
                    if (grid != null)
                    {
                        grid.LayoutTransform = null;
                    }
                    window.LayoutTransform = null;
                    window.UpdateLayout();
                    if (base.MainWindow != null)
                    {
                        SmartAudioLog.Log(string.Concat(new object[] { "[Restore]***Restoring Original Window Size*** window.Width ***After*** = ", window.Width, " window.Height before = ", window.Height }), new object[] { Severity.INFORMATION });
                    }
                }
                Workaround110052078705416.InvokeWorkaround110052078705416(window);
                double num13 = (SystemParameters.PrimaryScreenWidth - width) / 2.0;
                double num14 = (SystemParameters.PrimaryScreenHeight - height) / 2.0;
                window.Left = (num13 > 0.0) ? num13 : window.Left;
                window.Top = (num14 > 0.0) ? num14 : window.Top;
                current.currentScreenWidth = primaryScreenWidth;
                current.currentScreenHeight = primaryScreenHeight;
                SmartAudioLog.Log(string.Concat(new object[] { "[Restore]*** Primary Width=", primaryScreenWidth, " Primary Height ", primaryScreenHeight, " new Width", width, " new Height", height }));
                SmartAudioLog.Log(string.Concat(new object[] { "[Restore]*** New Window Location new Left=", num13, " new Top ", num14, " Window left ", window.Left, " Window Top ", window.Top }));
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("App::AdjustWindowSize", Severity.WARNING, exception);
            }
            return scaleX;
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (false)
            {
                MessageBox.Show(Resources.SA_SARuntimeError + ":" + e.Exception.Message + "\n" + Resources.SA_ApplicationMustExit, "SmartAudio", MessageBoxButton.OK, MessageBoxImage.Hand);
                SmartAudioLog.Log("Application Error", Severity.FATALERROR, e.Exception);
                base.Shutdown(-1);
            }
            e.Handled = true;
        }

        private void ApplyAdditionalSkin(string skinFileName)
        {
            try
            {
                Collection<ResourceDictionary> mergedDictionaries = base.Resources.MergedDictionaries;
                foreach (ResourceDictionary dictionary in this.LoadResources(skinFileName))
                {
                    try
                    {
                        mergedDictionaries.Add(dictionary);
                    }
                    catch (ArgumentException exception)
                    {
                        SmartAudioLog.Log("mergedDicts.Add(resource) threw ArgumentException, key already exists", Severity.FATALERROR, exception);
                        return;
                    }
                    catch (InvalidOperationException exception2)
                    {
                        SmartAudioLog.Log("mergedDicts.Add(resource) threw InvalidOperationException, resource is locked", Severity.FATALERROR, exception2);
                        return;
                    }
                    catch (Exception exception3)
                    {
                        SmartAudioLog.Log("App.LoadSkin", Severity.FATALERROR, exception3);
                        MessageBox.Show("mergedDicts.Add(resource) threw an Exception", "SmartAudio");
                        this._audioFactory = null;
                        return;
                    }
                }
                if (this.OnNewStyleApplied != null)
                {
                    SmartAudioLog.Log("App.LoadSkin: about to apply new style 2", new object[] { Severity.INFORMATION });
                    this.OnNewStyleApplied();
                }
            }
            catch (Exception exception4)
            {
                SmartAudioLog.Log("App.LoadSkin", Severity.FATALERROR, exception4);
                MessageBox.Show(Resources.SA_FailedToInitialize, "SmartAudio");
                this._audioFactory = null;
            }
        }

        private void ApplyDefaultSkin()
        {
            ResourceDictionary item = Application.LoadComponent(new Uri(@".\DefaultIncludes.xaml", UriKind.Relative)) as ResourceDictionary;
            Collection<ResourceDictionary> mergedDictionaries = base.Resources.MergedDictionaries;
            try
            {
                if (mergedDictionaries.Count > 0)
                {
                    mergedDictionaries.Clear();
                }
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("Error in function LoadSkin " + exception.Message);
            }
            try
            {
                mergedDictionaries.Add(item);
            }
            catch (ArgumentException exception2)
            {
                SmartAudioLog.Log("mergedDicts.Add(defaults) threw ArgumentException, key already exists", Severity.FATALERROR, exception2);
            }
            catch (InvalidOperationException exception3)
            {
                SmartAudioLog.Log("mergedDicts.Add(defaults) threw InvalidOperationException, resource is locked", Severity.FATALERROR, exception3);
            }
            catch (Exception exception4)
            {
                SmartAudioLog.Log("App.LoadSkin", Severity.FATALERROR, exception4);
                MessageBox.Show("mergedDicts.Add(defaults) threw an Exception", "SmartAudio");
                this._audioFactory = null;
            }
        }

        public void ApplySkin()
        {
            Application current = Application.Current;
            this.ApplyDefaultSkin();
            foreach (SkinDescriptor descriptor in this.GetSortedSkinList())
            {
                SmartAudioLog.Log("*** Skin Applied  " + descriptor.ToString());
                FileInfo info = new FileInfo(descriptor.AssemblyName);
                if ((info != null) && (info.Name.ToUpper().CompareTo("SMARTAUDIO.EXE") != 0))
                {
                    this.ApplyAdditionalSkin(descriptor.AssemblyName);
                }
            }
        }

        public void ApplySkin(Uri skinDictionaryUri)
        {
            ResourceDictionary item = Application.LoadComponent(skinDictionaryUri) as ResourceDictionary;
            ResourceDictionary dictionary2 = Application.LoadComponent(new Uri(@".\DefaultIncludes.xaml", UriKind.Relative)) as ResourceDictionary;
            Collection<ResourceDictionary> mergedDictionaries = base.Resources.MergedDictionaries;
            if (mergedDictionaries.Count > 0)
            {
                mergedDictionaries.Clear();
            }
            mergedDictionaries.Add(dictionary2);
            mergedDictionaries.Add(item);
        }

        public bool ChangeLocale(string language)
        {
            try
            {
                this._currentLocale = language;
                CultureInfo info = new CultureInfo(language);
                Resources.Culture = info;
                Thread.CurrentThread.CurrentCulture = info;
                Thread.CurrentThread.CurrentUICulture = info;
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("App.ChangeLocale", Severity.FATALERROR, exception);
                return false;
            }
            return true;
        }

        public void CloseSystemTray()
        {
            if (this._systemTray != null)
            {
                this._systemTray.CloseTrayIcon();
            }
        }

        public void CollectVersionInformation()
        {
            this.HDAudioAPIVersion = "0.0.0.0";
            try
            {
                FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(Path.Combine(Environment.CurrentDirectory, "CXHDAudioAPI.dll"));
                this.HDAudioAPIVersion = versionInfo.FileVersion;
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("App:CollectVersionInformation - GetVersionInfo threw an exception  ", Severity.FATALERROR, exception);
            }
        }

        public void CreatMainWindow()
        {
            App current = Application.Current as App;
            MainWindow window = new MainWindow();
            base.MainWindow = window;
            if (this._systemTray == null)
            {
                this._systemTray = new SmartAudio.SystemTray((MainWindow) base.MainWindow, this._audioFactory);
            }
            this.OnNewStyleApplied = (OnNewSkinApplied) Delegate.Combine(this.OnNewStyleApplied, new OnNewSkinApplied(window.OnNewSkinApplied));
            if (current.IsRunningInBackground)
            {
                window.ShowInTaskbar = false;
                window.Visibility = Visibility.Hidden;
            }
            else if (current.IsRunningSliently)
            {
                window.ShowInTaskbar = false;
                window.Visibility = Visibility.Hidden;
            }
            if (current.Settings.INISettings.IsLayeredWindowEnabled)
            {
                window.AllowsTransparency = false;
                window.WindowStyle = WindowStyle.ToolWindow;
                window.Title = "";
            }
            window.Show();
            if (current.IsRunningSliently && !current.Settings.RunOnSystemTray)
            {
                this._systemTray.CloseTrayIcon();
            }
        }

        public void EnumerateLanguages()
        {
            this._availableLanguages.Clear();
            FileInfo info = new FileInfo(base.GetType().Assembly.Location);
            DirectoryInfo info2 = new DirectoryInfo(info.DirectoryName);
            if (info2 != null)
            {
                foreach (DirectoryInfo info3 in info2.GetDirectories())
                {
                    try
                    {
                        if ((info3.Name.ToUpper() != "ZH-CN") && (info3.Name.ToUpper() != "ZH-TW"))
                        {
                            LocaleInfo item = SALocalization.LocalizationTable[info3.Name.ToUpper()];
                            if (((item != null) && SALocalization.IsValidLocaleFolder(info3.Name)) && this.IsLocaleInstalled(info3.Name))
                            {
                                this._availableLanguages.Add(item);
                                SmartAudioLog.Log("EnumerateLanguages : Locale " + item.LocaleName + " is added to the list of supported languages", new object[] { Severity.INFORMATION });
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        SmartAudioLog.Log("EnumerateLanguages : Locale " + info3.Name.ToUpper() + " not found in LocalizationTable", Severity.FATALERROR, exception);
                    }
                }
            }
        }

        public void EnumerateSkins()
        {
            this._skins = new List<SkinDescriptor>();
            FileInfo[] files = new DirectoryInfo(".").GetFiles("*.dll");
            Application current = Application.Current;
            this._skins.Clear();
            this.AddSkinAssembly(base.GetType().Assembly, base.GetType().Assembly.Location);
            foreach (FileInfo info2 in files)
            {
                try
                {
                    Assembly assembly = null;
                    try
                    {
                        assembly = Assembly.LoadFrom(info2.Name);
                    }
                    catch
                    {
                        continue;
                    }
                    this.AddSkinAssembly(assembly, info2.Name);
                }
                catch (Exception exception)
                {
                    SmartAudioLog.Log("App.EnumerateSkins", Severity.FATALERROR, exception);
                }
            }
        }

        ~App()
        {
            this._settings.Save();
        }

        private Guid GetDefaultSkin()
        {
            foreach (SkinDescriptor descriptor in this._skins)
            {
                if (descriptor.SelectAsDefault)
                {
                    return descriptor.UniqueID;
                }
            }
            return Guid.Empty;
        }

        private string GetSkinAssemblyName(Guid guid)
        {
            foreach (SkinDescriptor descriptor in this._skins)
            {
                if (descriptor.UniqueID == guid)
                {
                    return descriptor.AssemblyName;
                }
            }
            return string.Empty;
        }

        private List<Stream> GetSkinBamlStreams(AssemblyName skinAssemblyName)
        {
            List<Stream> list = new List<Stream>();
            Assembly assembly = Assembly.Load(skinAssemblyName);
            foreach (string str in assembly.GetManifestResourceNames())
            {
                if (assembly.GetManifestResourceInfo(str).ResourceLocation != ResourceLocation.ContainedInAnotherAssembly)
                {
                    using (ResourceReader reader = new ResourceReader(assembly.GetManifestResourceStream(str)))
                    {
                        foreach (DictionaryEntry entry in reader)
                        {
                            if (this.IsRelevantResource(entry))
                            {
                                list.Add(entry.Value as Stream);
                            }
                        }
                    }
                }
            }
            return list;
        }

        public List<SkinDescriptor> GetSortedSkinList()
        {
            App current = Application.Current as App;
            bool flag = false;
            List<SkinDescriptor> list = new List<SkinDescriptor>();
            SortedList<string, SkinDescriptor> list2 = new SortedList<string, SkinDescriptor>();
            foreach (SkinDescriptor descriptor in this._skins)
            {
                FileInfo info = new FileInfo(descriptor.AssemblyName);
                if ((info != null) && ((info == null) || (info.Name.ToUpper().CompareTo("SMARTAUDIO.EXE") != 0)))
                {
                    try
                    {
                        if (!flag && (((current.Settings.SelectedSkin != Guid.Empty) && (current.Settings.SelectedSkin == descriptor.UniqueID)) || ((descriptor.SkinPriority == SkinPriority.FirstPriority) && descriptor.IsDefault)))
                        {
                            descriptor.SelectAsDefault = true;
                            flag = true;
                            if (list.Count == 0)
                            {
                                list.Add(descriptor);
                            }
                            else
                            {
                                list.IndexOf(descriptor, 0);
                            }
                        }
                        else
                        {
                            descriptor.SelectAsDefault = false;
                            list2.Add(info.Name, descriptor);
                        }
                    }
                    catch (Exception exception)
                    {
                        SmartAudioLog.Log("Error in GetSortedSkinList() ", Severity.INFORMATION, exception);
                    }
                }
            }
            if (!flag)
            {
                foreach (SkinDescriptor descriptor2 in this._skins)
                {
                    FileInfo info2 = new FileInfo(descriptor2.AssemblyName);
                    if ((info2 != null) && ((info2 == null) || (info2.Name.ToUpper().CompareTo("SMARTAUDIO.EXE") != 0)))
                    {
                        try
                        {
                            if ((descriptor2.SkinPriority == SkinPriority.SecondPriority) && descriptor2.IsDefault)
                            {
                                descriptor2.SelectAsDefault = true;
                                flag = true;
                                list.IndexOf(descriptor2, 0);
                            }
                            else
                            {
                                descriptor2.SelectAsDefault = false;
                                list2.Add(info2.Name, descriptor2);
                            }
                        }
                        catch (Exception exception2)
                        {
                            SmartAudioLog.Log("Error in GetSortedSkinList() ", Severity.INFORMATION, exception2);
                        }
                    }
                }
            }
            foreach (KeyValuePair<string, SkinDescriptor> pair in list2)
            {
                list.Add(pair.Value);
            }
            return list;
        }

        public string GetSpecificLocale(string localeName)
        {
            string str = "";
            if (localeName == "zh-Hant")
            {
                str = "zh-CHT";
            }
            else if (localeName == "zh-Hans")
            {
                str = "zh-CHS";
            }
            else
            {
                return null;
            }
            foreach (CultureInfo info in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
            {
                if (info.TwoLetterISOLanguageName == "zh")
                {
                    CultureInfo info2 = new CultureInfo(info.Name, false);
                    if (this._audioFactory.get_IsLocaleInstalled(info2.LCID) && (info.Parent.Name == str))
                    {
                        return info.Name;
                    }
                }
            }
            return null;
        }

        public bool InitApplication(CxHDAudioFactoryClass audioFactory)
        {
            bool flag = false;
            if (audioFactory != null)
            {
                this._audioFactory = audioFactory;
                this.SetupUnhandledException();
                this._availableLanguages = new List<LocaleInfo>();
                this.loadAndInitAllParams();
                this._audioFactory.add_OnUninstallClose(new _ICxHDAudioFactoryEvents_OnUninstallCloseEventHandler(this._audioFactory_OnUninstallClose));
                this._audioFactory.add_OnSessionSwitch(new _ICxHDAudioFactoryEvents_OnSessionSwitchEventHandler(this._audioFactory_OnSessionSwitch));
                this._audioFactory.add_OnTaskbarCreated(new _ICxHDAudioFactoryEvents_OnTaskbarCreatedEventHandler(this._audioFactory_OnTaskbarCreated));
                this._audioFactory.add_OnDriverUnload(new _ICxHDAudioFactoryEvents_OnDriverUnloadEventHandler(this._audioFactory_OnDriverUnload));
                if ((this.Settings.INISettings.IsJackRetaskingEnabled && this.IsJackRetaskable) && (this._jackRetaskingPopup != null))
                {
                    this._jackRetaskingPopup.JackReassignment.JackPortChanged += new JackPortChangedHandler(this.JackReassignment_JackPortChanged);
                }
                flag = true;
            }
            SmartAudioLog.Log("App.InitApplication: returning value = " + flag, new object[] { Severity.INFORMATION });
            return flag;
        }

        public bool IsInstalledSpecificLocale(string localeName)
        {
            Application current = Application.Current;
            foreach (CultureInfo info in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
            {
                if (info.TwoLetterISOLanguageName == "zh")
                {
                    SmartAudioLog.Log("IsInstalledSpecificLocale: ci.Name = " + info.Name, new object[] { Severity.INFORMATION });
                    CultureInfo info2 = new CultureInfo(info.Name, false);
                    if (this._audioFactory.get_IsLocaleInstalled(info2.LCID))
                    {
                        SmartAudioLog.Log("IsInstalledSpecificLocale: _audioFactory.get_IsLocaleInstalled for " + info2.LCID + "returned true", new object[] { Severity.INFORMATION });
                        if (info.Parent.Name == localeName)
                        {
                            SmartAudioLog.Log("IsInstalledSpecificLocale: ci.Parent.Name = " + info.Parent.Name + "localeName = " + localeName + ". returning true.", new object[] { Severity.INFORMATION });
                            return true;
                        }
                        if (((info.Parent.Name == "zh-Hans") || (info.Parent.Name == "zh-CHS")) && (localeName == "zh-Hans"))
                        {
                            SmartAudioLog.Log("IsInstalledSpecificLocale: ci.Parent.Name = " + info.Parent.Name + "localeName = " + localeName + ". returning true.", new object[] { Severity.INFORMATION });
                            return true;
                        }
                        if (((info.Parent.Name == "zh-Hant") || (info.Parent.Name == "zh-CHT")) && (localeName == "zh-Hant"))
                        {
                            SmartAudioLog.Log("IsInstalledSpecificLocale: ci.Parent.Name = " + info.Parent.Name + "localeName = " + localeName + ". returning true.", new object[] { Severity.INFORMATION });
                            return true;
                        }
                    }
                }
            }
            SmartAudioLog.Log("IsInstalledSpecificLocale: for localeName = " + localeName + " returning false.", new object[] { Severity.INFORMATION });
            return false;
        }

        public bool IsLocaleInstalled(string localeName)
        {
            if ((this._audioFactory.HostOperatingSystem != CxHostOperatingSystemType.WindowsXP) && (this._audioFactory.HostOperatingSystem != CxHostOperatingSystemType.WindowsXP64))
            {
                return true;
            }
            try
            {
                if (localeName.StartsWith("zh"))
                {
                    return this.IsInstalledSpecificLocale(localeName);
                }
                CultureInfo info = new CultureInfo(localeName, false);
                if (!this._audioFactory.get_IsLocaleInstalled(info.LCID))
                {
                    SmartAudioLog.Log("IsLocaleInstalled: get_IsLocaleInstalled returned false", new object[] { Severity.INFORMATION });
                    return false;
                }
                SmartAudioLog.Log("IsLocaleInstalled: returning true", new object[] { Severity.INFORMATION });
                return true;
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("Failed in IsLocaleInstalled.", Severity.WARNING, exception);
                return false;
            }
        }

        private bool IsRelevantResource(DictionaryEntry entry)
        {
            string key = entry.Key as string;
            Stream stream = entry.Value as Stream;
            return ((string.Compare(Path.GetExtension(key).ToLower(), ".baml", true) == 0) && (stream != null));
        }

        private void JackReassignment_JackPortChanged(int jack, int selectedJack)
        {
            MainWindow.theCurrent.UpdateJackPorts(jack, selectedJack);
        }

        private void loadAndInitAllParams()
        {
            try
            {
                this._settings.LoadInfSettings(this._audioFactory);
                this._settings.LoadINISettings();
                this._settings.InitRunInTaskBarSetting();
                this._settings.InitHPLimiterSetting();
                this._settings.InitHeadPhoneLimiterImage();
            }
            catch (Exception exception)
            {
                MessageBox.Show(Resources.SA_AudioFactoryCreateError, "SmartAudio");
                SmartAudioLog.Log("App.AudioFactory", Severity.FATALERROR, exception);
                this._audioFactory = null;
            }
            if (this.Settings.INISettings.IsJackRetaskingEnabled && this.IsJackRetaskable)
            {
                if (this._jackRetaskingPopup == null)
                {
                    this._jackRetaskingPopup = new SmartAudio.JackRetaskingPopup();
                }
                if (this._jackRetaskingPopup != null)
                {
                    this._jackRetaskingPopup.Visibility = Visibility.Hidden;
                    this._jackRetaskingPopup.HDAudioConfig = this._audioFactory.DeviceIOConfig;
                    this._jackRetaskingPopup.AudioFactory = this._audioFactory;
                }
                else
                {
                    SmartAudioLog.Log("App.AudioFactory: Could not create JackRetaskingPopup", new object[] { Severity.FATALERROR });
                }
            }
            else
            {
                this._jackRetaskingPopup = null;
            }
            this.EnumerateSkins();
            this.SelectDefaultSkin();
            this.EnumerateLanguages();
            this.ApplySkin();
            this._audioFactory.IsDemoMode = this._demoMode;
            if (this._storage.NumberOfSAIIActivations == 0)
            {
                this._storage.HPLimiterDefaultSetting = this._settings.HPLimiterSetting;
            }
            this._storage.NumberOfSAIIActivations++;
            this._storage.Save();
            this._isRunningMinimized = false;
            this.CollectVersionInformation();
        }

        protected List<ResourceDictionary> LoadResources(string assemblyPath)
        {
            AssemblyName assemblyName = AssemblyName.GetAssemblyName(assemblyPath);
            List<ResourceDictionary> list = new List<ResourceDictionary>();
            foreach (Stream stream in this.GetSkinBamlStreams(assemblyName))
            {
                ResourceDictionary item = BamlHelper.LoadBaml<ResourceDictionary>(stream);
                if (item != null)
                {
                    list.Add(item);
                }
            }
            return list;
        }

        public void LoadSkin(Guid skinInstanceGUID)
        {
            SmartAudioLog.Log("App.LoadSkin: Entering LoadSkin", new object[] { Severity.INFORMATION });
            if (skinInstanceGUID == Guid.Empty)
            {
                SmartAudioLog.Log("App.LoadSkin: skinInstanceGUID was = Guid.Empty", new object[] { Severity.INFORMATION });
            }
            else
            {
                try
                {
                    ResourceDictionary item = Application.LoadComponent(new Uri(@".\DefaultIncludes.xaml", UriKind.Relative)) as ResourceDictionary;
                    Collection<ResourceDictionary> mergedDictionaries = base.Resources.MergedDictionaries;
                    try
                    {
                        if (mergedDictionaries.Count > 0)
                        {
                            mergedDictionaries.Clear();
                        }
                    }
                    catch (Exception exception)
                    {
                        SmartAudioLog.Log("Error in function LoadSkin " + exception.Message);
                    }
                    SmartAudioLog.Log("App.LoadSkin: About to do try block for mergedDicts.Add(defaults)", new object[] { Severity.INFORMATION });
                    try
                    {
                        mergedDictionaries.Add(item);
                    }
                    catch (ArgumentException exception2)
                    {
                        SmartAudioLog.Log("mergedDicts.Add(defaults) threw ArgumentException, key already exists", Severity.FATALERROR, exception2);
                        return;
                    }
                    catch (InvalidOperationException exception3)
                    {
                        SmartAudioLog.Log("mergedDicts.Add(defaults) threw InvalidOperationException, resource is locked", Severity.FATALERROR, exception3);
                        return;
                    }
                    catch (Exception exception4)
                    {
                        SmartAudioLog.Log("App.LoadSkin", Severity.FATALERROR, exception4);
                        MessageBox.Show("mergedDicts.Add(defaults) threw an Exception", "SmartAudio");
                        this._audioFactory = null;
                        return;
                    }
                    if (skinInstanceGUID.ToString().ToUpper().CompareTo(this.DefaultSkinGUID.ToUpper()) == 0)
                    {
                        if (this.OnNewStyleApplied != null)
                        {
                            SmartAudioLog.Log("App.LoadSkin: about to apply new style 1", new object[] { Severity.INFORMATION });
                            this.OnNewStyleApplied();
                        }
                    }
                    else
                    {
                        string skinAssemblyName = this.GetSkinAssemblyName(skinInstanceGUID);
                        if (skinAssemblyName == string.Empty)
                        {
                            SmartAudioLog.Log("App.LoadSkin: askinFileName was empty", new object[] { Severity.INFORMATION });
                        }
                        else
                        {
                            foreach (ResourceDictionary dictionary2 in this.LoadResources(skinAssemblyName))
                            {
                                SmartAudioLog.Log("App.LoadSkin: About to do try block for mergedDicts.Add(resource)" + dictionary2, new object[] { Severity.INFORMATION });
                                try
                                {
                                    mergedDictionaries.Add(dictionary2);
                                }
                                catch (ArgumentException exception5)
                                {
                                    SmartAudioLog.Log("mergedDicts.Add(resource) threw ArgumentException, key already exists", Severity.FATALERROR, exception5);
                                    return;
                                }
                                catch (InvalidOperationException exception6)
                                {
                                    SmartAudioLog.Log("mergedDicts.Add(resource) threw InvalidOperationException, resource is locked", Severity.FATALERROR, exception6);
                                    return;
                                }
                                catch (Exception exception7)
                                {
                                    SmartAudioLog.Log("App.LoadSkin", Severity.FATALERROR, exception7);
                                    MessageBox.Show("mergedDicts.Add(resource) threw an Exception", "SmartAudio");
                                    this._audioFactory = null;
                                    return;
                                }
                            }
                            if (this.OnNewStyleApplied != null)
                            {
                                SmartAudioLog.Log("App.LoadSkin: about to apply new style 2", new object[] { Severity.INFORMATION });
                                this.OnNewStyleApplied();
                            }
                        }
                    }
                }
                catch (Exception exception8)
                {
                    SmartAudioLog.Log("App.LoadSkin", Severity.FATALERROR, exception8);
                    MessageBox.Show(Resources.SA_FailedToInitialize, "SmartAudio");
                    this._audioFactory = null;
                }
            }
        }

        private void OnShutdownApplication()
        {
            this.CloseSystemTray();
            base.Shutdown();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            if (this._audioFactory != null)
            {
                this.CreatMainWindow();
            }
        }

        public void RefreshEndPoints()
        {
            MainWindow mainWindow = base.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                ((MainAudioControlPage) mainWindow.GetPage("MainAudioPage")).ReenumerateEndPoints();
            }
        }

        public void ResetJackPopup()
        {
            if (this._jackRetaskingPopup != null)
            {
                this._jackRetaskingPopup.ResetJack();
            }
        }

        public bool ResetLocale()
        {
            App current = Application.Current as App;
            this._currentOSCulture = current.DefaultLocale;
            if (this._settings.OverrideCurrentLocale)
            {
                this.SetLocale(this._settings.SelectedLocale, false);
            }
            else if (SALocalization.IsValidLocaleFolder(this._currentOSCulture))
            {
                this.SetLocale(this._currentOSCulture, false);
            }
            else
            {
                this.SetLocale(CultureInfo.CurrentUICulture.Name, false);
            }
            return true;
        }

        public void RunOnSystemTray()
        {
            if (this._settings.RunOnSystemTray)
            {
                this._systemTray.Run();
            }
        }

        private void SAUnhandledExceptionEventHandler(object sender, UnhandledExceptionEventArgs args)
        {
            try
            {
                MessageBox.Show(Resources.SA_FailedToInitialize, "SmartAudio");
                SmartAudioLog.Log("App.SAUnhandledExceptionEventHandler", Severity.FATALERROR, (Exception) args.ExceptionObject);
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("App.SAUnhandledExceptionEventHandler", Severity.FATALERROR, exception);
            }
            finally
            {
                int exitCode = -1;
                Application.Current.Shutdown(exitCode);
            }
        }

        public void SelectDefaultSkin()
        {
            bool flag = false;
            App current = Application.Current as App;
            foreach (SkinDescriptor descriptor in this._skins)
            {
                if (!flag && (((current.Settings.SelectedSkin != Guid.Empty) && (current.Settings.SelectedSkin == descriptor.UniqueID)) || ((descriptor.SkinPriority == SkinPriority.FirstPriority) && descriptor.IsDefault)))
                {
                    descriptor.SelectAsDefault = true;
                    flag = true;
                }
                else
                {
                    descriptor.SelectAsDefault = false;
                }
            }
            if (!flag)
            {
                foreach (SkinDescriptor descriptor2 in this._skins)
                {
                    if ((descriptor2.SkinPriority == SkinPriority.SecondPriority) && descriptor2.IsDefault)
                    {
                        descriptor2.SelectAsDefault = true;
                        flag = true;
                    }
                    else
                    {
                        descriptor2.SelectAsDefault = false;
                    }
                }
            }
        }

        public bool SetLocale()
        {
            this._currentOSCulture = CultureInfo.CurrentUICulture.Name;
            SmartAudioLog.Log("App:SetLocale - CurrentCulture from the OS is " + this._currentOSCulture + " ", new object[] { Severity.INFORMATION });
            this._defaultLocale = this._currentOSCulture;
            if (this._settings.OverrideCurrentLocale)
            {
                this.SetLocale(this._settings.SelectedLocale, false);
                SmartAudioLog.Log("App:SetLocale - Overriding CurrentCulture from the Settings file with " + this._settings.SelectedLocale + " ", new object[] { Severity.INFORMATION });
            }
            else if (SALocalization.IsValidLocaleFolder(CultureInfo.CurrentUICulture.Name))
            {
                this.SetLocale(CultureInfo.CurrentUICulture.Name, false);
                SmartAudioLog.Log("App:SetLocale - IsValidLocaleFolder returned true. Setting locale to CultureInfo.CurrentUICulture.Name = " + CultureInfo.CurrentUICulture.Name + " ", new object[] { Severity.INFORMATION });
            }
            else
            {
                this.SetLocale(this._settings.SelectedLocale, false);
                SmartAudioLog.Log("App:SetLocale - IsValidLocaleFolder returned false. Setting locale to _settings.SelectedLocale = " + this._settings.SelectedLocale + " ", new object[] { Severity.INFORMATION });
            }
            return true;
        }

        public bool SetLocale(string language, bool silentMode)
        {
            CultureInfo info;
            Application current = Application.Current;
            SmartAudioLog.Log("SetLocale: upon entry, language =  " + language, new object[] { Severity.INFORMATION });
            string mappedName = SALocalization.GetMappedName(language.ToUpper());
            SmartAudioLog.Log("SetLocale: mappedLanguageName =  " + mappedName, new object[] { Severity.INFORMATION });
            string str3 = this._currentLocale;
            try
            {
                this._currentLocale = mappedName;
                info = new CultureInfo(mappedName);
                Resources.Culture = info;
                SmartAudioLog.Log("SetLocale: setting SmartAudio.Properties.Resources.Culture with MappedName =  " + info, new object[] { Severity.INFORMATION });
            }
            catch (ArgumentException exception)
            {
                SmartAudioLog.Log(string.Format(Resources.SA_LocalizationError, mappedName), Severity.WARNING, exception);
                try
                {
                    string name = language;
                    string str5 = name;
                    if (str5 != null)
                    {
                        if (str5 == "zh-SG")
                        {
                            name = "zh-CN";
                        }
                        else if ((str5 == "zh-HK") || (str5 == "zh-MO"))
                        {
                            goto Label_00F4;
                        }
                    }
                    goto Label_00FB;
                Label_00F4:
                    name = "zh-TW";
                Label_00FB:
                    this._currentLocale = name;
                    info = new CultureInfo(name);
                    SmartAudioLog.Log("SetLocale: setting SmartAudio.Properties.Resources.Culture =  " + info, new object[] { Severity.INFORMATION });
                    Resources.Culture = info;
                }
                catch (Exception exception2)
                {
                    SmartAudioLog.Log(string.Format(Resources.SA_LocalizationError, language), Severity.FATALERROR, exception2);
                    if (!silentMode)
                    {
                        MessageBox.Show(string.Format(Resources.SA_LocalizationError, language), "SmartAudio");
                        if (!this.ChangeLocale(str3))
                        {
                            this.SetLocale(this._defaultLocale, silentMode);
                        }
                    }
                    return true;
                }
            }
            finally
            {
                SmartAudioLog.Log("SetLocale: doing the finally", new object[] { Severity.INFORMATION });
                CultureInfo info2 = new CultureInfo("en-US");
                Thread.CurrentThread.CurrentCulture = info2;
                Thread.CurrentThread.CurrentUICulture = info2;
            }
            return true;
        }

        [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.ControlAppDomain)]
        public void SetupUnhandledException()
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(this.SAUnhandledExceptionEventHandler);
        }

        public void ShowJackPopup(bool visibility)
        {
            if (this._jackRetaskingPopup != null)
            {
                this._jackRetaskingPopup.Visibility = visibility ? Visibility.Visible : Visibility.Hidden;
                this._jackRetaskingPopup.Show();
            }
        }

        public void ShutdownDeviceEvents()
        {
            MainWindow mainWindow = base.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                ((MainAudioControlPage) mainWindow.GetPage("MainAudioPage")).ShutDownDeviceEvents();
            }
        }

        public void UpdateJackPorts(int jack, int selectedJack)
        {
            if (this._jackRetaskingPopup != null)
            {
                this._jackRetaskingPopup.updateJackPorts(jack, selectedJack);
            }
        }

        public string ActivePage
        {
            get => 
                this._currentAtivePage;
            set
            {
                this._currentAtivePage = value;
            }
        }

        public CxHDAudioFactory AudioFactory =>
            this._audioFactory;

        public List<LocaleInfo> AvailableLanguages =>
            this._availableLanguages;

        public string CurrentLocale =>
            this._currentLocale;

        public string CurrentOSCulture =>
            this._currentOSCulture;

        public string DefaultLocale =>
            this._defaultLocale;

        public string HDAudioAPIVersion
        {
            get => 
                this._HDAudioAPIVersion;
            set
            {
                this._HDAudioAPIVersion = value;
            }
        }

        public bool IsDemoMode
        {
            get => 
                this._demoMode;
            set
            {
                this._demoMode = value;
            }
        }

        public bool IsJackConfigurationChangedS3_S4
        {
            get => 
                this._isJackConfigurationChangedS3_S4;
            set
            {
                this._isJackConfigurationChangedS3_S4 = value;
            }
        }

        public bool IsJackRetaskable
        {
            get
            {
                try
                {
                    if (this._audioFactory.DeviceIOConfig.JackEnumerator.Count < 3)
                    {
                        return false;
                    }
                    for (int i = 1; i < 4; i++)
                    {
                        CxHDAudioJackConfig config = this._audioFactory.DeviceIOConfig.JackEnumerator[i] as CxHDAudioJackConfig;
                        int num2 = 0;
                        if (config != null)
                        {
                            if (config.get_IsDeviceTypeSupported(CxIOJackType.HeadPhoneJack))
                            {
                                num2++;
                            }
                            if (config.get_IsDeviceTypeSupported(CxIOJackType.MicophoneJack))
                            {
                                num2++;
                            }
                            if (config.get_IsDeviceTypeSupported(CxIOJackType.LineIN))
                            {
                                num2++;
                            }
                            if (config.get_IsDeviceTypeSupported(CxIOJackType.LineOut))
                            {
                                num2++;
                            }
                            if (num2 >= 2)
                            {
                                return true;
                            }
                        }
                    }
                }
                catch (Exception exception)
                {
                    SmartAudioLog.Log("App::IsJackRetaskable ", Severity.WARNING, exception);
                }
                return false;
            }
        }

        public bool IsMultiChannelSupported
        {
            get => 
                this._isMultiChannelSupported;
            set
            {
                this._isMultiChannelSupported = value;
            }
        }

        public bool IsResumeFromS3_S4
        {
            get => 
                this._isResumeFromS3_S4;
            set
            {
                this._isResumeFromS3_S4 = value;
            }
        }

        public bool IsRunningInBackground
        {
            get => 
                this._isRunningInBackground;
            set
            {
                this._isRunningInBackground = value;
            }
        }

        public bool IsRunningMinimized
        {
            get => 
                this._isRunningMinimized;
            set
            {
                this._isRunningMinimized = value;
            }
        }

        public bool IsRunningSliently
        {
            get => 
                this._isRunningSliently;
            set
            {
                this._isRunningSliently = value;
            }
        }

        public SmartAudio.JackRetaskingPopup JackRetaskingPopup =>
            this._jackRetaskingPopup;

        public int MaxNumOfSupportedChannels
        {
            get => 
                this._maxNumOfSupportedChannels;
            set
            {
                this._maxNumOfSupportedChannels = value;
            }
        }

        public SmartAudioSettings Settings
        {
            get => 
                this._settings;
            set
            {
                this._settings = value;
            }
        }

        public List<SkinDescriptor> Skins =>
            this._skins;

        public CxHDAudioEndPoint SPAudioEndPoint
        {
            get => 
                this._SPAudioEndPoint;
            set
            {
                this._SPAudioEndPoint = value;
                if (this.OnRenumeratedEvent != null)
                {
                    this.OnRenumeratedEvent();
                }
            }
        }

        public CxSpeakerConfigType SpeakerConfigType
        {
            get => 
                this._speakerConfigType;
            set
            {
                this._speakerConfigType = value;
            }
        }

        public string SPToolTip
        {
            get => 
                this._SPToolTip;
            set
            {
                this._SPToolTip = value;
            }
        }

        public SmartAudioStorage Storage
        {
            get => 
                this._storage;
            set
            {
                this._storage = value;
            }
        }

        public SmartAudio.SystemTray SystemTray =>
            this._systemTray;
    }
}

