using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class CoatingColorSelectionQuery
{
  public static CoatingColorSelection Evaluate(WorldTile? tile, bool block)
  {
    if (tile is not WorldTile value)
    {
      return default;
    }

    return block
      ? new CoatingColorSelection(value.IsFullbrightBlock, value.IsInvisibleBlock)
      : new CoatingColorSelection(value.IsFullbrightWall, value.IsInvisibleWall);
  }
}
