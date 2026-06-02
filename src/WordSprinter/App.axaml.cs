using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using WordSprinter.Services;
using WordSprinter.Services.BookParser;
using WordSprinter.ViewModels;
using WordSprinter.Views;

namespace WordSprinter;

public partial class App : Application
{
    private ServiceProvider? _services;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();
        var db = new DatabaseService();
        await db.LoadAsync();

        services.AddSingleton(db);
        services.AddSingleton<ISettingsService>(db);
        services.AddSingleton<CalibreService>();
        services.AddSingleton<BookParserFactory>();
        services.AddSingleton<LibraryService>();
        services.AddSingleton<MainWindowViewModel>();

        _services = services.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = _services.GetRequiredService<MainWindowViewModel>()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
