namespace MinimalImplementation.Combat.Projectiles;

/// <summary>Maps only projectile dispatch styles with migrated owner-thread evidence.</summary>
public static class ProjectileAiProfileCatalog
{
  private static readonly IReadOnlyDictionary<int, ProjectileAiProfile> Profiles =
    new Dictionary<int, ProjectileAiProfile>
    {
      [1] = new(
        1,
        ProjectileAiProfileKind.Arrow,
        false,
        true,
        "Version4 Projectile.AI_001 type 1: timer, gravity at 15, downward speed cap 16; complete same-version Projectile.cs:53559-53656, 55164-55176, 55351-55363")
    };

  public static ProjectileAiProfile Resolve(int aiStyle)
  {
    return Profiles.TryGetValue(aiStyle, out ProjectileAiProfile? profile)
      ? profile
      : new ProjectileAiProfile(
        aiStyle,
        ProjectileAiProfileKind.Unsupported,
        UsesTarget: false,
        UsesTileCollision: false,
        Evidence: "No MinimalImplementation profile has been migrated for this Version4 dispatch style.");
  }

  public static bool TryGet(int aiStyle, out ProjectileAiProfile profile)
  {
    return Profiles.TryGetValue(aiStyle, out profile!);
  }
}
