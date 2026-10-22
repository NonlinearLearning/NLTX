namespace MinimalImplementation.Combat.Projectiles;

/// <summary>Detached liquid state; Projectile owns the only committed instance value.</summary>
public sealed record ProjectileLiquidState(
    bool Wet, bool LavaWet, bool HoneyWet, bool ShimmerWet, byte WetCount) {
  // Fresh packet-27 instances use defaults, without NewProjectile's spawn-time query.
  public static ProjectileLiquidState Dry { get; } = new(false, false, false, false, 0);

  internal ProjectileLiquidState AdvanceArrow(ProjectileLiquidFacts facts) {
    byte wetCount = WetCount;
    if (!facts.WetCollision && Wet && wetCount == 0) {
      wetCount = 10;
    }
    if (wetCount > 0) {
      wetCount--;
    }
    return new ProjectileLiquidState(
        facts.WetCollision,
        facts.WetCollision && (LavaWet || facts.LavaCollision),
        facts.WetCollision && (HoneyWet || facts.Honey),
        facts.WetCollision && (ShimmerWet || facts.Shimmer),
        wetCount);
  }
}
