using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.Modules;

/// <summary>
/// Bounded idempotency window for the same internally captured packet envelope.
/// New transport arrivals receive new sequences and are still validated normally. Cached raw
/// frames are admitted only while both the result-count and byte budgets remain available; an
/// entry also expires after the configured retention period. Recently retired epochs remain
/// tombstoned so a late frame cannot reopen a closed session's replay state; the tombstone window
/// is bounded, after which the P02 session gate remains authoritative for old epochs.
/// </summary>
public sealed class ModuleDispatchReplayCache
{
  public const int DefaultMaximumTrackedEpochs = 255;
  public const int DefaultMaximumCachedResults = 4096;
  public const int DefaultMaximumCachedBytes = 8 * 1024 * 1024;
  public const int DefaultMaximumRetiredEpochTombstones = 4096;
  public static readonly TimeSpan DefaultRetention = TimeSpan.FromMinutes(5);

  private readonly object _gate = new();
  private readonly int _maximumTrackedEpochs;
  private readonly int _maximumCachedResults;
  private readonly int _maximumCachedBytes;
  private readonly int _maximumRetiredEpochTombstones;
  private readonly TimeSpan _retention;
  private readonly Dictionary<SessionEpoch, EpochState> _epochs = new();
  private readonly HashSet<SessionEpoch> _retiredEpochs = new();
  private readonly LinkedList<SessionEpoch> _retiredEpochOrder = new();
  private readonly Dictionary<ReplayKey, CacheEntry> _results = new();
  private readonly LinkedList<ReplayKey> _resultOrder = new();
  private int _cachedBytes;

  public ModuleDispatchReplayCache(
    int maximumTrackedEpochs = DefaultMaximumTrackedEpochs,
    int maximumCachedResults = DefaultMaximumCachedResults,
    int maximumCachedBytes = DefaultMaximumCachedBytes,
    TimeSpan? retention = null,
    int maximumRetiredEpochTombstones = DefaultMaximumRetiredEpochTombstones)
  {
    if (maximumTrackedEpochs <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maximumTrackedEpochs));
    }

    if (maximumCachedResults <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maximumCachedResults));
    }

    if (maximumCachedBytes <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maximumCachedBytes));
    }

    if (maximumRetiredEpochTombstones <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maximumRetiredEpochTombstones));
    }

    TimeSpan effectiveRetention = retention ?? DefaultRetention;
    if (effectiveRetention <= TimeSpan.Zero)
    {
      throw new ArgumentOutOfRangeException(nameof(retention));
    }

    _maximumTrackedEpochs = maximumTrackedEpochs;
    _maximumCachedResults = maximumCachedResults;
    _maximumCachedBytes = maximumCachedBytes;
    _maximumRetiredEpochTombstones = maximumRetiredEpochTombstones;
    _retention = effectiveRetention;
  }

  public int TrackedEpochCount
  {
    get
    {
      lock (_gate)
      {
        return _epochs.Count;
      }
    }
  }

  public int CachedResultCount
  {
    get
    {
      lock (_gate)
      {
        return _results.Count;
      }
    }
  }

  public int CachedByteCount
  {
    get
    {
      lock (_gate)
      {
        return _cachedBytes;
      }
    }
  }

  public bool IsRetired(SessionEpoch epoch)
  {
    if (!epoch.IsValid)
    {
      return false;
    }

    lock (_gate)
    {
      return _retiredEpochs.Contains(epoch);
    }
  }

  /// <summary>
  /// Removes results older than the configured retention period using a caller-supplied captured
  /// receipt time. The cache does not create a timer or read the process clock.
  /// </summary>
  public int EvictExpiredResults(DateTimeOffset observedReceivedAt)
  {
    lock (_gate)
    {
      int count = _results.Count;
      RemoveExpired(observedReceivedAt);
      return count - _results.Count;
    }
  }

  public ModuleDispatchResult Execute(
    InboundPacketEnvelope envelope,
    Func<ModuleDispatchResult> dispatch)
  {
    ArgumentNullException.ThrowIfNull(envelope);
    ArgumentNullException.ThrowIfNull(dispatch);

    lock (_gate)
    {
      RemoveExpired(envelope.ReceivedAt);

      if (_retiredEpochs.Contains(envelope.Epoch))
      {
        return ModuleDispatchResult.Rejected(
          ProtocolErrorCode.StaleEpoch,
          "The module envelope belongs to a retired session epoch.");
      }

      if (!_epochs.TryGetValue(envelope.Epoch, out EpochState? state))
      {
        if (_epochs.Count >= _maximumTrackedEpochs)
        {
          return ModuleDispatchResult.Rejected(
            ProtocolErrorCode.RateLimited,
            "The module dispatcher has reached its active-session replay-state limit.");
        }

        state = new EpochState();
        _epochs.Add(envelope.Epoch, state);
      }

      var key = new ReplayKey(envelope.Epoch, envelope.Sequence);
      if (_results.TryGetValue(key, out CacheEntry? cached))
      {
        return envelope.RawFrame.Span.SequenceEqual(cached.RawFrame)
          ? cached.Result
          : ModuleDispatchResult.Rejected(
            ProtocolErrorCode.Conflict,
            "An envelope sequence was reused with different raw frame bytes.");
      }

      if (envelope.Sequence.Value <= state.LastSequence)
      {
        return ModuleDispatchResult.Rejected(
          ProtocolErrorCode.Conflict,
          "An old module envelope has left the replay window and cannot be applied again.");
      }

      // Mark the sequence consumed before invoking the owner. If a defect throws after a
      // partial side effect, the same envelope cannot repeat that side effect on retry.
      state.LastSequence = envelope.Sequence.Value;
      ModuleDispatchResult result = dispatch();
      Remember(
        key,
        envelope.RawFrame.ToArray(),
        envelope.ReceivedAt,
        result.ForReplay());
      return result;
    }
  }

  public bool RetireEpoch(SessionEpoch epoch)
  {
    if (!epoch.IsValid)
    {
      throw new ArgumentException("A valid session epoch is required.", nameof(epoch));
    }

    lock (_gate)
    {
      bool firstRetirement = _retiredEpochs.Add(epoch);
      if (firstRetirement)
      {
        _retiredEpochOrder.AddLast(epoch);
        while (_retiredEpochOrder.Count > _maximumRetiredEpochTombstones)
        {
          SessionEpoch expired = _retiredEpochOrder.First!.Value;
          _retiredEpochOrder.RemoveFirst();
          _retiredEpochs.Remove(expired);
        }
      }

      _epochs.Remove(epoch);
      ReplayKey[] keys = _results.Keys.Where(key => key.Epoch == epoch).ToArray();
      foreach (ReplayKey key in keys)
      {
        Remove(key);
      }

      return firstRetirement;
    }
  }

  private void Remember(
    ReplayKey key,
    byte[] rawFrame,
    DateTimeOffset receivedAt,
    ModuleDispatchResult result)
  {
    if (rawFrame.Length > _maximumCachedBytes)
    {
      return;
    }

    while (_results.Count >= _maximumCachedResults
        || _cachedBytes > _maximumCachedBytes - rawFrame.Length)
    {
      if (_resultOrder.First is not LinkedListNode<ReplayKey> oldest)
      {
        break;
      }

      Remove(oldest.Value);
    }

    LinkedListNode<ReplayKey> orderNode = _resultOrder.AddLast(key);
    _results.Add(key, new CacheEntry(rawFrame, result, orderNode, receivedAt));
    _cachedBytes += rawFrame.Length;
  }

  private void Remove(ReplayKey key)
  {
    if (_results.Remove(key, out CacheEntry? entry))
    {
      _resultOrder.Remove(entry.OrderNode);
      _cachedBytes -= entry.RawFrame.Length;
    }
  }

  private void RemoveExpired(DateTimeOffset currentReceivedAt)
  {
    ReplayKey[] expiredKeys = _results
      .Where(pair => currentReceivedAt >= pair.Value.ReceivedAt
        && currentReceivedAt - pair.Value.ReceivedAt >= _retention)
      .Select(static pair => pair.Key)
      .ToArray();
    foreach (ReplayKey key in expiredKeys)
    {
      Remove(key);
    }
  }

  private sealed class EpochState
  {
    public long LastSequence { get; set; } = -1;
  }

  private sealed record CacheEntry(
    byte[] RawFrame,
    ModuleDispatchResult Result,
    LinkedListNode<ReplayKey> OrderNode,
    DateTimeOffset ReceivedAt);

  private readonly record struct ReplayKey(SessionEpoch Epoch, PacketSequence Sequence);
}
