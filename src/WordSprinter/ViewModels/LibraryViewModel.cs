using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WordSprinter.Models;
using WordSprinter.Services;

namespace WordSprinter.ViewModels;

public partial class LibraryViewModel : ViewModelBase
{
    private readonly LibraryService _library;
    private readonly MainWindowViewModel _nav;

    [ObservableProperty]
    private ObservableCollection<BookEntry> _books = new();

    public bool IsLibraryEmpty => Books.Count == 0;

    partial void OnBooksChanged(ObservableCollection<BookEntry> value)
    {
        value.CollectionChanged += (_, _) => OnPropertyChanged(nameof(IsLibraryEmpty));
        OnPropertyChanged(nameof(IsLibraryEmpty));
    }

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    public LibraryViewModel(LibraryService library, MainWindowViewModel nav)
    {
        _library = library;
        _nav = nav;
        _ = LoadBooksAsync();
    }

    private async Task LoadBooksAsync()
    {
        IsLoading = true;
        try
        {
            var books = await _library.GetAllBooksAsync();
            Books = new ObservableCollection<BookEntry>(books);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load library: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task OpenBook(BookEntry entry)
    {
        IsLoading = true;
        try
        {
            var progress = await _library.GetProgressAsync(entry.Id);
            var words = await _library.LoadWordListAsync(entry.Id);
            _nav.NavigateToReader(entry, words);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to open book: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task ImportFilesAsync(IEnumerable<string> filePaths)
    {
        foreach (var path in filePaths)
        {
            IsLoading = true;
            ErrorMessage = null;
            try
            {
                var entry = await _library.ImportBookAsync(path);
                Books.Insert(0, entry);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }
    }

    [RelayCommand]
    private async Task RemoveBook(BookEntry entry)
    {
        await _library.RemoveBookAsync(entry.Id);
        Books.Remove(entry);
    }

    [RelayCommand]
    private void OpenSettings() => _nav.NavigateToSettings();
}
