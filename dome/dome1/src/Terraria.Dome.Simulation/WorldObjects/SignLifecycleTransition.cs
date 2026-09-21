namespace Terraria.Dome.Simulation.WorldObjects;

public readonly record struct SignLifecycleTransition(
  SignLifecycleComponent Previous,
  SignLifecycleComponent Current,
  long Revision)
{
  public static SignLifecycleTransition Delete(long revision)
  {
    return new(SignLifecycleComponent.Active, SignLifecycleComponent.Deleted, revision);
  }
}
