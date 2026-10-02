namespace Terraria.Content;

public sealed record NpcCoreDefinition(int DefaultLifeMax, int DefaultDamage, int DefaultDefense)
{
  public float KnockBackResist { get; init; } = 1f;

  public float TakenDamageMultiplier { get; init; } = 1f;

  public int LifeRegenDefault { get; init; }

  public int Rarity { get; init; }

  public int Value { get; init; }
}
