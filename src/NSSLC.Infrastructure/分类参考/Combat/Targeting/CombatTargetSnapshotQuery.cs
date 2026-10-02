using System.Numerics;

namespace Terraria.Combat.Targeting;

public static class CombatTargetSnapshotQuery
{
  public static CombatTargetSnapshot Create(
    CombatTargetKind targetKind,
    CombatRectangle hitbox,
    Vector2 velocity)
  {
    return new CombatTargetSnapshot(
      targetKind,
      hitbox,
      hitbox.Width,
      hitbox.Height,
      new Vector2(hitbox.X, hitbox.Y),
      velocity);
  }
}
