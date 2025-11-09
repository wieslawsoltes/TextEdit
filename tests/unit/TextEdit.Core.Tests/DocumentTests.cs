using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TextEdit.Core.Documents;

namespace TextEdit.Core.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2007", Justification = "Async test methods run under xUnit synchronization context.")]
public sealed class DocumentTests
{
    private static readonly string[] SnapshotChunksExpectation = ["alpha", " beta"];
    private static readonly string LargeSample = new string('b', 80_000);

    [Fact]
    public void DocumentCreatesIdAndInitialVersion()
    {
        var document = new Document("hello world");

        Assert.NotEqual(default, document.Id);
        Assert.Equal(0, document.Version.Sequence);
        Assert.False(document.IsDirty);
        Assert.Equal("hello world", document.GetText());
    }

    [Fact]
    public void InsertUpdatesVersionAndDirtyState()
    {
        var document = new Document("abc");
        var initialVersion = document.Version;

        document.Insert(document.Length, "def");

        Assert.Equal("abcdef", document.GetText());
        Assert.True(document.IsDirty);
        Assert.True(document.Version.Sequence > initialVersion.Sequence);
    }

    [Fact]
    public void DeleteUpdatesVersionAndLength()
    {
        var document = new Document("abcdef");
        var previousVersion = document.Version;

        document.Delete(2, 3);

        Assert.Equal("abf", document.GetText());
        Assert.True(document.Version.Sequence > previousVersion.Sequence);
        Assert.Equal(3, document.Length);
    }

    [Fact]
    public void UndoAndRedoMutationsAdvanceVersion()
    {
        var document = new Document("abc");
        document.Insert(0, "z");
        var afterInsert = document.Version;

        Assert.True(document.Undo());
        Assert.NotEqual(afterInsert, document.Version);
        Assert.Equal("abc", document.GetText());

        var afterUndo = document.Version;
        Assert.True(document.Redo());
        Assert.NotEqual(afterUndo, document.Version);
        Assert.Equal("zabc", document.GetText());
    }

    [Fact]
    public void SnapshotCapturesVersionAndChunks()
    {
        var document = new Document("alpha");
        document.Insert(document.Length, " beta");

        var snapshot = document.CreateSnapshot();

        Assert.Equal(document.Id, snapshot.Id);
        Assert.Equal(document.Version, snapshot.Version);
        Assert.Equal("alpha beta", snapshot.GetText());
        Assert.Equal(SnapshotChunksExpectation, snapshot.GetChunks().Select(m => new string(m.Span)));
    }

    [Fact]
    public void MarkCleanClearsDirtyState()
    {
        var document = new Document("abc");
        document.Insert(document.Length, "d");
        Assert.True(document.IsDirty);

        document.MarkClean();
        Assert.False(document.IsDirty);
    }

    [Fact]
    public void CreateDocumentWithExistingIdUsesProvidedValue()
    {
        var id = DocumentId.CreateNew();
        var document = new Document("abc", id: id);

        Assert.Equal(id, document.Id);
    }

    [Fact]
    public async Task ChangeBusPublishesInsertEventsAsync()
    {
        var bus = new DocumentChangeBus(TimeSpan.Zero);
        var document = new Document("abc", changeBus: bus);
        var changes = new List<DocumentChangeSet>();
        using var subscription = bus.Subscribe(document.Id, changes.Add);

        document.Insert(document.Length, "def");

        await Task.Delay(10);
        Assert.Single(changes);
        var change = Assert.Single(changes[0].Changes);
        Assert.Equal(DocumentChangeKind.Insert, change.Kind);
        Assert.Equal("def", change.Text);
    }

    [Fact]
    public async Task ChangeBusTransactionBatchesMultipleChangesAsync()
    {
        var bus = new DocumentChangeBus(TimeSpan.Zero);
        var document = new Document(string.Empty, changeBus: bus);
        DocumentChangeSet? captured = null;
        using var subscription = bus.Subscribe(document.Id, set => captured = set);

        using (document.BeginChangeTransaction())
        {
            document.Insert(0, "hello");
            document.Insert(5, " world");
        }

        await Task.Delay(10);
        Assert.NotNull(captured);
        Assert.Equal(2, captured!.Changes.Count);
    }

    [Fact]
    public async Task ChangeBusThrottlesRapidChangesAsync()
    {
        var bus = new DocumentChangeBus(TimeSpan.FromMilliseconds(25));
        var document = new Document("a", changeBus: bus);
        var tcs = new TaskCompletionSource<DocumentChangeSet?>(TaskCreationOptions.RunContinuationsAsynchronously);

        using var subscription = bus.Subscribe(document.Id, set => tcs.TrySetResult(set));

        document.Insert(1, "b");
        document.Insert(2, "c");

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(250));
        var changeSet = await tcs.Task.WaitAsync(cts.Token);
        Assert.NotNull(changeSet);
        Assert.Equal(2, changeSet!.Changes.Count);
    }

    [Fact]
    public void LargeDocumentInsertAndDeleteRemainConsistent()
    {
        var document = new Document();
        document.Insert(0, LargeSample);
        Assert.Equal(LargeSample.Length, document.Length);

        document.Delete(10, 50);
        Assert.Equal(LargeSample.Length - 50, document.Length);
        Assert.True(document.IsDirty);
    }

    [Fact]
    public void DocumentHandlesCrLfAndSurrogatePairs()
    {
        var content = "alpha\r\nbeta👍\r\ngamma";
        var document = new Document(content);

        Assert.Equal(content.Length, document.Length);
        document.Delete(5, 2); // remove CRLF after alpha
        Assert.Equal("alphabeta👍\r\ngamma", document.GetText());
    }
}
