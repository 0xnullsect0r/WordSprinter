using Microsoft.Data.Sqlite;
using System.IO;
using WordSprinter.Models;

namespace WordSprinter.Services;

public class DatabaseService : ISettingsService
{
    private readonly string _dbPath;
    private SqliteConnection? _connection;

    public DatabaseService()
    {
        string appData = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "WordSprinter");
        Directory.CreateDirectory(appData);
        _dbPath = Path.Combine(appData, "wordsprinter.db");
    }

    public async Task LoadAsync()
    {
        _connection = new SqliteConnection($"Data Source={_dbPath}");
        await _connection.OpenAsync();
        await MigrateAsync();
    }

    private async Task MigrateAsync()
    {
        using var cmd = _connection!.CreateCommand();
        cmd.CommandText = "PRAGMA user_version";
        long version = (long)(await cmd.ExecuteScalarAsync() ?? 0L);

        if (version < 1)
        {
            await ExecuteAsync(@"
                CREATE TABLE IF NOT EXISTS Books (
                    Id TEXT PRIMARY KEY,
                    Title TEXT NOT NULL,
                    Author TEXT NOT NULL DEFAULT '',
                    FilePath TEXT NOT NULL,
                    CoverPath TEXT,
                    WordCount INTEGER NOT NULL DEFAULT 0,
                    DateAdded TEXT NOT NULL,
                    FileHash TEXT
                );
                CREATE TABLE IF NOT EXISTS ReadingProgress (
                    BookId TEXT PRIMARY KEY REFERENCES Books(Id) ON DELETE CASCADE,
                    CurrentWordIndex INTEGER NOT NULL DEFAULT 0,
                    LastReadAt TEXT NOT NULL,
                    LastWpm INTEGER NOT NULL DEFAULT 250
                );
                CREATE TABLE IF NOT EXISTS Settings (
                    Key TEXT PRIMARY KEY,
                    Value TEXT NOT NULL
                );
                PRAGMA user_version = 1;
            ");
        }
    }

    private async Task ExecuteAsync(string sql)
    {
        using var cmd = _connection!.CreateCommand();
        cmd.CommandText = sql;
        await cmd.ExecuteNonQueryAsync();
    }

    // ISettingsService
    public string? GetSetting(string key)
    {
        using var cmd = _connection!.CreateCommand();
        cmd.CommandText = "SELECT Value FROM Settings WHERE Key = $key";
        cmd.Parameters.AddWithValue("$key", key);
        return cmd.ExecuteScalar() as string;
    }

    public async Task SetSettingAsync(string key, string value)
    {
        using var cmd = _connection!.CreateCommand();
        cmd.CommandText = "INSERT INTO Settings(Key, Value) VALUES($k,$v) ON CONFLICT(Key) DO UPDATE SET Value=$v";
        cmd.Parameters.AddWithValue("$k", key);
        cmd.Parameters.AddWithValue("$v", value);
        await cmd.ExecuteNonQueryAsync();
    }

    // Book operations
    public async Task InsertBookAsync(BookEntry entry, string? fileHash)
    {
        using var cmd = _connection!.CreateCommand();
        cmd.CommandText = @"INSERT INTO Books(Id,Title,Author,FilePath,CoverPath,WordCount,DateAdded,FileHash)
                            VALUES($id,$title,$author,$path,$cover,$wc,$date,$hash)";
        cmd.Parameters.AddWithValue("$id", entry.Id);
        cmd.Parameters.AddWithValue("$title", entry.Title);
        cmd.Parameters.AddWithValue("$author", entry.Author);
        cmd.Parameters.AddWithValue("$path", entry.FilePath);
        cmd.Parameters.AddWithValue("$cover", (object?)entry.CoverPath ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$wc", entry.WordCount);
        cmd.Parameters.AddWithValue("$date", entry.DateAdded.ToString("o"));
        cmd.Parameters.AddWithValue("$hash", (object?)fileHash ?? DBNull.Value);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<IReadOnlyList<BookEntry>> GetAllBooksAsync()
    {
        var list = new List<BookEntry>();
        using var cmd = _connection!.CreateCommand();
        cmd.CommandText = "SELECT Id,Title,Author,FilePath,CoverPath,WordCount,DateAdded FROM Books ORDER BY DateAdded DESC";
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new BookEntry(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.IsDBNull(4) ? null : reader.GetString(4),
                reader.GetInt32(5),
                DateTime.Parse(reader.GetString(6))
            ));
        }
        return list;
    }

    public async Task DeleteBookAsync(string id)
    {
        using var cmd = _connection!.CreateCommand();
        cmd.CommandText = "DELETE FROM Books WHERE Id = $id";
        cmd.Parameters.AddWithValue("$id", id);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task SaveProgressAsync(string bookId, int wordIndex, int wpm)
    {
        using var cmd = _connection!.CreateCommand();
        cmd.CommandText = @"INSERT INTO ReadingProgress(BookId,CurrentWordIndex,LastReadAt,LastWpm)
                            VALUES($bid,$idx,$at,$wpm)
                            ON CONFLICT(BookId) DO UPDATE SET CurrentWordIndex=$idx,LastReadAt=$at,LastWpm=$wpm";
        cmd.Parameters.AddWithValue("$bid", bookId);
        cmd.Parameters.AddWithValue("$idx", wordIndex);
        cmd.Parameters.AddWithValue("$at", DateTime.UtcNow.ToString("o"));
        cmd.Parameters.AddWithValue("$wpm", wpm);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<ReadingProgress?> GetProgressAsync(string bookId)
    {
        using var cmd = _connection!.CreateCommand();
        cmd.CommandText = "SELECT CurrentWordIndex,LastReadAt,LastWpm FROM ReadingProgress WHERE BookId=$bid";
        cmd.Parameters.AddWithValue("$bid", bookId);
        using var r = await cmd.ExecuteReaderAsync();
        if (!await r.ReadAsync()) return null;
        return new ReadingProgress(bookId, r.GetInt32(0), DateTime.Parse(r.GetString(1)), r.GetInt32(2));
    }

    public async Task<bool> HashExistsAsync(string hash)
    {
        using var cmd = _connection!.CreateCommand();
        cmd.CommandText = "SELECT 1 FROM Books WHERE FileHash=$h LIMIT 1";
        cmd.Parameters.AddWithValue("$h", hash);
        return await cmd.ExecuteScalarAsync() != null;
    }
}
