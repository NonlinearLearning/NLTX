using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Projectiles;

/// <summary>One Arrow substep's lifetime completion and optional packet-29 retirement.</summary>
public sealed record ProjectileTickCompletionResult(
    OwnerCommitResult Result,
    ProjectileAiTickResult Tick,
    ProjectileTerminationCommitResult? Retirement,
    ProjectilePacketBody? Broadcast)
{
  public bool IsExpired => Tick.Expired;

  public bool IsRetired => Retirement?.RetiredProjectile is not null;
}
