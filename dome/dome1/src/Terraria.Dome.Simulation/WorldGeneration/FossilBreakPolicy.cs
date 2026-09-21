namespace Terraria.Dome.Simulation.WorldGeneration;

public static class FossilBreakPolicy
{
  public const ushort FossilTileType = 404;

  public static FossilBreakDecision Evaluate(
    ushort tileType,
    bool fossilBreak,
    bool belowTileIsSolid,
    bool isTopNeighbor,
    bool fail)
  {
    bool canBegin = tileType == FossilTileType && !fossilBreak;
    int rollExclusiveUpperBound = belowTileIsSolid && (!isTopNeighbor || fail) ? 15 : 4;
    return new FossilBreakDecision(canBegin, rollExclusiveUpperBound);
  }
}
