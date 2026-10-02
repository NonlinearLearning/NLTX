namespace Terraria.EntityLifecycleAttribution;

public static class DeathAttributionQuery
{
  public static int? GetSourceProjectileType(PlayerDeathAttributionSnapshot snapshot)
  {
    return snapshot.SourceProjectileLocalIndex == -1
      ? null
      : snapshot.SourceProjectileType;
  }
}
