using System.Text;
using Xunit;

namespace CodingChallenges.WordCount.Tests;

public class WordCounterTests
{
    private readonly WordCounter _sut = new();

    private async Task<CountResult> CountAsync(string input, Options options, Encoding? encoding = null)
    {
        encoding ??= Encoding.UTF8;
        using var ms = new MemoryStream(encoding.GetBytes(input));
        return await _sut.CountAsync(ms, options);
    }

    [Theory(DisplayName = "Should return correct counts for basic inputs")]
    [InlineData("Hello, world!", 0, 2, 13, 13)]
    [InlineData("Hello\nworld foo\n", 2, 3, 16, 16)]
    [InlineData("", 0, 0, 0, 0)]
    public async Task CountAsync_WithBasicInputs_ReturnsExpectedCounts(string input, long expectedLines,
        long expectedWords, long expectedChars, long expectedBytes)
    {
        var result = await CountAsync(input, new Options(true, true, true, true));

        result.Should()
            .HaveLineCount(expectedLines)
            .And.HaveWordCount(expectedWords)
            .And.HaveCharacterCount(expectedChars)
            .And.HaveByteCount(expectedBytes);
    }

    [Fact(DisplayName = "Should return zero counts when no options are enabled")]
    public async Task CountAsync_WithNoOptionsEnabled_ReturnsZeroCounts()
    {
        var result = await CountAsync("hello world\nfoo", new Options());

        result.Should()
            .HaveNoLines()
            .And.HaveNoWords()
            .And.HaveNoCharacters()
            .And.HaveNoBytes();
    }

    [Fact(DisplayName = "Should return only word count when only words option is enabled")]
    public async Task CountAsync_WithOnlyWordsEnabled_ReturnsOnlyWordCount()
    {
        var result = await CountAsync("hello\nworld", new Options(words: true));

        result.Should()
            .HaveWordCount(2)
            .And.HaveNoLines()
            .And.HaveNoCharacters()
            .And.HaveNoBytes();
    }

    [Theory(DisplayName = "Should count words based on whitespace separation")]
    [InlineData("hello world", 2)]
    [InlineData("   hello   world   ", 2)]
    [InlineData("hello \n \t world", 2)]
    [InlineData("multiple\nlines\n\nwords", 3)]
    [InlineData("   ", 0)]
    [InlineData("\n\n\n", 0)]
    [InlineData("word1\vword2\fword3", 3)]
    [InlineData("hello\u00A0world", 2)]
    [InlineData("hello,world!", 1)]
    [InlineData("hello,world! this-is a.test", 3)]
    [InlineData("hello👍world 👍", 2)]
    public async Task CountAsync_WithWhitespaceAndPunctuation_ReturnsExpectedWordCount(string input, long expected)
    {
        var result = await CountAsync(input, new Options(words: true));
        result.Should().HaveWordCount(expected);
    }

    [Fact(DisplayName = "Should count a word only once even if split across internal buffers")]
    public async Task CountAsync_WithWordAcrossBufferBoundary_CountsWordOnce()
    {
        var input = new string('a', 65535) + "WORD";
        var result = await CountAsync(input, new Options(words: true));
        result.Should().HaveWordCount(1);
    }

    [Theory(DisplayName = "Should count lines based on newline characters")]
    [InlineData("no newline", 0)]
    [InlineData("one\n", 1)]
    [InlineData("one\ntwo", 1)]
    [InlineData("one\ntwo\nthree\n", 3)]
    [InlineData("\n\n\n", 3)]
    [InlineData("a\r\nb\r\nc", 2)]
    [InlineData("a\rb\rc", 0)]
    [InlineData("a\nb\r\nc\rd", 2)]
    public async Task CountAsync_WithVariousNewlines_ReturnsExpectedLineCount(string input, long expectedLines)
    {
        var result = await CountAsync(input, new Options(lines: true));
        result.Should().HaveLineCount(expectedLines);
    }

    [Theory(DisplayName = "Should count characters as Unicode scalar values and bytes as UTF-8 length")]
    [InlineData("abc", 3, 3)]
    [InlineData("你好", 2, 6)]
    [InlineData("🙂", 1, 4)]
    [InlineData("a🙂b", 3, 6)]
    [InlineData("¢", 1, 2)]
    [InlineData("€", 1, 3)]
    [InlineData("𐍈", 1, 4)]
    public async Task CountAsync_WithUnicodeInput_ReturnsExpectedCharacterAndByteCounts(
        string input,
        long expectedCharacters,
        long expectedBytes)
    {
        var result = await CountAsync(input, new Options(chars: true, bytes: true));

        result.Should()
            .HaveCharacterCount(expectedCharacters)
            .And.HaveByteCount(expectedBytes);
    }

    [Fact(DisplayName = "Should correctly count characters and bytes for large Unicode input")]
    public async Task CountAsync_WithLargeUnicodeInput_ReturnsExpectedCharacterAndByteCounts()
    {
        const string chunk = "🙂你好"; // 3 characters, 10 bytes (UTF-8)
        const int repetitions = 5000;

        var input = string.Concat(Enumerable.Repeat(chunk, repetitions));

        const int expectedCharacters = 3 * repetitions;
        const int expectedBytes = 10 * repetitions;

        var result = await CountAsync(input, new Options(chars: true, bytes: true));

        result.Should()
            .HaveCharacterCount(expectedCharacters,
                "each chunk contains 3 Unicode scalar values")
            .And.HaveByteCount(expectedBytes,
                "each chunk encodes to 10 bytes in UTF-8");
    }

    [Fact(DisplayName = "Should handle large input and return correct line and word counts")]
    public async Task CountAsync_WithLargeInput_ReturnsExpectedLineAndWordCounts()
    {
        var input = string.Join('\n', Enumerable.Repeat("hello world", 10_000));

        var result = await CountAsync(input, new Options(lines: true, words: true));

        result.Should()
            .HaveLineCount(9_999)
            .And.HaveWordCount(20_000);
    }

    [Fact(DisplayName = "Should count a very long word as a single word")]
    public async Task CountAsync_WithLongSingleWord_ReturnsOneWord()
    {
        var input = new string('a', 100_000);

        var result = await CountAsync(input, new Options(words: true));

        result.Should().HaveWordCount(1);
    }
}