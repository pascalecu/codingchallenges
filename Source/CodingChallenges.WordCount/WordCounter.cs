namespace CodingChallenges.WordCount;

public class WordCounter : IWordCounter
{
    public async Task<CountResult> CountAsync(Stream stream, Options options, CancellationToken ct = default)
    {
        await Task.Yield();
        return new CountResult(
            Lines: -1,
            Words: -1,
            Bytes: -1,
            Chars: -1
        );
    }
}