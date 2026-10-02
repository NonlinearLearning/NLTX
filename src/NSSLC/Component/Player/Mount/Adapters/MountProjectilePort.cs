using System.Numerics;

namespace Terraria.Player.Mount;

public readonly record struct MountProjectileIntent(
  ContentId<ProjectileDefinition> ProjectileType,
  Vector2 Position,
  Vector2 Velocity,
  int Damage,
  float Knockback)
{
  public bool IsValid => Damage >= 0 && !float.IsNaN(Knockback);
}

public interface IMountProjectilePort
{
  MountAdapterCommitResult Commit(in MountProjectileIntent intent);
}
