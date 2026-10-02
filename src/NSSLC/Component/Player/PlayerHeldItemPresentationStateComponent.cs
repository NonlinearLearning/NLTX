using System.Numerics;

namespace Terraria.Player;

// status: implemented-isolated-core
// source-members: P09-713, P09-714
// crossSubsystemOwner: held projectile link remains PlayerUse/Projectile integration-review
public sealed class PlayerHeldItemPresentationStateComponent
{
  public float ItemRotation { get; internal set; }

  public Vector2 ItemLocation { get; internal set; }
}
