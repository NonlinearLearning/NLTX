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

    if (!metadata.HasWorldSurface)
    {
      return new(false, WorldSlimeRainStartRejection.NoWorldSurface);
    }

    return new(true, WorldSlimeRainStartRejection.None);
  }
}
