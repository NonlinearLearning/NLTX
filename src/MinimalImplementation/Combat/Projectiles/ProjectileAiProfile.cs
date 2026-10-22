namespace MinimalImplementation.Combat.Projectiles;

/// <summary>Typed behavior families selected by the Version4 projectile aiStyle dispatch.</summary>
public enum ProjectileAiProfileKind
{
  Unsupported,
  Straight,
  Arrow,
  Boomerang,
  Gravity,
  Homing,
  Whip
}

public sealed record ProjectileAiProfile(
  int AiStyle,
  ProjectileAiProfileKind Kind,
  bool UsesTarget,
  bool UsesTileCollision,
  string Evidence)
{
  public bool IsSupported => Kind != ProjectileAiProfileKind.Unsupported;
}
