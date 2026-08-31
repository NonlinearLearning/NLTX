namespace Terraria.Dome.Simulation.Commands;

public readonly record struct SpawnProjectileCommand(
  PlayerHandle Owner,
  float X,
  float Y,
  int Facing,
  int Damage,
  int LifetimeTicks,
  int ProjectileType = 1,
  int BehaviorId = 1,
  float InitialVelocityY = 0.0f,
  int MaximumPenetration = 1,
  float ProjectileSpeed = 0.0f,
  int AuthoritativeDamage = 0,
  float AuthoritativeKnockback = 0.0f,
  ushort BannerIdToRespondTo = 0,
  float Ai0 = 0.0f,
  float Ai1 = 0.0f,
  float Ai2 = 1.0f,
  string MiscText = "",
  bool IsSentry = false,
  ushort MinionSpawnItemType = 0,
  int MinionSpawnItemPrefix = 0,
  bool IsDd2Summon = false,
  bool UseZeroVelocity = false)
{
  public bool Dd2Summon => IsDd2Summon;
}
