using System.Buffers;
using System.Text;

namespace CodingChallenges.WordCount;

public class WordCounter : IWordCounter
{
    private const int BufferSize = 64 * 1024;

    public async Task<CountResult> CountAsync(Stream stream, Options options, CancellationToken ct = default)
    {
        long lines = 0, words = 0, chars = 0, bytes = 0;
        bool inWord = false;
        byte[] buffer = ArrayPool<byte>.Shared.Rent(BufferSize);
        int leftover = 0;

        try
        {
            while (true)
            {
                int read = await stream.ReadAsync(buffer.AsMemory(leftover, buffer.Length - leftover), ct);
                if (read == 0 && leftover == 0) break;

                int total = read + leftover;
                bytes += read;
                int consumed = 0;
                
                var span = buffer.AsSpan(0, total);

                while (consumed < span.Length)
                {
                    byte b = span[consumed];

                    // ASCII FAST PATH
                    if (b < 0x80)
                    {
                        if (b == '\n') lines++;

                        if (b == ' ' || b is >= 0x09 and <= 0x0D)
                        {
                            inWord = false;
                        }
                        else if (!inWord)
                        {
                            inWord = true;
                            words++;
                        }

                        chars++;
                        consumed++;
                    }
                    else
                    {
                        // UNICODE SLOW PATH
                        var status = Rune.DecodeFromUtf8(span[consumed..], out Rune rune, out int bytesConsumed);
                        
                        if (status == OperationStatus.Done)
                        {
                            if (Rune.IsWhiteSpace(rune))
                            {
                                inWord = false;
                            }
                            else if (!inWord)
                            {
                                inWord = true;
                                words++;
                            }
                            // Note: Rune handles newlines like U+0085, but wc usually only counts \n
                            if (rune.Value == '\n') lines++;

                            chars++;
                            consumed += bytesConsumed;
                        }
                        else if (status == OperationStatus.NeedMoreData)
                        {
                            break; // Exit inner loop to read more bytes
                        }
                        else
                        {
                            // Invalid UTF-8: count as 1 char/byte, treat as word part
                            chars++;
                            consumed++;
                            if (!inWord) { inWord = true; words++; }
                        }
                    }
                }

                leftover = span.Length - consumed;
                if (leftover > 0)
                {
                    span.Slice(consumed).CopyTo(buffer);
                }

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