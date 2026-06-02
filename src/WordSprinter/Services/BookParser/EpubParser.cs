using System.Text;
using VersOne.Epub;
using WordSprinter.Models;

namespace WordSprinter.Services.BookParser;

public class EpubParser : IBookParser
{
    private readonly Tokenizer _tokenizer = new();
    private readonly HtmlTextExtractor _extractor = new();

    public bool CanParse(string fileExtension) =>
        fileExtension.Equals(".epub", StringComparison.OrdinalIgnoreCase);

    public async Task<ParsedBook> ParseAsync(string filePath, CancellationToken ct = default)
    {
        var book = await EpubReader.ReadBookAsync(filePath);

        string title = book.Title ?? System.IO.Path.GetFileNameWithoutExtension(filePath);
        string author = book.AuthorList?.FirstOrDefault() ?? "Unknown";

        byte[]? coverBytes = null;
        if (book.CoverImage != null)
            coverBytes = book.CoverImage;

        var allFootnotes = new List<string>();
        var chapters = new List<ChapterMarker>();
        var wordList = new List<string>();

        // Build chapter markers from navigation
        var navTitles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (book.Navigation != null)
        {
            foreach (var navItem in book.Navigation)
                if (navItem.HtmlContentFile != null)
                    navTitles[navItem.HtmlContentFile.Key] = navItem.Title ?? "";
        }

        foreach (var contentFile in book.ReadingOrder)
        {
            ct.ThrowIfCancellationRequested();

            if (navTitles.TryGetValue(contentFile.Key, out string? chapterTitle) && !string.IsNullOrEmpty(chapterTitle))
                chapters.Add(new ChapterMarker(wordList.Count, chapterTitle));

            var footnotes = new List<string>();
            string rawText = _extractor.ExtractText(contentFile.Content ?? "", footnotes);
            string cleaned = TextCleaningPipeline.Clean(rawText);
            var words = _tokenizer.Tokenize(cleaned);
            wordList.AddRange(words);

            // Append footnotes inline
            foreach (var fn in footnotes)
            {
                string fnCleaned = TextCleaningPipeline.Clean($"[Footnote: {fn}]");
                wordList.AddRange(_tokenizer.Tokenize(fnCleaned));
            }
        }

        return new ParsedBook(title, author, coverBytes, wordList, chapters);
    }
}
