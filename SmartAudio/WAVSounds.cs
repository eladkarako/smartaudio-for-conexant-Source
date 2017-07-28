namespace SmartAudio
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    public class WAVSounds
    {
        public int SND_ALIAS = 0x10000;
        public int SND_ALIAS_ID = 0x110000;
        public int SND_APPLICATION = 0x80;
        public int SND_ASYNC = 1;
        public int SND_FILENAME = 0x20000;
        public int SND_LOOP = 8;
        public int SND_MEMORY = 4;
        public int SND_NODEFAULT = 2;
        public int SND_NOSTOP = 0x10;
        public int SND_NOWAIT = 0x2000;
        public int SND_PURGE = 0x40;
        public int SND_RESOURCE = 0x40004;
        public int SND_SYNC;

        public void Play(string wfname, int SoundFlags)
        {
            byte[] buffer = new byte[0x100];
            PlaySound(Encoding.ASCII.GetBytes(wfname), SoundFlags);
        }

        [DllImport("WinMM.dll")]
        public static extern bool PlaySound(byte[] wfname, int fuSound);
        public void StopPlay()
        {
            PlaySound(null, this.SND_PURGE);
        }
    }
}

