using WordSprinter.Models;

namespace WordSprinter.Services.BookParser;

public interface IBookParser
{
    Task<ParsedBook> ParseAsync(string filePath, CancellationToken ct = default);
    bool CanParse(string fileExtension);
}
