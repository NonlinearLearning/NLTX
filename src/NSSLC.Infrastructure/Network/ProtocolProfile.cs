using Terraria.Network;

namespace NSSLC.Infrastructure.Network;

public sealed class ProtocolProfile {
  private readonly Dictionary<(PacketDirection, byte), PacketBinding> _read = new();
  private readonly Dictionary<(PacketDirection, Type), PacketBinding> _write = new();

  public string Key { get; }
  public string HelloVersion { get; }
  public IReadOnlyCollection<PacketBinding> Bindings => _read.Values;

  public ProtocolProfile(string key, string helloVersion, IEnumerable<PacketBinding> bindings) {
    ArgumentException.ThrowIfNullOrWhiteSpace(key);
    ArgumentException.ThrowIfNullOrWhiteSpace(helloVersion);
    Key = key;
    HelloVersion = helloVersion;
    foreach (PacketBinding binding in bindings) {
      if (binding.Direction is not (PacketDirection.ClientToServer or PacketDirection.ServerToClient)) {
        throw new ArgumentException("A binding must specify one actual direction.");
      }
      _read.Add((binding.Direction, binding.MessageId), binding);
      _write.Add((binding.Direction, binding.PacketType), binding);
    }
  }

  public PacketBinding Find(PacketDirection direction, byte messageId) {
    if (!_read.TryGetValue((direction, messageId), out PacketBinding? binding)) {
      throw new PacketProtocolException("UnknownFormat", messageId);
    }
    return binding;
  }

  public PacketBinding Find(PacketDirection direction, Type packetType) {
    if (!_write.TryGetValue((direction, packetType), out PacketBinding? binding)) {
      throw new PacketEncodingException("No format is registered for this type and direction.");
    }
    return binding;
  }
}
