namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct SignUpdateIntent(
  byte PlayerSlot,
  int SignId,
  short TileX,
  short TileY,
  string Text,
  bool SuppressOpenSign = false);
