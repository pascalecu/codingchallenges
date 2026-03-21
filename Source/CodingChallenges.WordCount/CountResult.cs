namespace CodingChallenges.WordCount;

public record struct CountResult(
    long Lines = 0,
    long Words = 0,
    long Bytes = 0,
    long Chars = 0)
{
    public static CountResult operator +(CountResult left, CountResult right) =>
        new(
            left.Lines + right.Lines,
            left.Words + right.Words,
            left.Bytes + right.Bytes,
            left.Chars + right.Chars
        );

    public void operator += (CountResult right)
    {
        Lines += right.Lines;
        Words += right.Words;
        Bytes += right.Bytes;
        Chars += right.Chars;
    }

    public static readonly CountResult Empty = default;
}