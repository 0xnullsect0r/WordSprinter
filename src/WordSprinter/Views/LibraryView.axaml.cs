using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using WordSprinter.ViewModels;

namespace WordSprinter.Views;

public partial class LibraryView : UserControl
{
    public LibraryView()
    {
        InitializeComponent();
        AddHandler(DragDrop.DropEvent, OnDrop);
        AddHandler(DragDrop.DragOverEvent, OnDragOver);
        DragDrop.SetAllowDrop(this, true);
    }

    private void OnDragOver(object? sender, DragEventArgs e)
    {
        e.DragEffects = e.Data.Contains(DataFormats.Files) ? DragDropEffects.Copy : DragDropEffects.None;
    }

    private async void OnDrop(object? sender, DragEventArgs e)
    {
        if (DataContext is not LibraryViewModel vm) return;
        if (!e.Data.Contains(DataFormats.Files)) return;
        var files = e.Data.GetFiles();
        if (files == null) return;
        var paths = files.Select(f => f.Path.LocalPath);
        await vm.ImportFilesAsync(paths);
    }

    private async void OnImportButtonClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not LibraryViewModel vm) return;

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Import Book",
            AllowMultiple = true,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("Ebook Files")
                {
                    Patterns = new[] { "*.epub", "*.pdf", "*.mobi", "*.azw", "*.azw3", "*.kfx" }
                },
                new FilePickerFileType("All Files")
                {
                    Patterns = new[] { "*" }
                }
            }
        });

        if (files.Count == 0) return;
        var paths = files.Select(f => f.Path.LocalPath);
        await vm.ImportFilesAsync(paths);
    }
}
