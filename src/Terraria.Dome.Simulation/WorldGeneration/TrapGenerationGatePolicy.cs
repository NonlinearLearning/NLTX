namespace Terraria.Dome.Simulation.WorldGeneration;

public static class TrapGenerationGatePolicy
{
  public static TrapGenerationGate Evaluate(
    bool denySomeGeneration,
    bool actuallyNoTrapsForReal,
    bool notTheBees,
    bool noTrapsWorldGen,
    bool remixWorldGen)
  {
    bool shouldRun = !denySomeGeneration && !actuallyNoTrapsForReal &&
      (!notTheBees || noTrapsWorldGen || remixWorldGen);
    return new TrapGenerationGate(shouldRun, shouldRun);
  }
}
