namespace WordSprinter.Models;

public record RsvpToken(
    string RawWord,
    string DisplayWord,
    int OrpIndex,
    int DisplayDurationMs,
    int GlobalIndex
);
