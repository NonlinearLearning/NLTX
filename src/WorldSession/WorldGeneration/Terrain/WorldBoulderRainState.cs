namespace Terraria.WorldGeneration.Terrain;

public sealed class WorldBoulderRainState
{
  public bool IsRaining { get; private set; }

  public WorldBoulderRainTransition Advance(
    bool worldSurfaceAvailable,
    bool drunkWorld,
    bool goodWorld,
    bool remixWorld,
    bool storming)
  {
    if (!worldSurfaceAvailable)
    {
      return new(IsRaining, IsRaining, ShouldNotifyProgression: false);
    }

    bool wasRaining = IsRaining;
    bool isRaining = drunkWorld && goodWorld && !remixWorld && storming;
    IsRaining = isRaining;

    return new(
      wasRaining,
      isRaining,
      ShouldNotifyProgression: wasRaining && !isRaining);
  }

  public void Reset()
  {
    IsRaining = false;
  }
}
