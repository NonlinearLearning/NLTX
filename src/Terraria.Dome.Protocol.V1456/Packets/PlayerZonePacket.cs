namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct PlayerZonePacket(
  byte PlayerSlot,
  byte Zone1,
  byte Zone2,
  byte Zone3,
  byte Zone4,
  byte Zone5,
  byte TownNpcs);
