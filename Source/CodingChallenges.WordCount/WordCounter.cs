using System.Buffers;
using System.Text;

namespace CodingChallenges.WordCount;

public class WordCounter : IWordCounter
{
    private const int BufferSize = 64 * 1024;

    public async Task<CountResult> CountAsync(Stream stream, Options options, CancellationToken ct = default)
    {
        long lines = 0, words = 0, chars = 0, bytes = 0;
        var inWord = false;
        var buffer = ArrayPool<byte>.Shared.Rent(BufferSize);
        var leftover = 0;

        try
        {
            while (true)
            {
                var read = await stream.ReadAsync(buffer.AsMemory(leftover, buffer.Length - leftover), ct);
                if (read == 0 && leftover == 0) break;

                var total = read + leftover;
                bytes += read;
                var consumed = 0;

                var span = buffer.AsSpan(0, total);

                while (consumed < span.Length)
                {
                    var b = span[consumed];

                    // ASCII FAST PATH
                    if (b < 0x80)
                    {
                        if (b == '\n') lines++;
                        if (b is 0x20 or >= 0x09 and <= 0x0D) inWord = false;
                        else if (!inWord)
                        {
                            inWord = true;
                            words++;
                        }

                        chars++;
                        consumed++;
                    }
                    else // Unicode slow path
                    {
                        var status = Rune.DecodeFromUtf8(span[consumed..], out Rune rune, out int bytesConsumed);

                        switch (status)
                        {
                            case OperationStatus.Done:
                                if (Rune.IsWhiteSpace(rune)) inWord = false;
                                else if (!inWord)
                                {
                                    inWord = true;
                                    words++;
                                }

                                if (rune.Value == '\n') lines++;
                                chars++;
                                consumed += bytesConsumed;
                                break;

                            case OperationStatus.NeedMoreData:
                                goto EndOfBuffer;

                            case OperationStatus.InvalidData:
                                chars++;
                                consumed++;
                                if (!inWord)
                                {
                                    inWord = true;
                                    words++;
                                }

                                break;
                        }
                    }
                }

                EndOfBuffer:
                leftover = span.Length - consumed;
                if (leftover > 0)
                    span[consumed..].CopyTo(buffer);

                if (read == 0) break;
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }

        return new CountResult(
            Lines: options.Lines ? lines : 0,
            Words: options.Words ? words : 0,
            Bytes: options.Bytes ? bytes : 0,
            Chars: options.Chars ? chars : 0
        );
    }
}