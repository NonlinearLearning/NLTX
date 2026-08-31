namespace Terraria.Dome.Simulation.WorldGeneration;

public static class MeteorShowerAdvancePolicy
{
  public static MeteorShowerProgression Advance(
    MeteorShowerProgression current,
    bool reset,
    bool fastForward,
    bool impactCommitted)
  {
    if (reset || fastForward)
    {
      return new MeteorShowerProgression(0);
    }

    if (!impactCommitted || current.RemainingCount == 0)
    {
      return current;
    }

    return new MeteorShowerProgression(current.RemainingCount - 1);
  }
}
