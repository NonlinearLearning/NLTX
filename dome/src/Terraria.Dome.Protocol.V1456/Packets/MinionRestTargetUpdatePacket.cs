namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct MinionRestTargetUpdatePacket(
  byte PlayerSlot,
  float PositionX,
  float PositionY);
