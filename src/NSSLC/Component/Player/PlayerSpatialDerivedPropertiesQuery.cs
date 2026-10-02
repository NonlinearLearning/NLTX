using System;
using System.Numerics;

namespace Terraria.Player;

public static class PlayerSpatialDerivedPropertiesQuery
{
  private const float StandingStillVelocityThreshold = 0.05f;

  public static Vector2 BlehOldPositionFixer => -Vector2.UnitY;

  public static float HeightOffsetHitboxCenter(
    in PlayerSpatialSnapshot snapshot)
  {
    if (snapshot.MountActive)
    {
      return snapshot.MountPlayerOffsetHitbox;
    }

    if (snapshot.PortableStoolInUse)
    {
      return snapshot.PortableStoolHeightBoost - snapshot.PortableStoolVisualYOffset;
    }

    return 0.0f;
  }

  public static int HeightOffsetBoost(in PlayerSpatialSnapshot snapshot)
  {
    if (snapshot.MountActive)
    {
      return snapshot.MountHeightBoost;
    }

    return snapshot.PortableStoolInUse ? snapshot.PortableStoolHeightBoost : 0;
  }

  public static PlayerSpatialHitbox HitboxForBestiaryNearbyCheck(
    in PlayerSpatialSnapshot snapshot)
  {
    PlayerSpatialHitbox hitbox = new(
      (int)snapshot.Position.X,
      (int)snapshot.Position.Y,
      snapshot.Width,
      snapshot.Height);
    return hitbox.Inflate(300, 200);
  }

  public static bool IsConsideredStandingStill(in PlayerSpatialSnapshot snapshot)
  {
    return MathF.Abs(snapshot.Velocity.X) < StandingStillVelocityThreshold &&
      MathF.Abs(snapshot.Velocity.Y) < StandingStillVelocityThreshold;
  }

  public static float BaseHeight(in PlayerSpatialSnapshot snapshot)
  {
    return snapshot.Height - HeightOffsetBoost(snapshot);
  }

  public static Vector2 MountedCenter(in PlayerSpatialSnapshot snapshot)
  {
    float heightOffsetHitboxCenter = HeightOffsetHitboxCenter(snapshot);
    return new Vector2(
      snapshot.Position.X + snapshot.Width / 2,
      snapshot.Position.Y + BaseHeight(snapshot) / 2.0f + heightOffsetHitboxCenter);
  }

  public static Vector2 VisualPosition(in PlayerSpatialSnapshot snapshot)
  {
    return snapshot.Position + new Vector2(0.0f, snapshot.GfxOffY);
  }

  public static bool CCed(in PlayerSpatialSnapshot snapshot)
  {
    return snapshot.Frozen || snapshot.Webbed || snapshot.Stoned;
  }
}
