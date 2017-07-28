namespace SmartAudio
{
    using CxHDAudioAPILib;
    using SmartAudio.Properties;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Markup;
    using System.Windows.Media.Animation;
    using System.Windows.Media.Effects;
    using System.Windows.Media.Imaging;
    using System.Windows.Threading;

    public class GraphicEqualizersPage : UserControl, ISmartAudioPage, IComponentConnector
    {
        internal ImageCheckBox _3D;
        internal ImageCheckBox _3DImmersion;
        internal Preview3DSound _3dImmersionContent;
        internal StackPanel _3DProfiles;
        internal ListBox _audioEqualizerBands;
        private CxHDAudioFactory _audioFactory;
        internal GlassBackPlate _audioOutputsPanelBack;
        internal ImageCheckBox _bass;
        internal TextBlock _BassTxt;
        internal ImageCheckBox _cbPhantomBass;
        private bool _change3DSettigs = true;
        internal ImageListItem _classic;
        private bool _contentLoaded;
        private int _currentSelectedEndPoint;
        internal ImageListItem _custom;
        private CxHDAudioConfig _cxHDAudioconfig;
        internal ImageListItem _dance;
        internal ListBox _endPointType;
        internal EqualizerBar _eqBand1;
        internal EqualizerBar _eqBand10;
        internal EqualizerBar _eqBand2;
        internal EqualizerBar _eqBand3;
        internal EqualizerBar _eqBand4;
        internal EqualizerBar _eqBand5;
        internal EqualizerBar _eqBand6;
        internal EqualizerBar _eqBand7;
        internal EqualizerBar _eqBand8;
        internal EqualizerBar _eqBand9;
        private CxHDAudioEqualizerClass _equalizer;
        private List<CxHDAudioRenderDevice> _equalizerEndPoints;
        private List<CxHDAudioEqualizerClass> _equalizerList;
        internal ListBox _equalizerProfiles;
        internal Grid _grid;
        internal ImageListItem _headphones;
        private bool _is3DImmersionrUnavailable;
        private bool _isPhantomSpeakerUnavailable;
        internal ImageListItem _jazz;
        internal CheckBox _nightMode;
        internal PreviewPhantomBass _phantomBass;
        internal StackPanel _PhantomBassPanel;
        internal ImageCheckBox _phantomSpeakers;
        internal Preview3DSpeaker _phantomSpeakersPreview;
        internal ImageListItem _pop;
        internal ImageListItem _power;
        private List<CxSAEQProfile> _predefinedEQProfiles;
        internal Grid _PreviewGrid;
        internal RowDefinition _row2;
        internal ImageListItem _speakers;
        internal StackPanel _tab;
        internal TextBlock _text3D;
        internal TextBlock _textBass;
        private bool _updatingEQ;
        internal ImageListItem _voice;
        internal GlassBackPlate glassBackPlate1;
        private int m_3DEffectIndex = -1;
        private int m_SPProfileIndex = -1;
        internal TextBlock text3DImmersion;
        internal TextBlock textPhantomSpeakers;

        public GraphicEqualizersPage()
        {
            try
            {
                this._updatingEQ = false;
                this.InitializeComponent();
                this._phantomSpeakersPreview.Visibility = Visibility.Hidden;
                this._3D.Selected = true;
                this._bass.Selected = false;
                this._3D.ShowLabel = false;
                this._3D.OnItemStateChanged += new ItemStateChanged(this._3D_OnItemStateChanged);
                this._bass.ShowLabel = false;
                this._bass.OnItemStateChanged += new ItemStateChanged(this._bass_OnItemStateChanged);
                this._text3D.MouseLeftButtonDown += new MouseButtonEventHandler(this._text3D_MouseLeftButtonDown);
                this._textBass.MouseLeftButtonDown += new MouseButtonEventHandler(this._bass_MouseLeftButtonDown);
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("GraphicEqualizersPage::GraphicEqualizersPage()", Severity.FATALERROR, exception);
            }
        }

        private void _3D_OnItemStateChanged(ImageCheckBox item, bool newState)
        {
            if (this._3D.Selected)
            {
                this._bass.Selected = false;
                this.Display3D(true);
            }
        }

        private void _3DSettings_OnItemStateChanged(ImageCheckBox item, bool newState)
        {
            if (this.SelectedEndPoint != 1)
            {
                App current = Application.Current as App;
                if (this.SelectedEndPoint == 0)
                {
                    this.m_SPProfileIndex = this._equalizerProfiles.SelectedIndex;
                }
                if (this.m_SPProfileIndex != -1)
                {
                    CxSAEQProfile local1 = this._predefinedEQProfiles[this.m_SPProfileIndex];
                }
                this._change3DSettigs = false;
                if (this.SelectedEndPoint == 0)
                {
                    this.SelectCustom();
                }
                this._change3DSettigs = true;
                item.Selected = newState;
                try
                {
                    if (item == this._phantomSpeakers)
                    {
                        this._3DImmersion.Selected = false;
                        this._3DProfiles.Visibility = Visibility.Visible;
                        this._phantomSpeakers.Visibility = Visibility.Visible;
                        this.textPhantomSpeakers.Visibility = Visibility.Visible;
                        this._phantomSpeakersPreview.Visibility = Visibility.Visible;
                        this._3dImmersionContent.Visibility = Visibility.Hidden;
                        if (newState)
                        {
                            this.Set3DSetting(CxHDAudio3DEffects.PhantomSpeakers);
                            current.Settings.SP3DEffect = CxHDAudio3DEffects.PhantomSpeakers;
                            this.SetUpPhantomSpeakerPreview(true);
                        }
                        else
                        {
                            this.Set3DSetting(CxHDAudio3DEffects.No3dEffects);
                            current.Settings.SP3DEffect = CxHDAudio3DEffects.No3dEffects;
                            this.SetUpPhantomSpeakerPreview(false);
                        }
                    }
                    else if (item == this._3DImmersion)
                    {
                        this._phantomSpeakers.Selected = false;
                        this._phantomSpeakersPreview.Visibility = Visibility.Hidden;
                        this._3dImmersionContent.Visibility = Visibility.Visible;
                        this._3DProfiles.Visibility = Visibility.Visible;
                        this._3DImmersion.Visibility = Visibility.Visible;
                        this.text3DImmersion.Visibility = Visibility.Visible;
                        if (newState)
                        {
                            this.Set3DSetting(CxHDAudio3DEffects.ThreeDimensionImmersion);
                            current.Settings.SP3DEffect = CxHDAudio3DEffects.ThreeDimensionImmersion;
                            this.SetUp3DImmersionPreview(true);
                        }
                        else
                        {
                            this.Set3DSetting(CxHDAudio3DEffects.No3dEffects);
                            current.Settings.SP3DEffect = CxHDAudio3DEffects.No3dEffects;
                            this.SetUp3DImmersionPreview(false);
                        }
                    }
                }
                catch (Exception exception)
                {
                    SmartAudioLog.Log("GraphicEqualizersPage::_3DSettings_OnItemStateChanged() " + exception.Message);
                }
            }
        }

        private void _audioEqualizerBands_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this._audioEqualizerBands.SelectedIndex != -1)
            {
                ((EqualizerBar) ((ListBoxItem) this._audioEqualizerBands.Items[this._audioEqualizerBands.SelectedIndex]).Content).SetFocus();
            }
        }

        private void _audioFactory_OnHeadphoneStatusChanged(CxJackPluginStatus newStatus)
        {
            this._audioFactory.remove_OnHeadphoneStatusChanged(new _ICxHDAudioFactoryEvents_OnHeadphoneStatusChangedEventHandler(this._audioFactory_OnHeadphoneStatusChanged));
            base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new ReEnumerate(this.OnReEnumerate));
        }

        private void _bass_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!this._bass.Selected)
            {
                this._bass.SelectItem();
                this.Display3D(false);
            }
        }

        private void _bass_OnItemStateChanged(ImageCheckBox item, bool newState)
        {
            if (this._bass.Selected)
            {
                this._3D.Selected = false;
                this.Display3D(false);
            }
        }

        private void _cbPhantomBass_OnItemStateChanged(ImageCheckBox item, bool newState)
        {
            try
            {
                if (item.Selected)
                {
                    this._phantomBass.doPreview(true);
                    this._phantomBass.BitmapEffect = null;
                }
                else
                {
                    BlurBitmapEffect effect = new BlurBitmapEffect {
                        Radius = 5.0
                    };
                    this._phantomBass.BitmapEffect = effect;
                    this._phantomBass.doPreview(false);
                }
                if (this._equalizer != null)
                {
                    this._equalizer.IsPhantomBassEnabled = item.Selected;
                }
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("GraphicEqualizersPage::_cbPhantomBass_OnItemStateChanged()- " + exception.Message);
            }
        }

        [DebuggerNonUserCode]
        internal Delegate _CreateDelegate(Type delegateType, string handler) => 
            Delegate.CreateDelegate(delegateType, this, handler);

        private void _endPointType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this._updatingEQ = true;
            try
            {
                if (this._endPointType.SelectedIndex >= this._equalizerList.Count)
                {
                    this._endPointType.SelectedIndex = 0;
                }
                else if (this._endPointType.SelectedIndex != -1)
                {
                    this.SelectedEndPoint = this._endPointType.SelectedIndex;
                    this._currentSelectedEndPoint = this._endPointType.SelectedIndex;
                    if ((this._endPointType.SelectedIndex == 0) && this.IsThreeDimensionPageEnabled())
                    {
                        this._phantomSpeakers.ReadOnly = false;
                        this._3DImmersion.ReadOnly = false;
                        this._3DProfiles.BitmapEffect = null;
                        this._phantomSpeakersPreview.BitmapEffect = null;
                        this._3dImmersionContent.BitmapEffect = null;
                    }
                    else if ((this._endPointType.SelectedIndex == 1) && this.IsThreeDimensionPageEnabled())
                    {
                        if (this._phantomSpeakers.Selected)
                        {
                            this.m_3DEffectIndex = 0;
                        }
                        else if (this._3DImmersion.Selected)
                        {
                            this.m_3DEffectIndex = 1;
                        }
                        else
                        {
                            this.m_3DEffectIndex = -1;
                        }
                        BlurBitmapEffect effect = new BlurBitmapEffect {
                            Radius = 5.0
                        };
                        this._3DProfiles.BitmapEffect = effect;
                        this._phantomSpeakersPreview.BitmapEffect = effect;
                        this._3dImmersionContent.BitmapEffect = effect;
                        this._phantomSpeakersPreview.doPreview(false);
                        this._3dImmersionContent.doPreview(false);
                        this._phantomSpeakers.ReadOnly = true;
                        this._3DImmersion.ReadOnly = true;
                    }
                    if ((this._endPointType.SelectedIndex == 0) && this.IsPhantomBassAvailable())
                    {
                        this._cbPhantomBass.BitmapEffect = null;
                        this._cbPhantomBass.ReadOnly = false;
                        this._BassTxt.BitmapEffect = null;
                        this.EnablePhantomBass(true);
                        if (this._bass.Selected)
                        {
                            this.Display3D(false);
                        }
                        else
                        {
                            this.Display3D(true);
                        }
                    }
                    else if ((this._endPointType.SelectedIndex == 1) && this.IsPhantomBassAvailable())
                    {
                        BlurBitmapEffect effect2 = new BlurBitmapEffect {
                            Radius = 5.0
                        };
                        this._BassTxt.BitmapEffect = effect2;
                        this._cbPhantomBass.BitmapEffect = effect2;
                        this._cbPhantomBass.ReadOnly = true;
                        this.EnablePhantomBass(false);
                    }
                    for (int i = 0; i < this._endPointType.Items.Count; i++)
                    {
                        ((ImageListItem) ((ListBoxItem) this._endPointType.Items[i]).Content).Selected = ((ListBoxItem) this._endPointType.Items[i]).IsSelected;
                    }
                    if ((this._equalizerList.Count > 0) && (this._endPointType.SelectedIndex < this._equalizerList.Count))
                    {
                        this._equalizer = this._equalizerList[this._endPointType.SelectedIndex];
                        base.DataContext = this._equalizer;
                        if (this.IsThreeDimensionPageEnabled())
                        {
                            this._phantomSpeakersPreview.DataContext = this._equalizer;
                        }
                        this._equalizerProfiles.SelectionChanged -= new SelectionChangedEventHandler(this._equalizerProfiles_SelectionChanged);
                        this.SelectCurrentProfile();
                        this._equalizerProfiles.SelectionChanged += new SelectionChangedEventHandler(this._equalizerProfiles_SelectionChanged);
                        if (((this.SelectedEndPoint == 0) && (this._equalizer.EQProfile == CxSAEQProfile.Custom)) && this.IsThreeDimensionPageEnabled())
                        {
                            if (this.m_3DEffectIndex == 0)
                            {
                                this._phantomSpeakers.Selected = true;
                                this._3DSettings_OnItemStateChanged(this._phantomSpeakers, true);
                            }
                            else if (this.m_3DEffectIndex == 1)
                            {
                                this._3DImmersion.Selected = true;
                                this._3DSettings_OnItemStateChanged(this._3DImmersion, true);
                            }
                        }
                    }
                    else
                    {
                        this._equalizer = null;
                        base.DataContext = null;
                        this._phantomSpeakersPreview.DataContext = null;
                        this._equalizerProfiles.SelectedIndex = -1;
                        this.InitializeEqualizerBands(null);
                    }
                    this.RefreshSliders();
                    if (this._endPointType.SelectedIndex == 0)
                    {
                        this.Display3D(!this._bass.Selected);
                    }
                }
                else
                {
                    SmartAudioLog.Log("_endPointType_SelectionChanged and _endPointType.SelectedIndex = " + this._endPointType.SelectedIndex, new object[] { Severity.INFORMATION });
                }
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("GraphicEqualizersPage::_endPointType_SelectionChanged() " + exception.Message);
            }
            this._updatingEQ = false;
        }

        private void _equalizerProfiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                for (int i = 0; i < this._equalizerProfiles.Items.Count; i++)
                {
                    ((ImageListItem) ((ListBoxItem) this._equalizerProfiles.Items[i]).Content).Selected = ((ListBoxItem) this._equalizerProfiles.Items[i]).IsSelected;
                }
                if (((this._equalizer != null) && (this._equalizerProfiles.SelectedIndex >= 0)) && (this._equalizerProfiles.SelectedIndex < this._predefinedEQProfiles.Count))
                {
                    CxSAEQProfile selectedProfile = this._predefinedEQProfiles[this._equalizerProfiles.SelectedIndex];
                    this._equalizer.EQProfile = selectedProfile;
                    if (this.SelectedEndPoint == 0)
                    {
                        this.Select3DProfile(selectedProfile);
                        this.m_SPProfileIndex = this._equalizerProfiles.SelectedIndex;
                    }
                    if (selectedProfile == CxSAEQProfile.Voice)
                    {
                        this._equalizer.NightMode = true;
                    }
                    else if (selectedProfile != CxSAEQProfile.Custom)
                    {
                        this._equalizer.NightMode = false;
                    }
                    this._nightMode.IsChecked = new bool?(this._equalizer.NightMode);
                    this._nightMode.ToolTip = this._nightMode.IsChecked.Value ? Resources.SA_NightModeON : Resources.SA_NightModeOFF;
                }
                this.RefreshSliders();
                if ((this._cxHDAudioconfig != null) && (sender != null))
                {
                    this._cxHDAudioconfig.NotifyRegistryViaSAIIService = 0x52;
                }
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("GraphicEqualizersPage::_equalizerProfiles_SelectionChanged() " + exception.Message);
            }
        }

        private void _nightMode_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                this._equalizer.NightMode = true;
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("GraphicEqualizerPage._nightMode_Checked", Severity.FATALERROR, exception);
            }
            this._nightMode.ToolTip = Resources.SA_NightModeON;
            if (this._equalizer.EQProfile != CxSAEQProfile.Voice)
            {
                this.SelectCustom();
            }
        }

        private void _nightMode_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                this._equalizer.NightMode = false;
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("Failed in _nightMode_Unchecked.", Severity.WARNING, exception);
            }
            this._nightMode.ToolTip = Resources.SA_NightModeOFF;
            if (this._equalizer.EQProfile == CxSAEQProfile.Voice)
            {
                this.SelectCustom();
            }
        }

        private void _text3D_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!this._3D.Selected)
            {
                this._3D.SelectItem();
                this.Display3D(true);
            }
        }

        private void _threeDSettingsContainer_MouseEnter(object sender, MouseEventArgs e)
        {
            DoubleAnimation animation = new DoubleAnimation {
                From = 0.0,
                To = 1.0,
                Duration = new Duration(TimeSpan.FromMilliseconds(500.0)),
                AutoReverse = false
            };
            this._phantomSpeakersPreview.BeginAnimation(UIElement.OpacityProperty, animation);
        }

        private void _threeDSettingsContainer_MouseLeave(object sender, MouseEventArgs e)
        {
            DoubleAnimation animation = new DoubleAnimation {
                From = 1.0,
                To = 0.0,
                Duration = new Duration(TimeSpan.FromMilliseconds(500.0)),
                AutoReverse = false
            };
            this._phantomSpeakersPreview.BeginAnimation(UIElement.OpacityProperty, animation);
        }

        private void AddEqualizerEndPoint(CxHDAudioRenderDevice renderDevice)
        {
            ListBoxItem newItem = new ListBoxItem();
            ImageListItem item2 = new ImageListItem();
            newItem.Content = item2;
            item2.ShowLabel = false;
            item2.Height = 80.0;
            item2.Width = 80.0;
            item2.ImageHeight = 80.0;
            item2.ImageWidth = 80.0;
            switch (renderDevice.RenderDeviceType)
            {
                case CxRenderDeviceType.HeadphonesDevice:
                    item2.SelectedImage = this.GetBitmap(new Uri("/SmartAudio;component/Resources/Images/EQ/Headphone-Selected.png", UriKind.RelativeOrAbsolute));
                    item2.UnselectedImage = this.GetBitmap(new Uri("/SmartAudio;component/Resources/Images/EQ/Headphone.png", UriKind.RelativeOrAbsolute));
                    item2.Selected = false;
                    this._endPointType.Items.Add(newItem);
                    return;
            }
            item2.SelectedImage = this.GetBitmap(new Uri("/SmartAudio;component/Resources/Images/EQ/Speakers-Selected.png", UriKind.RelativeOrAbsolute));
            item2.UnselectedImage = this.GetBitmap(new Uri("/SmartAudio;component/Resources/Images/EQ/Speakers.png", UriKind.RelativeOrAbsolute));
            item2.Selected = false;
            this._endPointType.Items.Add(newItem);
        }

        private void audioFactory_OnHeadphoneStatusChanged(CxJackPluginStatus newStatus)
        {
            this.EnumerateEqualizeEndPoints();
        }

        private void CheckAndDisable3DPage()
        {
            if (!this.IsThreeDimensionPageEnabled())
            {
                BlurBitmapEffect effect = new BlurBitmapEffect {
                    Radius = 5.0
                };
                this._3DProfiles.BitmapEffect = effect;
                this._phantomSpeakersPreview.BitmapEffect = effect;
                this._3dImmersionContent.BitmapEffect = effect;
                this._phantomSpeakersPreview.doPreview(false);
                this._3dImmersionContent.doPreview(false);
                this._phantomSpeakers.ReadOnly = true;
                this._3DImmersion.ReadOnly = true;
            }
        }

        private void CheckAndDisablePhantomBass()
        {
            if (!this.IsPhantomBassAvailable())
            {
                BlurBitmapEffect effect = new BlurBitmapEffect {
                    Radius = 5.0
                };
                this._cbPhantomBass.ReadOnly = true;
                this._phantomBass.BitmapEffect = effect;
                this._PhantomBassPanel.BitmapEffect = effect;
                this._phantomBass.doPreview(false);
            }
        }

        private void Display3D(bool flag)
        {
            if (flag)
            {
                if (this.IsPhantomSpeakerUnAvailable() && this.Is3DImmersionrUnavailable())
                {
                    this._3DProfiles.Visibility = Visibility.Hidden;
                    this._phantomSpeakersPreview.Visibility = Visibility.Hidden;
                    this._3dImmersionContent.Visibility = Visibility.Hidden;
                    if (this.IsPhantomBassAvailable())
                    {
                        this._PhantomBassPanel.Visibility = Visibility.Visible;
                        this._phantomBass.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        this._PhantomBassPanel.Visibility = Visibility.Hidden;
                        this._phantomBass.Visibility = Visibility.Hidden;
                    }
                }
                else
                {
                    this._3DProfiles.Visibility = Visibility.Visible;
                    if (!this.IsPhantomSpeakerUnAvailable() && !this.Is3DImmersionrUnavailable())
                    {
                        if ((this._equalizer != null) && (this._equalizer.ThreeDimensionEffects == CxHDAudio3DEffects.ThreeDimensionImmersion))
                        {
                            this._phantomSpeakersPreview.Visibility = Visibility.Hidden;
                            this._3dImmersionContent.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            this._phantomSpeakersPreview.Visibility = Visibility.Visible;
                            this._3dImmersionContent.Visibility = Visibility.Hidden;
                        }
                    }
                    else if (!this.IsPhantomSpeakerUnAvailable())
                    {
                        this._3DImmersion.Visibility = Visibility.Hidden;
                        this.text3DImmersion.Visibility = Visibility.Hidden;
                        this._3dImmersionContent.Visibility = Visibility.Hidden;
                        this._phantomSpeakers.Margin = new Thickness(59.0, 1.0, 0.0, 0.0);
                        this._phantomSpeakers.Visibility = Visibility.Visible;
                        this._phantomSpeakersPreview.Visibility = Visibility.Visible;
                        this.textPhantomSpeakers.Visibility = Visibility.Visible;
                        if ((this._equalizer != null) && (this._equalizer.ThreeDimensionEffects == CxHDAudio3DEffects.PhantomSpeakers))
                        {
                            this._phantomSpeakers.Selected = true;
                        }
                        else
                        {
                            this._phantomSpeakers.Selected = false;
                        }
                    }
                    else
                    {
                        this._phantomSpeakers.Visibility = Visibility.Hidden;
                        this.textPhantomSpeakers.Visibility = Visibility.Hidden;
                        this._phantomSpeakersPreview.Visibility = Visibility.Hidden;
                        this._3DImmersion.Margin = new Thickness(-57.0, 1.0, 0.0, 0.0);
                        this._3dImmersionContent.Visibility = Visibility.Visible;
                        this._3DImmersion.Visibility = Visibility.Visible;
                        this.text3DImmersion.Visibility = Visibility.Visible;
                        if ((this._equalizer != null) && (this._equalizer.ThreeDimensionEffects == CxHDAudio3DEffects.ThreeDimensionImmersion))
                        {
                            this._3DImmersion.Selected = true;
                        }
                        else
                        {
                            this._3DImmersion.Selected = false;
                        }
                    }
                    this._PhantomBassPanel.Visibility = Visibility.Hidden;
                    this._phantomBass.Visibility = Visibility.Hidden;
                }
            }
            else
            {
                this._3DProfiles.Visibility = Visibility.Hidden;
                this._phantomSpeakersPreview.Visibility = Visibility.Hidden;
                this._3dImmersionContent.Visibility = Visibility.Hidden;
                if (this.IsPhantomBassAvailable())
                {
                    this._PhantomBassPanel.Visibility = Visibility.Visible;
                    this._phantomBass.Visibility = Visibility.Visible;
                }
                else
                {
                    this._PhantomBassPanel.Visibility = Visibility.Hidden;
                    this._phantomBass.Visibility = Visibility.Hidden;
                }
            }
            if ((this._3DProfiles.Visibility == Visibility.Hidden) && (this._PhantomBassPanel.Visibility == Visibility.Hidden))
            {
                this.glassBackPlate1.Visibility = Visibility.Hidden;
            }
            else
            {
                this.glassBackPlate1.Visibility = Visibility.Visible;
            }
        }

        private void Enable3DImmersion(bool isEnabled)
        {
            if (isEnabled)
            {
                this._3DImmersion.Selected = true;
                this._3dImmersionContent.doPreview(true);
                this._3dImmersionContent.BitmapEffect = null;
            }
            else
            {
                this._3DImmersion.Selected = false;
                this._3dImmersionContent.doPreview(false);
                BlurBitmapEffect effect = new BlurBitmapEffect {
                    Radius = 5.0
                };
                this._3dImmersionContent.BitmapEffect = effect;
            }
        }

        private void EnablePhantomBass(bool isEnabled)
        {
            if (isEnabled)
            {
                this._cbPhantomBass.Selected = true;
                this._phantomBass.doPreview(true);
                this._phantomBass.BitmapEffect = null;
            }
            else
            {
                this._cbPhantomBass.Selected = false;
                BlurBitmapEffect effect = new BlurBitmapEffect {
                    Radius = 5.0
                };
                this._phantomBass.BitmapEffect = effect;
                this._phantomBass.doPreview(false);
            }
        }

        private void EnablePhantomSpeaker(bool isEnabled)
        {
            if (isEnabled)
            {
                this._phantomSpeakers.Selected = true;
                this._phantomSpeakersPreview.doPreview(true);
                this._phantomSpeakersPreview.BitmapEffect = null;
            }
            else
            {
                this._phantomSpeakers.Selected = false;
                this._phantomSpeakersPreview.doPreview(false);
                BlurBitmapEffect effect = new BlurBitmapEffect {
                    Radius = 5.0
                };
                this._phantomSpeakersPreview.BitmapEffect = effect;
            }
        }

        private void EnumerateEqualizeEndPoints()
        {
            try
            {
                CxHDAudioEqualizerClass item = null;
                CxHDAudioEqualizerClass class3 = null;
                App current = Application.Current as App;
                if ((current != null) || (current.AudioFactory != null))
                {
                    this._equalizerList.Clear();
                    foreach (CxHDAudioEndPoint point in current.AudioFactory.get_EndPointEnumerator(false))
                    {
                        string friendlyName = point.FriendlyName;
                        CxHDAudioRenderDevice device = point as CxHDAudioRenderDevice;
                        if (device != null)
                        {
                            string text2 = point.FriendlyName;
                            CxHDAudioEqualizerClass equalizerSettings = point.EqualizerSettings as CxHDAudioEqualizerClass;
                            if (equalizerSettings != null)
                            {
                                if ((device.RenderDeviceType == CxRenderDeviceType.InternalSpeakers) || (device.RenderDeviceType == CxRenderDeviceType.ExternalSpeakers))
                                {
                                    item = equalizerSettings;
                                    current.Settings.SP3DEffect = equalizerSettings.ThreeDimensionEffects;
                                    current.Settings.Save();
                                }
                                else if (device.RenderDeviceType == CxRenderDeviceType.HeadphonesDevice)
                                {
                                    class3 = equalizerSettings;
                                    current.Settings.SP3DEffect = equalizerSettings.ThreeDimensionEffects;
                                    current.Settings.Save();
                                }
                            }
                        }
                    }
                    if (item != null)
                    {
                        this._equalizerList.Add(item);
                    }
                    if ((class3 != null) && current.AudioFactory.DeviceIOConfig.HeadphonePresent)
                    {
                        this._equalizerList.Add(class3);
                        base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new SetHeadphoneEQAccess(this.OnSetHeadphoneEQAccess), true);
                    }
                    else
                    {
                        base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new SetHeadphoneEQAccess(this.OnSetHeadphoneEQAccess), false);
                    }
                    if ((((this._audioFactory.AudioDirector.AudioDirectorMode == CxHDAudioAudioDirectorMode.SingleStream) || (this._audioFactory.HostOperatingSystem == CxHostOperatingSystemType.WindowsXP)) || (this._audioFactory.HostOperatingSystem == CxHostOperatingSystemType.WindowsXP64)) && current.AudioFactory.DeviceIOConfig.HeadphonePresent)
                    {
                        this._endPointType.SelectedIndex = 1;
                        this._endPointType_SelectionChanged(null, null);
                    }
                    if ((this._endPointType.Items.Count > 0) && (this._equalizerList.Count > 0))
                    {
                        this._equalizer = this._equalizerList[this.SelectedEndPoint];
                        this._endPointType.SelectedIndex = this.SelectedEndPoint;
                        this._currentSelectedEndPoint = this.SelectedEndPoint;
                    }
                }
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("GraphicEqualizersPage::EnumerateEqualizeEndPoints()", Severity.FATALERROR, exception);
            }
        }

        public BitmapImage GetBitmap(Uri uri)
        {
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.UriSource = uri;
            image.EndInit();
            return image;
        }

        private CxHDAudio3DEffects GetCustom3DSettings()
        {
            App current = Application.Current as App;
            return current.Settings.SP3DEffect;
        }

        private int GetSelectedProfile()
        {
            try
            {
                CxSAEQProfile eQProfile = this._equalizer.EQProfile;
                for (int i = 0; i < this._predefinedEQProfiles.Count; i++)
                {
                    if (((CxSAEQProfile) this._predefinedEQProfiles[i]) == eQProfile)
                    {
                        return i;
                    }
                }
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("GraphicEqualizersPage::GetSelectedProfile()", Severity.FATALERROR, exception);
            }
            return 0;
        }

        private void GraphicEqualizersPage_OnEQBandValueChanged(UserControl control, double newValue)
        {
            if (!this._updatingEQ)
            {
                this.SelectCustom();
                CxSAEQProfile local1 = this._predefinedEQProfiles[this._equalizerProfiles.SelectedIndex];
            }
        }

        private void InitGUI()
        {
            App current = Application.Current as App;
            this._isPhantomSpeakerUnavailable = current.Settings.INISettings.IsPhantomSpeakerUnavailable;
            this._is3DImmersionrUnavailable = current.Settings.INISettings.Is3DImmersionrUnavailable;
            if (!this.IsPhantomBassAvailable() || (this._isPhantomSpeakerUnavailable && this._is3DImmersionrUnavailable))
            {
                this._3D.Visibility = Visibility.Hidden;
                this._bass.Visibility = Visibility.Hidden;
                this._text3D.Visibility = Visibility.Hidden;
                this._textBass.Visibility = Visibility.Hidden;
                int num = 15;
                this._grid.Margin = new Thickness(this._grid.Margin.Left, this._grid.Margin.Top - num, this._grid.Margin.Right, this._grid.Margin.Bottom);
                this._nightMode.Margin = new Thickness(this._nightMode.Margin.Left, this._nightMode.Margin.Top, this._nightMode.Margin.Right, this._nightMode.Margin.Bottom - (num / 2));
            }
            if (current.Settings.INISettings.IsPhantomBassSliderHidden)
            {
                this._phantomBass.IsSliderHidden = true;
            }
            if (current.Settings.INISettings.IsPhantomSpekaerSliderHidden)
            {
                this._phantomSpeakersPreview.IsSliderHidden = true;
            }
            if (current.Settings.INISettings.Is3DImmersionSliderHidden)
            {
                this._3dImmersionContent.IsSliderHidden = true;
            }
            this.Display3D(true);
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (!this._contentLoaded)
            {
                this._contentLoaded = true;
                Uri resourceLocator = new Uri("/SmartAudio;component/graphicequalizerspage.xaml", UriKind.Relative);
                Application.LoadComponent(this, resourceLocator);
            }
        }

        private void InitializeEqualizerBands(CxHDAudioEqualizerClass equalizer)
        {
            try
            {
                int num = 0;
                if (equalizer != null)
                {
                    foreach (CxEQBand band in equalizer.EQBands)
                    {
                        ((EqualizerBar) ((ListBoxItem) this._audioEqualizerBands.Items[num++]).Content).EQBand = band;
                    }
                }
                else
                {
                    for (num = 0; num < this._audioEqualizerBands.Items.Count; num++)
                    {
                        ((EqualizerBar) ((ListBoxItem) this._audioEqualizerBands.Items[num]).Content).EQBand = null;
                    }
                }
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("GraphicEqualizersPage::InitializeEqualizerBands() " + exception.Message);
            }
        }

        public bool InitializePage(CxHDAudioFactory audioFactory)
        {
            this._updatingEQ = true;
            try
            {
                this._audioFactory = audioFactory;
                this._audioFactory.add_OnHeadphoneStatusChanged(new _ICxHDAudioFactoryEvents_OnHeadphoneStatusChangedEventHandler(this._audioFactory_OnHeadphoneStatusChanged));
                this._nightMode.Checked += new RoutedEventHandler(this._nightMode_Checked);
                this._equalizerEndPoints = new List<CxHDAudioRenderDevice>();
                this._equalizerList = new List<CxHDAudioEqualizerClass>();
                this._predefinedEQProfiles = new List<CxSAEQProfile>();
                this._predefinedEQProfiles.Add(CxSAEQProfile.None);
                this._predefinedEQProfiles.Add(CxSAEQProfile.Jazz);
                this._predefinedEQProfiles.Add(CxSAEQProfile.Dance);
                this._predefinedEQProfiles.Add(CxSAEQProfile.Concert);
                this._predefinedEQProfiles.Add(CxSAEQProfile.Classical);
                this._predefinedEQProfiles.Add(CxSAEQProfile.Voice);
                this._predefinedEQProfiles.Add(CxSAEQProfile.Custom);
                this._equalizerProfiles.SelectionChanged += new SelectionChangedEventHandler(this._equalizerProfiles_SelectionChanged);
                this.InitGUI();
                this.EnumerateEqualizeEndPoints();
                this.HDAudioconfig = audioFactory.DeviceIOConfig;
                this._nightMode.DataContext = this._equalizer;
                for (int i = 0; i < this._audioEqualizerBands.Items.Count; i++)
                {
                    ((EqualizerBar) ((ListBoxItem) this._audioEqualizerBands.Items[i]).Content).OnEQBandValueChanged += new OnEQBandChanged(this.GraphicEqualizersPage_OnEQBandValueChanged);
                }
                this.SelectCurrentProfile();
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("GraphicEqualizersPage::InitializePage()", Severity.FATALERROR, exception);
                throw exception;
            }
            bool nightMode = false;
            try
            {
                if (this._equalizer != null)
                {
                    this._nightMode.IsChecked = new bool?(this._equalizer.NightMode);
                    nightMode = this._equalizer.NightMode;
                }
            }
            catch (Exception exception2)
            {
                SmartAudioLog.Log("GraphicEqualizersPage::InitializePage()", Severity.FATALERROR, exception2);
            }
            this._nightMode.ToolTip = nightMode ? Resources.SA_NightModeON : Resources.SA_NightModeOFF;
            this._updatingEQ = false;
            this._nightMode.Checked += new RoutedEventHandler(this._nightMode_Checked);
            this._nightMode.Unchecked += new RoutedEventHandler(this._nightMode_Unchecked);
            this.SetSpeakerHeadphoneToolTip();
            return true;
        }

        private bool Is3DImmersionrUnavailable() => 
            this._is3DImmersionrUnavailable;

        private bool IsLenovoPackage()
        {
            App current = Application.Current as App;
            if (current.AudioFactory == null)
            {
                return false;
            }
            return current.Settings.INISettings.IsLenovoPackageEnabled;
        }

        private bool IsPhantomBassAvailable()
        {
            bool isPhantomBassEnabled = false;
            try
            {
                App current = Application.Current as App;
                isPhantomBassEnabled = current.Settings.INISettings.IsPhantomBassEnabled;
                if (this._equalizer == null)
                {
                    return isPhantomBassEnabled;
                }
                switch (this._equalizer.IsPhantomBassAvailable())
                {
                    case DriverInformation.IsNotSupported:
                        SmartAudioLog.Log("GraphicEqualizersPage, PhantomBass is not supported in the driver.");
                        return false;

                    case DriverInformation.IsSupportedWithoutData:
                        SmartAudioLog.Log("GraphicEqualizersPage, PhantomBass is  supported but the profile is not available.");
                        return false;
                }
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("GraphicEqualizersPage::IsPhantomBassAvailable()- " + exception.Message);
            }
            return isPhantomBassEnabled;
        }

        private bool IsPhantomSpeakerUnAvailable() => 
            this._isPhantomSpeakerUnavailable;

        private bool IsThreeDimensionPageEnabled()
        {
            App current = Application.Current as App;
            return current.Settings.INFSettings.ThreeDimensionPageEnabled;
        }

        public bool Localize()
        {
            this._power.ToolTip = Resources.SA_XAML_Off;
            this._jazz.ToolTip = Resources.SA_XAML_Jazz;
            this._dance.ToolTip = Resources.SA_XAML_Dance;
            this._pop.ToolTip = Resources.SA_XAML_Concert;
            this._classic.ToolTip = Resources.SA_XAML_Classical;
            this._voice.ToolTip = Resources.SA_VOICE;
            this._custom.ToolTip = Resources.SA_XAML_Custom;
            this._nightMode.ToolTip = Resources.SA_XAML_NightMode;
            this._phantomSpeakers.ToolTip = Resources.SA_XAML_WideSpeakersEffect;
            this._3DImmersion.ToolTip = Resources.SA_XAML_3DSoundEffect;
            this._cbPhantomBass.ToolTip = Resources.SA_BassEffect;
            this._bass.ToolTip = Resources.SA_Bass;
            this._textBass.ToolTip = Resources.SA_Bass;
            this._textBass.Text = Resources.SA_Bass;
            this.SetSpeakerHeadphoneToolTip();
            return true;
        }

        public void OnReEnumerate()
        {
            this.EnumerateEqualizeEndPoints();
            this.SelectCurrentProfile();
            this._audioFactory.add_OnHeadphoneStatusChanged(new _ICxHDAudioFactoryEvents_OnHeadphoneStatusChangedEventHandler(this._audioFactory_OnHeadphoneStatusChanged));
        }

        private void OnSetHeadphoneEQAccess(bool isEQEnabled)
        {
            App current = Application.Current as App;
            if (isEQEnabled && current.AudioFactory.DeviceIOConfig.HeadphonePresent)
            {
                this._headphones.BitmapEffect = null;
            }
            else
            {
                BlurBitmapEffect effect = new BlurBitmapEffect {
                    Radius = 5.0
                };
                this._headphones.BitmapEffect = effect;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            this.UpdatePhantomBass();
            this.CheckAndDisable3DPage();
            this.CheckAndDisablePhantomBass();
        }

        public void Refresh3D()
        {
            this._phantomSpeakersPreview.UpdateSpread();
            this._3dImmersionContent.UpdateSpread();
            this._phantomBass.UpdateSpread();
        }

        public void refreshEndpointType()
        {
            this._endPointType.SelectedIndex = this._currentSelectedEndPoint;
            this.SelectedEndPoint = this._currentSelectedEndPoint;
            this._endPointType_SelectionChanged(null, null);
        }

        public void RefreshSliders()
        {
            try
            {
                for (int i = 0; i < this._audioEqualizerBands.Items.Count; i++)
                {
                    ((EqualizerBar) ((ListBoxItem) this._audioEqualizerBands.Items[i]).Content).Refresh();
                }
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("GraphicEqualizersPage::RefreshSliders() " + exception.Message);
            }
        }

        public void Reset()
        {
            double defaultPhantomSpeakerSetting;
            try
            {
                App current = Application.Current as App;
                current.Settings.SP3DEffect = CxHDAudio3DEffects.No3dEffects;
                for (int i = 0; i < this._equalizerList.Count; i++)
                {
                    CxHDAudioEqualizerClass class2 = this._equalizerList[i];
                    foreach (CxEQBand band in class2.EQBands)
                    {
                        band.Gain = 0.0;
                    }
                    class2.EQProfile = CxSAEQProfile.None;
                }
                if (current.AudioFactory.DeviceIOConfig.HeadphonePresent)
                {
                    this._endPointType.SelectedIndex = 1;
                }
                else
                {
                    this._endPointType.SelectedIndex = 0;
                }
                this._currentSelectedEndPoint = this._endPointType.SelectedIndex;
                this._endPointType_SelectionChanged(null, null);
                this._equalizerProfiles.SelectedIndex = -1;
                this.Select3DProfile(this._equalizer.EQProfile);
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("GraphicEqualizersPage::Reset() " + exception.Message);
            }
            try
            {
                this._nightMode.IsChecked = new bool?(this._equalizer.DefaultNightModeSetting);
                this._nightMode.ToolTip = this._nightMode.IsChecked.Value ? Resources.SA_NightModeON : Resources.SA_NightModeOFF;
            }
            catch (Exception exception2)
            {
                SmartAudioLog.Log("GraphicEqualizersPage::Reset()- " + exception2.Message);
            }
            this.SelectCurrentProfile();
            try
            {
                defaultPhantomSpeakerSetting = this._equalizer.Default3DImmersionSetting;
                this._equalizer.ImmersionSpread = defaultPhantomSpeakerSetting;
                this._3dImmersionContent.UpdateSpread();
            }
            catch (Exception exception3)
            {
                SmartAudioLog.Log("GraphicEqualizersPage::Reset()- " + exception3.Message);
            }
            try
            {
                defaultPhantomSpeakerSetting = this._equalizer.DefaultPhantomSpeakerSetting;
                this._equalizer.PhantomSpeakerSpread = defaultPhantomSpeakerSetting;
                this._phantomSpeakersPreview.UpdateSpread();
            }
            catch (Exception exception4)
            {
                SmartAudioLog.Log("GraphicEqualizersPage::Reset()- " + exception4.Message);
            }
            try
            {
                if (this.IsPhantomBassAvailable())
                {
                    this._equalizer.ResetPhantomBass();
                    this.UpdatePhantomBass();
                    if (this._bass.Selected)
                    {
                        this._bass.Selected = false;
                        this._3D.Selected = true;
                        this.Display3D(true);
                    }
                }
            }
            catch (Exception exception5)
            {
                SmartAudioLog.Log("GraphicEqualizersPage.ResetPhantomBass " + exception5.Message);
            }
        }

        private void ResetEQBands()
        {
            foreach (CxEQBand band in this._equalizer.EQBands)
            {
                band.Gain = 0.0;
            }
        }

        private void Select3dProfile(CxHDAudio3DEffects newSetting)
        {
            switch (newSetting)
            {
                case CxHDAudio3DEffects.No3dEffects:
                    this.EnablePhantomSpeaker(false);
                    this.Enable3DImmersion(false);
                    break;

                case CxHDAudio3DEffects.PhantomSpeakers:
                    this._phantomSpeakersPreview.Visibility = Visibility.Visible;
                    this.EnablePhantomSpeaker(true);
                    this._3dImmersionContent.Visibility = Visibility.Hidden;
                    this.Enable3DImmersion(false);
                    break;

                case CxHDAudio3DEffects.ThreeDimensionImmersion:
                    this._3dImmersionContent.Visibility = Visibility.Visible;
                    this.Enable3DImmersion(true);
                    this._phantomSpeakersPreview.Visibility = Visibility.Hidden;
                    this.EnablePhantomSpeaker(false);
                    break;
            }
            this.Display3D(true);
        }

        public void Select3DProfile(CxSAEQProfile selectedProfile)
        {
            if (this.IsThreeDimensionPageEnabled())
            {
                if ((selectedProfile == CxSAEQProfile.Concert) && !this.IsPhantomSpeakerUnAvailable())
                {
                    this.Set3DSetting(CxHDAudio3DEffects.PhantomSpeakers);
                    this.Select3dProfile(CxHDAudio3DEffects.PhantomSpeakers);
                }
                else if ((selectedProfile == CxSAEQProfile.Voice) && !this.Is3DImmersionrUnavailable())
                {
                    this.Set3DSetting(CxHDAudio3DEffects.ThreeDimensionImmersion);
                    this.Select3dProfile(CxHDAudio3DEffects.ThreeDimensionImmersion);
                }
                else if ((this.SelectedEndPoint == 0) && (selectedProfile == CxSAEQProfile.Custom))
                {
                    this.Set3DSetting(this.GetCustom3DSettings());
                    this.Select3dProfile(this.GetCustom3DSettings());
                }
                else if (this._change3DSettigs)
                {
                    this.Set3DSetting(CxHDAudio3DEffects.No3dEffects);
                    this.Select3dProfile(CxHDAudio3DEffects.No3dEffects);
                }
                if (this._bass.Selected)
                {
                    this.Display3D(false);
                }
            }
        }

        public void SelectCurrentProfile()
        {
            App current = Application.Current as App;
            if (!current.IsDemoMode)
            {
                try
                {
                    this._equalizerProfiles.SelectedIndex = this.GetSelectedProfile();
                    for (int i = 0; i < this._equalizerProfiles.Items.Count; i++)
                    {
                        ((ImageListItem) ((ListBoxItem) this._equalizerProfiles.Items[i]).Content).Selected = ((ListBoxItem) this._equalizerProfiles.Items[i]).IsSelected;
                    }
                    if (this.SelectedEndPoint == 0)
                    {
                        this.Select3DProfile(this._equalizer.EQProfile);
                    }
                    for (int j = 0; j < this._audioEqualizerBands.Items.Count; j++)
                    {
                        ((EqualizerBar) ((ListBoxItem) this._audioEqualizerBands.Items[j]).Content).OnEQBandValueChanged -= new OnEQBandChanged(this.GraphicEqualizersPage_OnEQBandValueChanged);
                    }
                    this.InitializeEqualizerBands(this._equalizer);
                    this._equalizerProfiles_SelectionChanged(null, null);
                    this.RefreshSliders();
                    for (int k = 0; k < this._audioEqualizerBands.Items.Count; k++)
                    {
                        ((EqualizerBar) ((ListBoxItem) this._audioEqualizerBands.Items[k]).Content).OnEQBandValueChanged += new OnEQBandChanged(this.GraphicEqualizersPage_OnEQBandValueChanged);
                    }
                }
                catch (Exception exception)
                {
                    SmartAudioLog.Log("GraphicEqualizersPage::SelectCurrentProfile()", Severity.FATALERROR, exception);
                }
            }
        }

        private void SelectCustom()
        {
            this._equalizerProfiles.SelectedIndex = 6;
            if (this._equalizerProfiles.SelectedIndex != -1)
            {
                ((ImageListItem) ((ListBoxItem) this._equalizerProfiles.Items[this._equalizerProfiles.SelectedIndex]).Content).Selected = true;
            }
        }

        private void Set3DSetting(CxHDAudio3DEffects effects)
        {
            try
            {
                if (this._equalizer != null)
                {
                    if (effects == CxHDAudio3DEffects.ThreeDimensionImmersion)
                    {
                        this._equalizer.Spread = this._equalizer.ImmersionSpread;
                        this._3dImmersionContent.UpdateSpread();
                    }
                    else if (effects == CxHDAudio3DEffects.PhantomSpeakers)
                    {
                        this._equalizer.Spread = this._equalizer.PhantomSpeakerSpread;
                        this._phantomSpeakersPreview.UpdateSpread();
                    }
                    this._equalizer.ThreeDimensionEffects = effects;
                }
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("GraphicEqualizersPage::Set3DSetting() " + exception.Message);
            }
        }

        private void SetNightMode(bool flag)
        {
            try
            {
                this._equalizer.NightMode = flag;
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("GraphicEqualizerPage.SetNightMode", Severity.FATALERROR, exception);
            }
        }

        public void SetSpeakerHeadphoneToolTip()
        {
            if (((this._audioFactory.HostOperatingSystem == CxHostOperatingSystemType.WindowsVista) || (this._audioFactory.HostOperatingSystem == CxHostOperatingSystemType.WindowsVista64)) || (this._audioFactory.HostOperatingSystem == CxHostOperatingSystemType.Windows7))
            {
                if (this.IsLenovoPackage())
                {
                    this._speakers.ToolTip = Resources.SA_Speaker;
                    this._headphones.ToolTip = Resources.SA_CHeadPhones;
                }
                else
                {
                    object page = MainWindow.theCurrent.GetPage("MainAudioPage");
                    if (page != null)
                    {
                        this._speakers.ToolTip = ((MainAudioControlPage) page).getAudioDeviceOSName(CxRenderDeviceType.InternalSpeakers);
                        if ((((this._audioFactory.HostOperatingSystem == CxHostOperatingSystemType.WindowsVista) || (this._audioFactory.HostOperatingSystem == CxHostOperatingSystemType.WindowsVista64)) || (this._audioFactory.HostOperatingSystem == CxHostOperatingSystemType.Windows7)) && (this._audioFactory.AudioDirector.AudioDirectorMode == CxHDAudioAudioDirectorMode.SingleStream))
                        {
                            this._headphones.ToolTip = Resources.SA_CHeadPhones;
                        }
                        else
                        {
                            this._headphones.ToolTip = ((MainAudioControlPage) page).getAudioDeviceName(CxRenderDeviceType.HeadphonesDevice);
                        }
                    }
                }
            }
            else
            {
                this.SetSpeakerHeadphoneToolTipForXP();
            }
        }

        private void SetSpeakerHeadphoneToolTipForXP()
        {
            if ((this._audioFactory.HostOperatingSystem == CxHostOperatingSystemType.WindowsXP) || (this._audioFactory.HostOperatingSystem == CxHostOperatingSystemType.WindowsXP64))
            {
                this._speakers.ToolTip = Resources.SA_Speaker;
                this._headphones.ToolTip = Resources.SA_CHeadPhones;
            }
        }

        private void SetUp3DImmersionPreview(bool on)
        {
            if (on)
            {
                this._3dImmersionContent.BitmapEffect = null;
                this._3dImmersionContent.doPreview(true);
            }
            else
            {
                BlurBitmapEffect effect = new BlurBitmapEffect {
                    Radius = 5.0
                };
                this._3dImmersionContent.BitmapEffect = effect;
                this._3dImmersionContent.doPreview(false);
            }
        }

        private void SetUpPhantomSpeakerPreview(bool on)
        {
            if (on)
            {
                this._phantomSpeakersPreview.BitmapEffect = null;
                this._phantomSpeakersPreview.doPreview(true);
            }
            else
            {
                BlurBitmapEffect effect = new BlurBitmapEffect {
                    Radius = 5.0
                };
                this._phantomSpeakersPreview.BitmapEffect = effect;
                this._phantomSpeakersPreview.doPreview(false);
            }
        }

        [DebuggerNonUserCode, EditorBrowsable(EditorBrowsableState.Never)]
        void IComponentConnector.Connect(int connectionId, object target)
        {
            switch (connectionId)
            {
                case 1:
                    ((GraphicEqualizersPage) target).Loaded += new RoutedEventHandler(this.Page_Loaded);
                    return;

                case 2:
                    this._grid = (Grid) target;
                    return;

                case 3:
                    this._row2 = (RowDefinition) target;
                    return;

                case 4:
                    this._tab = (StackPanel) target;
                    return;

                case 5:
                    this._3D = (ImageCheckBox) target;
                    return;

                case 6:
                    this._bass = (ImageCheckBox) target;
                    return;

                case 7:
                    this._text3D = (TextBlock) target;
                    return;

                case 8:
                    this._textBass = (TextBlock) target;
                    return;

                case 9:
                    this.glassBackPlate1 = (GlassBackPlate) target;
                    return;

                case 10:
                    this._endPointType = (ListBox) target;
                    this._endPointType.SelectionChanged += new SelectionChangedEventHandler(this._endPointType_SelectionChanged);
                    return;

                case 11:
                    this._speakers = (ImageListItem) target;
                    return;

                case 12:
                    this._headphones = (ImageListItem) target;
                    return;

                case 13:
                    this._audioOutputsPanelBack = (GlassBackPlate) target;
                    return;

                case 14:
                    this._audioEqualizerBands = (ListBox) target;
                    this._audioEqualizerBands.SelectionChanged += new SelectionChangedEventHandler(this._audioEqualizerBands_SelectionChanged);
                    return;

                case 15:
                    this._eqBand1 = (EqualizerBar) target;
                    return;

                case 0x10:
                    this._eqBand2 = (EqualizerBar) target;
                    return;

                case 0x11:
                    this._eqBand3 = (EqualizerBar) target;
                    return;

                case 0x12:
                    this._eqBand4 = (EqualizerBar) target;
                    return;

                case 0x13:
                    this._eqBand5 = (EqualizerBar) target;
                    return;

                case 20:
                    this._eqBand6 = (EqualizerBar) target;
                    return;

                case 0x15:
                    this._eqBand7 = (EqualizerBar) target;
                    return;

                case 0x16:
                    this._eqBand8 = (EqualizerBar) target;
                    return;

                case 0x17:
                    this._eqBand9 = (EqualizerBar) target;
                    return;

                case 0x18:
                    this._eqBand10 = (EqualizerBar) target;
                    return;

                case 0x19:
                    this._equalizerProfiles = (ListBox) target;
                    return;

                case 0x1a:
                    this._power = (ImageListItem) target;
                    return;

                case 0x1b:
                    this._jazz = (ImageListItem) target;
                    return;

                case 0x1c:
                    this._dance = (ImageListItem) target;
                    return;

                case 0x1d:
                    this._pop = (ImageListItem) target;
                    return;

                case 30:
                    this._classic = (ImageListItem) target;
                    return;

                case 0x1f:
                    this._voice = (ImageListItem) target;
                    return;

                case 0x20:
                    this._custom = (ImageListItem) target;
                    return;

                case 0x21:
                    this._nightMode = (CheckBox) target;
                    return;

                case 0x22:
                    this._PreviewGrid = (Grid) target;
                    return;

                case 0x23:
                    this._3dImmersionContent = (Preview3DSound) target;
                    return;

                case 0x24:
                    this._phantomSpeakersPreview = (Preview3DSpeaker) target;
                    return;

                case 0x25:
                    this._phantomBass = (PreviewPhantomBass) target;
                    return;

                case 0x26:
                    this._3DProfiles = (StackPanel) target;
                    return;

                case 0x27:
                    this._phantomSpeakers = (ImageCheckBox) target;
                    return;

                case 40:
                    this._3DImmersion = (ImageCheckBox) target;
                    return;

                case 0x29:
                    this.text3DImmersion = (TextBlock) target;
                    return;

                case 0x2a:
                    this.textPhantomSpeakers = (TextBlock) target;
                    return;

                case 0x2b:
                    this._PhantomBassPanel = (StackPanel) target;
                    return;

                case 0x2c:
                    this._cbPhantomBass = (ImageCheckBox) target;
                    return;

                case 0x2d:
                    this._BassTxt = (TextBlock) target;
                    return;
            }
            this._contentLoaded = true;
        }

        public void TogglePhantomBass()
        {
            this._cbPhantomBass.Selected = !this._cbPhantomBass.Selected;
            this._cbPhantomBass_OnItemStateChanged(this._cbPhantomBass, this._cbPhantomBass.Selected);
        }

        private void UpdatePhantomBass()
        {
            if ((this._equalizer != null) && (this.SelectedEndPoint != 1))
            {
                try
                {
                    this._phantomBass.UpdateSpread();
                    this.EnablePhantomBass(this._equalizer.IsPhantomBassEnabled);
                }
                catch (Exception exception)
                {
                    SmartAudioLog.Log("GraphicEqualizersPage::ResetPhantomBass()- " + exception.Message);
                }
            }
        }

        public CxHDAudioFactory AudioFactory
        {
            get => 
                this._audioFactory;
            set
            {
                this._audioFactory = value;
                if (this._audioFactory != null)
                {
                    this.InitializePage(this._audioFactory);
                }
            }
        }

        public string FriendlyName =>
            "Graphic Equalizer Page";

        public CxHDAudioConfig HDAudioconfig
        {
            get => 
                this._cxHDAudioconfig;
            set
            {
                this._cxHDAudioconfig = value;
            }
        }

        private int SelectedEndPoint
        {
            get
            {
                App current = Application.Current as App;
                if (current.Settings.SelectedEQEndPoint >= this._equalizerList.Count)
                {
                    return 0;
                }
                return current.Settings.SelectedEQEndPoint;
            }
            set
            {
                App current = Application.Current as App;
                current.Settings.SelectedEQEndPoint = value;
            }
        }
    }
}

