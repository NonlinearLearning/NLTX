namespace Terraria.Dome.Simulation.WorldGeneration;

public static class TileMergeCullApplyQuery
{
  public static TileMergeNeighbors Apply(TileMergeCullMask mask, TileMergeNeighbors neighbors)
  {
    return new TileMergeNeighbors(
      mask.CullUp ? -1 : neighbors.Up,
      mask.CullDown ? -1 : neighbors.Down,
      mask.CullLeft ? -1 : neighbors.Left,
      mask.CullRight ? -1 : neighbors.Right,
      mask.CullUpLeft ? -1 : neighbors.UpLeft,
      mask.CullUpRight ? -1 : neighbors.UpRight,
      mask.CullDownLeft ? -1 : neighbors.DownLeft,
      mask.CullDownRight ? -1 : neighbors.DownRight);
  }
}
