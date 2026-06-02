namespace WordSprinter.Services;

public interface ISettingsService
{
    string? GetSetting(string key);
    Task SetSettingAsync(string key, string value);
    Task LoadAsync();
}
