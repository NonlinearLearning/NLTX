using System.Collections.Concurrent;
using Terraria.Network;

namespace NSSLC.Infrastructure.Network;

/// <summary>Records bounded projectile uploads for the headless host without simulating them.</summary>
public sealed class SteamProjectilePacketHandler : IPacketHandler<SteamProjectileSyncPacket>,
    IPacketHandler<SteamProjectileKillPacket> {
  public sealed record Observation(long SyncCount, long KillCount, uint LastKey,
      int? LastProjectileType);

  private readonly ConcurrentDictionary<byte, Observation> _observed = new();

  public IReadOnlyDictionary<byte, Observation> Snapshot() {
    return new Dictionary<byte, Observation>(_observed);
  }

  public ValueTask<PacketHandlingResult> HandleAsync(NetworkSessionContext context,
      SteamProjectileSyncPacket packet, CancellationToken cancellationToken) {
    cancellationToken.ThrowIfCancellationRequested();
    byte sender = context.Actor.PlayerSlot;
    if ((byte)packet.Key != sender || packet.State.OwnerSlot != sender) {
      return ValueTask.FromResult(new PacketHandlingResult(false,
          rejectionCode: "ProjectileSenderMismatch"));
    }
    _observed.AddOrUpdate(sender, new Observation(1, 0, packet.Key, packet.State.ProjectileType),
        (_, previous) => previous with { SyncCount = previous.SyncCount + 1,
          LastKey = packet.Key, LastProjectileType = packet.State.ProjectileType });
    return ValueTask.FromResult(new PacketHandlingResult(true));
  }

  public ValueTask<PacketHandlingResult> HandleAsync(NetworkSessionContext context,
      SteamProjectileKillPacket packet, CancellationToken cancellationToken) {
    cancellationToken.ThrowIfCancellationRequested();
    byte sender = context.Actor.PlayerSlot;
    if ((byte)packet.Key != sender) {
      return ValueTask.FromResult(new PacketHandlingResult(false,
          rejectionCode: "ProjectileSenderMismatch"));
    }
    _observed.AddOrUpdate(sender, new Observation(0, 1, packet.Key, null),
        (_, previous) => previous with { KillCount = previous.KillCount + 1,
          LastKey = packet.Key });
    return ValueTask.FromResult(new PacketHandlingResult(true));
  }
}
