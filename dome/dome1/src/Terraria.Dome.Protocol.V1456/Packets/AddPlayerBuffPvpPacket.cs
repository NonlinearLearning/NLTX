namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct AddPlayerBuffPvpPacket(
  byte PlayerSlot,
  ushort BuffType,
  int DurationTicks);
