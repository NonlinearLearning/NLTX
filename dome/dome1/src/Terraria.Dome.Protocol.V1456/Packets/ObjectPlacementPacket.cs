namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct ObjectPlacementPacket(
  short TileX,
  short TileY,
  short ObjectType,
  short Style,
  byte Alternate,
  sbyte Random,
  bool DirectionRight)
{
  public bool IsValid => ObjectType > 0 && Style >= 0;
}
