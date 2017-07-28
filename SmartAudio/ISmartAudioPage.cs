namespace SmartAudio
{
    using CxHDAudioAPILib;
    using System;

    internal interface ISmartAudioPage
    {
        bool Localize();

        CxHDAudioFactory AudioFactory { get; set; }

        string FriendlyName { get; }
    }
}

