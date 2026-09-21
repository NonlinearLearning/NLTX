namespace Terraria.WorldSession.Queries;

public static class WorldSurfaceQuery
{
  public static bool NoFunctionalSurface(double worldSurface)
  {
    return worldSurface <= 30d;
  }

  public static bool HasFunctionalSurface(double worldSurface)
  {
    return !NoFunctionalSurface(worldSurface);
  }
}
