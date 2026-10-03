using Terraria.Network;

namespace NSSLC.Infrastructure.Network;

public sealed class ProtocolFacts {
  private readonly Dictionary<(byte, PacketDirection, string), object> _values = new();
  private bool _frozen;
  public string Version { get; }

  public ProtocolFacts(string? version = null) {
    Version = version ?? Guid.NewGuid().ToString("N");
    ArgumentException.ThrowIfNullOrWhiteSpace(Version);
  }

  public void Bind<T>(byte messageId, PacketDirection direction, string name, T value)
      where T : notnull {
    if (_frozen) {
      throw new InvalidOperationException("Protocol facts are frozen.");
    }
    ArgumentNullException.ThrowIfNull(value);
    _values.Add((messageId, direction, name), value);
  }

  public T Get<T>(byte messageId, PacketDirection direction, string name) {
    _frozen = true;
    if (!_values.TryGetValue((messageId, direction, name), out object? value)
        || value is not T typed) {
      throw new InvalidOperationException($"Missing protocol fact {messageId}/{direction}/{name}.");
    }
    return typed;
  }
}
