namespace EntityEcs.Components;

public enum TerminationReason : byte
{
  None,
  LifetimeExpired,
  ExplicitlyRemoved,
  Collision,
  ParentDestroyed,
  WorldUnload,
  DefinitionInvalid,
  CompatibilityRemoval,
}
