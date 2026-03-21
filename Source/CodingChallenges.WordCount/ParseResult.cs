namespace CodingChallenges.WordCount;

public class ParseResult
{
    public Options? Options { get; private init; }
    public bool ShowHelp { get; private init; }
    public string? ErrorMessage { get; private init; }

    public bool IsSuccess => ErrorMessage is null && !ShowHelp && Options is not null;

    public static ParseResult Success(Options options) => new() { Options = options };
    public static ParseResult Help() => new() { ShowHelp = true };
    public static ParseResult Failure(string message) => new() { ErrorMessage = message };
}