using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WordSprinter.Services;

namespace WordSprinter.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly DatabaseService _db;
    private readonly MainWindowViewModel _nav;

    [ObservableProperty] private string _theme = "System";
    [ObservableProperty] private int _defaultWpm = 250;
    [ObservableProperty] private string _calibrePath = "";

    public string[] ThemeOptions { get; } = { "System", "Light", "Dark" };

    public SettingsViewModel(DatabaseService db, MainWindowViewModel nav)
    {
        _db = db;
        _nav = nav;
        Theme = db.GetSetting("Theme") ?? "System";
        DefaultWpm = int.TryParse(db.GetSetting("DefaultWpm"), out int wpm) ? wpm : 250;
        CalibrePath = db.GetSetting("CalibrePath") ?? "";
    }

    partial void OnThemeChanged(string value) => _ = _db.SetSettingAsync("Theme", value);
    partial void OnDefaultWpmChanged(int value) => _ = _db.SetSettingAsync("DefaultWpm", value.ToString());
    partial void OnCalibrePathChanged(string value) => _ = _db.SetSettingAsync("CalibrePath", value);

    [RelayCommand]
    private void GoBack() => _nav.NavigateToLibrary();
}
