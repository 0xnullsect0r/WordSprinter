using System.Text;
using System.Text.RegularExpressions;

namespace WordSprinter.Services.BookParser;

public static class TextCleaningPipeline
{
    private static readonly Regex MultiSpace = new(@"\s+", RegexOptions.Compiled);
    private static readonly Regex ZeroWidth = new(@"[​-‍﻿]", RegexOptions.Compiled);

    public static string Clean(string raw)
    {
        // NFC normalize
        string text = raw.Normalize(NormalizationForm.FormC);
        // Remove zero-width chars
        text = ZeroWidth.Replace(text, "");
        // Pad dashes
        text = text.Replace('—', ' ').Replace('–', ' ');
        // Collapse whitespace
        text = MultiSpace.Replace(text, " ").Trim();
        return text;
    }
}
