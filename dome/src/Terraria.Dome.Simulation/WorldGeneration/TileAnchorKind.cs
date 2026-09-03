using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

[Flags]
public enum TileAnchorKind
{
  None = 0,
  SolidTile = 1,
  SolidBottom = 2,
  SolidWithTop = 4,
  SolidSide = 8,
  Table = 16,
  EmptyTile = 32
}
