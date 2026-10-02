using System.Numerics;

namespace Terraria.Combat;

// Stores minion rest and attack target compatibility facts.
// NPC registry validation and type 99/115 network projection remain external.
public sealed class PlayerMinionTargetComponent
{
  public Vector2 RestTargetPoint { get; set; }

  // This is a legacy NPC slot, not a stable ECS entity identity.
  public int AttackTarget { get; set; } = -1;

  public bool HasRestTarget => RestTargetPoint != Vector2.Zero;
}
