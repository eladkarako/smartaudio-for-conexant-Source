namespace SmartAudio
{
    using CxHDAudioAPILib;
    using SmartAudio.Properties;
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Forms;
    using System.Windows.Input;
    using System.Windows.Markup;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using System.Windows.Threading;

    public class PreviewMultiChannelSpeakerSetup : System.Windows.Controls.UserControl, IHDAudioPreview, IComponentConnector
    {
        internal Canvas _animCanvas;
        private CxHDAudioAGC _audioAGC;
        private CxHDAudioChannelEnumeratorClass _audioChannelEnumerator;
        private Point _balance;
        private CxHDAudioChannel _centerChannel;
        internal Image _centerSpeaker;
        private bool _contentLoaded;
        private CxHDAudioConfig _cxHDAudioconfig;
        internal Image _floor;
        private MultiChannelBalance _frontChannels;
        internal Dot _head;
        private int _index;
        private bool _isDragging;
        private bool _isPositionAdjustable;
        private bool _isQuit;
        internal Image _leftFrontSpeaker;
        internal Image _leftMiddleSpeaker;
        internal Image _leftRearSpeaker;
        private CxHDMasterVolumeControl _masterVolumeControl;
        private double _maxRange;
        private double _minRange;
        internal ImageCheckBox _muteButton;
        internal PathFigureCollection _pathCollection1;
        internal Path _pathSp1;
        private AudioPlayer _player1;
        private MultiChannelBalance _rearChannels;
        internal Image _rightFrontSpeaker;
        internal Image _rightMiddleSpeaker;
        internal Image _rightRearSpeaker;
        private CxSpeakerConfigType _speakerConfiguration;
        private string _speakerID;
        private int _state;
        internal Image _subwoofer;
        private CxHDAudioChannel _subwooferChannel;
        private Point _subwooferPosition;
        private System.Windows.Forms.Timer _timer;
        internal Image _TV;
        private AutoResetEvent _updateEvent;
        private BackgroundWorker _workerThread;
        private int _x1Sp1 = -1;
        private const int _x1Sp1CM = 0x71;
        private const int _x1Sp2CM = 0x124;
        private const int _x1Sp3CM = 0xba;
        private const int _x1Sp4CM = 0xe9;
        private const int _x1Sp5CM = 0x49;
        private const int _x1Sp6CM = 0x159;
        private int _x2Sp1 = -1;
        private const int _x2Sp1CM = 0x71;
        private const int _x2Sp2CM = 0x124;
        private const int _x2Sp3CM = 0xc5;
        private const int _x2Sp5CM = 0x49;
        private const int _x2Sp6CM = 0x159;
        private int _x3Sp1 = -1;
        private const int _x3Sp1CM = 0x71;
        private const int _x3Sp2CM = 0x124;
        private const int _x3Sp3CM = 0xd0;
        private const int _x3Sp5CM = 0x49;
        private const int _x3Sp6CM = 0x159;
        private int _x4Sp1 = -1;
        private const int _x4Sp1CM = 0x71;
        private const int _x4Sp2CM = 0x124;
        private const int _x4Sp3CM = 0xdd;
        private const int _x4Sp5CM = 0x49;
        private const int _x4Sp6CM = 0x159;
        private int _y1Sp1 = -1;
        private const int _y1Sp1CM = 0x37;
        private const int _y1Sp2CM = 0x36;
        private const int _y1Sp3CM = 10;
        private const int _y1Sp4CM = 0x80;
        private const int _y1Sp5CM = 190;
        private const int _y1Sp6CM = 190;
        private int _y2Sp1 = -1;
        private const int _y2Sp1CM = 0x3e;
        private const int _y2Sp2CM = 0x3d;
        private const int _y2Sp3CM = 10;
        private const int _y2Sp5CM = 0xcd;
        private const int _y2Sp6CM = 0xcd;
        private int _y3Sp1 = -1;
        private const int _y3Sp1CM = 0x4b;
        private const int _y3Sp2CM = 0x4a;
        private const int _y3Sp3CM = 10;
        private const int _y3Sp5CM = 220;
        private const int _y3Sp6CM = 220;
        private int _y4Sp1 = -1;
        private const int _y4Sp1CM = 0x52;
        private const int _y4Sp2CM = 0x51;
        private const int _y4Sp3CM = 10;
        private const int _y4Sp5CM = 0xeb;
        private const int _y4Sp6CM = 0xeb;
        private Dot draggedDot;
        private Point startPointDot;
        private Point startPointMouse;
        private int[,] Transition = new int[,] { { 1, 1, 1 }, { -1, 4, 2 }, { -1, -1, 3 }, { -1, -1, 4 }, { -1, 5, 5 }, { -1, -1, -1 } };

        public event OnToneStoppedHandler OnToneStopped;

        public PreviewMultiChannelSpeakerSetup()
        {
            this.InitializeComponent();
            this._player1 = new AudioPlayer();
            this._player1.OnPlayerStopped += new PlayerStopped(this._player1_OnPlayerStopped);
            this._timer = new System.Windows.Forms.Timer();
            this._timer.Interval = 50;
            this._timer.Tick += new EventHandler(this._timer_Tick);
            RadialGradientBrush brush = new RadialGradientBrush {
                GradientStops = { 
                    new GradientStop(Colors.White, 0.0),
                    new GradientStop(Colors.Wheat, 1.0)
                }
            };
            brush.Freeze();
            this._pathSp1.Stroke = brush;
            this._frontChannels = new MultiChannelBalance();
            this._rearChannels = new MultiChannelBalance();
            this._subwooferPosition = new Point((Dot.RectArea.X + Dot.RectArea.Width) - 60.0, Dot.RectArea.Y + 40.0);
            this._updateEvent = new AutoResetEvent(false);
            base.Loaded += new RoutedEventHandler(this.PreviewMultiChannelSpeakerSetup_Loaded);
            base.Unloaded += new RoutedEventHandler(this.PreviewMultiChannelSpeakerSetup_Unloaded);
            this._head.Center = new Point(base.Width / 2.0, base.Height / 2.0);
            this._head.OnAutoAdjust = (SmartAudio.AutoAdjust) Delegate.Combine(this._head.OnAutoAdjust, new SmartAudio.AutoAdjust(this.AutoAdjust));
            this._isDragging = false;
        }

        [DebuggerNonUserCode]
        internal Delegate _CreateDelegate(System.Type delegateType, string handler) => 
            Delegate.CreateDelegate(delegateType, this, handler);

        private void _masterVolumeControl_OnMuted(int bValue, string context)
        {
            if (!this._isDragging && (this._audioAGC != null))
            {
                base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new OnMuteChanged(this.OnDeviceMuteStateChanged));
            }
        }

        private void _masterVolumeControl_OnVolumeChanged(double newValue, string context)
        {
            if (!this._isDragging)
            {
                App current = System.Windows.Application.Current as App;
                if (((current != null) && (current.AudioFactory.HostOperatingSystem != CxHostOperatingSystemType.WindowsXP)) && ((current.AudioFactory.HostOperatingSystem != CxHostOperatingSystemType.WindowsXP64) && this.IsPositionAdjustable))
                {
                    base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new AutoAdjustToCenter(this.OnAutoAdjustToCenter));
                }
            }
        }

        private void _muteButton_OnItemStateChanged(ImageCheckBox item, bool newState)
        {
            if (this._audioAGC != null)
            {
                this._audioAGC.SetEnabled(newState);
            }
        }

        private void _player1_OnPlayerStopped()
        {
            base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new UpdateOnPlayerStopped(this.OnUpdateOnPlayerStopped));
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            this._index++;
            if (this._index > 15)
            {
                this._index = 0;
            }
            this._pathCollection1.Clear();
            switch (this._state)
            {
                case 0:
                    this.TestSpeakers(30.0, 150.0);
                    return;

                case 1:
                    this.TestSpeakers(30.0, 150.0);
                    return;

                case 2:
                    this.TestSpeakers(30.0, 150.0);
                    return;

                case 3:
                    this.TestOneSpeaker();
                    return;

                case 4:
                    this.TestSpeakers(210.0, 330.0);
                    return;

                case 5:
                    this.TestSpeakers(210.0, 330.0);
                    return;
            }
            this._timer.Stop();
        }

        private void _workerThread_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!this._isQuit)
            {
                if (this._updateEvent.WaitOne(0x3e8, false))
                {
                    this.RefreshVolume();
                }
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

        public void AudioAjustToCenter()
        {
            this._head.ForceToCenter();
        }

        private void AutoAdjust()
        {
            try
            {
                double num;
                switch (this._speakerConfiguration)
                {
                    case CxSpeakerConfigType.StereoSpeakers:
                        num = Math.Max(this._frontChannels.LeftChannelVolume, this._frontChannels.RightChannelVolume);
                        this._frontChannels.LeftChannelVolume = num;
                        this._frontChannels.RightChannelVolume = num;
                        return;

                    case CxSpeakerConfigType.QuadSpeakers:
                    case CxSpeakerConfigType.FiveDotOneSpeakers:
                    {
                        double num2 = Math.Max(this._frontChannels.LeftChannelVolume, this._frontChannels.RightChannelVolume);
                        double num3 = Math.Max(this._rearChannels.LeftChannelVolume, this._rearChannels.RightChannelVolume);
                        num = Math.Max(num2, num3);
                        this._frontChannels.LeftChannelVolume = num;
                        this._frontChannels.RightChannelVolume = num;
                        this._rearChannels.LeftChannelVolume = num;
                        this._rearChannels.RightChannelVolume = num;
                        return;
                    }
                }
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("PreviewMultiChannelSpeakerSetup::AutoAdjust()", Severity.WARNING, exception);
            }
        }

        private double Distance(Point x1, Point x2) => 
            Math.Sqrt(Math.Pow(x1.X - x2.X, 2.0) + Math.Pow(x1.Y - x2.Y, 2.0));

        private string GetStreamFileName(int state)
        {
            SmartAudioSettings settings = ((App) System.Windows.Application.Current).Settings;
            switch (state)
            {
                case 0:
                    return settings.LeftSpeakerToneName;

                case 1:
                    return settings.RightSpeakerToneName;

                case 2:
                    return settings.CenterSpeakerToneName;

                case 3:
                    return settings.SubwooferToneName;

                case 4:
                    if (this._speakerConfiguration != CxSpeakerConfigType.QuadSpeakers)
                    {
                        return settings.RearLeftSpeakerToneName;
                    }
                    return settings.RearLeftSpeakerToneNameQuad;

                case 5:
                    if (this._speakerConfiguration != CxSpeakerConfigType.QuadSpeakers)
                    {
                        return settings.RearRightSpeakerToneName;
                    }
                    return settings.RearRightSpeakerToneNameQuad;
            }
            return "";
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (!this._contentLoaded)
            {
                this._contentLoaded = true;
                Uri resourceLocator = new Uri("/SmartAudio;component/previewmultichannelspeakersetup.xaml", UriKind.Relative);
                System.Windows.Application.LoadComponent(this, resourceLocator);
            }
        }

        public bool IsInternalSpeakerMuted()
        {
            if (this._audioAGC == null)
            {
                return false;
            }
            bool enabled = false;
            try
            {
                enabled = this._audioAGC.GetEnabled();
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("IsInternalSpeakerMuted(): Mute option not implemented", Severity.INFORMATION, exception);
            }
            return enabled;
        }

        public void Localize()
        {
            this._head.LocalizeToolTip();
            this._muteButton.ToolTip = this.IsInternalSpeakerMuted() ? Resources.SA_UnmuteInternalSpeaker : Resources.SA_MuteInternalSpeaker;
        }

        private void OnAutoAdjustToCenter()
        {
            this.Head.AutoAdjustToCenter();
        }

        private void OnDeviceMuteStateChanged()
        {
            if (this._muteButton.Visibility != Visibility.Hidden)
            {
                bool flag = this.IsInternalSpeakerMuted();
                this._muteButton.Selected = flag;
                this._muteButton.ToolTip = flag ? Resources.SA_UnmuteInternalSpeaker : Resources.SA_MuteInternalSpeaker;
            }
        }

        public void OnMasterVolumeChanged(double newValue)
        {
        }

        public void OnMasterVolumeChanging(double newValue)
        {
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs args)
        {
            base.OnMouseLeftButtonDown(args);
            this._isDragging = true;
            if (args.OriginalSource is Path)
            {
                DependencyObject parent = (args.OriginalSource as Path).Parent;
                if (parent is Dot)
                {
                    this.draggedDot = parent as Dot;
                    this.startPointDot = this.draggedDot.Center;
                    this.startPointMouse = args.GetPosition(this);
                    base.CaptureMouse();
                }
            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs args)
        {
            base.OnMouseLeftButtonUp(args);
            this._isDragging = false;
            base.ReleaseMouseCapture();
            this.draggedDot = null;
        }

        protected override void OnMouseMove(System.Windows.Input.MouseEventArgs args)
        {
            if (this._isDragging)
            {
                base.OnMouseMove(args);
                if (this.draggedDot != null)
                {
                    Point position = args.GetPosition(this);
                    Point point2 = new Point((this.startPointDot.X - this.startPointMouse.X) + position.X, (this.startPointDot.Y - this.startPointMouse.Y) + position.Y);
                    if (point2.X > (Dot.RectArea.X + Dot.RectArea.Width))
                    {
                        point2.X = Dot.RectArea.X + Dot.RectArea.Width;
                    }
                    if (point2.X < Dot.RectArea.X)
                    {
                        point2.X = Dot.RectArea.X;
                    }
                    if (point2.Y > (Dot.RectArea.Y + Dot.RectArea.Height))
                    {
                        point2.Y = Dot.RectArea.Y + Dot.RectArea.Height;
                    }
                    if (point2.Y < Dot.RectArea.Y)
                    {
                        point2.Y = Dot.RectArea.Y;
                    }
                    this.draggedDot.Center = point2;
                    this._balance = point2;
                    this._updateEvent.Set();
                }
            }
        }

        private void OnUpdateOnPlayerStopped()
        {
            this._pathCollection1.Clear();
            this._timer.Stop();
            if (this._state >= 0)
            {
                int num = this.Transition[this._state, ((int) this._speakerConfiguration) - 1];
                if (num >= 0)
                {
                    this._state = num;
                    this.SetupAnimation();
                    this._player1.Play(this._speakerID, this.GetStreamFileName(this._state), true, 2000.0);
                }
                else if (this.OnToneStopped != null)
                {
                    this.OnToneStopped();
                }
            }
        }

        public void PlayTestTone(string speakerID)
        {
            this._state = 0;
            this._speakerID = speakerID;
            this.SetupAnimation();
            if (this._speakerConfiguration < CxSpeakerConfigType.SevenDotOneSpeakers)
            {
                this._player1.Play(speakerID, this.GetStreamFileName(this._state), true, 2000.0);
            }
        }

        private void PreviewMultiChannelSpeakerSetup_Loaded(object sender, RoutedEventArgs e)
        {
            this._workerThread = new BackgroundWorker();
            this._workerThread.DoWork += new DoWorkEventHandler(this._workerThread_DoWork);
            this._isQuit = false;
            this._workerThread.RunWorkerAsync();
        }

        private void PreviewMultiChannelSpeakerSetup_Unloaded(object sender, RoutedEventArgs e)
        {
            this._isQuit = true;
        }

        private void RefreshVolume()
        {
            if (this._masterVolumeControl != null)
            {
                this._masterVolumeControl.remove_OnMuted(new _ICxHDAudioVolumeEvents_OnMutedEventHandler(this._masterVolumeControl_OnMuted));
                this._masterVolumeControl.remove_OnVolumeChanged(new _ICxHDAudioVolumeEvents_OnVolumeChangedEventHandler(this._masterVolumeControl_OnVolumeChanged));
            }
            Point point = this._balance;
            double num = this._frontChannels.Minimum + ((this._frontChannels.Maximum - this._frontChannels.Minimum) * (this.Distance(new Point(Dot.RectArea.X, Dot.RectArea.Y), point) / (this.Distance(new Point(Dot.RectArea.X, Dot.RectArea.Y), point) + this.Distance(point, new Point(Dot.RectArea.X + Dot.RectArea.Width, Dot.RectArea.Y)))));
            double num2 = this._rearChannels.Minimum + ((this._rearChannels.Maximum - this._rearChannels.Minimum) * (this.Distance(new Point(Dot.RectArea.X, Dot.RectArea.Y + Dot.RectArea.Height), point) / (this.Distance(new Point(Dot.RectArea.X, Dot.RectArea.Y + Dot.RectArea.Height), point) + this.Distance(point, new Point(Dot.RectArea.X + Dot.RectArea.Width, Dot.RectArea.Y + Dot.RectArea.Height)))));
            try
            {
                switch (this._speakerConfiguration)
                {
                    case CxSpeakerConfigType.StereoSpeakers:
                        this._frontChannels.Value = num;
                        goto Label_0236;

                    case CxSpeakerConfigType.QuadSpeakers:
                        this._frontChannels.Value = num;
                        this._rearChannels.Value = num2;
                        goto Label_0236;

                    case CxSpeakerConfigType.FiveDotOneSpeakers:
                        this._frontChannels.Value = num;
                        this._rearChannels.Value = num2;
                        goto Label_0236;
                }
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("PreviewMultiChannelSpeakerSetup::OnMouseMove()", Severity.WARNING, exception);
            }
        Label_0236:
            if (this._masterVolumeControl != null)
            {
                this._masterVolumeControl.add_OnMuted(new _ICxHDAudioVolumeEvents_OnMutedEventHandler(this._masterVolumeControl_OnMuted));
                this._masterVolumeControl.add_OnVolumeChanged(new _ICxHDAudioVolumeEvents_OnVolumeChangedEventHandler(this._masterVolumeControl_OnVolumeChanged));
            }
        }

        private void SetSpeakerMode(CxSpeakerConfigType type)
        {
            switch (type)
            {
                case CxSpeakerConfigType.StereoSpeakers:
                    this._centerSpeaker.Visibility = Visibility.Hidden;
                    this._subwoofer.Visibility = Visibility.Hidden;
                    this._leftFrontSpeaker.Visibility = Visibility.Visible;
                    this._rightFrontSpeaker.Visibility = Visibility.Visible;
                    this._leftRearSpeaker.Visibility = Visibility.Hidden;
                    this._rightRearSpeaker.Visibility = Visibility.Hidden;
                    this._leftMiddleSpeaker.Visibility = Visibility.Hidden;
                    this._rightMiddleSpeaker.Visibility = Visibility.Hidden;
                    return;

                case CxSpeakerConfigType.QuadSpeakers:
                    this._centerSpeaker.Visibility = Visibility.Hidden;
                    this._subwoofer.Visibility = Visibility.Hidden;
                    this._leftFrontSpeaker.Visibility = Visibility.Visible;
                    this._rightFrontSpeaker.Visibility = Visibility.Visible;
                    this._leftRearSpeaker.Visibility = Visibility.Visible;
                    this._rightRearSpeaker.Visibility = Visibility.Visible;
                    this._leftMiddleSpeaker.Visibility = Visibility.Hidden;
                    this._rightMiddleSpeaker.Visibility = Visibility.Hidden;
                    return;

                case CxSpeakerConfigType.FiveDotOneSpeakers:
                    this._centerSpeaker.Visibility = Visibility.Visible;
                    this._subwoofer.Visibility = Visibility.Visible;
                    this._leftFrontSpeaker.Visibility = Visibility.Visible;
                    this._rightFrontSpeaker.Visibility = Visibility.Visible;
                    this._leftRearSpeaker.Visibility = Visibility.Visible;
                    this._rightRearSpeaker.Visibility = Visibility.Visible;
                    this._leftMiddleSpeaker.Visibility = Visibility.Hidden;
                    this._rightMiddleSpeaker.Visibility = Visibility.Hidden;
                    return;

                case CxSpeakerConfigType.SevenDotOneSpeakers:
                    this._centerSpeaker.Visibility = Visibility.Visible;
                    this._subwoofer.Visibility = Visibility.Visible;
                    this._leftFrontSpeaker.Visibility = Visibility.Visible;
                    this._rightFrontSpeaker.Visibility = Visibility.Visible;
                    this._leftRearSpeaker.Visibility = Visibility.Visible;
                    this._rightRearSpeaker.Visibility = Visibility.Visible;
                    this._leftMiddleSpeaker.Visibility = Visibility.Visible;
                    this._rightMiddleSpeaker.Visibility = Visibility.Visible;
                    return;
            }
        }

        private void SetupAnimation()
        {
            switch (this._state)
            {
                case 0:
                    this._x1Sp1 = 0x71;
                    this._y1Sp1 = 0x37;
                    this._x2Sp1 = 0x71;
                    this._y2Sp1 = 0x3e;
                    this._x3Sp1 = 0x71;
                    this._y3Sp1 = 0x4b;
                    this._x4Sp1 = 0x71;
                    this._y4Sp1 = 0x52;
                    break;

                case 1:
                    this._x1Sp1 = 0x124;
                    this._y1Sp1 = 0x36;
                    this._x2Sp1 = 0x124;
                    this._y2Sp1 = 0x3d;
                    this._x3Sp1 = 0x124;
                    this._y3Sp1 = 0x4a;
                    this._x4Sp1 = 0x124;
                    this._y4Sp1 = 0x51;
                    break;

                case 2:
                    this._x1Sp1 = 0xba;
                    this._y1Sp1 = 10;
                    this._x2Sp1 = 0xc5;
                    this._y2Sp1 = 10;
                    this._x3Sp1 = 0xd0;
                    this._y3Sp1 = 10;
                    this._x4Sp1 = 0xdd;
                    this._y4Sp1 = 10;
                    break;

                case 3:
                    this._x1Sp1 = 0xe9;
                    this._y1Sp1 = 0x80;
                    break;

                case 4:
                    this._x1Sp1 = 0x49;
                    this._y1Sp1 = 190;
                    this._x2Sp1 = 0x49;
                    this._y2Sp1 = 0xcd;
                    this._x3Sp1 = 0x49;
                    this._y3Sp1 = 220;
                    this._x4Sp1 = 0x49;
                    this._y4Sp1 = 0xeb;
                    break;

                case 5:
                    this._x1Sp1 = 0x159;
                    this._y1Sp1 = 190;
                    this._x2Sp1 = 0x159;
                    this._y2Sp1 = 0xcd;
                    this._x3Sp1 = 0x159;
                    this._y3Sp1 = 220;
                    this._x4Sp1 = 0x159;
                    this._y4Sp1 = 0xeb;
                    break;
            }
            this._index = 0;
            this._timer.Start();
        }

        public void StopTestTone(string speakerID)
        {
            this._player1.Stop();
            this._state = -1;
            this._pathCollection1.Clear();
            this._timer.Stop();
            this._index = 0;
        }

        [EditorBrowsable(EditorBrowsableState.Never), DebuggerNonUserCode]
        void IComponentConnector.Connect(int connectionId, object target)
        {
            switch (connectionId)
            {
                case 1:
                    this._floor = (Image) target;
                    return;

                case 2:
                    this._centerSpeaker = (Image) target;
                    return;

                case 3:
                    this._subwoofer = (Image) target;
                    return;

                case 4:
                    this._leftFrontSpeaker = (Image) target;
                    return;

                case 5:
                    this._rightFrontSpeaker = (Image) target;
                    return;

                case 6:
                    this._TV = (Image) target;
                    return;

                case 7:
                    this._animCanvas = (Canvas) target;
                    return;

                case 8:
                    this._pathSp1 = (Path) target;
                    return;

                case 9:
                    this._pathCollection1 = (PathFigureCollection) target;
                    return;

                case 10:
                    this._leftRearSpeaker = (Image) target;
                    return;

                case 11:
                    this._rightRearSpeaker = (Image) target;
                    return;

                case 12:
                    this._leftMiddleSpeaker = (Image) target;
                    return;

                case 13:
                    this._rightMiddleSpeaker = (Image) target;
                    return;

                case 14:
                    this._head = (Dot) target;
                    return;

                case 15:
                    this._muteButton = (ImageCheckBox) target;
                    return;
            }
            this._contentLoaded = true;
        }

        public void TestOneSpeaker()
        {
            this.AddSingleFigure(this._pathCollection1, (double) this._x1Sp1, (double) this._y1Sp1, (double) (this._index * 2), 30.0, 150.0);
            if ((this._index * 2) > 10)
            {
                this.AddSingleFigure(this._pathCollection1, (double) this._x1Sp1, (double) this._y1Sp1, (double) ((this._index * 2) - 10), 30.0, 150.0);
            }
            if ((this._index * 2) > 20)
            {
                this.AddSingleFigure(this._pathCollection1, (double) this._x1Sp1, (double) this._y1Sp1, (double) ((this._index * 2) - 20), 30.0, 150.0);
            }
        }

        public void TestSpeakers(double angle1, double angle2)
        {
            this.AddSingleFigure(this._pathCollection1, (double) this._x1Sp1, (double) this._y1Sp1, (double) (this._index * 2), angle1, angle2);
            if ((this._index * 2) > 10)
            {
                this.AddSingleFigure(this._pathCollection1, (double) this._x1Sp1, (double) this._y1Sp1, (double) ((this._index * 2) - 10), angle1, angle2);
            }
            if ((this._index * 2) > 20)
            {
                this.AddSingleFigure(this._pathCollection1, (double) this._x1Sp1, (double) this._y1Sp1, (double) ((this._index * 2) - 20), angle1, angle2);
            }
            this.AddSingleFigure(this._pathCollection1, (double) this._x2Sp1, (double) this._y2Sp1, (double) (this._index * 2), angle1, angle2);
            if ((this._index * 2) > 10)
            {
                this.AddSingleFigure(this._pathCollection1, (double) this._x2Sp1, (double) this._y2Sp1, (double) ((this._index * 2) - 10), angle1, angle2);
            }
            if ((this._index * 2) > 20)
            {
                this.AddSingleFigure(this._pathCollection1, (double) this._x2Sp1, (double) this._y2Sp1, (double) ((this._index * 2) - 20), angle1, angle2);
            }
            this.AddSingleFigure(this._pathCollection1, (double) this._x3Sp1, (double) this._y3Sp1, (double) (this._index * 2), angle1, angle2);
            if ((this._index * 2) > 10)
            {
                this.AddSingleFigure(this._pathCollection1, (double) this._x3Sp1, (double) this._y3Sp1, (double) ((this._index * 2) - 10), angle1, angle2);
            }
            if ((this._index * 2) > 20)
            {
                this.AddSingleFigure(this._pathCollection1, (double) this._x3Sp1, (double) this._y3Sp1, (double) ((this._index * 2) - 20), angle1, angle2);
            }
            this.AddSingleFigure(this._pathCollection1, (double) this._x4Sp1, (double) this._y4Sp1, (double) (this._index * 2), angle1, angle2);
            if ((this._index * 2) > 10)
            {
                this.AddSingleFigure(this._pathCollection1, (double) this._x4Sp1, (double) this._y4Sp1, (double) ((this._index * 2) - 10), angle1, angle2);
            }
            if ((this._index * 2) > 20)
            {
                this.AddSingleFigure(this._pathCollection1, (double) this._x4Sp1, (double) this._y4Sp1, (double) ((this._index * 2) - 20), angle1, angle2);
            }
        }

        private void UpdatePosition()
        {
            try
            {
                Point point = new Point();
                if (this._centerChannel != null)
                {
                    point.Y = Dot.RectArea.Y + ((Dot.RectArea.Height * (this._centerChannel.VolumeControl.Volume - this.MinRange)) / (this.MaxRange - this.MinRange));
                }
                else
                {
                    point.Y = Dot.RectArea.Y + (Dot.RectArea.Height / 2.0);
                }
                point.X = Dot.RectArea.X + ((Dot.RectArea.Width * (this._frontChannels.Value - this._frontChannels.Minimum)) / (this._frontChannels.Maximum - this._frontChannels.Minimum));
                this._head.Center = point;
            }
            catch (Exception exception)
            {
                this._head.Center = new Point(Dot.RectArea.X + (Dot.RectArea.Width / 2.0), Dot.RectArea.Y + (Dot.RectArea.Height / 2.0));
                SmartAudioLog.Log("SmartAudioSettings::UpdatePosition(): Failed to compute the position of the head. " + exception.ToString(), new object[] { Severity.FATALERROR });
            }
        }

        public CxHDAudioAGC AudioAGC
        {
            get => 
                this._audioAGC;
            set
            {
                this._audioAGC = value;
                if (this._audioAGC != null)
                {
                    this._muteButton.Visibility = Visibility.Visible;
                    this._muteButton.Selected = this._audioAGC.GetEnabled();
                }
                else
                {
                    this._muteButton.Visibility = Visibility.Hidden;
                }
            }
        }

        public CxHDAudioChannelEnumeratorClass AudioChannelEnumerator
        {
            get => 
                this._audioChannelEnumerator;
            set
            {
                this._audioChannelEnumerator = value;
                if (this._audioChannelEnumerator.Count == 3)
                {
                    this._frontChannels.AddChannel((CxHDAudioChannel) this._audioChannelEnumerator[1], ChannelType.LeftChannel);
                    this._frontChannels.AddChannel((CxHDAudioChannel) this._audioChannelEnumerator[2], ChannelType.RightChannel);
                }
                else if (this._audioChannelEnumerator.Count == 6)
                {
                    this._frontChannels.AddChannel((CxHDAudioChannel) this._audioChannelEnumerator[1], ChannelType.LeftChannel);
                    this._frontChannels.AddChannel((CxHDAudioChannel) this._audioChannelEnumerator[2], ChannelType.RightChannel);
                    this._rearChannels.AddChannel((CxHDAudioChannel) this._audioChannelEnumerator[6], ChannelType.RightChannel);
                    this._rearChannels.AddChannel((CxHDAudioChannel) this._audioChannelEnumerator[5], ChannelType.LeftChannel);
                    this._centerChannel = (CxHDAudioChannel) this._audioChannelEnumerator[3];
                    this._subwooferChannel = (CxHDAudioChannel) this._audioChannelEnumerator[4];
                }
            }
        }

        public CxHDAudioChannel CenterChannel
        {
            get => 
                this._centerChannel;
            set
            {
                this._centerChannel = value;
            }
        }

        public CxHDAudioConfig HDAudioconfig
        {
            get => 
                this._cxHDAudioconfig;
            set
            {
                this._cxHDAudioconfig = value;
            }
        }

        public Dot Head =>
            this._head;

        public bool IsPositionAdjustable
        {
            get => 
                this._isPositionAdjustable;
            set
            {
                this._isPositionAdjustable = value;
                this._head.Visibility = this._isPositionAdjustable ? Visibility.Visible : Visibility.Hidden;
            }
        }

        public CxHDMasterVolumeControl MasterVolume
        {
            get => 
                this._masterVolumeControl;
            set
            {
                double num;
                double num2;
                uint num3;
                uint num4;
                if (this._masterVolumeControl != null)
                {
                    this._masterVolumeControl.remove_OnMuted(new _ICxHDAudioVolumeEvents_OnMutedEventHandler(this._masterVolumeControl_OnMuted));
                    this._masterVolumeControl.remove_OnVolumeChanged(new _ICxHDAudioVolumeEvents_OnVolumeChangedEventHandler(this._masterVolumeControl_OnVolumeChanged));
                }
                this._masterVolumeControl = value;
                this._frontChannels.MasterVolumeControlClass = value;
                this._rearChannels.MasterVolumeControlClass = value;
                this._masterVolumeControl.GetInternalRange(out num, out num2, out num3, out num4);
                this.MaxRange = num2;
                this.MinRange = num;
                this.UpdatePosition();
                if (this._masterVolumeControl != null)
                {
                    this._masterVolumeControl.add_OnMuted(new _ICxHDAudioVolumeEvents_OnMutedEventHandler(this._masterVolumeControl_OnMuted));
                    this._masterVolumeControl.add_OnVolumeChanged(new _ICxHDAudioVolumeEvents_OnVolumeChangedEventHandler(this._masterVolumeControl_OnVolumeChanged));
                }
            }
        }

        public double MaxRange
        {
            get => 
                this._maxRange;
            set
            {
                this._maxRange = value;
            }
        }

        public double MinRange
        {
            get => 
                this._minRange;
            set
            {
                this._minRange = value;
            }
        }

        public CxSpeakerConfigType SpeakerConfiguration
        {
            get => 
                this._speakerConfiguration;
            set
            {
                this._speakerConfiguration = value;
                this.SetSpeakerMode(this._speakerConfiguration);
                if (this._cxHDAudioconfig != null)
                {
                    try
                    {
                        this._cxHDAudioconfig.SetSpeakerConfig(((App) System.Windows.Application.Current).SPAudioEndPoint.FriendlyName, this._speakerConfiguration);
                    }
                    catch (Exception exception)
                    {
                        SmartAudioLog.Log("PreviewMultiChannelSpeakerSetup::SpeakerConfiguration: Failed to change the speaker configuration. " + exception.ToString(), new object[] { Severity.FATALERROR });
                    }
                }
            }
        }

        public CxHDAudioChannel SubwooferChannel
        {
            get => 
                this._subwooferChannel;
            set
            {
                this._subwooferChannel = value;
            }
        }

        private delegate void AutoAdjustToCenter();

        public delegate void OnToneStoppedHandler();

        private delegate void UpdateOnPlayerStopped();
    }
}

