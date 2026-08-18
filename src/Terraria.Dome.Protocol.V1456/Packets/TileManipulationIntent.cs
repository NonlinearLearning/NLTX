namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct TileManipulationIntent(
  TileManipulationAction Action,
  short X,
  short Y,
  short TileType,
  byte Style);
