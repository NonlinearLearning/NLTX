using System;
using Terraria.Dome.Simulation;

namespace Terraria.Dome.Protocol.V1456.Packets;

public static class ProjectileStateProjection
{
  public static ProjectileSyncPacket Project(ProjectileReplicationSnapshot snapshot)
  {
    int identity = snapshot.Identity == 0 ? snapshot.ReplicationId : snapshot.Identity;
    if (identity < short.MinValue || identity > short.MaxValue ||
        snapshot.Owner.Value < byte.MinValue || snapshot.Owner.Value > byte.MaxValue ||
        snapshot.ProjectileType < short.MinValue || snapshot.ProjectileType > short.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(snapshot));
    }

    float knockback = snapshot.DefinitionKnockback != 0.0f
      ? snapshot.DefinitionKnockback
      : snapshot.Knockback;
    int originalDamage = snapshot.DefinitionOriginalDamage != 0
      ? snapshot.DefinitionOriginalDamage
      : snapshot.OriginalDamage;

    return new ProjectileSyncPacket(
      (short)identity,
      snapshot.Position,
      snapshot.Velocity,
      (byte)snapshot.Owner.Value,
      (short)snapshot.ProjectileType,
      snapshot.Ai0 == 0.0f ? null : snapshot.Ai0,
      snapshot.Ai1 == 0.0f ? null : snapshot.Ai1,
      snapshot.Ai2 == 0.0f ? null : snapshot.Ai2,
      snapshot.Banner == 0 ? null : snapshot.Banner,
      checked((short)snapshot.Damage),
      knockback == 0.0f ? null : knockback,
      originalDamage == 0 ? null : checked((short)originalDamage),
      snapshot.Uuid == 0 ? null : snapshot.Uuid);
  }

  public static ProjectileSyncPacket Project(NpcProjectileReplicationSnapshot snapshot)
  {
    throw new NotSupportedException(
      "V1456 SyncProjectile cannot represent an NPC-owned projectile owner contract.");
  }
}
