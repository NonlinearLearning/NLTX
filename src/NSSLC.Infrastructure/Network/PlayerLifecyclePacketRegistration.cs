using Terraria.Network;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace NSSLC.Infrastructure.Network;

/// <summary>Registers the formal player identity, lifecycle and control handlers.</summary>
public static class PlayerLifecyclePacketRegistration
{
  public static void Register(
    PacketGateway gateway,
    PlayerLifecyclePacketHandlers handlers)
  {
    ArgumentNullException.ThrowIfNull(gateway);
    ArgumentNullException.ThrowIfNull(handlers);

    NetworkSessionStage uploadStages =
      NetworkSessionStage.AwaitPlayerData | NetworkSessionStage.Active;
    gateway.Register<SyncPlayerPacket>(
      new PacketPolicy(4, uploadStages, MaximumPerWindow: 4, MaximumBytesPerWindow: 1024),
      handlers);
    gateway.Register<PlayerSpawnPacket>(
      new PacketPolicy(12, NetworkSessionStage.Synchronizing,
        MaximumPerWindow: 2, MaximumBytesPerWindow: 32), handlers);
    gateway.Register<PlayerControlsPacket>(
      new PacketPolicy(13, NetworkSessionStage.Active,
        MaximumPerWindow: 120, MaximumBytesPerWindow: 4096), handlers);
    gateway.Register<PlayerActivePacket>(
      new PacketPolicy(14, NetworkSessionStage.Active,
        MaximumPerWindow: 4, MaximumBytesPerWindow: 8), handlers);
    gateway.Register<PlayerLifeManaPacket>(
      new PacketPolicy(16, uploadStages, MaximumPerWindow: 4, MaximumBytesPerWindow: 32),
      handlers);
    gateway.Register<Unknown42Packet>(
      new PacketPolicy(42, uploadStages, MaximumPerWindow: 4, MaximumBytesPerWindow: 32),
      handlers);
    gateway.Register<PlayerBuffsPacket>(
      new PacketPolicy(50, uploadStages, MaximumPerWindow: 120, MaximumBytesPerWindow: 12 * 1024),
      handlers);
  }
}
