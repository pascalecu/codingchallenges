namespace CodingChallenges.WordCount;

public static class ArgumentParser
{
    private readonly record struct Flags(
        bool Lines = false,
        bool Words = false,
        bool Bytes = false,
        bool Chars = false);

    public static ParseResult ParseArgs(ReadOnlySpan<string> args)
    {
        throw new NotImplementedException();
    }
}