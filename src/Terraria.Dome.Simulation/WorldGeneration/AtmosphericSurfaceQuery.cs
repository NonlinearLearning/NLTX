namespace Terraria.Dome.Simulation.WorldGeneration;

public static class AtmosphericSurfaceQuery
{
  public static bool IsSurfaceForAtmospherics(
    int y,
    AtmosphericSurfaceProfile profile)
  {
    if (!profile.IsRemixWorld)
    {
      return y <= profile.WorldSurfaceY;
    }

    return y > profile.RockLayerY && y < profile.WorldHeight - 350;
  }
}
