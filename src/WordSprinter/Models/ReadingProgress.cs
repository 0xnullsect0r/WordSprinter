namespace WordSprinter.Models;

public record ReadingProgress(
    string BookId,
    int CurrentWordIndex,
    DateTime LastReadAt,
    int LastWpm
);
