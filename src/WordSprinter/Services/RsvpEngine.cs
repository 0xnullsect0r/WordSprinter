using WordSprinter.Models;

namespace WordSprinter.Services;

public class RsvpEngine
{
    private static readonly int[] OrpTable = { 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 3, 3, 3, 3, 4 };

    public static int ComputeOrpIndex(string word)
    {
        int letterCount = 0;
        int firstLetterPos = -1;
        for (int i = 0; i < word.Length; i++)
        {
            if (char.IsLetter(word[i]))
            {
                if (firstLetterPos == -1) firstLetterPos = i;
                letterCount++;
            }
        }

        if (letterCount == 0) return 0;

        int tableIdx = Math.Min(letterCount, OrpTable.Length - 1);
        int orpLetterOffset = OrpTable[tableIdx];

        // Map letter-offset back to char index in original word
        int lettersFound = 0;
        for (int i = 0; i < word.Length; i++)
        {
            if (char.IsLetter(word[i]))
            {
                if (lettersFound == orpLetterOffset) return i;
                lettersFound++;
            }
        }
        return 0;
    }

    public static double ComputePunctuationFactor(string word)
    {
        for (int i = word.Length - 1; i >= 0; i--)
        {
            char c = word[i];
            if (c == '.' || c == '!' || c == '?') return 2.5;
            if (c == ',' || c == ';' || c == ':') return 1.5;
            if (!char.IsWhiteSpace(c)) break;
        }
        return 1.0;
    }

    public static int ComputeDurationMs(string word, int wpm)
    {
        if (wpm <= 0) wpm = 250;
        double baseMs = 60000.0 / wpm;
        double punctFactor = ComputePunctuationFactor(word);
        int letterCount = word.Count(char.IsLetter);
        double lengthPenalty = Math.Sqrt(Math.Max(letterCount, 1)) * 0.04 * baseMs * 0.9;
        int duration = (int)(baseMs * punctFactor + lengthPenalty);
        return Math.Max(duration, 100);
    }

    public RsvpToken TokenizeWord(string rawWord, int globalIndex, int wpm)
    {
        string displayWord = rawWord;
        int orpIndex = ComputeOrpIndex(displayWord);
        int durationMs = ComputeDurationMs(rawWord, wpm);
        return new RsvpToken(rawWord, displayWord, orpIndex, durationMs, globalIndex);
    }

    public IReadOnlyList<RsvpToken> BuildTokenList(IReadOnlyList<string> words, int wpm)
    {
        var tokens = new List<RsvpToken>(words.Count);
        for (int i = 0; i < words.Count; i++)
            tokens.Add(TokenizeWord(words[i], i, wpm));
        return tokens;
    }
}
