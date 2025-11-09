using System;
using System.Collections.Generic;
using System.Linq;
using TextEdit.Core.Documents;
using TextEdit.Core.Documents.Projections;

namespace TextEdit.Core.Tests;

public sealed class ProjectionBufferTests
{
    [Fact]
    public void ReadOnlyProjectionUpdatesWithDocument()
    {
        var bus = new DocumentChangeBus(TimeSpan.Zero);
        var document = new Document("hello", changeBus: bus);

        using var buffer = new ReadOnlyProjectionBuffer(document, bus);

        Assert.Equal("hello", buffer.CreateSnapshot().Text);

        document.Insert(document.Length, " world");

        Assert.Equal("hello world", buffer.CreateSnapshot().Text);
    }

    [Fact]
    public void MetadataProjectionIncludesOverlayInformation()
    {
        var bus = new DocumentChangeBus(TimeSpan.Zero);
        var document = new Document("Alpha beta", changeBus: bus);

        using var buffer = new MetadataProjectionBuffer(
            document,
            snapshot =>
            {
                var text = snapshot.GetText();
                var segments = new List<ProjectionSegment>();
                var words = text.Split(' ');
                foreach (var word in words)
                {
                    var kind = char.IsUpper(word[0])
                        ? ProjectionSegmentKind.Metadata
                        : ProjectionSegmentKind.Original;
                    var metadata = kind == ProjectionSegmentKind.Metadata
                        ? new Dictionary<string, object?> { ["classification"] = "keyword" }
                        : null;
                    segments.Add(new ProjectionSegment(word, kind, metadata));
                    segments.Add(new ProjectionSegment(" ", ProjectionSegmentKind.Literal));
                }

                if (segments.Count > 0)
                {
                    segments.RemoveAt(segments.Count - 1); // remove trailing space
                }

                return segments;
            },
            bus);

        var snapshot = buffer.CreateSnapshot();
        var keywordSegment = snapshot.Segments.First(s => s.Metadata?.ContainsKey("classification") == true);

        Assert.Equal("Alpha", keywordSegment.Text);
        Assert.Equal("keyword", keywordSegment.Metadata?["classification"]);
    }

    [Fact]
    public void DiffProjectionProducesAddedAndRemovedSegments()
    {
        var bus = new DocumentChangeBus(TimeSpan.Zero);
        var document = new Document("line1", changeBus: bus);
        var baseline = document.CreateSnapshot();

        using var diffBuffer = new DiffProjectionBuffer(document, baseline, bus);

        document.Insert(document.Length, " updated");

        var diffSnapshot = diffBuffer.CreateSnapshot();
        Assert.Contains(diffSnapshot.Segments, s => s.Kind == ProjectionSegmentKind.Removed);
        Assert.Contains(diffSnapshot.Segments, s => s.Kind == ProjectionSegmentKind.Added);
    }
}
