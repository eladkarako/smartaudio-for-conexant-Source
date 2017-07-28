namespace SmartAudio
{
    using System;

    public interface IChannelBalance
    {
        double Maximum { get; }

        double Minimum { get; }

        double Value { get; set; }
    }
}

