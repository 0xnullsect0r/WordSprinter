using WordSprinter.Models;

namespace WordSprinter.Services.BookParser;

public class BookParserFactory
{
    private readonly IReadOnlyList<IBookParser> _parsers;

    public BookParserFactory(CalibreService calibreService)
    {
        _parsers = new List<IBookParser>
        {
            new EpubParser(),
            new PdfParser(),
            new MobiParser(calibreService),
        };
    }

    public IBookParser GetParser(string filePath)
    {
        string ext = System.IO.Path.GetExtension(filePath);
        return _parsers.FirstOrDefault(p => p.CanParse(ext))
            ?? throw new NotSupportedException($"No parser available for extension '{ext}'.");
    }
}
