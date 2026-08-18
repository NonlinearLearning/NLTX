using Terraria.Dome.Simulation;

namespace Terraria.Dome.Protocol.V1456.Isolation;

public readonly record struct NetworkProjectileSlice(
  int ReplicationId,
  int Identity,
  int ProjectileType,
  int OwnerValue,
  bool IsActive,
  float PositionX,
  float PositionY,
  float VelocityX,
  float VelocityY,
  int Damage,
  int RemainingLifetime,
  float Ai0,
  float Ai1,
  float Ai2,
  float Knockback,
  int OriginalDamage,
  long Revision)
{
  public static NetworkProjectileSlice From(ProjectileReplicationSnapshot snapshot)
  {
    return new NetworkProjectileSlice(
      snapshot.ReplicationId,
      snapshot.Identity,
      snapshot.ProjectileType,
      snapshot.Owner.Value,
      snapshot.IsActive,
      snapshot.Position.X,
      snapshot.Position.Y,
      snapshot.Velocity.X,
      snapshot.Velocity.Y,
      snapshot.Damage,
      snapshot.RemainingLifetime,
      snapshot.Ai0,
      snapshot.Ai1,
      snapshot.Ai2,
      snapshot.Knockback,
      snapshot.OriginalDamage,
      snapshot.Revision);
  }
}
