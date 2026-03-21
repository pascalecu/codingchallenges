using FluentAssertions.Execution;

namespace CodingChallenges.WordCount.Tests;

public static class CountResultExtensions
{
    public static CountResultAssertions Should(this CountResult instance)
        => new(instance, AssertionChain.GetOrCreate());
}