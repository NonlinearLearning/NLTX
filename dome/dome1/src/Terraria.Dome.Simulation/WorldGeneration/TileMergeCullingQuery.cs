using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class TileMergeCullingQuery
{
  public static TileMergeCullMask Evaluate(
    WorldTile center,
    WorldTile? up,
    WorldTile? down,
    WorldTile? left,
    WorldTile? right,
    WorldTile? upLeft,
    WorldTile? upRight,
    WorldTile? downLeft,
    WorldTile? downRight,
    bool showInvisibleBlocks)
  {
    if (showInvisibleBlocks)
    {
      return default;
    }

    bool centerIsInvisible = center.IsInvisibleBlock;
    return new TileMergeCullMask(
      IsCulled(up, centerIsInvisible),
      IsCulled(down, centerIsInvisible),
      IsCulled(left, centerIsInvisible),
      IsCulled(right, centerIsInvisible),
      IsCulled(upLeft, centerIsInvisible),
      IsCulled(upRight, centerIsInvisible),
      IsCulled(downLeft, centerIsInvisible),
      IsCulled(downRight, centerIsInvisible));
  }

  private static bool IsCulled(WorldTile? neighbor, bool centerIsInvisible)
  {
    return neighbor is { IsInvisibleBlock: var isInvisible } &&
      isInvisible != centerIsInvisible;
  }
}
