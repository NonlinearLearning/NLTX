namespace Terraria.Dome.Simulation.WorldGeneration;

public static class ObjectDestructionGuardPolicy
{
  public static ObjectDestructionGuardDecision Evaluate(bool destroyObject)
  {
    return new ObjectDestructionGuardDecision(
      CanBegin: !destroyObject,
      SuppressNestedChecks: destroyObject);
  }
}
