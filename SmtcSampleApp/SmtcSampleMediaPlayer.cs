using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.System;
using Windows.UI.Core;
using Microsoft.UI.Xaml;
using Windows.Storage;

namespace SmtcSampleApp;

public class SmtSampleMediaPlayer : IDisposable
{
    private MediaPlayer _mPlayer = null!;
    private SystemMediaTransportControls _smtc = null!;
    private SystemMediaTransportControlsDisplayUpdater _smtcDisplayUpdater = null!;
    private StorageFile? _currentFile = null;
    private CancellationTokenSource? _playCts = null;

    public CancellationTokenSource? PlayerCancellationTokenSource
    {
        get => _playCts;
    }

    public List<Uri> Files { get; set; }

    public SmtSampleMediaPlayer(MediaPlayer? player)
    {
        _mPlayer = player ?? new MediaPlayer();
        _smtc = _mPlayer.SystemMediaTransportControls;
        Files = [];
        _smtc.ButtonPressed += SmtcButtonPressed;
        _smtc.IsPlayEnabled = _smtc.IsPauseEnabled = _smtc.IsStopEnabled = true;
        _smtcDisplayUpdater = _smtc.DisplayUpdater;
        _mPlayer.MediaOpened += OnMediaOpened;
        _mPlayer.Volume = .30;
    }

    public SmtSampleMediaPlayer(MediaPlayer? player, List<Uri> files) : this(player)
    {
        Files.AddRange(files);
    }

    public void Dispose()
    {
        _smtc.ButtonPressed -= SmtcButtonPressed;
        _currentFile = null;
        _mPlayer.Dispose();
    }

    public async Task OpenFile(Uri target)
    {
        Files.Add(target);
        _mPlayer.AudioCategory = MediaPlayerAudioCategory.Media;
        _currentFile = await StorageFile.GetFileFromPathAsync(target.ToString());
        _mPlayer.Source = MediaSource.CreateFromStorageFile(_currentFile);
    }

    public async Task Play()
    {
        if (_currentFile is null)
        {
            throw new ArgumentNullException("No file to play.");
        }

        // Cancel redundant 'play' tasks
        _playCts?.Cancel();
        _playCts = new CancellationTokenSource();
        Console.CancelKeyPress += ((sender, eventArgs) => _playCts?.Cancel());
        try
        {
            _mPlayer.Play();
            await WaitForMediaEndedAsync(_playCts.Token);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("playing is stopped...");
            _smtc.PlaybackStatus = MediaPlaybackStatus.Stopped;
        }
    }

    internal async Task<bool> WaitForMediaEndedAsync(CancellationToken ct)
    {
        var ts = new TaskCompletionSource<bool>();

        void OnMediaEnded(MediaPlayer sender, object args)
        {
            ts.SetResult(true);
            _smtc.PlaybackStatus = MediaPlaybackStatus.Closed;
            _mPlayer.MediaEnded -= OnMediaEnded;
        }

        _mPlayer.MediaEnded += OnMediaEnded;
        using (ct.Register(() => ts.SetCanceled()))
        {
            return await ts.Task;
        }
    }

    internal async void SmtcButtonPressed(SystemMediaTransportControls sender
        , SystemMediaTransportControlsButtonPressedEventArgs args)
    {
        await Task.Run(() =>
        {
            switch (args.Button)
            {
                case SystemMediaTransportControlsButton.Play:
                    _smtc.PlaybackStatus = MediaPlaybackStatus.Playing;
                    _mPlayer.Play();
                    break;
                case SystemMediaTransportControlsButton.Pause:
                    _smtc.PlaybackStatus = MediaPlaybackStatus.Paused;
                    _mPlayer.Pause();
                    break;
                default:
                    break;
            }
        });
    }

        internal async void PlaybackPositionChangeRequested(SystemMediaTransportControls sender
        , PlaybackPositionChangeRequestedEventArgs args)
    {

    }

    protected async virtual void OnMediaOpened(MediaPlayer sender, object args)
    {
        await _smtcDisplayUpdater.CopyFromFileAsync(MediaPlaybackType.Music, _currentFile);
        _smtcDisplayUpdater.Update();
    }
}
