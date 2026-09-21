namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct ShopOverridePacket(
  byte Slot,
  short Type,
  short Stack,
  byte Prefix,
  int Value,
  byte Flags);
