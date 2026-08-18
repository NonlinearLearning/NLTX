namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct PlayerEquipmentPacket(
  byte PlayerSlot,
  int SlotId,
  int Stack,
  byte Prefix,
  int ItemType,
  bool IsFavorited,
  bool IsNewAndShiny);
