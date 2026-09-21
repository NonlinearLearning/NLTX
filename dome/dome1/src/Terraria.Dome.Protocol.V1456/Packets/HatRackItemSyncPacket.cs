namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct HatRackItemSyncPacket(
  byte PlayerId,
  int EntityId,
  byte Slot,
  bool IsDye,
  int ItemType,
  byte Prefix);
