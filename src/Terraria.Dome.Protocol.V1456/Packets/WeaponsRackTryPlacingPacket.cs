namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct WeaponsRackTryPlacingPacket(
  short X,
  short Y,
  short ItemType,
  byte Prefix,
  short Stack);
