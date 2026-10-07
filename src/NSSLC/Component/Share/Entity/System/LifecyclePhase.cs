namespace EntityEcs.Components;

public enum LifecyclePhase : byte
{
  Uninitialized,
  Admitted,
  Active,
  Ending,
  Retired,
}
