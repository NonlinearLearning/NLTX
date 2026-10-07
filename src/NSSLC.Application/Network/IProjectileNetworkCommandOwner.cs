using Terraria.Projectile;

namespace Terraria.Network;

/// <summary>
/// Applies authenticated projectile network commands through the active game owner.
/// </summary>
public interface IProjectileNetworkCommandOwner
{
  /// <summary>
  /// Resolves the sender's active world, rejects hostile types other than the
  /// Version4 type-949 exception, applies state through the lifecycle owner, and
  /// returns authoritative relay dispatches. A Version4 silent no-op should be
  /// represented as accepted with no dispatch because gateway rejection closes
  /// the connection.
  /// </summary>
  ValueTask<PacketHandlingResult> ApplyProjectileAsync(
    NetworkSessionContext sender,
    ProjectileNetworkApplyCommand command,
    CancellationToken cancellationToken);

  /// <summary>
  /// Applies sender-scoped packet-29 termination through the lifecycle owner and
  /// returns any authoritative relay dispatches. A missing or stale projectile
  /// may be represented as accepted with no dispatch to preserve the source no-op.
  /// </summary>
  ValueTask<PacketHandlingResult> TerminateProjectileAsync(
    NetworkSessionContext sender,
    ProjectileNetworkTerminateCommand command,
    CancellationToken cancellationToken);
}
