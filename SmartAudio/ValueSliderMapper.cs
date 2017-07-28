namespace SmartAudio
{
    using System;

    public class ValueSliderMapper
    {
        public double _maxValue;
        public SmartAudio.PeakMeterBAR _progressBar;

        public ValueSliderMapper(double Value, SmartAudio.PeakMeterBAR progress)
        {
            this._maxValue = Value;
            this._progressBar = progress;
        }

        public double MaxValue =>
            this._maxValue;

        public SmartAudio.PeakMeterBAR PeakMeterBAR =>
            this._progressBar;
    }
}

