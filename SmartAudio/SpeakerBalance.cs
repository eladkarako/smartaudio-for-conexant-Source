namespace SmartAudio
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct SpeakerBalance
    {
        public double frontLeftSpeakerBalance;
        public double frontRightSpeakerBalance;
        public double centerSpeakerBalance;
        public double subwooferBalance;
        public double rearLeftSpeakerBalance;
        public double rearRightSpeakerBalance;
        public double sideLeftSpeakerBalance;
        public double sideRightSpeakerBalance;
    }
}

