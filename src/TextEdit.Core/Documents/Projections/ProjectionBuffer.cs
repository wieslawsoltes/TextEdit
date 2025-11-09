using System;
using System.Collections.Generic;
using System.Linq;

namespace TextEdit.Core.Documents.Projections;

/// <summary>
/// Provides a base for projection buffers that track a backing document.
/// </summary>
public abstract class ProjectionBuffer : IDisposable
{
    private readonly Document _document;
    private readonly DocumentChangeBus _changeBus;
    private readonly IDisposable _subscription;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectionBuffer"/> class.
    /// </summary>
    protected ProjectionBuffer(Document document, DocumentChangeBus? changeBus = null, bool initializeSnapshot = true)
    {
        _document = document ?? throw new ArgumentNullException(nameof(document));
        _changeBus = changeBus ?? DocumentChangeBus.Global;
        _subscription = _changeBus.Subscribe(document.Id, _ => RebuildSnapshot());
        if (initializeSnapshot)
        {
            RebuildSnapshot();
        }
    }

    /// <summary>
    /// Gets the most recent snapshot.
    /// </summary>
    public ProjectionSnapshot CurrentSnapshot { get; private set; } = default!;

    /// <summary>
    /// Builds a snapshot using the provided document snapshot.
    /// </summary>
    protected abstract IReadOnlyList<ProjectionSegment> BuildSegments(DocumentSnapshot documentSnapshot);

    /// <summary>
    /// Creates and returns an immutable snapshot.
    /// </summary>
    public ProjectionSnapshot CreateSnapshot()
    {
        EnsureNotDisposed();
        return CurrentSnapshot;
    }

    /// <summary>
    /// Releases the projection buffer and unsubscribes from change notifications.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Allows derived classes to run initial snapshot logic after construction.
    /// </summary>
    protected void InitializeSnapshot() => RebuildSnapshot();

    protected void RebuildSnapshot()
    {
        var documentSnapshot = _document.CreateSnapshot();
        var segments = BuildSegments(documentSnapshot);

        CurrentSnapshot = new ProjectionSnapshot(
            _document.Id,
            documentSnapshot.Version,
            segments);
    }

    /// <summary>
    /// Disposes managed resources. Override to release additional state.
    /// </summary>
    /// <param name="disposing">True when called via <see cref="Dispose()"/>.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _subscription.Dispose();
        }

        _disposed = true;
    }

    private void EnsureNotDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, GetType().Name);
    }
}
