using System.Collections.Concurrent;
using Terraria.Network;

namespace NSSLC.Infrastructure.Network;

/// <summary>Stores client UUIDs for currently bound connection epochs.</summary>
public sealed class SessionClientUuidRegistry {
  private readonly ConcurrentDictionary<ConnectionIdentity, string> _values = new();

  public IReadOnlyDictionary<ConnectionIdentity, string> Snapshot() {
    return new Dictionary<ConnectionIdentity, string>(_values);
  }

  public bool TryGet(ConnectionIdentity connection, out string? clientUuid) {
    return _values.TryGetValue(connection, out clientUuid);
  }

  public bool TryRecord(ConnectionIdentity connection, string clientUuid) {
    return _values.TryAdd(connection, clientUuid);
  }

  public bool Remove(ConnectionIdentity connection) {
    return _values.TryRemove(connection, out _);
  }
}
