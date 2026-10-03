using Terraria.Network;

namespace NSSLC.Infrastructure.Network;

public sealed class PacketSnapshotCache : IDisposable {
  private sealed record Entry(PacketSnapshotCacheKey Key, byte[] Frame, long Created);

  private readonly object _gate = new();
  private readonly ProtocolProfile _profile;
  private readonly PacketByteBudget _budget;
  private readonly TimeProvider _time;
  private readonly Dictionary<PacketSnapshotCacheKey, LinkedListNode<Entry>> _entries = new();
  private readonly LinkedList<Entry> _lru = new();
  private readonly int _maximumBytes;
  private readonly int _maximumItems;
  private readonly TimeSpan _ttl;
  private int _used;
  private bool _disposed;

  public string ProfileKey => _profile.Key;
  public int UsedBytes { get { lock (_gate) { return _used; } } }
  public int Count { get { lock (_gate) { return _entries.Count; } } }

  public PacketSnapshotCache(ProtocolProfile profile, PacketByteBudget budget,
      int maximumBytes = 16 * 1024 * 1024, int maximumItems = 256,
      TimeSpan? ttl = null, TimeProvider? timeProvider = null) {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumBytes);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumItems);
    _profile = profile;
    _budget = budget;
    _maximumBytes = maximumBytes;
    _maximumItems = maximumItems;
    _ttl = ttl ?? TimeSpan.FromSeconds(2);
    if (_ttl <= TimeSpan.Zero) {
      throw new ArgumentOutOfRangeException(nameof(ttl));
    }
    _time = timeProvider ?? TimeProvider.System;
  }

  public bool TryGet(PacketSnapshotCacheKey key, out ReadOnlyMemory<byte> frame) {
    ValidateKey(key);
    lock (_gate) {
      ObjectDisposedException.ThrowIf(_disposed, this);
      SweepExpiredCore();
      if (!_entries.TryGetValue(key, out LinkedListNode<Entry>? node)) {
        frame = default;
        return false;
      }
      _lru.Remove(node);
      _lru.AddLast(node);
      frame = (byte[])node.Value.Frame.Clone();
      return true;
    }
  }

  public ValueTask<bool> TryStoreAsync<TPacket>(PacketSnapshotCacheKey key, TPacket packet,
      CancellationToken cancellationToken = default) {
    ArgumentNullException.ThrowIfNull(packet);
    if (packet.GetType() != typeof(TPacket)) {
      throw new PacketEncodingException("Use the snapshot's exact registered packet type.");
    }
    return TryStoreObjectAsync(key, packet, cancellationToken);
  }

  internal async ValueTask<bool> TryStoreObjectAsync(PacketSnapshotCacheKey key, object packet,
      CancellationToken cancellationToken) {
    ValidateKey(key);
    PacketBinding binding = _profile.Find(PacketDirection.ServerToClient, packet.GetType());
    if (binding.MessageId != 10) {
      throw new PacketEncodingException("Only explicitly shared section snapshots may be cached.");
    }
    await _budget.LargeCodecGate.WaitAsync(cancellationToken).ConfigureAwait(false);
    int reservation = 0;
    try {
      lock (_gate) {
        ObjectDisposedException.ThrowIf(_disposed, this);
        SweepExpiredCore();
        while (!_budget.TryReserve(ushort.MaxValue)) {
          if (_lru.First is null) {
            return false;
          }
          Remove(_lru.First);
        }
        reservation = ushort.MaxValue;
      }
      cancellationToken.ThrowIfCancellationRequested();
      byte[] frame = binding.Encode(packet);
      lock (_gate) {
        ObjectDisposedException.ThrowIf(_disposed, this);
        cancellationToken.ThrowIfCancellationRequested();
        if (frame.Length > _maximumBytes) {
          return false;
        }
        LinkedListNode<Entry>[] sameSection = _entries.Values.Where(node =>
            node.Value.Key.WorldKey == key.WorldKey && node.Value.Key.Section == key.Section
            && node.Value.Key.Variant == key.Variant).ToArray();
        if (sameSection.Any(node => node.Value.Key.WorldGeneration > key.WorldGeneration
            || (node.Value.Key.WorldGeneration == key.WorldGeneration
                && node.Value.Key.SnapshotRevision > key.SnapshotRevision))) {
          return false;
        }
        foreach (LinkedListNode<Entry> node in sameSection) {
          Remove(node);
        }
        while (_used > _maximumBytes - frame.Length || _entries.Count >= _maximumItems) {
          Remove(_lru.First!);
        }
        var stored = new Entry(key, frame, _time.GetTimestamp());
        _entries.Add(key, _lru.AddLast(stored));
        _used += frame.Length;
        _budget.Release(reservation - frame.Length);
        reservation = 0;
        return true;
      }
    } finally {
      if (reservation != 0) {
        _budget.Release(reservation);
      }
      _budget.LargeCodecGate.Release();
    }
  }

  public void InvalidateWorld(Guid worldKey) {
    lock (_gate) {
      ObjectDisposedException.ThrowIf(_disposed, this);
      foreach (LinkedListNode<Entry> node in _entries.Values
          .Where(item => item.Value.Key.WorldKey == worldKey).ToArray()) {
        Remove(node);
      }
    }
  }

  public void SweepExpired() {
    lock (_gate) {
      ObjectDisposedException.ThrowIf(_disposed, this);
      SweepExpiredCore();
    }
  }

  public void Dispose() {
    lock (_gate) {
      if (_disposed) {
        return;
      }
      _disposed = true;
      while (_lru.First is not null) {
        Remove(_lru.First);
      }
    }
  }

  private void SweepExpiredCore() {
    foreach (LinkedListNode<Entry> node in _entries.Values
        .Where(item => _time.GetElapsedTime(item.Value.Created) >= _ttl).ToArray()) {
      Remove(node);
    }
  }

  private void Remove(LinkedListNode<Entry> node) {
    _entries.Remove(node.Value.Key);
    _lru.Remove(node);
    _used -= node.Value.Frame.Length;
    _budget.Release(node.Value.Frame.Length);
  }

  private void ValidateKey(PacketSnapshotCacheKey key) {
    if (key.ProfileKey != _profile.Key || key.WorldKey == Guid.Empty
        || key.WorldGeneration < 0 || key.SnapshotRevision < 0
        || string.IsNullOrWhiteSpace(key.Variant)) {
      throw new ArgumentException("Snapshot cache key must identify the current shared world format.");
    }
  }
}
