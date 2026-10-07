using Terraria.Network;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace NSSLC.Infrastructure.Network;

/// <summary>Registers bounded compatibility ingress for reserved protocol messages.</summary>
public static class ReservedPacketRegistration {
  public static bool IsProductionDirectionEnabled(byte messageId, PacketDirection direction) {
    if (direction is not (PacketDirection.ClientToServer or PacketDirection.ServerToClient)) {
      throw new ArgumentOutOfRangeException(nameof(direction));
    }
    if (messageId == 0) {
      return false;
    }
    if (messageId is 15 or 25 or 26 or 44 or 67 or 83 or 138) {
      return direction == PacketDirection.ClientToServer;
    }
    return true;
  }

  public static void RegisterClientSyncedInventory(PacketGateway gateway) {
    ArgumentNullException.ThrowIfNull(gateway);

    gateway.Register<ClientSyncedInventoryPacket>(
        new PacketPolicy(138, NetworkSessionStage.AwaitPlayerData | NetworkSessionStage.Active,
            MaximumPerWindow: 120, MaximumBytesPerWindow: 1),
        new ClientSyncedInventoryPacketHandler());
  }

  public static void RegisterKnownNoEffectPackets(PacketGateway gateway) {
    ArgumentNullException.ThrowIfNull(gateway);

    var handlers = new ReservedNoEffectPacketHandlers();
    gateway.Register<Unknown15Packet>(new PacketPolicy(15, NetworkSessionStage.Active,
        MaximumPerWindow: 120, MaximumBytesPerWindow: 1), handlers);
    gateway.Register<Unused25Packet>(new PacketPolicy(25, NetworkSessionStage.Active,
        MaximumPerWindow: 120, MaximumBytesPerWindow: 1), handlers);
    gateway.Register<Unused26Packet>(new PacketPolicy(26, NetworkSessionStage.Active,
        MaximumPerWindow: 120, MaximumBytesPerWindow: 1), handlers);
    gateway.Register<Unknown44Packet>(new PacketPolicy(44, NetworkSessionStage.Active,
        MaximumPerWindow: 120, MaximumBytesPerWindow: 1), handlers);
    gateway.Register<Unknown67Packet>(new PacketPolicy(67, NetworkSessionStage.Active,
        MaximumPerWindow: 120, MaximumBytesPerWindow: 1), handlers);
    gateway.Register<Unused83Packet>(new PacketPolicy(83, NetworkSessionStage.Active,
        MaximumPerWindow: 120, MaximumBytesPerWindow: 1), handlers);
  }
}
