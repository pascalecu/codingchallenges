namespace CodingChallenges.WordCount;

public record Options
{
    public bool Lines { get; init; }
    public bool Words { get; init; }
    public bool Bytes { get; init; }
    public bool Chars { get; init; }
    
    public List<string> Files { get; init; } = [];
}