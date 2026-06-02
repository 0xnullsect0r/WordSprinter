namespace WordSprinter.Models;

public record BookEntry(
    string Id,
    string Title,
    string Author,
    string FilePath,
    string? CoverPath,
    int WordCount,
    DateTime DateAdded
);
