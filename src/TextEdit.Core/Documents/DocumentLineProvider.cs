using System;
using System.Collections.Generic;
using TextEdit.Core.Caret;

namespace TextEdit.Core.Documents;

/// <summary>
/// Provides line metrics for a <see cref="DocumentSnapshot"/>.
/// </summary>
public sealed class DocumentLineProvider : ITextLineProvider
{
    private readonly int[] _lineLengths;

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentLineProvider"/> class.
    /// </summary>
    public DocumentLineProvider(DocumentSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        _lineLengths = ComputeLineLengths(snapshot.GetText());
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentLineProvider"/> class from raw text.
    /// </summary>
    public DocumentLineProvider(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        _lineLengths = ComputeLineLengths(text);
    }

    /// <inheritdoc/>
    public int LineCount => _lineLengths.Length;

    /// <inheritdoc/>
    public int GetLineLength(int lineIndex)
    {
        if ((uint)lineIndex >= (uint)_lineLengths.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(lineIndex));
        }

        return _lineLengths[lineIndex];
    }

    private static int[] ComputeLineLengths(string text)
    {
        var lengths = new List<int>();

        if (text.Length == 0)
        {
            lengths.Add(0);
            return lengths.ToArray();
        }

        var currentLength = 0;

        for (var i = 0; i < text.Length; i++)
        {
            var ch = text[i];
            if (ch == '\r')
            {
                lengths.Add(currentLength);
                currentLength = 0;

                if (i + 1 < text.Length && text[i + 1] == '\n')
                {
                    i++;
                }

                continue;
            }

            if (ch == '\n')
            {
                lengths.Add(currentLength);
                currentLength = 0;
                continue;
            }

            currentLength++;
        }

        lengths.Add(currentLength);
        return lengths.ToArray();
    }
}
