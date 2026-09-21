namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct DoorToggleIntent(
  DoorToggleAction Action,
  short TileX,
  short TileY,
  bool Direction);
