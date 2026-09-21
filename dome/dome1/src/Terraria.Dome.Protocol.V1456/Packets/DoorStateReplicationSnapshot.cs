namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct DoorStateReplicationSnapshot(
  DoorToggleAction Action,
  short TileX,
  short TileY,
  bool Direction);
