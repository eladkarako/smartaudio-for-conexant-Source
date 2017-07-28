namespace SmartAudio
{
    using CxHDAudioAPILib;
    using System;

    public class ChannelBalance : IChannelBalance
    {
        private CxHDAudioChannelEnumeratorClass _audioChannelEnumerator;
        private CxHDMasterVolumeControl _audioMasterVolumeControl;
        private double _delataOverMax;
        private double _faderSpan;
        private double _faderStep;
        private CxHDAudioChannel _leftChannel;
        private double _maximum;
        private double _minimum;
        private CxHDAudioChannel _rightChannel;
        private double _sliderSpan;

        public ChannelBalance()
        {
            this._minimum = this._maximum = 0.0;
        }

        private double Max(double value1, double value2)
        {
            if (value1 <= value2)
            {
                return value2;
            }
            return value1;
        }

        private double Min(double value1, double value2)
        {
            if (value1 >= value2)
            {
                return value2;
            }
            return value1;
        }

        public CxHDAudioChannelEnumeratorClass AudioChannelEmulator
        {
            get => 
                this._audioChannelEnumerator;
            set
            {
                this._audioChannelEnumerator = value;
                this._leftChannel = (CxHDAudioChannel) this.AudioChannelEmulator[1];
                this._rightChannel = (CxHDAudioChannel) this.AudioChannelEmulator[2];
            }
        }

        public CxHDMasterVolumeControl MasterVolumeControlClass
        {
            get => 
                this._audioMasterVolumeControl;
            set
            {
                this._audioMasterVolumeControl = value;
                if (this._audioMasterVolumeControl != null)
                {
                    double minRange = 0.0;
                    double maxRange = 0.0;
                    uint steps = 0;
                    uint stepCount = 0;
                    this._audioMasterVolumeControl.GetInternalRange(out minRange, out maxRange, out steps, out stepCount);
                    this._faderSpan = maxRange - minRange;
                    if (steps == 0)
                    {
                        steps = 1;
                    }
                    this._faderStep = this._faderSpan / ((double) steps);
                    this._sliderSpan = steps;
                    this._minimum = 0.0;
                    this._maximum = this._sliderSpan;
                }
            }
        }

        public double Maximum =>
            this._maximum;

        public double Minimum =>
            this._minimum;

        public double Value
        {
            get
            {
                double num = 0.0;
                if (this._rightChannel.VolumeControl.Volume == this._leftChannel.VolumeControl.Volume)
                {
                    this._delataOverMax = 0.0;
                }
                else
                {
                    num = this.Max(this._rightChannel.VolumeControl.Volume, this._leftChannel.VolumeControl.Volume);
                    if (num > 0.0)
                    {
                        this._delataOverMax = (this._rightChannel.VolumeControl.Volume - this._leftChannel.VolumeControl.Volume) / num;
                    }
                }
                return ((this._sliderSpan * (this._delataOverMax + 1.0)) / 2.0);
            }
            set
            {
                double num = value * this._faderStep;
                double num2 = 0.0;
                num2 = this.Max(this._rightChannel.VolumeControl.Volume, this._leftChannel.VolumeControl.Volume);
                if (num > (this._faderSpan / 2.0))
                {
                    this._rightChannel.VolumeControl.Volume = num2;
                    this._leftChannel.VolumeControl.Volume = Math.Ceiling((double) ((2.0 * num2) * (1.0 - (num / this._faderSpan))));
                }
                else
                {
                    this._rightChannel.VolumeControl.Volume = Math.Ceiling((double) ((2.0 * num2) * (num / this._faderSpan)));
                    this._leftChannel.VolumeControl.Volume = num2;
                }
            }
        }
    }
}

