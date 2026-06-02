using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WordSprinter.Models;
using WordSprinter.Services;

namespace WordSprinter.ViewModels;

public partial class ReaderViewModel : ViewModelBase
{
    private readonly BookEntry _book;
    private readonly LibraryService _library;
    private readonly MainWindowViewModel _nav;
    private readonly RsvpEngine _engine = new();
    private IReadOnlyList<RsvpToken> _tokens;
    private int _currentIndex;
    private CancellationTokenSource? _cts;
    private Task? _playTask;

    [ObservableProperty] private string _leftPart = "";
    [ObservableProperty] private string _orpLetter = "A";
    [ObservableProperty] private string _rightPart = "";
    [ObservableProperty] private int _wpm = 250;
    [ObservableProperty] private double _progress;
    [ObservableProperty] private bool _isPlaying;
    [ObservableProperty] private int _fontSize = 28;
    [ObservableProperty] private string _bookTitle = "";
    [ObservableProperty] private string? _errorMessage;

    public ReaderViewModel(BookEntry book, IReadOnlyList<string> words, LibraryService library, MainWindowViewModel nav)
    {
        _book = book;
        _library = library;
        _nav = nav;
        BookTitle = book.Title;
        _tokens = _engine.BuildTokenList(words, _wpm);
        ShowToken(0);
    }

    partial void OnWpmChanged(int value)
    {
        if (_tokens == null || _tokens.Count == 0) return;
        bool wasPlaying = IsPlaying;
        if (wasPlaying) StopPlayback();
        _tokens = _engine.BuildTokenList(_tokens.Select(t => t.RawWord).ToList(), value);
        ShowToken(_currentIndex);
        if (wasPlaying) StartPlayback();
    }

    [RelayCommand]
    private void PlayPause()
    {
        if (IsPlaying) StopPlayback();
        else StartPlayback();
    }

    [RelayCommand]
    private void Back5Words()
    {
        bool wasPlaying = IsPlaying;
        if (wasPlaying) StopPlayback();
        _currentIndex = Math.Max(0, _currentIndex - 5);
        ShowToken(_currentIndex);
        if (wasPlaying) StartPlayback();
    }

    [RelayCommand]
    private void Forward5Words()
    {
        bool wasPlaying = IsPlaying;
        if (wasPlaying) StopPlayback();
        _currentIndex = Math.Min(_tokens.Count - 1, _currentIndex + 5);
        ShowToken(_currentIndex);
        if (wasPlaying) StartPlayback();
    }

    [RelayCommand]
    private async Task GoBack()
    {
        StopPlayback();
        await SaveProgressAsync();
        _nav.NavigateToLibrary();
    }

    [RelayCommand]
    private void DecreaseFontSize() => FontSize = Math.Max(14, FontSize - 2);

    [RelayCommand]
    private void IncreaseFontSize() => FontSize = Math.Min(72, FontSize + 2);

    private void StartPlayback()
    {
        if (_tokens.Count == 0) return;
        _cts = new CancellationTokenSource();
        IsPlaying = true;
        _playTask = PlaybackLoopAsync(_cts.Token);
    }

    private void StopPlayback()
    {
        _cts?.Cancel();
        IsPlaying = false;
    }

    private async Task PlaybackLoopAsync(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested && _currentIndex < _tokens.Count)
            {
                var token = _tokens[_currentIndex];
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    ShowToken(_currentIndex);
                    Progress = _tokens.Count > 0 ? (double)_currentIndex / _tokens.Count * 100.0 : 0;
                });
                await Task.Delay(token.DisplayDurationMs, ct);
                _currentIndex++;
            }

            if (!ct.IsCancellationRequested)
            {
                await Dispatcher.UIThread.InvokeAsync(() => IsPlaying = false);
                await SaveProgressAsync();
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                IsPlaying = false;
                ErrorMessage = $"Playback error: {ex.Message}";
            });
        }
    }

    private void ShowToken(int index)
    {
        if (_tokens.Count == 0) return;
        index = Math.Clamp(index, 0, _tokens.Count - 1);
        var token = _tokens[index];
        string w = token.DisplayWord;
        int orp = Math.Clamp(token.OrpIndex, 0, Math.Max(0, w.Length - 1));
        LeftPart = w[..orp];
        OrpLetter = w.Length > 0 ? w[orp].ToString() : "";
        RightPart = orp + 1 < w.Length ? w[(orp + 1)..] : "";
    }

    private Task SaveProgressAsync() =>
        _library.SaveProgressAsync(_book.Id, _currentIndex, Wpm);
}
