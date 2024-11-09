// SMTC with MediaPlayer

using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;

namespace SmtcSampleApp;

public static class Program
{
    private static MediaPlayer _mPlayer = null!;
    private static SystemMediaTransportControls _smtc = null!;


    public static async Task<int> Main(string[] args)
    {
        _mPlayer = new MediaPlayer();
        _smtc = _mPlayer.SystemMediaTransportControls;
        var sampleMusicFile = new Uri(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), ""));

        _mPlayer.AudioCategory = MediaPlayerAudioCategory.Media;
        _mPlayer.Source = MediaSource.CreateFromUri(sampleMusicFile);
        _mPlayer.AutoPlay = true;
        _mPlayer.Volume = .20;
        _mPlayer.Play();

        await Task.Delay(Timeout.Infinite);

        return 0;
    }
}



// events


// SMTC with custom implementations
