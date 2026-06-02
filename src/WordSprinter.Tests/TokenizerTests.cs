using WordSprinter.Services;
using Xunit;

namespace WordSprinter.Tests;

public class TokenizerTests
{
    private readonly Tokenizer _tokenizer = new();

    [Fact]
    public void Tokenize_SplitsOnWhitespace()
    {
        var result = _tokenizer.Tokenize("Hello world foo");
        Assert.Equal(3, result.Count);
        Assert.Equal("Hello", result[0]);
        Assert.Equal("world", result[1]);
        Assert.Equal("foo", result[2]);
    }

    [Fact]
    public void Tokenize_EmptyString_ReturnsEmpty()
    {
        var result = _tokenizer.Tokenize("");
        Assert.Empty(result);
    }

    [Fact]
    public void Tokenize_PreservesPunctuation()
    {
        var result = _tokenizer.Tokenize("Hello, world.");
        Assert.Equal(2, result.Count);
        Assert.Equal("Hello,", result[0]);
        Assert.Equal("world.", result[1]);
    }
}
