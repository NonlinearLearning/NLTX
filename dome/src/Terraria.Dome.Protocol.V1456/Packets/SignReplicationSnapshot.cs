namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct SignReplicationSnapshot(
  int SignId,
  short TileX,
  short TileY,
  string Text,
  byte PlayerSlot,
  bool SuppressOpenSign,
  long Revision);
