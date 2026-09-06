namespace Terraria.WorldGeneration.Components;

public readonly record struct PersistentEntityId(ulong Value)
{
  public bool IsValid => Value != 0;
}
