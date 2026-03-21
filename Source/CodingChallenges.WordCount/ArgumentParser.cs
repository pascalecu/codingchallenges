namespace CodingChallenges.WordCount;

public static class ArgumentParser
{
    private readonly record struct Flags(
        bool Lines = false,
        bool Words = false,
        bool Bytes = false,
        bool Chars = false);

    public static ParseResult ParseArgs(ReadOnlySpan<string> args)
    {
        Flags flags = new();
        List<string> files = new(capacity: args.Length);
        var treatRemainingAsFiles = false;

        foreach (var arg in args)
        {
            if (treatRemainingAsFiles)
            {
                files.Add(arg);
                continue;
            }

            switch (arg)
            {
                case "--":
                    treatRemainingAsFiles = true;
                    continue;
                case "-":
                    files.Add(arg);
                    continue;
            }

            if (arg.StartsWith("--", StringComparison.Ordinal))
            {
                var (updatedFlags, result) = ParseLongOption(arg, flags);
                if (result is not null) return result;
                flags = updatedFlags;
                continue;
            }

            if (arg.StartsWith('-') && arg.Length > 1)
            {
                var (updatedFlags, result) = ParseShortOptions(arg, flags);
                if (result is not null) return result;
                flags = updatedFlags;
                continue;
            }

            files.Add(arg);
        }

        if (flags is { Lines: false, Words: false, Bytes: false, Chars: false })
        {
            flags = flags with { Lines = true, Words = true, Bytes = true };
        }

        if (flags is { Bytes: true, Chars: true })
        {
            return ParseResult.Failure("Error: options -c and -m are mutually exclusive.");
        }

        return ParseResult.Success(new Options
        {
            Lines = flags.Lines,
            Words = flags.Words,
            Bytes = flags.Bytes,
            Chars = flags.Chars,
            Files = files
        });
    }

    private static (Flags Flags, ParseResult? Result) ParseLongOption(string arg, Flags current)
    {
        return arg switch
        {
            "--lines" => (current with { Lines = true }, null),
            "--words" => (current with { Words = true }, null),
            "--bytes" => (current with { Bytes = true }, null),
            "--chars" => (current with { Chars = true }, null),
            "--help" => (current, ParseResult.Help()),
            _ => (current, ParseResult.Failure($"Unknown option: {arg}"))
        };
    }

    private static (Flags Flags, ParseResult? Result) ParseShortOptions(string arg, Flags current)
    {
        var state = current;

        foreach (var ch in arg.AsSpan(1))
        {
            var (newState, result) = ch switch
            {
                'l' => (state with { Lines = true }, null),
                'w' => (state with { Words = true }, null),
                'c' => (state with { Bytes = true }, null), // -c = byte count
                'm' => (state with { Chars = true }, null), // -m = character count
                'h' => (state, ParseResult.Help()),
                _ => (state, ParseResult.Failure($"Unknown option: -{ch}"))
            };

            if (result is not null)
                return (newState, result);

            state = newState;
        }

        return (state, null);
    }
}