using Terraria.Network;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace NSSLC.Infrastructure.Network;

/// <summary>Registers the formal Player state handlers independently of host composition.</summary>
public static class PlayerStatePacketRegistration
{
  public static void Register(PacketGateway gateway, PlayerStatePacketHandlers handlers)
  {
    ArgumentNullException.ThrowIfNull(gateway);
    ArgumentNullException.ThrowIfNull(handlers);

    gateway.Register<TogglePVPPacket>(Active(30), handlers);
    gateway.Register<PlayerHealPacket>(Active(35), handlers);
    gateway.Register<SyncPlayerZonePacket>(Active(36), handlers);
    gateway.Register<SyncTalkNPCPacket>(Active(40), handlers);
    gateway.Register<ItemRotationAndAnimationPacket>(Active(41), handlers);
    gateway.Register<TeamChangePacket>(Active(45), handlers);
    gateway.Register<AddPlayerBuffPvPPacket>(Active(55), handlers);
    gateway.Register<Unknown66Packet>(Active(66), handlers);
    gateway.Register<QuestsCountSyncPacket>(Active(76), handlers);
    gateway.Register<PlayerStealthPacket>(Active(84), handlers);
    gateway.Register<MinionRestTargetUpdatePacket>(Active(99), handlers);
    gateway.Register<MinionAttackTargetUpdatePacket>(Active(115), handlers);
    gateway.Register<UpdatePlayerLuckFactorsPacket>(Active(134), handlers);
  }

  private static PacketPolicy Active(byte messageId)
  {
    return new PacketPolicy(messageId, NetworkSessionStage.Active,
      MaximumPerWindow: 120, MaximumBytesPerWindow: 1024);
  }
}
