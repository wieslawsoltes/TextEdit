using System.Linq;
using TextEdit.Core.Buffers;

namespace TextEdit.Core.Tests;

public sealed class PieceTreeTextBufferTests
{
    private static readonly string[] BufferChunksExpectation = ["Say ", "hello", " world"];
    private static readonly string[] SnapshotChunksExpectation = ["alpha", " beta"];
    private static readonly string LargeSample = new string('a', 100_000);

    [Fact]
    public void GetTextReturnsRequestedSubstring()
    {
        var buffer = new PieceTreeTextBuffer("abcdef");
        buffer.Insert(3, "XYZ");

        var slice = buffer.GetText(2, 4);

        Assert.Equal("cXYZ", slice);
    }

    [Fact]
    public void ConstructorWithInitialTextPopulatesPieces()
    {
        var buffer = new PieceTreeTextBuffer("hello");

        Assert.Equal(5, buffer.Length);
        Assert.Equal("hello", buffer.ToString());
    }

    [Fact]
    public void InsertAppendsTextWhenBufferEmpty()
    {
        var buffer = new PieceTreeTextBuffer();

        buffer.Insert(0, "abc");

        Assert.Equal(3, buffer.Length);
        Assert.Equal("abc", buffer.ToString());
    }

    [Fact]
    public void InsertInMiddleSplitsPiece()
    {
        var buffer = new PieceTreeTextBuffer("abcd");

        buffer.Insert(2, "XYZ");

        Assert.Equal("abXYZcd", buffer.ToString());
        Assert.Equal(7, buffer.Length);
    }

    [Fact]
    public void DeleteRemovesSpecifiedRange()
    {
        var buffer = new PieceTreeTextBuffer("abcdef");

        buffer.Delete(2, 3);

        Assert.Equal("abf", buffer.ToString());
        Assert.Equal(3, buffer.Length);
    }

    [Fact]
    public void DeleteClampLengthWhenRangeExtendsPastEnd()
    {
        var buffer = new PieceTreeTextBuffer("abcdef");

        buffer.Delete(3, 100);

        Assert.Equal("abc", buffer.ToString());
    }

    [Fact]
    public void UndoInsertRestoresPreviousContent()
    {
        var buffer = new PieceTreeTextBuffer("abc");
        buffer.Insert(3, "def");

        Assert.True(buffer.Undo());
        Assert.Equal("abc", buffer.ToString());
        Assert.True(buffer.CanRedo);
        Assert.False(buffer.CanUndo);
    }

    [Fact]
    public void UndoDeleteRestoresDeletedText()
    {
        var buffer = new PieceTreeTextBuffer("abcdef");
        buffer.Delete(1, 3);

        Assert.Equal("aef", buffer.ToString());
        Assert.True(buffer.Undo());
        Assert.Equal("abcdef", buffer.ToString());
    }

    [Fact]
    public void RedoReappliesChangeAfterUndo()
    {
        var buffer = new PieceTreeTextBuffer("abc");
        buffer.Insert(3, "d");
        buffer.Undo();

        Assert.True(buffer.Redo());
        Assert.Equal("abcd", buffer.ToString());
    }

    [Fact]
    public void UndoCoalescesSequentialTyping()
    {
        var buffer = new PieceTreeTextBuffer();
        buffer.Insert(0, "a");
        buffer.Insert(1, "b");
        buffer.Insert(2, "c");

        Assert.True(buffer.Undo());
        Assert.Equal(string.Empty, buffer.ToString());
        Assert.True(buffer.Redo());
        Assert.Equal("abc", buffer.ToString());
    }

    [Fact]
    public void UndoCoalescesBackspaceDeletes()
    {
        var buffer = new PieceTreeTextBuffer("hello");

        buffer.Delete(4, 1); // remove 'o'
        buffer.Delete(3, 1); // remove final 'l'

        Assert.Equal("hel", buffer.ToString());
        Assert.True(buffer.Undo());
        Assert.Equal("hello", buffer.ToString());
    }

    [Fact]
    public void LargeInsertMaintainsIntegrity()
    {
        var buffer = new PieceTreeTextBuffer();

        buffer.Insert(0, LargeSample);

        Assert.Equal(LargeSample.Length, buffer.Length);
        Assert.Equal(LargeSample, buffer.ToString());
    }

    [Fact]
    public void DeleteAcrossCrLfSegmentsRemovesWholeLines()
    {
        const string Source = "line1\r\nline2\r\nline3\r\n";
        var buffer = new PieceTreeTextBuffer(Source);

        // Remove the middle line including trailing newline.
        var start = Source.IndexOf("line2", StringComparison.Ordinal);
        buffer.Delete(start, "line2\r\n".Length);

        Assert.Equal("line1\r\nline3\r\n", buffer.ToString());
    }

    [Fact]
    public void SurrogatePairsRemainIntact()
    {
        const string Emoji = "👍";
        var buffer = new PieceTreeTextBuffer();

        buffer.Insert(0, Emoji);
        buffer.Insert(buffer.Length, Emoji);

        Assert.Equal(4, buffer.Length); // each emoji is two UTF-16 code units
        Assert.Equal("👍👍", buffer.ToString());

        buffer.Delete(2, 2);
        Assert.Equal("👍", buffer.ToString());
        Assert.Equal(2, buffer.Length);
    }

    [Fact]
    public void RedoClearedAfterNewOperation()
    {
        var buffer = new PieceTreeTextBuffer("abc");
        buffer.Insert(3, "d");
        buffer.Undo();

        buffer.Insert(3, "X");

        Assert.False(buffer.CanRedo);
        Assert.Equal("abcX", buffer.ToString());
    }

    [Fact]
    public void SnapshotPreservesHistoricalContent()
    {
        var buffer = new PieceTreeTextBuffer("one two");

        var snapshot = buffer.CreateSnapshot();
        buffer.Insert(3, "-insert-");

        Assert.Equal("one two", snapshot.GetText());
        Assert.NotEqual(snapshot.GetText(), buffer.ToString());
    }

    [Fact]
    public void ChunksExposeUnderlyingSegments()
    {
        var buffer = new PieceTreeTextBuffer("hello");
        buffer.Insert(5, " world");
        buffer.Insert(0, "Say ");

        var chunks = buffer.GetChunks().Select(m => new string(m.Span)).ToArray();

        Assert.Equal(BufferChunksExpectation, chunks);
    }

    [Fact]
    public void SnapshotChunksRemainStableAfterMutations()
    {
        var buffer = new PieceTreeTextBuffer("alpha");
        buffer.Insert(5, " beta");
        var snapshot = buffer.CreateSnapshot();

        buffer.Delete(0, buffer.Length);
        buffer.Insert(0, "gamma");

        var snapshotChunks = snapshot.GetChunks().Select(m => new string(m.Span)).ToArray();

        Assert.Equal(SnapshotChunksExpectation, snapshotChunks);
        Assert.Equal("gamma", buffer.ToString());
    }
}
