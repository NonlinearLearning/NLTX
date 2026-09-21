namespace Terraria.EntityLifecycleAttribution;

public readonly record struct EntityIdentityState(
  int RuntimeEntityId,
  int CompatibilitySlot,
  int Generation,
  int? NetworkId,
  string? PersistentId)
{
  public bool HasRuntimeIdentity => RuntimeEntityId >= 0;

  public bool HasCompatibilitySlot => CompatibilitySlot >= 0 && Generation > 0;
}
