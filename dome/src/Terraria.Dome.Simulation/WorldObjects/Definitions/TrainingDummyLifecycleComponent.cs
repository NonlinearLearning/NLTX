namespace Terraria.Dome.Simulation.WorldObjects.Definitions;

public readonly record struct TrainingDummyLifecycleComponent(bool IsActive, bool IsValid)
{
  public static TrainingDummyLifecycleComponent Active => new(true, true);

  public static TrainingDummyLifecycleComponent Invalid => new(false, false);

  public TrainingDummyLifecycleComponent Deactivate()
  {
    return this with { IsActive = false };
  }

  public TrainingDummyLifecycleComponent Validate(bool tileValid, bool npcLinkValid)
  {
    return this with { IsValid = tileValid, IsActive = IsActive && tileValid && npcLinkValid };
  }
}
