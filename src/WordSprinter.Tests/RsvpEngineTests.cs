using WordSprinter.Services;
using Xunit;

namespace WordSprinter.Tests;

public class RsvpEngineTests
{
    [Theory]
    [InlineData("I", 0)]
    [InlineData("am", 0)]
    [InlineData("the", 1)]
    [InlineData("word", 1)]
    [InlineData("words", 1)]
    [InlineData("sprint", 1)]
    [InlineData("sprints", 2)]
    [InlineData("sprinting", 2)]
    [InlineData("speedreader", 3)]
    [InlineData("presentations", 3)]
    [InlineData("Antidisestablishmentarianism", 4)]
    public void OrpIndex_CorrectForWordLength(string word, int expectedIndex)
    {
        int result = RsvpEngine.ComputeOrpIndex(word);
        Assert.Equal(expectedIndex, result);
    }

    [Theory]
    [InlineData("hello.", 2.5)]
    [InlineData("world!", 2.5)]
    [InlineData("really?", 2.5)]
    [InlineData("fast,", 1.5)]
    [InlineData("read;", 1.5)]
    [InlineData("note:", 1.5)]
    [InlineData("normal", 1.0)]
    public void PunctuationFactor_Correct(string word, double expected)
    {
        Assert.Equal(expected, RsvpEngine.ComputePunctuationFactor(word), precision: 1);
    }

    [Fact]
    public void Duration_HasFloorOf100Ms()
    {
        // At 1000 WPM, base is 60ms — should hit floor
        int duration = RsvpEngine.ComputeDurationMs("I", 1000);
        Assert.True(duration >= 100);
    }

    [Fact]
    public void Duration_SentenceEndIsLonger()
    {
        int normal = RsvpEngine.ComputeDurationMs("word", 250);
        int sentenceEnd = RsvpEngine.ComputeDurationMs("word.", 250);
        Assert.True(sentenceEnd > normal);
    }
}
