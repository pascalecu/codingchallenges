namespace CodingChallenges.WordCount;

public record Options
{
    public Options()
    {
    }

    public Options(bool lines = false, bool words = false, bool chars = false, bool bytes = false)
    {
        Lines = lines;
        Words = words;
        Chars = chars;
        Bytes = bytes;
    }

    public bool Lines { get; init; }
    public bool Words { get; init; }
    public bool Bytes { get; init; }
    public bool Chars { get; init; }

    public List<string> Files { get; init; } = [];
}