namespace WordSprinter.Services;

public class CalibreNotFoundException : Exception
{
    public CalibreNotFoundException() : base("Calibre (ebook-convert) not found. Please configure the Calibre path in Settings.") { }
}

public class ConversionException : Exception
{
    public ConversionException(string message) : base(message) { }
}

public class CalibreService
{
    private readonly ISettingsService _settings;

    public CalibreService(ISettingsService settings)
    {
        _settings = settings;
    }

    public bool IsAvailable()
    {
        string exe = GetEbookConvertPath();
        return System.IO.File.Exists(exe) || IsOnPath("ebook-convert");
    }

    public async Task<string> ConvertToEpubAsync(string inputPath, CancellationToken ct = default)
    {
        string exe = GetEbookConvertPath();
        if (!System.IO.File.Exists(exe) && !IsOnPath("ebook-convert"))
            throw new CalibreNotFoundException();

        string tempEpub = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            $"wordsprinter_{Guid.NewGuid():N}.epub");

        var psi = new System.Diagnostics.ProcessStartInfo
        {
            FileName = System.IO.File.Exists(exe) ? exe : "ebook-convert",
            Arguments = $"\"{inputPath}\" \"{tempEpub}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = System.Diagnostics.Process.Start(psi)
            ?? throw new ConversionException("Failed to start ebook-convert process.");

        await process.WaitForExitAsync(ct);

        if (process.ExitCode != 0)
        {
            string err = await process.StandardError.ReadToEndAsync(ct);
            throw new ConversionException($"ebook-convert failed (exit {process.ExitCode}): {err}");
        }

        return tempEpub;
    }

    private string GetEbookConvertPath()
    {
        string calibrePath = _settings.GetSetting("CalibrePath") ?? "";
        if (!string.IsNullOrEmpty(calibrePath))
        {
            if (System.IO.File.Exists(calibrePath)) return calibrePath;
            string joined = System.IO.Path.Combine(calibrePath, "ebook-convert");
            if (System.IO.File.Exists(joined)) return joined;
            string joinedExe = joined + ".exe";
            if (System.IO.File.Exists(joinedExe)) return joinedExe;
        }
        return "ebook-convert";
    }

    private static bool IsOnPath(string fileName)
    {
        try
        {
            using var p = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = fileName,
                Arguments = "--version",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });
            p?.WaitForExit(2000);
            return p?.ExitCode == 0;
        }
        catch { return false; }
    }
}
