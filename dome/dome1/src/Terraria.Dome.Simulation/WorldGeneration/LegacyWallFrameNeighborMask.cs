using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

[Flags]
public enum LegacyWallFrameNeighborMask
{
  None = 0,
  Above = 1,
  Left = 2,
  Right = 4,
  Below = 8
}
