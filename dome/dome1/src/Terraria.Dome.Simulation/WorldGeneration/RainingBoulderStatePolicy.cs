namespace Terraria.Dome.Simulation.WorldGeneration;

public static class RainingBoulderStatePolicy
{
  public static RainingBoulderStateTransition Evaluate(
    bool previousIsRainingBoulders,
    bool drunkWorld,
    bool getGoodWorld,
    bool remixWorld,
    bool isStorming)
  {
    bool isRainingBoulders = drunkWorld && getGoodWorld && !remixWorld && isStorming;
    return new RainingBoulderStateTransition(
      isRainingBoulders,
      previousIsRainingBoulders && !isRainingBoulders);
  }
}
