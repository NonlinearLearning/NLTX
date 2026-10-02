namespace Terraria.EntityLifecycleAttribution;

public readonly record struct EntityReference(int RuntimeEntityId, int Generation)
{
  public bool IsValid => RuntimeEntityId >= 0 && Generation > 0;
}
