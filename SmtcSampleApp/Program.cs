// SMTC with MediaPlayer

using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;

namespace SmtcSampleApp;

public static class Program
{
    private static MediaPlayer _mPlayer = null!;
    private static SystemMediaTransportControls _smtc = null!;

    public static async Task Main(string[] args)
    {
        var sampleMusicFile = new Uri(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), ""));
        var player = new SmtSampleMediaPlayer(new MediaPlayer());

        await player.OpenFile(sampleMusicFile);
        await player.Play();
        player.Dispose();
    }
}



// events


// SMTC with custom implementations
