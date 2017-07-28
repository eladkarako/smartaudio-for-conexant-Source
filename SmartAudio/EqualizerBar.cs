namespace SmartAudio
{
    using CxHDAudioAPILib;
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Markup;

    public class EqualizerBar : UserControl, IComponentConnector
    {
        private bool _contentLoaded;
        private CxEQBand _eqBand;
        internal System.Windows.Controls.Label _label;
        internal Grid _mainGrid;
        private bool _showAdvancedSettings;
        private bool _updating;
        internal Slider _volumeSlider;

        public event OnEQBandChanged OnEQBandValueChanged;

        public EqualizerBar()
        {
            this.InitializeComponent();
            this._volumeSlider.ValueChanged += new RoutedPropertyChangedEventHandler<double>(this._volumeSlider_ValueChanged);
            base.Focusable = true;
            this._updating = false;
            Keyboard.Focus(this._volumeSlider);
            this._volumeSlider.Focusable = true;
        }

        private void _volumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!this._updating && (this.OnEQBandValueChanged != null))
            {
                double num = this._volumeSlider.Value;
                if (this._eqBand != null)
                {
                    this._eqBand.Gain = num;
                }
                this.OnEQBandValueChanged(this, this._volumeSlider.Value);
            }
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (!this._contentLoaded)
            {
                this._contentLoaded = true;
                Uri resourceLocator = new Uri("/SmartAudio;component/equalizerbar.xaml", UriKind.Relative);
                Application.LoadComponent(this, resourceLocator);
            }
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            Keyboard.Focus(this._volumeSlider);
        }

        public void Refresh()
        {
            this._updating = true;
            if (this._eqBand != null)
            {
                this._volumeSlider.Value = this._eqBand.Gain;
            }
            this._updating = false;
        }

        public void SetFocus()
        {
            Keyboard.Focus(this._volumeSlider);
        }

        [DebuggerNonUserCode, EditorBrowsable(EditorBrowsableState.Never)]
        void IComponentConnector.Connect(int connectionId, object target)
        {
            switch (connectionId)
            {
                case 1:
                    this._mainGrid = (Grid) target;
                    return;

                case 2:
                    this._volumeSlider = (Slider) target;
                    return;

                case 3:
                    this._label = (System.Windows.Controls.Label) target;
                    return;
            }
            this._contentLoaded = true;
        }

        public CxEQBand EQBand
        {
            get => 
                this._eqBand;
            set
            {
                this._eqBand = value;
                if (value != null)
                {
                    this._updating = true;
                    this._volumeSlider.ValueChanged -= new RoutedPropertyChangedEventHandler<double>(this._volumeSlider_ValueChanged);
                    this._volumeSlider.Value = this._eqBand.Gain;
                    this._volumeSlider.ValueChanged += new RoutedPropertyChangedEventHandler<double>(this._volumeSlider_ValueChanged);
                    this._updating = false;
                }
                else
                {
                    this._volumeSlider.Value = 0.0;
                }
            }
        }

        public string Label
        {
            get => 
                ((string) this._label.Content);
            set
            {
                this._label.Content = value;
            }
        }

        public bool ShowAdvancedSettings
        {
            get => 
                this._showAdvancedSettings;
            set
            {
                this._showAdvancedSettings = value;
            }
        }
    }
}

