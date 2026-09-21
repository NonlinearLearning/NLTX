namespace Terraria.EntityLifecycleAttribution;

public readonly record struct DeathCauseProjection(
  int? SourceProjectileType,
  int SourceItemType,
  int SourceItemPrefix,
  string? CustomReason)
{
  public static DeathCauseProjection Create(PlayerDeathAttributionSnapshot snapshot)
  {
    return new DeathCauseProjection(
      DeathAttributionQuery.GetSourceProjectileType(snapshot),
      snapshot.SourceItemType,
      snapshot.SourceItemPrefix,
      snapshot.CustomReason);
  }
}
