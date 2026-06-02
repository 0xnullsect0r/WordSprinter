namespace WordSprinter.Models;

public record ParsedBook(
    string Title,
    string Author,
    byte[]? CoverImageBytes,
    IReadOnlyList<string> Words,
    IReadOnlyList<ChapterMarker> Chapters
);
