namespace WordSprinter.Services;

public class Tokenizer
{
    public IReadOnlyList<string> Tokenize(string plainText)
    {
        if (string.IsNullOrWhiteSpace(plainText))
            return Array.Empty<string>();

        return plainText.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
    }
}
