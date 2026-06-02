using WordSprinter.Models;

namespace WordSprinter.Services.BookParser;

public class MobiParser : IBookParser
{
    private readonly CalibreService _calibre;
    private readonly EpubParser _epubParser;

    public MobiParser(CalibreService calibre)
    {
        _calibre = calibre;
        _epubParser = new EpubParser();
    }

    public bool CanParse(string fileExtension) =>
        fileExtension.Equals(".mobi", StringComparison.OrdinalIgnoreCase) ||
        fileExtension.Equals(".azw", StringComparison.OrdinalIgnoreCase) ||
        fileExtension.Equals(".azw3", StringComparison.OrdinalIgnoreCase) ||
        fileExtension.Equals(".kfx", StringComparison.OrdinalIgnoreCase);

    public async Task<ParsedBook> ParseAsync(string filePath, CancellationToken ct = default)
    {
        if (!_calibre.IsAvailable())
            throw new CalibreNotFoundException();

        string tempEpub = await _calibre.ConvertToEpubAsync(filePath, ct);
        try
        {
            return await _epubParser.ParseAsync(tempEpub, ct);
        }
        finally
        {
            try { System.IO.File.Delete(tempEpub); } catch { }
        }
    }
}
