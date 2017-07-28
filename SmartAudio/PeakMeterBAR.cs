namespace SmartAudio
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Shapes;

    public class PeakMeterBAR : StackPanel
    {
        private Brush _backColor;
        private List<Brush> _brushes;
        private Brush _colorHigh;
        private int _currentIndex;
        public double _falloff;
        public int _falloffSpeed = 10;
        private double _largeChange;
        private int _LEDCount = 0x10;
        private double _maximum;
        private double _medRangeDefault = 80.0;
        private double _medRangeValue;
        private double _minimum;
        private double _minRangeValue;
        private Brush _peakColor;
        private List<Rectangle> _rectangles;
        private double _smallChange;
        private int _speed;
        private double _value;
        private const double cxyMargin = 4.0;

        static PeakMeterBAR()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(PeakMeterBAR), new FrameworkPropertyMetadata(typeof(PeakMeterBAR)));
        }

        public PeakMeterBAR(List<Brush> brushes)
        {
            this._brushes = brushes;
            this._rectangles = new List<Rectangle>();
            LinearGradientBrush brush = new LinearGradientBrush {
                StartPoint = new Point(0.0, 0.0),
                EndPoint = new Point(1.0, 1.0),
                GradientStops = { 
                    new GradientStop((Color) ColorConverter.ConvertFromString("#71abff"), 0.0),
                    new GradientStop((Color) ColorConverter.ConvertFromString("#24245A"), 1.0)
                }
            };
            this._backColor = new SolidColorBrush((Color) ColorConverter.ConvertFromString("#1061d4"));
            LinearGradientBrush brush2 = new LinearGradientBrush {
                StartPoint = new Point(0.0, 0.0),
                EndPoint = new Point(1.0, 1.0),
                GradientStops = { 
                    new GradientStop((Color) ColorConverter.ConvertFromString("#B28DBDFF"), 0.0),
                    new GradientStop((Color) ColorConverter.ConvertFromString("#008DBDFF"), 1.5)
                }
            };
            this._colorHigh = new SolidColorBrush((Color) ColorConverter.ConvertFromString("#0F123F"));
            brush2 = new LinearGradientBrush {
                StartPoint = new Point(0.0, 0.0),
                EndPoint = new Point(0.0, 1.0),
                GradientStops = { 
                    new GradientStop(Colors.Silver, 0.0),
                    new GradientStop(Colors.White, 1.0)
                }
            };
            this._peakColor = brush2;
        }

        private void CreateLEDs()
        {
            this._rectangles.Clear();
            double num = (this.MedRangeValue == 0.0) ? Math.Abs((double) (this.Maximum - this.Minimum)) : this.Maximum;
            int num2 = (this.LEDCount > 1) ? this.LEDCount : ((int) ((num * this.DecrementPercent) / 100.0));
            double num1 = (this.Maximum * num2) / num;
            double num4 = (this.MedRangeDefault * num2) / num;
            if (this.MedRangeValue == 0.0)
            {
                double num5 = (Math.Abs(this.Minimum) * num2) / num;
            }
            Brush backColor = this.BackColor;
            new Pen(this.GridPenColor, 0.0);
            new Pen(this.FalloffColor, 0.0);
            Rect rect = new Rect(0.0, 0.0, base.Width, base.Height);
            Size size = new Size(base.Width, base.Height / ((double) num2));
            Rect rect2 = new Rect(rect.X, rect.Y, size.Width, size.Height);
            for (int i = 0; i < num2; i++)
            {
                Rectangle element = new Rectangle {
                    Stroke = this.GridColor,
                    Fill = this.GridColor,
                    Height = rect2.Height - 4.0,
                    Width = rect2.Width
                };
                base.Children.Add(element);
                this._rectangles.Add(element);
                if (i < (num2 - 1))
                {
                    element = new Rectangle {
                        Stroke = Brushes.Transparent,
                        Fill = Brushes.Transparent,
                        Height = 4.0,
                        Width = rect2.Width
                    };
                    base.Children.Add(element);
                }
            }
        }

        public void Initialize(double min, double medRange, double maximum)
        {
            this._value = maximum;
            this._minimum = min;
            this._maximum = maximum;
            this._minRangeValue = 0.0;
            this._medRangeValue = 0.0;
            this._medRangeValue = 0.0;
            this._falloff = maximum;
            this._speed = this._falloffSpeed;
            this.CreateLEDs();
        }

        protected static bool InRange(double value, double rangeMin, double rangeMax) => 
            ((value >= rangeMin) && (value <= rangeMax));

        private double Max(double val1, double val2)
        {
            if (val1 <= val2)
            {
                return val2;
            }
            return val1;
        }

        private double Min(double val1, double val2)
        {
            if (val1 <= val2)
            {
                return val2;
            }
            return val1;
        }

        protected void OnRender()
        {
            if (this._rectangles.Count != 0)
            {
                if (this.MedRangeValue != 0.0)
                {
                    double maximum = this.Maximum;
                }
                else
                {
                    Math.Abs((double) (this.Maximum - this.Minimum));
                }
                double num = (this.MedRangeValue == 0.0) ? Math.Abs((double) (this.Maximum - this.Minimum)) : this.Maximum;
                int lEDCount = this.LEDCount;
                double rangeMax = (this._minRangeValue * lEDCount) / num;
                double rangeMin = (this._minRangeValue * lEDCount) / num;
                double num5 = lEDCount;
                if (this.MedRangeValue == 0.0)
                {
                    rangeMin = (Math.Abs(this._minRangeValue) * lEDCount) / num;
                    rangeMax = 0.0;
                }
                int num7 = (int) Math.Abs((double) ((this.Value * lEDCount) / num));
                double num8 = Math.Abs((double) ((this._falloff * lEDCount) / num));
                Math.Abs((double) ((this.Maximum * lEDCount) / num));
                Math.Abs((double) ((this.Minimum * lEDCount) / num));
                for (int i = 0; i < lEDCount; i++)
                {
                    this._currentIndex = i;
                    Brush backColor = this.BackColor;
                    if (this.MedRangeValue == 0.0)
                    {
                        double num10 = rangeMin + num7;
                        if (InRange((double) i, num10, rangeMin - 1.0))
                        {
                            backColor = this.ColorNormal;
                        }
                        else if ((i >= rangeMin) && InRange((double) i, rangeMin, num10))
                        {
                            backColor = this.ColorHigh;
                        }
                        else
                        {
                            backColor = (i < rangeMin) ? this.ColorNormalBack : this.ColorHighBack;
                        }
                    }
                    else if (num7 < i)
                    {
                        if (this.ColoredGrid)
                        {
                            if (InRange((double) i, 0.0, rangeMax))
                            {
                                backColor = this.ColorNormalBack;
                            }
                            else if (InRange((double) i, rangeMax + 1.0, rangeMin))
                            {
                                backColor = this.ColorMediumBack;
                            }
                            else if (InRange((double) i, rangeMin + 1.0, num5))
                            {
                                backColor = this.ColorHighBack;
                            }
                        }
                    }
                    else if (InRange((double) i, 0.0, rangeMax))
                    {
                        backColor = this.ColorNormal;
                    }
                    else if (InRange((double) i, rangeMax + 1.0, rangeMin))
                    {
                        backColor = this.ColorMedium;
                    }
                    else if (InRange((double) i, rangeMin + 1.0, num5))
                    {
                        backColor = this.ColorHigh;
                    }
                    this._rectangles[i].Stroke = Brushes.Transparent;
                    this._rectangles[i].Fill = backColor;
                }
                if (num8 < lEDCount)
                {
                    this._rectangles[(int) num8].Fill = this._peakColor;
                }
            }
        }

        public void UpdateFallOff()
        {
            bool flag = true;
            if (this._value > this.Minimum)
            {
                int num = ((int) (this.Maximum - this.Minimum)) / this.LEDCount;
                this._value -= num >> 1;
                if (this._value < this.Minimum)
                {
                    this._value = this.Minimum;
                }
                flag = false;
            }
            if (this._speed > 0)
            {
                this._speed--;
                flag = false;
            }
            if ((this._speed == 0) && (this._falloff > this.Minimum))
            {
                int num2 = ((int) (this.Maximum - this.Minimum)) / this.LEDCount;
                this._falloff -= num2 >> 1;
                if (this._falloff < this.Minimum)
                {
                    this._falloff = this.Minimum;
                }
                flag = false;
            }
            if (!flag)
            {
                this.OnRender();
            }
        }

        private Brush BackColor
        {
            get
            {
                if (this._currentIndex >= this._brushes.Count)
                {
                    return this._backColor;
                }
                return this._brushes[this._currentIndex];
            }
        }

        private bool ColoredGrid =>
            true;

        private Brush ColorHigh =>
            this._colorHigh;

        private Brush ColorHighBack =>
            this.BackColor;

        private Brush ColorMedium =>
            Brushes.LightBlue;

        private Brush ColorMediumBack =>
            Brushes.LightBlue;

        private Brush ColorNormal =>
            Brushes.LightBlue;

        private Brush ColorNormalBack =>
            Brushes.CornflowerBlue;

        public double DecrementPercent =>
            Math.Abs((double) (this.Maximum - (this.Minimum / ((double) this.LEDCount))));

        public double Falloff
        {
            get => 
                this._falloff;
            set
            {
                this._falloff = value;
            }
        }

        private Brush FalloffColor =>
            Brushes.CornflowerBlue;

        private Brush GridColor =>
            this.BackColor;

        private Brush GridPenColor =>
            Brushes.CornflowerBlue;

        public double LargeChange
        {
            get => 
                this._largeChange;
            set
            {
                this._largeChange = value;
            }
        }

        public int LEDCount
        {
            get => 
                this._LEDCount;
            set
            {
                this._LEDCount = value;
            }
        }

        public double Maximum
        {
            get => 
                this._maximum;
            set
            {
                this._maximum = value;
            }
        }

        public double MedRangeDefault
        {
            get
            {
                this._medRangeDefault = (this.Maximum + this.Minimum) / 2.0;
                return this._medRangeDefault;
            }
            set
            {
                this._medRangeDefault = value;
            }
        }

        public double MedRangeValue
        {
            get => 
                this._medRangeValue;
            set
            {
                this._medRangeValue = value;
            }
        }

        public double Minimum
        {
            get => 
                this._minimum;
            set
            {
                this._minimum = value;
            }
        }

        public double MinRangeValue
        {
            get => 
                this._minRangeValue;
            set
            {
                this._minRangeValue = value;
            }
        }

        public double SmallChange
        {
            get => 
                this._smallChange;
            set
            {
                this._smallChange = value;
            }
        }

        public double Value
        {
            get => 
                this._value;
            set
            {
                if (((value >= this.Minimum) && (value <= this.Maximum)) && (this._value < value))
                {
                    this._value = value;
                    if (this._falloff < this._value)
                    {
                        this._falloff = value;
                        this._speed = this._falloffSpeed;
                    }
                }
            }
        }
    }
}

