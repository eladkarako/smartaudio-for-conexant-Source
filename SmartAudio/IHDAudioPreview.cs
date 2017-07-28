namespace SmartAudio
{
    using CxHDAudioAPILib;
    using System;

    public interface IHDAudioPreview
    {
        void Localize();
        void OnMasterVolumeChanged(double newValue);
        void OnMasterVolumeChanging(double newValue);

        CxHDAudioChannelEnumeratorClass AudioChannelEnumerator { get; set; }

        CxHDMasterVolumeControl MasterVolume { get; set; }
    }
}

