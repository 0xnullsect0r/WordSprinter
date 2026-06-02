using WordSprinter.Models;

namespace WordSprinter.Services.BookParser;

/// <summary>
/// PDF parser. Currently throws NotSupportedException as the PdfPig library
/// version available in this environment is incompatible. PDF support can be
/// restored by referencing a compatible UglyToad.PdfPig package.
/// </summary>
public class PdfParser : IBookParser
{
    public bool CanParse(string fileExtension) =>
        fileExtension.Equals(".pdf", StringComparison.OrdinalIgnoreCase);

    public Task<ParsedBook> ParseAsync(string filePath, CancellationToken ct = default)
    {
        throw new NotSupportedException(
            "PDF parsing is not available in this build. Please convert your PDF to EPUB first.");
    }
}
