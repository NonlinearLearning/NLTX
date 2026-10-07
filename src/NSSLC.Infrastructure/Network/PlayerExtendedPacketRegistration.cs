using Terraria.Network;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace NSSLC.Infrastructure.Network;

public static class PlayerExtendedPacketRegistration
{
  public static void Register(PacketGateway gateway, PlayerExtendedPacketHandlers handlers)
  {
    ArgumentNullException.ThrowIfNull(gateway);
    ArgumentNullException.ThrowIfNull(handlers);

    gateway.Register<ManaEffectPacket>(Active(43), handlers);
    gateway.Register<MiscDataSyncPacket>(Active(51), handlers);
    gateway.Register<RequestTeleportationByServerPacket>(Active(73), handlers);
    gateway.Register<TeleportPlayerThroughPortalPacket>(Active(96), handlers);
    gateway.Register<NebulaLevelupRequestPacket>(Active(102), handlers);
    gateway.Register<DeadPlayerPacket>(Active(135), handlers);
    gateway.Register<SyncTilePickingPacket>(Active(125), handlers);
    gateway.Register<SyncLoadoutPacket>(
      new PacketPolicy(147, NetworkSessionStage.AwaitPlayerData | NetworkSessionStage.Active,
        MaximumPerWindow: 120, MaximumBytesPerWindow: 1024), handlers);
    gateway.Register<SpectatePlayerPacket>(Active(150), handlers);
    gateway.Register<TeamChangeFromUIPacket>(Active(157), handlers);
  }

  private static PacketPolicy Active(byte messageId)
  {
    return new PacketPolicy(messageId, NetworkSessionStage.Active,
      MaximumPerWindow: 120, MaximumBytesPerWindow: 1024);
  }
}
