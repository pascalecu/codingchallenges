namespace CodingChallenges.WordCount;

public static class ParseResultExtensions
{
    extension(ParseResult result)
    {
        public ParseResult HandleHelp(Action<TextWriter> printHelp, TextWriter? writer = null)
        {
            if (!result.ShowHelp)
                return result;

            printHelp(writer ?? Console.Out);
            Environment.Exit(0);

            return result;
        }

        public ParseResult HandleError(Action<string?, TextWriter> printError, TextWriter? writer = null)
        {
            if (result.IsSuccess)
                return result;

            printError(result.ErrorMessage, writer ?? Console.Error);
            Environment.Exit(1);
            return result;
        }

        public ParseResult HandleOptions(Action<Options> handler)
        {
            if (result is { IsSuccess: true, Options: not null })
                handler(result.Options);

            return result;
        }
    }
}