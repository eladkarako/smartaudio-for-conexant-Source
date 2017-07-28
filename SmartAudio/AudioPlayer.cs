namespace SmartAudio
{
    using CxHDAudioAPILib;
    using System;
    using System.Runtime.CompilerServices;
    using System.Timers;

    internal class AudioPlayer
    {
        private CxHDAudioMediaPlayer _audioPlayer = new CxHDAudioMediaPlayerClass();
        private bool _playing = false;
        private Timer _timer = new Timer();

        public event PlayerStopped OnPlayerStopped;

        public AudioPlayer()
        {
            this._timer.Elapsed += new ElapsedEventHandler(this._timer_Elapsed);
        }

        public void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this._playing = false;
            this._timer.Stop();
            this._audioPlayer.StopMedia();
            if (this.OnPlayerStopped != null)
            {
                this.OnPlayerStopped();
            }
        }

        public void Play(CxAudioEndPoint endPoint, string strFileName, bool enableTimer, double timer)
        {
            if (enableTimer)
            {
                this._timer.Interval = timer;
                this._timer.Start();
            }
            this._playing = true;
            try
            {
                this._audioPlayer.PlayMedia(endPoint, strFileName);
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("AudioPlayer::Play() Failed to play ", Severity.FATALERROR, exception);
            }
        }

        public void Play(string endPointID, string strFileName, bool enableTimer, double timer)
        {
            if (enableTimer)
            {
                this._timer.Interval = timer;
                this._timer.Start();
            }
            this._playing = true;
            try
            {
                this._audioPlayer.PlayMediaFile(endPointID, strFileName);
            }
            catch (Exception exception)
            {
                SmartAudioLog.Log("AudioPlayer::Play() Failed to play ", Severity.FATALERROR, exception);
            }
        }

        public void Stop()
        {
            this._audioPlayer.StopMedia();
            if (this.OnPlayerStopped != null)
            {
                this.OnPlayerStopped();
            }
        }

        public bool IsPlaying =>
            this._playing;
    }
}

