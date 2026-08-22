namespace Terraria.Dome.Simulation.WorldGeneration;

public static class OrdinaryTreeGroundQuery
{
  public static bool IsSuitable(ushort tileType)
  {
    return tileType is
      2 or
      23 or
      60 or
      70 or
      109 or
      147 or
      199 or
      477 or
      492 or
      633 or
      661 or
      662;
  }
}
