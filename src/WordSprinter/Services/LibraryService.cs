using System.IO;
using System.Security.Cryptography;
using System.Text;
using WordSprinter.Models;
using WordSprinter.Services.BookParser;

namespace WordSprinter.Services;

public class LibraryService
{
    private readonly DatabaseService _db;
    private readonly BookParserFactory _parserFactory;
    private readonly string _coversDir;
    private readonly string _wordlistsDir;

    public LibraryService(DatabaseService db, BookParserFactory parserFactory)
    {
        _db = db;
        _parserFactory = parserFactory;
        string appData = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "WordSprinter");
        _coversDir = Path.Combine(appData, "covers");
        _wordlistsDir = Path.Combine(appData, "wordlists");
        Directory.CreateDirectory(_coversDir);
        Directory.CreateDirectory(_wordlistsDir);
    }

    public Task<IReadOnlyList<BookEntry>> GetAllBooksAsync() => _db.GetAllBooksAsync();

    public async Task<BookEntry> ImportBookAsync(string filePath, CancellationToken ct = default)
    {
        // Dedup check
        string hash = await ComputeHashAsync(filePath, ct);
        if (await _db.HashExistsAsync(hash))
            throw new InvalidOperationException("This book is already in your library.");

        var parser = _parserFactory.GetParser(filePath);
        var parsed = await parser.ParseAsync(filePath, ct);

        string id = Guid.NewGuid().ToString("N");

        // Save cover
        string? coverPath = null;
        if (parsed.CoverImageBytes is { Length: > 0 })
        {
            coverPath = Path.Combine(_coversDir, $"{id}.jpg");
            await File.WriteAllBytesAsync(coverPath, parsed.CoverImageBytes, ct);
        }

        // Save word list binary
        string wordlistPath = Path.Combine(_wordlistsDir, $"{id}.bin");
        await SaveWordListAsync(wordlistPath, parsed.Words, ct);

        var entry = new BookEntry(id, parsed.Title, parsed.Author, filePath, coverPath, parsed.Words.Count, DateTime.UtcNow);
        await _db.InsertBookAsync(entry, hash);
        return entry;
    }

    public async Task<IReadOnlyList<string>> LoadWordListAsync(string bookId, CancellationToken ct = default)
    {
        string path = Path.Combine(_wordlistsDir, $"{bookId}.bin");
        if (!File.Exists(path)) return Array.Empty<string>();

        using var fs = File.OpenRead(path);
        using var br = new BinaryReader(fs, Encoding.UTF8, leaveOpen: true);
        int count = br.ReadInt32();
        var words = new List<string>(count);
        for (int i = 0; i < count; i++)
        {
            ct.ThrowIfCancellationRequested();
            words.Add(br.ReadString());
        }
        return words;
    }

    public async Task RemoveBookAsync(string bookId)
    {
        await _db.DeleteBookAsync(bookId);
        TryDelete(Path.Combine(_coversDir, $"{bookId}.jpg"));
        TryDelete(Path.Combine(_wordlistsDir, $"{bookId}.bin"));
    }

    public Task SaveProgressAsync(string bookId, int wordIndex, int wpm) =>
        _db.SaveProgressAsync(bookId, wordIndex, wpm);

    public Task<ReadingProgress?> GetProgressAsync(string bookId) =>
        _db.GetProgressAsync(bookId);

    private static void TryDelete(string path) { try { File.Delete(path); } catch { } }

    private static async Task SaveWordListAsync(string path, IReadOnlyList<string> words, CancellationToken ct)
    {
        using var fs = File.Create(path);
        using var bw = new BinaryWriter(fs, Encoding.UTF8, leaveOpen: true);
        bw.Write(words.Count);
        foreach (var w in words)
        {
            ct.ThrowIfCancellationRequested();
            bw.Write(w);
        }
    }

    private static async Task<string> ComputeHashAsync(string filePath, CancellationToken ct)
    {
        using var fs = File.OpenRead(filePath);
        using var sha = SHA256.Create();
        byte[] hash = await sha.ComputeHashAsync(fs, ct);
        return Convert.ToHexString(hash);
    }
}
