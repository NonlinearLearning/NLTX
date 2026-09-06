namespace Terraria.Content;

public sealed record ProjectileCombatDefinition(
  int Damage,
  float KnockBack,
  bool Friendly,
  bool Hostile)
{
  public bool Melee { get; init; }

  public bool Ranged { get; init; }

  public bool Magic { get; init; }

  public bool ColdDamage { get; init; }

  public bool Trap { get; init; }

  public bool NpcProjectile { get; init; }
}
