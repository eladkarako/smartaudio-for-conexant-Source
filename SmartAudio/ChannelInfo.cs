namespace SmartAudio
{
    using CxHDAudioAPILib;
    using System;

    public class ChannelInfo
    {
        private CxHDAudioChannel _channel1;
        private SmartAudio.ChannelType _channelType;

        public ChannelInfo(CxHDAudioChannel channel1, SmartAudio.ChannelType channelType)
        {
            this._channel1 = channel1;
            this._channelType = channelType;
        }

        public CxHDAudioChannel Channel
        {
            get => 
                this._channel1;
            set
            {
                this._channel1 = value;
            }
        }

        public SmartAudio.ChannelType ChannelType
        {
            get => 
                this._channelType;
            set
            {
                this._channelType = value;
            }
        }
    }
}

