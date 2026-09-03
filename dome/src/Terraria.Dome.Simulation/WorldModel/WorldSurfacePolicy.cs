namespace Terraria.Dome.Simulation.WorldModel;

/// <summary>Owns the legacy NoFunctionalSurface threshold query.</summary>
public static class WorldSurfacePolicy
{
  public const double NoFunctionalSurfaceThreshold = 30.0;

  public static bool NoFunctionalSurface(WorldMetadata metadata)
  {
    return metadata.HasNoFunctionalSurface;
  }
}
