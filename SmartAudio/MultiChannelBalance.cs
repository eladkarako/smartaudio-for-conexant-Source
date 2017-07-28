namespace SmartAudio
{
    using CxHDAudioAPILib;
    using System;
    using System.Collections.Generic;

    public class MultiChannelBalance : IChannelBalance
    {
        private CxHDAudioChannelEnumeratorClass _audioChannelEnumerator;
        private CxHDMasterVolumeControl _audioMasterVolumeControl;
        private List<ChannelInfo> _channels;
        private double _delataOverMax;
        private double _faderSpan;
        private double _faderStep;
        private double _maximum;
        private double _minimum;
        private double _sliderSpan;

        public MultiChannelBalance()
        {
            this._minimum = this._maximum = 0.0;
            this._channels = new List<ChannelInfo>();
        }

        public void AddChannel(CxHDAudioChannel channel, ChannelType type)
        {
            this._channels.Add(new ChannelInfo(channel, type));
        }

        private double GetChannel(ChannelType type)
        {
            double num = 0.0;
            int num2 = 0;
            foreach (ChannelInfo info in this._channels)
            {
                if (info.ChannelType == type)
                {
                    num += info.Channel.VolumeControl.Volume;
                    num2++;
                }
            }
            if (num2 <= 0)
            {
                return 0.0;
            }
            return (num / ((double) num2));
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

        private void SetChannel(ChannelType type, double value)
        {
            foreach (ChannelInfo info in this._channels)
            {
                if (info.ChannelType == type)
                {
                    info.Channel.VolumeControl.Volume = value;
                }
            }
        }

        public CxHDAudioChannelEnumeratorClass AudioChannelEmulator
        {
            get => 
                this._audioChannelEnumerator;
            set
            {
                this._audioChannelEnumerator = value;
                this._channels.Clear();
            }
        }

        public double LeftChannelVolume
        {
            get => 
                this.GetChannel(ChannelType.LeftChannel);
            set
            {
                this.SetChannel(ChannelType.LeftChannel, value);
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

        public double RightChannelVolume
        {
            get => 
                this.GetChannel(ChannelType.RightChannel);
            set
            {
                this.SetChannel(ChannelType.RightChannel, value);
            }
        }

        public double Value
        {
            get
            {
                double num = 0.0;
                if (this.RightChannelVolume == this.LeftChannelVolume)
                {
                    this._delataOverMax = 0.0;
                }
                else
                {
                    num = this.Max(this.RightChannelVolume, this.LeftChannelVolume);
                    if (num > 0.0)
                    {
                        this._delataOverMax = (this.RightChannelVolume - this.LeftChannelVolume) / num;
                    }
                }
                return ((this._sliderSpan * (this._delataOverMax + 1.0)) / 2.0);
            }
            set
            {
                double num = value * this._faderStep;
                double num2 = 0.0;
                num2 = this.Max(this.RightChannelVolume, this.LeftChannelVolume);
                if (num > (this._faderSpan / 2.0))
                {
                    this.RightChannelVolume = num2;
                    this.LeftChannelVolume = Math.Ceiling((double) ((2.0 * num2) * (1.0 - (num / this._faderSpan))));
                }
                else
                {
                    this.RightChannelVolume = Math.Ceiling((double) ((2.0 * num2) * (num / this._faderSpan)));
                    this.LeftChannelVolume = num2;
                }
            }
        }
    }
}

