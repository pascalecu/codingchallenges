using FluentAssertions;
using FluentAssertions.Execution;

namespace CodingChallenges.WordCount.Tests;

using Xunit;

public class ArgumentParserTests
{
    /// <summary>
    /// Asserts that the Options object matches the expected flags and optionally files.
    /// </summary>
    private static void VerifyOptions(
        ParseResult result,
        bool lines = false,
        bool words = false,
        bool bytes = false,
        bool chars = false,
        IEnumerable<string>? files = null)
    {
        result.IsSuccess.Should().BeTrue("because ParseArgs should have succeeded");
        result.Options.Should().NotBeNull();

        var expected = new Options
        {
            Lines = lines,
            Words = words,
            Bytes = bytes,
            Chars = chars,
            Files = [.. files ?? []]
        };

        result.Options.Should().BeEquivalentTo(expected);
    }

    private static void VerifyNoOptionsSet(ParseResult result)
        => VerifyOptions(result,
            lines: false,
            words: false,
            bytes: false,
            chars: false,
            files: []);

    // -------------------------
    // Single short, long, and clustered options
    // -------------------------

    [Theory(DisplayName = "Should correctly toggle flags for valid short, long, and clustered arguments")]
    [InlineData("-l", true, false, false, false)]
    [InlineData("-w", false, true, false, false)]
    [InlineData("-c", false, false, true, false)]
    [InlineData("-m", false, false, false, true)]
    [InlineData("--lines", true, false, false, false)]
    [InlineData("--words", false, true, false, false)]
    [InlineData("--bytes", false, false, true, false)]
    [InlineData("--chars", false, false, false, true)]
    [InlineData("-lwc", true, true, true, false)]
    [InlineData("-clw", true, true, true, false)]
    [InlineData("-mlw", true, true, false, true)]
    [InlineData("-cw", false, true, true, false)]
    [InlineData("-wlc", true, true, true, false)] // order-invariant
    public void ParseArgs_ValidFlag_SetsCorrespondingOption(
        string arg,
        bool expectLines,
        bool expectWords,
        bool expectBytes,
        bool expectChars)
    {
        var result = ArgumentParser.ParseArgs([arg]);
        
        VerifyOptions(
            result,
            lines: expectLines,
            words: expectWords,
            bytes: expectBytes,
            chars: expectChars);
    }

    [Fact(DisplayName = "Should aggregate flags when multiple short and long options are mixed")]
    public void ParseArgs_MixedOptionStyles_AggregatesAllFlags()
    {
        var result = ArgumentParser.ParseArgs(["-cw", "--lines"]);

        VerifyOptions(
            result,
            lines: true,
            words: true,
            bytes: true,
            chars: false);
    }

    [Fact(DisplayName = "Should apply default flags (lines, words, bytes) when no options are specified")]
    public void ParseArgs_NoArguments_AppliesDefaultWcBehavior()
    {
        var result = ArgumentParser.ParseArgs([]);

        VerifyOptions(result,
            lines: true,
            words: true,
            bytes: true,
            chars: false);
    }

    // -------------------------
    // Help flags
    // -------------------------

    [Theory(DisplayName = "Should trigger help mode when help flags are detected")]
    [InlineData("-h")]
    [InlineData("--help")]
    public void ParseArgs_HelpRequested_ReturnsShowHelpTrue(string arg)
    {
        var result = ArgumentParser.ParseArgs([arg]);
        using (new AssertionScope())
        {
            result.ShowHelp.Should().BeTrue();
            result.Options.Should().BeNull();
        }
    }

    [Theory(DisplayName = "Should prioritize help and ignore all other arguments/files")]
    [InlineData("-h", "file.txt", "-l")]
    [InlineData("-h", "-w")]
    public void ParseArgs_HelpFlagPresent_ShortCircuitsAndIgnoresOtherInputs(params string[] args)
    {
        var result = ArgumentParser.ParseArgs(args);

        using (new AssertionScope())
        {
            result.ShowHelp.Should().BeTrue();
            result.Options.Should().BeNull();
        }
    }

    // -------------------------
    // Mutual exclusion
    // -------------------------

    [Theory(DisplayName = "Should fail when mutually exclusive options (-c and -m) are provided")]
    [InlineData("-cm")]
    [InlineData("-mc")]
    [InlineData("--chars", "--bytes")]
    [InlineData("--bytes", "--chars")]
    [InlineData("-c", "--chars")]
    [InlineData("--bytes", "-m")]
    public void ParseArgs_MutuallyExclusiveFlags_ReturnsFailure(params string[] args)
    {
        var result = ArgumentParser.ParseArgs(args);

        using (new AssertionScope())
        {
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("mutually exclusive");
        }
    }

    // -------------------------
    // Unknown options
    // -------------------------

    [Theory(DisplayName = "Should return error message when an unrecognized flag is encountered")]
    [InlineData("-z")]
    [InlineData("-xw")]
    [InlineData("--foobar")]
    [InlineData("--xyz")]
    public void ParseArgs_UnknownOption_ReturnsFailureWithErrorMessage(string arg)
    {
        var result = ArgumentParser.ParseArgs([arg]);

        using (new AssertionScope())
        {
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Unknown option");
        }
    }

    // -------------------------
    // Files and double-dash handling
    // -------------------------
    
    public static TheoryData<string[], string[], bool, bool, bool, bool> FileArguments => new()
    {
        { ["file1.txt", "file2.txt"], ["file1.txt", "file2.txt"], true, true, true, false },
        { ["-l", "--", "-w", "file.txt"], ["-w", "file.txt"], true, false, false, false },
        { ["--", "--", "file.txt"], ["--", "file.txt"], true, true, true, false },
        { ["--"], [], true, true, true, false },
        { ["-l", "--", "file1.txt", "--", "file2.txt"], ["file1.txt", "--", "file2.txt"], true, false, false, false }
    };

    [Theory(DisplayName = "Should correctly identify file paths and respect the '--' parsing delimiter")]
    [MemberData(nameof(FileArguments))]
    public void ParseArgs_FilesAndDelimiters_ParsesPathsCorrectly(
        string[] args,
        string[] expectedFiles,
        bool expectLines,
        bool expectWords,
        bool expectBytes,
        bool expectChars)
    {
        var result = ArgumentParser.ParseArgs(args);

        VerifyOptions(
            result,
            lines: expectLines,
            words: expectWords,
            bytes: expectBytes,
            chars: expectChars,
            files: expectedFiles);
    }
}