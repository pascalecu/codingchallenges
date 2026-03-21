using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;

namespace CodingChallenges.WordCount.Tests;

public class CountResultAssertions(CountResult instance, AssertionChain assertionChain)
    : ObjectAssertions<CountResult, CountResultAssertions>(instance, assertionChain)
{
    protected override string Identifier => nameof(CountResult);

    public AndConstraint<CountResultAssertions> HaveLineCount(long expected, string because = "", params object[] args)
        => HaveMetric(Subject.Lines, expected, "line", because, args);

    public AndConstraint<CountResultAssertions> HaveWordCount(long expected, string because = "", params object[] args)
        => HaveMetric(Subject.Words, expected, "word", because, args);

    public AndConstraint<CountResultAssertions> HaveCharacterCount(long expected, string because = "",
        params object[] args)
        => HaveMetric(Subject.Chars, expected, "character", because, args);

    public AndConstraint<CountResultAssertions> HaveByteCount(long expected, string because = "", params object[] args)
        => HaveMetric(Subject.Bytes, expected, "byte", because, args);

    public AndConstraint<CountResultAssertions> HaveNoLines() => HaveLineCount(0);
    public AndConstraint<CountResultAssertions> HaveNoWords() => HaveWordCount(0);
    public AndConstraint<CountResultAssertions> HaveNoCharacters() => HaveCharacterCount(0);
    public AndConstraint<CountResultAssertions> HaveNoBytes() => HaveByteCount(0);

    private AndConstraint<CountResultAssertions> HaveMetric(
        long actual,
        long expected,
        string name,
        string because,
        object[] args)
    {
        var expectedText = expected switch { 0 => "no", 1 => "one", _ => expected.ToString() };
        var label = expected == 1 ? name : $"{name}s";

        CurrentAssertionChain
            .ForCondition(actual == expected)
            .BecauseOf(because, args)
            .FailWith(
                $"Expected {{context:{Identifier}}} to have {expectedText} {label}{{reason}}, but found {actual}.");

        return new AndConstraint<CountResultAssertions>(this);
    }
}