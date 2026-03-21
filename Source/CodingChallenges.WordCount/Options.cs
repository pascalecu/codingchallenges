namespace CodingChallenges.WordCount;

public record Options
{
    public Options()
    {
    }

    public Options(bool lines, bool words, bool chars, bool bytes)
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