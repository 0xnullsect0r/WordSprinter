using CommunityToolkit.Mvvm.ComponentModel;
using WordSprinter.Models;
using WordSprinter.Services;

namespace WordSprinter.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly LibraryService _library;
    private readonly DatabaseService _db;

    [ObservableProperty]
    private ViewModelBase _currentView = null!;

    public MainWindowViewModel(LibraryService library, DatabaseService db)
    {
        _library = library;
        _db = db;
        CurrentView = new LibraryViewModel(library, this);
    }

    public void NavigateToReader(BookEntry book, IReadOnlyList<string> words)
    {
        CurrentView = new ReaderViewModel(book, words, _library, this);
    }

    public void NavigateToLibrary()
    {
        CurrentView = new LibraryViewModel(_library, this);
    }

    public void NavigateToSettings()
    {
        CurrentView = new SettingsViewModel(_db, this);
    }
}
