using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Projectiles;

public sealed record ProjectileTerminationCommitResult(
  OwnerCommitResult Result,
  ProjectileView? RetiredProjectile) {
  /// <summary>Project once after successful retirement; G2 owns recipient delivery.</summary>
  public ProjectilePacketBody? CreateKillBody() {
    if (Result.Status != OwnerCommitStatus.Applied || RetiredProjectile is null) {
      return null;
    }
    return new ProjectilePacketBody(29, ignoreEpoch: null,
        ProjectilePacketPayloadCodec.EncodeKillBody(RetiredProjectile));
  }
}
