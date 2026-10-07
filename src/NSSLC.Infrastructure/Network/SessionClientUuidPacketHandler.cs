using System.Text;
using Terraria.Network;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace NSSLC.Infrastructure.Network;

/// <summary>
/// Validates packet 68 without turning the client UUID into an authorization input.
/// Vanilla reads and discards this value; the registry preserves it for diagnostics and
/// reconnect correlation only.
/// </summary>
public sealed class SessionClientUuidPacketHandler : IPacketHandler<Unknown68Packet> {
  private const int MaximumClientUuidBytes = 128;
  private readonly SessionClientUuidRegistry _registry;

  public SessionClientUuidPacketHandler(SessionClientUuidRegistry registry) {
    _registry = registry ?? throw new ArgumentNullException(nameof(registry));
  }

  public SessionClientUuidRegistry Registry => _registry;

  public ValueTask<PacketHandlingResult> HandleAsync(NetworkSessionContext context,
      Unknown68Packet packet, CancellationToken cancellationToken) {
    cancellationToken.ThrowIfCancellationRequested();
    if (string.IsNullOrWhiteSpace(packet.ClientUuid)) {
      return Rejected("ClientUuidRequired");
    }
    if (packet.ClientUuid.IndexOf('\0') >= 0) {
      return Rejected("ClientUuidContainsNull");
    }
    if (Encoding.UTF8.GetByteCount(packet.ClientUuid) > MaximumClientUuidBytes) {
      return Rejected("ClientUuidTooLong");
    }
    if (!_registry.TryRecord(context.Connection, packet.ClientUuid)) {
      return Rejected("DuplicateClientUuid");
    }
    return ValueTask.FromResult(new PacketHandlingResult(true));
  }

  private static ValueTask<PacketHandlingResult> Rejected(string code) {
    return ValueTask.FromResult(new PacketHandlingResult(false, rejectionCode: code));
  }
}
