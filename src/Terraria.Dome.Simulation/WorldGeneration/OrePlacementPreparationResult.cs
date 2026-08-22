using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct OrePlacementPreparationResult(
  ushort TileType,
  IReadOnlyList<(int X, int Y)> Cells)
{
  public bool IsPrepared => Cells is { Count: > 0 };
}
