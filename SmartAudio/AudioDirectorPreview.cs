namespace SmartAudio
{
    using CxHDAudioAPILib;
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Forms;
    using System.Windows.Markup;
    using System.Windows.Media;
    using System.Windows.Media.Effects;
    using System.Windows.Shapes;
    using System.Windows.Threading;

    public class AudioDirectorPreview : System.Windows.Controls.UserControl, IComponentConnector
    {
        private CxHDAudioDirector _audioDirector;
        private bool _contentLoaded;
        private static ArrayList _headPhoneAL;
        internal System.Windows.Controls.ListBox _pcPlayback;
        private AudioPlayer _player1;
        private AudioPlayer _player2;
        internal ImageCheckBox _playerImage1;
        internal ImageCheckBox _playerImage2;
        internal Image _preview;
        private static ArrayList _speakerAL;
        internal System.Windows.Controls.Button _tone1Playback;
        internal System.Windows.Controls.Button _tone2Playback;
        internal Canvas m_animCanvas;
        internal Canvas m_animCanvas2;
        private bool m_Hp1Selected;
        private int m_index;
        internal Path m_pathHp1;
        internal Path m_pathHp2;
        internal Path m_pathSp1;
        internal Path m_pathSp2;
        private bool m_Sp1Selected;
        private int m_toneIndex = -1;
        private int m_x1Hp1 = -1;
        private const int m_x1Hp1CM = 0x44;
        private const int m_x1Hp1MS = 60;
        private int m_x1Sp1 = -1;
        private const int m_x1Sp1CM = 0x1c;
        private const int m_x1Sp1MS = 0x19;
        private int m_x2Hp1 = -1;
        private const int m_x2Hp1CM = 0x77;
        private const int m_x2Hp1MS = 0x69;
        private int m_x2Sp1 = -1;
        private const int m_x2Sp1CM = 190;
        private const int m_x2Sp1MS = 0xc1;
        private int m_y1Hp1 = -1;
        private const int m_y1Hp1CM = 0x12;
        private const int m_y1Hp1MS = 7;
        private int m_y1Sp1 = -1;
        private const int m_y1Sp1CM = 0x19;
        private const int m_y1Sp1MS = 10;
        private int m_y2Hp1 = -1;
        private const int m_y2Hp1CM = 0x29;
        private const int m_y2Hp1MS = 0x1f;
        private int m_y2Sp1 = -1;
        private const int m_y2Sp1CM = 0x27;
        private const int m_y2Sp1MS = 0x1c;
        internal PathFigureCollection pathCollection1;
        internal PathFigureCollection pathCollection2;
        internal PathFigureCollection pathCollection3;
        internal PathFigureCollection pathCollection4;
        private Timer timer;

        public AudioDirectorPreview()
        {
            this.InitializeComponent();
            this._player1 = new AudioPlayer();
            this._player2 = new AudioPlayer();
            this._pcPlayback.Visibility = Visibility.Visible;
            this._player1.OnPlayerStopped += new PlayerStopped(this._player1_OnPlayerStopped);
            this._player2.OnPlayerStopped += new PlayerStopped(this._player2_OnPlayerStopped);
            this.m_index = 0;
            this.timer = new Timer();
            this.timer.Interval = 0x37;
            this.timer.Tick += new EventHandler(this.timer_Tick);
            _speakerAL = new ArrayList();
            _headPhoneAL = new ArrayList();
        }

        [DebuggerNonUserCode]
        internal Delegate _CreateDelegate(System.Type delegateType, string handler) => 
            Delegate.CreateDelegate(delegateType, this, handler);

        private void _player1_OnPlayerStopped()
        {
            base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new OnStreamStopped(this.OnStream2Stopped));
        }

        private void _player2_OnPlayerStopped()
        {
            base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new OnStreamStopped(this.OnStream1Stopped));
        }

        private void _tone1Play_Click(object sender, RoutedEventArgs e)
        {
            if (this.AudioDirector.AudioDirectorMode == CxHDAudioAudioDirectorMode.SingleStream)
            {
                if (_speakerAL.Count == 0)
                {
                    return;
                }
                this._player1.Play((string) _speakerAL[0], ((App) System.Windows.Application.Current).Settings.RedStreamFileName, true, 2200.0);
                this.doPreview(true, 1, 2);
            }
            else
            {
                if (_headPhoneAL.Count == 0)
                {
                    return;
                }
                this._player1.Play((string) _headPhoneAL[0], ((App) System.Windows.Application.Current).Settings.RedStreamFileName, true, 2200.0);
                this.doPreview(true, 1, 1);
            }
            this._playerImage1.Selected = true;
            this.SetBlurEffect(this._playerImage1, true);
        }

        private void _tone2Play_Click(object sender, RoutedEventArgs e)
        {
            if (_speakerAL.Count != 0)
            {
                this._player2.Play((string) _speakerAL[0], ((App) System.Windows.Application.Current).Settings.BlueStreamFileName, true, 2200.0);
                this._playerImage2.Selected = true;
                this.SetBlurEffect(this._playerImage2, true);
                this.doPreview(true, 2, 2);
            }
        }

        public static void AddHeadPhoneAL(string endPointID)
        {
            if (_headPhoneAL != null)
            {
                _headPhoneAL.Add(endPointID);
            }
        }

        public void AddSingleFigure(PathFigureCollection pathCollection, double x0, double y0, double r, double t0, double t1)
        {
            PathFigure figure = new PathFigure {
                StartPoint = new Point(x0 + (r * Math.Cos((t0 * 3.1415926535897931) / 180.0)), y0 + (r * Math.Sin((t0 * 3.1415926535897931) / 180.0))),
                IsClosed = false
            };
            ArcSegment segment = new ArcSegment(new Point(x0 + (r * Math.Cos((t1 * 3.1415926535897931) / 180.0)), y0 + (r * Math.Sin((t1 * 3.1415926535897931) / 180.0))), new Size(r, r), 0.0, false, SweepDirection.Clockwise, true);
            figure.Segments.Add(segment);
            pathCollection.Add(figure);
        }

        public static void AddSpeakerAL(string endPointID)
        {
            if (_speakerAL != null)
            {
                _speakerAL.Add(endPointID);
            }
        }

        public void changePreviewMode(CxHDAudioAudioDirectorMode mode)
        {
            if (mode == CxHDAudioAudioDirectorMode.SingleStream)
            {
                this.SetupSoundFieldForClassicMode();
            }
            else if (mode == CxHDAudioAudioDirectorMode.MultiStream)
            {
                this.SetupSoundFieldForMultiStreamMode();
            }
        }

        public static void clearSpeakerHeadPhoneALs()
        {
            if (_speakerAL != null)
            {
                _speakerAL.Clear();
            }
            if (_headPhoneAL != null)
            {
                _headPhoneAL.Clear();
            }
        }

        public void doPreview(bool on, int toneIndex, int endPointIndex)
        {
            this.m_toneIndex = toneIndex;
            if (!on || (this.m_toneIndex != -1))
            {
                if (on && (base.Visibility == Visibility.Visible))
                {
                    RadialGradientBrush brush = new RadialGradientBrush();
                    if (toneIndex == 1)
                    {
                        brush.GradientStops.Add(new GradientStop(Colors.Red, 0.0));
                        brush.GradientStops.Add(new GradientStop(Colors.Red, 1.0));
                    }
                    else if (toneIndex == 2)
                    {
                        brush.GradientStops.Add(new GradientStop(Colors.Blue, 0.0));
                        brush.GradientStops.Add(new GradientStop(Colors.Blue, 1.0));
                    }
                    brush.Freeze();
                    if (endPointIndex == 1)
                    {
                        this.m_Hp1Selected = true;
                        this.m_Sp1Selected = false;
                        this.m_pathHp1.Stroke = brush;
                        this.pathCollection1.Clear();
                        if (this.AudioDirector.AudioDirectorMode == CxHDAudioAudioDirectorMode.SingleStream)
                        {
                            this.m_x1Hp1 = 0x44;
                            this.m_y1Hp1 = 0x12;
                            this.m_x2Hp1 = 0x77;
                            this.m_y2Hp1 = 0x29;
                        }
                        else
                        {
                            this.m_x1Hp1 = 60;
                            this.m_y1Hp1 = 7;
                            this.m_x2Hp1 = 0x69;
                            this.m_y2Hp1 = 0x1f;
                        }
                    }
                    else if (endPointIndex == 2)
                    {
                        this.m_Hp1Selected = false;
                        this.m_Sp1Selected = true;
                        this.pathCollection2.Clear();
                        if (this.AudioDirector.AudioDirectorMode == CxHDAudioAudioDirectorMode.SingleStream)
                        {
                            if (this.IsHeadphonePresent)
                            {
                                this.m_x1Hp1 = 0x44;
                                this.m_y1Hp1 = 0x12;
                                this.m_x2Hp1 = 0x77;
                                this.m_y2Hp1 = 0x29;
                                this.m_Hp1Selected = true;
                                this.m_Sp1Selected = false;
                                this.m_pathHp1.Stroke = brush;
                            }
                            else
                            {
                                this.m_x1Sp1 = 0x1c;
                                this.m_y1Sp1 = 0x19;
                                this.m_x2Sp1 = 190;
                                this.m_y2Sp1 = 0x27;
                                this.m_pathSp1.Stroke = brush;
                            }
                            if ((toneIndex == 1) && !this.IsHeadphonePresent)
                            {
                                this.pathCollection3.Clear();
                                RadialGradientBrush brush2 = new RadialGradientBrush {
                                    GradientStops = { 
                                        new GradientStop(Colors.Blue, 0.0),
                                        new GradientStop(Colors.Blue, 1.0)
                                    }
                                };
                                brush2.Freeze();
                                this.m_pathSp2.Stroke = brush2;
                                this.AddSingleFigure(this.pathCollection3, 28.0, 25.0, 26.0, 100.0, 195.0);
                                this.AddSingleFigure(this.pathCollection3, 28.0, 25.0, 20.0, 100.0, 195.0);
                                this.AddSingleFigure(this.pathCollection3, 28.0, 25.0, 12.0, 100.0, 195.0);
                                this.AddSingleFigure(this.pathCollection3, 190.0, 39.0, 26.0, 15.0, 105.0);
                                this.AddSingleFigure(this.pathCollection3, 190.0, 39.0, 20.0, 15.0, 105.0);
                                this.AddSingleFigure(this.pathCollection3, 190.0, 39.0, 12.0, 15.0, 105.0);
                            }
                            else if ((toneIndex == 2) && this.IsHeadphonePresent)
                            {
                                this.pathCollection4.Clear();
                                RadialGradientBrush brush3 = new RadialGradientBrush {
                                    GradientStops = { 
                                        new GradientStop(Colors.Red, 0.0),
                                        new GradientStop(Colors.Red, 1.0)
                                    }
                                };
                                brush3.Freeze();
                                this.m_pathHp2.Stroke = brush3;
                                this.AddSingleFigure(this.pathCollection4, 68.0, 18.0, 30.0, 30.0, 150.0);
                                this.AddSingleFigure(this.pathCollection4, 68.0, 18.0, 22.0, 30.0, 150.0);
                                this.AddSingleFigure(this.pathCollection4, 68.0, 18.0, 15.0, 30.0, 150.0);
                                this.AddSingleFigure(this.pathCollection4, 119.0, 41.0, 30.0, 30.0, 150.0);
                                this.AddSingleFigure(this.pathCollection4, 119.0, 41.0, 22.0, 30.0, 150.0);
                                this.AddSingleFigure(this.pathCollection4, 119.0, 41.0, 15.0, 30.0, 150.0);
                            }
                            else if ((toneIndex == 2) && !this.IsHeadphonePresent)
                            {
                                this.pathCollection3.Clear();
                                RadialGradientBrush brush4 = new RadialGradientBrush {
                                    GradientStops = { 
                                        new GradientStop(Colors.Red, 0.0),
                                        new GradientStop(Colors.Red, 1.0)
                                    }
                                };
                                brush4.Freeze();
                                this.m_pathSp2.Stroke = brush4;
                                this.AddSingleFigure(this.pathCollection3, 28.0, 25.0, 30.0, 100.0, 195.0);
                                this.AddSingleFigure(this.pathCollection3, 28.0, 25.0, 22.0, 100.0, 195.0);
                                this.AddSingleFigure(this.pathCollection3, 28.0, 25.0, 15.0, 100.0, 195.0);
                                this.AddSingleFigure(this.pathCollection3, 190.0, 39.0, 30.0, 15.0, 105.0);
                                this.AddSingleFigure(this.pathCollection3, 190.0, 39.0, 22.0, 15.0, 105.0);
                                this.AddSingleFigure(this.pathCollection3, 190.0, 39.0, 15.0, 15.0, 105.0);
                            }
                        }
                        else
                        {
                            this.m_pathSp1.Stroke = brush;
                            this.m_x1Sp1 = 0x19;
                            this.m_y1Sp1 = 10;
                            this.m_x2Sp1 = 0xc1;
                            this.m_y2Sp1 = 0x1c;
                        }
                    }
                    this.timer.Start();
                }
                else
                {
                    if (this.AudioDirector.AudioDirectorMode == CxHDAudioAudioDirectorMode.SingleStream)
                    {
                        this.SetupSoundFieldForClassicMode();
                    }
                    else
                    {
                        this.SetupSoundFieldForMultiStreamMode();
                    }
                    this.timer.Stop();
                }
            }
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (!this._contentLoaded)
            {
                this._contentLoaded = true;
                Uri resourceLocator = new Uri("/SmartAudio;component/audiodirectorpreview.xaml", UriKind.Relative);
                System.Windows.Application.LoadComponent(this, resourceLocator);
            }
        }

        private void OnStream1Stopped()
        {
            this._playerImage2.Selected = false;
            this.doPreview(false, -1, -1);
            this.SetBlurEffect(this._playerImage2, false);
        }

        private void OnStream2Stopped()
        {
            this._playerImage1.Selected = false;
            this.doPreview(false, -1, -1);
            this.SetBlurEffect(this._playerImage1, false);
        }

        private void SetBlurEffect(ImageCheckBox icb, bool on)
        {
            if (icb != null)
            {
                if (on)
                {
                    icb.ReadOnly = true;
                    BlurBitmapEffect effect = new BlurBitmapEffect {
                        Radius = 5.0
                    };
                    icb.BitmapEffect = effect;
                }
                else
                {
                    icb.ReadOnly = false;
                    icb.BitmapEffect = null;
                }
            }
        }

        private void SetupSoundFieldForClassicMode()
        {
            this.pathCollection1.Clear();
            this.pathCollection2.Clear();
            this.pathCollection3.Clear();
            this.pathCollection4.Clear();
            RadialGradientBrush brush = new RadialGradientBrush {
                GradientStops = { 
                    new GradientStop(Colors.Red, 0.0),
                    new GradientStop(Colors.Red, 1.0)
                }
            };
            brush.Freeze();
            RadialGradientBrush brush2 = new RadialGradientBrush {
                GradientStops = { 
                    new GradientStop(Colors.Blue, 0.0),
                    new GradientStop(Colors.Blue, 1.0)
                }
            };
            brush2.Freeze();
            if (this.IsHeadphonePresent)
            {
                this.m_pathHp1.Stroke = brush;
                this.m_pathHp2.Stroke = brush2;
                this.AddSingleFigure(this.pathCollection1, 68.0, 18.0, 30.0, 30.0, 150.0);
                this.AddSingleFigure(this.pathCollection1, 68.0, 18.0, 22.0, 30.0, 150.0);
                this.AddSingleFigure(this.pathCollection1, 68.0, 18.0, 15.0, 30.0, 150.0);
                this.AddSingleFigure(this.pathCollection1, 119.0, 41.0, 30.0, 30.0, 150.0);
                this.AddSingleFigure(this.pathCollection1, 119.0, 41.0, 22.0, 30.0, 150.0);
                this.AddSingleFigure(this.pathCollection1, 119.0, 41.0, 15.0, 30.0, 150.0);
                this.AddSingleFigure(this.pathCollection4, 68.0, 18.0, 26.0, 30.0, 150.0);
                this.AddSingleFigure(this.pathCollection4, 68.0, 18.0, 20.0, 30.0, 150.0);
                this.AddSingleFigure(this.pathCollection4, 68.0, 18.0, 12.0, 30.0, 150.0);
                this.AddSingleFigure(this.pathCollection4, 119.0, 41.0, 26.0, 30.0, 150.0);
                this.AddSingleFigure(this.pathCollection4, 119.0, 41.0, 20.0, 30.0, 150.0);
                this.AddSingleFigure(this.pathCollection4, 119.0, 41.0, 12.0, 30.0, 150.0);
            }
            else
            {
                this.m_pathSp1.Stroke = brush2;
                this.m_pathSp2.Stroke = brush;
                this.AddSingleFigure(this.pathCollection3, 28.0, 25.0, 30.0, 100.0, 195.0);
                this.AddSingleFigure(this.pathCollection3, 28.0, 25.0, 22.0, 100.0, 195.0);
                this.AddSingleFigure(this.pathCollection3, 28.0, 25.0, 15.0, 100.0, 195.0);
                this.AddSingleFigure(this.pathCollection3, 190.0, 39.0, 30.0, 15.0, 105.0);
                this.AddSingleFigure(this.pathCollection3, 190.0, 39.0, 22.0, 15.0, 105.0);
                this.AddSingleFigure(this.pathCollection3, 190.0, 39.0, 15.0, 15.0, 105.0);
                this.AddSingleFigure(this.pathCollection2, 28.0, 25.0, 26.0, 100.0, 195.0);
                this.AddSingleFigure(this.pathCollection2, 28.0, 25.0, 20.0, 100.0, 195.0);
                this.AddSingleFigure(this.pathCollection2, 28.0, 25.0, 12.0, 100.0, 195.0);
                this.AddSingleFigure(this.pathCollection2, 190.0, 39.0, 26.0, 15.0, 105.0);
                this.AddSingleFigure(this.pathCollection2, 190.0, 39.0, 20.0, 15.0, 105.0);
                this.AddSingleFigure(this.pathCollection2, 190.0, 39.0, 12.0, 15.0, 105.0);
            }
        }

        private void SetupSoundFieldForMultiStreamMode()
        {
            this.pathCollection1.Clear();
            this.pathCollection2.Clear();
            this.pathCollection3.Clear();
            this.pathCollection4.Clear();
            RadialGradientBrush brush = new RadialGradientBrush {
                GradientStops = { 
                    new GradientStop(Colors.Red, 0.0),
                    new GradientStop(Colors.Red, 1.0)
                }
            };
            brush.Freeze();
            RadialGradientBrush brush2 = new RadialGradientBrush {
                GradientStops = { 
                    new GradientStop(Colors.Blue, 0.0),
                    new GradientStop(Colors.Blue, 1.0)
                }
            };
            brush2.Freeze();
            this.m_pathHp1.Stroke = brush;
            this.m_pathSp1.Stroke = brush2;
            this.AddSingleFigure(this.pathCollection1, 60.0, 7.0, 30.0, 30.0, 150.0);
            this.AddSingleFigure(this.pathCollection1, 60.0, 7.0, 20.0, 30.0, 150.0);
            this.AddSingleFigure(this.pathCollection1, 60.0, 7.0, 10.0, 30.0, 150.0);
            this.AddSingleFigure(this.pathCollection1, 105.0, 31.0, 30.0, 30.0, 150.0);
            this.AddSingleFigure(this.pathCollection1, 105.0, 31.0, 20.0, 30.0, 150.0);
            this.AddSingleFigure(this.pathCollection1, 105.0, 31.0, 10.0, 30.0, 150.0);
            this.AddSingleFigure(this.pathCollection2, 25.0, 10.0, 30.0, 100.0, 195.0);
            this.AddSingleFigure(this.pathCollection2, 25.0, 10.0, 20.0, 100.0, 195.0);
            this.AddSingleFigure(this.pathCollection2, 25.0, 10.0, 10.0, 100.0, 195.0);
            this.AddSingleFigure(this.pathCollection2, 193.0, 28.0, 30.0, 15.0, 105.0);
            this.AddSingleFigure(this.pathCollection2, 193.0, 28.0, 20.0, 15.0, 105.0);
            this.AddSingleFigure(this.pathCollection2, 193.0, 28.0, 10.0, 15.0, 105.0);
        }

        [DebuggerNonUserCode, EditorBrowsable(EditorBrowsableState.Never)]
        void IComponentConnector.Connect(int connectionId, object target)
        {
            switch (connectionId)
            {
                case 1:
                    this._preview = (Image) target;
                    return;

                case 2:
                    this._pcPlayback = (System.Windows.Controls.ListBox) target;
                    return;

                case 3:
                    this._tone1Playback = (System.Windows.Controls.Button) target;
                    this._tone1Playback.Click += new RoutedEventHandler(this._tone1Play_Click);
                    return;

                case 4:
                    this._playerImage1 = (ImageCheckBox) target;
                    return;

                case 5:
                    this._tone2Playback = (System.Windows.Controls.Button) target;
                    this._tone2Playback.Click += new RoutedEventHandler(this._tone2Play_Click);
                    return;

                case 6:
                    this._playerImage2 = (ImageCheckBox) target;
                    return;

                case 7:
                    this.m_animCanvas = (Canvas) target;
                    return;

                case 8:
                    this.m_pathHp1 = (Path) target;
                    return;

                case 9:
                    this.pathCollection1 = (PathFigureCollection) target;
                    return;

                case 10:
                    this.m_pathHp2 = (Path) target;
                    return;

                case 11:
                    this.pathCollection4 = (PathFigureCollection) target;
                    return;

                case 12:
                    this.m_animCanvas2 = (Canvas) target;
                    return;

                case 13:
                    this.m_pathSp1 = (Path) target;
                    return;

                case 14:
                    this.pathCollection2 = (PathFigureCollection) target;
                    return;

                case 15:
                    this.m_pathSp2 = (Path) target;
                    return;

                case 0x10:
                    this.pathCollection3 = (PathFigureCollection) target;
                    return;
            }
            this._contentLoaded = true;
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            this.m_index++;
            if (this.m_index > 15)
            {
                this.m_index = 0;
            }
            if (this.m_Hp1Selected)
            {
                this.pathCollection1.Clear();
                this.AddSingleFigure(this.pathCollection1, (double) this.m_x1Hp1, (double) this.m_y1Hp1, (double) (this.m_index * 2), 30.0, 150.0);
                if ((this.m_index * 2) > 10)
                {
                    this.AddSingleFigure(this.pathCollection1, (double) this.m_x1Hp1, (double) this.m_y1Hp1, (double) ((this.m_index * 2) - 10), 30.0, 150.0);
                }
                if ((this.m_index * 2) > 20)
                {
                    this.AddSingleFigure(this.pathCollection1, (double) this.m_x1Hp1, (double) this.m_y1Hp1, (double) ((this.m_index * 2) - 20), 30.0, 150.0);
                }
                this.AddSingleFigure(this.pathCollection1, (double) this.m_x2Hp1, (double) this.m_y2Hp1, (double) (this.m_index * 2), 30.0, 150.0);
                if ((this.m_index * 2) > 10)
                {
                    this.AddSingleFigure(this.pathCollection1, (double) this.m_x2Hp1, (double) this.m_y2Hp1, (double) ((this.m_index * 2) - 10), 30.0, 150.0);
                }
                if ((this.m_index * 2) > 20)
                {
                    this.AddSingleFigure(this.pathCollection1, (double) this.m_x2Hp1, (double) this.m_y2Hp1, (double) ((this.m_index * 2) - 20), 30.0, 150.0);
                }
            }
            if (this.m_Sp1Selected)
            {
                this.pathCollection2.Clear();
                this.AddSingleFigure(this.pathCollection2, (double) this.m_x1Sp1, (double) this.m_y1Sp1, (double) (this.m_index * 2), 100.0, 195.0);
                if ((this.m_index * 2) > 10)
                {
                    this.AddSingleFigure(this.pathCollection2, (double) this.m_x1Sp1, (double) this.m_y1Sp1, (double) ((this.m_index * 2) - 10), 100.0, 195.0);
                }
                if ((this.m_index * 2) > 20)
                {
                    this.AddSingleFigure(this.pathCollection2, (double) this.m_x1Sp1, (double) this.m_y1Sp1, (double) ((this.m_index * 2) - 20), 100.0, 195.0);
                }
                this.AddSingleFigure(this.pathCollection2, (double) this.m_x2Sp1, (double) this.m_y2Sp1, (double) (this.m_index * 2), 15.0, 105.0);
                if ((this.m_index * 2) > 10)
                {
                    this.AddSingleFigure(this.pathCollection2, (double) this.m_x2Sp1, (double) this.m_y2Sp1, (double) ((this.m_index * 2) - 10), 15.0, 105.0);
                }
                if ((this.m_index * 2) > 20)
                {
                    this.AddSingleFigure(this.pathCollection2, (double) this.m_x2Sp1, (double) this.m_y2Sp1, (double) ((this.m_index * 2) - 20), 15.0, 105.0);
                }
            }
        }

        public CxHDAudioDirector AudioDirector
        {
            get => 
                this._audioDirector;
            set
            {
                this._audioDirector = value;
            }
        }

        public CxHDAudioAudioDirectorMode AudioDirectorMode
        {
            set
            {
                if (value == CxHDAudioAudioDirectorMode.SingleStream)
                {
                    if (this.IsHeadphonePresent)
                    {
                        this._preview.Style = (Style) base.FindResource("SA_AudioDirectorClassicModePreviewHP");
                    }
                    else
                    {
                        this._preview.Style = (Style) base.FindResource("SA_AudioDirectorClassicModePreviewNOHP");
                    }
                    this.changePreviewMode(CxHDAudioAudioDirectorMode.SingleStream);
                }
                else
                {
                    this._pcPlayback.Visibility = Visibility.Visible;
                    if (this.IsHeadphonePresent)
                    {
                        this._preview.Style = (Style) base.FindResource("SA_AudioDirectorMultistreamModePreviewHP");
                    }
                    else
                    {
                        this._preview.Style = (Style) base.FindResource("SA_AudioDirectorMultistreamModePreviewNOHP");
                    }
                    this.changePreviewMode(CxHDAudioAudioDirectorMode.MultiStream);
                }
            }
        }

        private CxHDAudioAudioDirectorMode CurrentAudioDirectorMode
        {
            get
            {
                CxHDAudioAudioDirectorMode singleStream = CxHDAudioAudioDirectorMode.SingleStream;
                try
                {
                    singleStream = this._audioDirector.AudioDirectorMode;
                }
                catch (Exception exception)
                {
                    SmartAudioLog.Log("AudioDirectorPreview::get_CurrentAudioDirectorMode() failed ", Severity.FATALERROR, exception);
                }
                return singleStream;
            }
        }

        public bool IsHeadphonePresent =>
            ((App) System.Windows.Application.Current).AudioFactory.DeviceIOConfig.HeadphonePresent;
    }
}

