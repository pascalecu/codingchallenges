using System.IO.Pipelines;

namespace CodingChallenges.WordCount;

public interface IWordCounter
{
    Task<CountResult> CountAsync(Stream stream, Options options, CancellationToken ct = default);
}