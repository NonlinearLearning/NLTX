namespace Terraria.Content;

public sealed record ProjectilePenetrationDefinition(
  int DefaultPenetrate,
  int MaxPenetrate,
  bool StopsDealingDamageAfterPenetrateHits)
{
  public bool UsesLocalNpcImmunity { get; init; }

  public bool UsesIdStaticNpcImmunity { get; init; }

  public bool AppliesImmunityTimeOnSingleHits { get; init; }

  public int LocalNpcHitCooldown { get; init; } = -2;

  public int IdStaticNpcHitCooldown { get; init; } = -1;
}
