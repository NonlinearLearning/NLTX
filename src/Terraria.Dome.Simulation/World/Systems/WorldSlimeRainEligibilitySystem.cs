using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldModel.Systems;

public enum WorldSlimeRainStartRejection
{
  None,
  UnknownWorldVariant,
  RemixWorld,
  NoWorldSurface
}

public readonly record struct WorldSlimeRainEligibilityResult(
  bool IsEligible,
  WorldSlimeRainStartRejection Rejection);

public sealed class WorldSlimeRainEligibilitySystem
{
  private const double MinimumWorldSurface = 50.0;

  public WorldSlimeRainEligibilityResult Evaluate(WorldMetadata metadata)
  {
    if (metadata.IsRemixWorld is not bool isRemixWorld)
    {
      return new(false, WorldSlimeRainStartRejection.UnknownWorldVariant);
    }

    if (isRemixWorld)
    {
      return new(false, WorldSlimeRainStartRejection.RemixWorld);
    }

    if (metadata.WorldSurface is not double worldSurface ||
        !double.IsFinite(worldSurface) ||
        worldSurface <= MinimumWorldSurface)
    {
      return new(false, WorldSlimeRainStartRejection.NoWorldSurface);
    }

    return new(true, WorldSlimeRainStartRejection.None);
  }
}
