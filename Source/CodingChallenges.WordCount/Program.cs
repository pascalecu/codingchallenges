using CodingChallenges.WordCount;


var result =
    ArgumentParser
        .ParseArgs(args)
        .HandleHelp(PrintHelp)
        .HandleError(PrintError)
        .HandleOptions(PrintOptions);

return;

static void PrintOptions(Options options)
{
    var filesDisplay = options.Files.Count == 0
        ? "Reading from standard input..."
        : $"Files: {string.Join(", ", options.Files)}";

    Console.WriteLine(
        $"""
         Options selected:
           Lines: {options.Lines} | Words: {options.Words} | Bytes: {options.Bytes} | Chars: {options.Chars}
         {filesDisplay}
         """);
}

static void PrintHelp(TextWriter writer)
{
    var program = AppDomain.CurrentDomain.FriendlyName;

    writer.WriteLine($"""
                      Usage: {program} [OPTION]... [FILE]...
                      Print newline, word, and byte counts for each FILE.

                      Options:
                        -c, --bytes    print the byte counts
                        -m, --chars    print the character counts
                        -l, --lines    print the newline counts
                        -w, --words    print the word counts
                        -h, --help     display this help and exit

                      Use -- to stop option parsing and treat remaining arguments as files.
                      """);
}

static void PrintError(string? message, TextWriter writer)
{
    writer.WriteLine($"{message ?? "an unexpected error occurred"}");
    writer.WriteLine("Try '--help' for more information.");
}