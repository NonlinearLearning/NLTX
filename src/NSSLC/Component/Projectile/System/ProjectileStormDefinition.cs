using System;
using System.Numerics;

using EntityEcs.Queries;

namespace Terraria.Projectile;

public readonly record struct ProjectileStormDefinition(
  float StartAngle,
  float AnglePerBullet,
  int BulletsInStorm,
  float BulletsProgressInStormStartNormalized,
  float BulletsProgressInStormBonusByIndexNormalized,
  float StormTotalRange,
  Vector2 BulletSize)
{
  public float GetBulletProgress(int bulletIndex)
  {
    return BulletsProgressInStormStartNormalized
      + BulletsProgressInStormBonusByIndexNormalized * (float)bulletIndex;
  }

  public bool IsValid(int bulletIndex)
  {
    float bulletProgress = GetBulletProgress(bulletIndex);
    return bulletProgress >= 0.0f && bulletProgress <= 1.0f;
  }

  public Vector2 GetBulletPosition(int bulletIndex, Vector2 centerPoint)
  {
    float angle = StartAngle + AnglePerBullet * (float)bulletIndex;
    Vector2 direction = new(
      (float)Math.Cos(angle),
      (float)Math.Sin(angle));

    return centerPoint
      + direction * StormTotalRange * GetBulletProgress(bulletIndex);
  }

  public EntityHitbox GetBulletHitbox(int bulletIndex, Vector2 centerPoint)
  {
    Vector2 bulletPosition = GetBulletPosition(bulletIndex, centerPoint);
    return new EntityHitbox(
      (int)(bulletPosition.X - BulletSize.X / 2.0f),
      (int)(bulletPosition.Y - BulletSize.Y / 2.0f),
      (int)BulletSize.X,
      (int)BulletSize.Y);
  }
}
