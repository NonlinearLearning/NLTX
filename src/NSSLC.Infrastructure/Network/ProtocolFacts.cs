using Terraria.Network;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace NSSLC.Infrastructure.Network;

public sealed class ProtocolFacts {
  private readonly Dictionary<(byte, PacketDirection, string), object> _values = new();
  private bool _frozen;
  public string Version { get; }
  public ProtocolInputs? Inputs { get; }

  public ProtocolFacts(string? version = null) {
    Version = version ?? Guid.NewGuid().ToString("N");
    ArgumentException.ThrowIfNullOrWhiteSpace(Version);
  }

  public ProtocolFacts(ProtocolInputs inputs, string? version = null) : this(version) {
    ArgumentNullException.ThrowIfNull(inputs);
    if (!ReferenceEquals(inputs, ProtocolInputs.Instance)) {
      throw new ArgumentException("Protocol facts must use the initialized packet model.",
          nameof(inputs));
    }
    Inputs = inputs;
  }

  public void FreezePacketInputs() {
    if (Inputs is null || !ReferenceEquals(Inputs, ProtocolInputs.Instance)) {
      throw new InvalidOperationException("The packet protocol requires initialized ProtocolInputs.");
    }
    _frozen = true;
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
