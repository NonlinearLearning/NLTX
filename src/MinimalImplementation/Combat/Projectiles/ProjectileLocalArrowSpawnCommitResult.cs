using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Projectiles;

public sealed record ProjectileLocalArrowSpawnCommitResult(
    OwnerCommitResult Result,
    ProjectileView? Projectile,
    ProjectilePacketBody? Broadcast,
    ProjectileChannelTrackingIntent? ChannelTracking);
