namespace Terraria.WorldSession.Queries;

public static class WorldGeometryQuery
{
  public static int UnderworldLayer(int maxTilesY)
  {
    if (maxTilesY < 200)
    {
      throw new ArgumentOutOfRangeException(nameof(maxTilesY));
    }

    return maxTilesY - 200;
  }
}
