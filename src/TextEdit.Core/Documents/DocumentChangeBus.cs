using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace TextEdit.Core.Documents;

/// <summary>
/// Publishes document change notifications to subscribers with optional batching and throttling.
/// </summary>
public sealed class DocumentChangeBus
{
    private readonly TimeSpan _throttleWindow;
    private readonly object _gate = new();
    private readonly List<Subscription> _subscriptions = new();
    private readonly Dictionary<DocumentId, PendingState> _pending = new();
    private readonly Dictionary<DocumentId, TransactionState> _transactions = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentChangeBus"/> class.
    /// </summary>
    /// <param name="throttleWindow">Minimum duration to batch together sequential changes.</param>
    public DocumentChangeBus(TimeSpan throttleWindow)
    {
        _throttleWindow = throttleWindow < TimeSpan.Zero ? TimeSpan.Zero : throttleWindow;
    }

    /// <summary>
    /// Gets the global bus instance used by default documents.
    /// </summary>
    public static DocumentChangeBus Global { get; } = new(TimeSpan.FromMilliseconds(0));

    /// <summary>
    /// Subscribes to change notifications.
    /// </summary>
    /// <param name="documentId">Optional document filter; when <c>null</c> all documents are observed.</param>
    /// <param name="handler">Delegate invoked with each batch of changes.</param>
    public IDisposable Subscribe(DocumentId? documentId, Action<DocumentChangeSet> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        var subscription = new Subscription(this, documentId, handler);
        lock (_gate)
        {
            _subscriptions.Add(subscription);
        }

        return subscription;
    }

    /// <summary>
    /// Begins a transactional change scope for the specified document.
    /// </summary>
    /// <param name="documentId">The document identifier.</param>
    public IDisposable BeginTransaction(DocumentId documentId)
    {
        lock (_gate)
        {
            if (_transactions.TryGetValue(documentId, out var state))
            {
                state.Depth++;
                return new TransactionScope(this, documentId);
            }

            _transactions[documentId] = new TransactionState();
            return new TransactionScope(this, documentId);
        }
    }

    /// <summary>
    /// Flushes any pending batched changes for the specified document immediately.
    /// </summary>
    public void FlushPending(DocumentId documentId)
    {
        PendingState? pending;
        lock (_gate)
        {
            if (!_pending.TryGetValue(documentId, out pending))
            {
                return;
            }

            _pending.Remove(documentId);
        }

        var changes = pending.ConsumeChanges();
        pending.Dispose();
        Emit(documentId, pending.Version, changes);
    }

    internal void PublishChange(DocumentId documentId, DocumentVersion version, DocumentChange change)
    {
        List<DocumentChange>? immediatePublish = null;
        lock (_gate)
        {
            if (_transactions.TryGetValue(documentId, out var transaction))
            {
                transaction.Changes.Add(change);
                transaction.Version = version;
                return;
            }

            if (_throttleWindow == TimeSpan.Zero)
            {
                immediatePublish = new List<DocumentChange> { change };
            }
            else
            {
                if (!_pending.TryGetValue(documentId, out var pending))
                {
                    pending = new PendingState();
                    _pending[documentId] = pending;
                }

                pending.Changes.Add(change);
                pending.Version = version;
                pending.ResetTimer(this, documentId, _throttleWindow);
                return;
            }
        }

        if (immediatePublish is not null)
        {
            Emit(documentId, version, immediatePublish);
        }
    }

    private void CompleteTransaction(DocumentId documentId)
    {
        List<DocumentChange>? toPublish = null;
        DocumentVersion version = default;

        lock (_gate)
        {
            if (!_transactions.TryGetValue(documentId, out var state))
            {
                return;
            }

            state.Depth--;
            if (state.Depth <= 0)
            {
                _transactions.Remove(documentId);
                if (state.Changes.Count > 0)
                {
                    toPublish = state.Changes.ToList();
                    version = state.Version;
                }
            }
        }

        if (toPublish is not null)
        {
            Emit(documentId, version, toPublish);
        }
    }

    private void Emit(DocumentId documentId, DocumentVersion version, List<DocumentChange> changes)
    {
        if (changes.Count == 0)
        {
            return;
        }

        Subscription[] listeners;
        var changeSet = new DocumentChangeSet(documentId, version, changes.ToArray());

        lock (_gate)
        {
            listeners = _subscriptions.ToArray();
        }

        foreach (var subscription in listeners)
        {
            if (subscription.CanHandle(documentId))
            {
                subscription.Handler(changeSet);
            }
        }
    }

    private void OnPendingTimer(DocumentId documentId)
    {
        PendingState? pending;
        lock (_gate)
        {
            if (!_pending.TryGetValue(documentId, out pending))
            {
                return;
            }

            _pending.Remove(documentId);
        }

        var changes = pending.ConsumeChanges();
        pending.Dispose();
        Emit(documentId, pending.Version, changes);
    }

    private sealed class Subscription : IDisposable
    {
        private readonly DocumentChangeBus _bus;
        private bool _disposed;

        public Subscription(DocumentChangeBus bus, DocumentId? documentId, Action<DocumentChangeSet> handler)
        {
            _bus = bus;
            DocumentId = documentId;
            Handler = handler;
        }

        public DocumentId? DocumentId { get; }

        public Action<DocumentChangeSet> Handler { get; }

        public bool CanHandle(DocumentId documentId)
            => !_disposed && (!DocumentId.HasValue || DocumentId.Value.Equals(documentId));

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            lock (_bus._gate)
            {
                _bus._subscriptions.Remove(this);
            }
        }
    }

    private sealed class TransactionState
    {
        public int Depth { get; set; } = 1;

        public List<DocumentChange> Changes { get; } = new();

        public DocumentVersion Version { get; set; }
    }

    private sealed class TransactionScope : IDisposable
    {
        private readonly DocumentChangeBus _bus;
        private readonly DocumentId _documentId;
        private bool _disposed;

        public TransactionScope(DocumentChangeBus bus, DocumentId documentId)
        {
            _bus = bus;
            _documentId = documentId;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _bus.CompleteTransaction(_documentId);
        }
    }

    private sealed class PendingState : IDisposable
    {
        private Timer? _timer;

        public List<DocumentChange> Changes { get; } = new();

        public DocumentVersion Version { get; set; }

        public void ResetTimer(DocumentChangeBus bus, DocumentId documentId, TimeSpan dueTime)
        {
            DisposeTimer();
            _timer = new Timer(_ => bus.OnPendingTimer(documentId), null, dueTime, Timeout.InfiniteTimeSpan);
        }

        public List<DocumentChange> ConsumeChanges()
        {
            var result = new List<DocumentChange>(Changes);
            Changes.Clear();
            return result;
        }

        public void DisposeTimer()
        {
            _timer?.Dispose();
            _timer = null;
        }

        public void Dispose()
        {
            DisposeTimer();
        }
    }
}
