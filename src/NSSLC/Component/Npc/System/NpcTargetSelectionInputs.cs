using System.Numerics;

namespace Terraria.Npc;

public readonly record struct NpcTargetSelectionInputs(
  NpcTargetSelectionStrategy Strategy,
  NpcTargetGeometrySnapshot Source,
  int Direction,
  int DirectionY,
  int OldDirection,
  int OldDirectionY,
  int OldTarget,
  bool CollideX,
  bool CollideY,
  bool Confused,
  bool Boss,
  bool FaceTarget,
  IReadOnlyList<NpcPlayerTargetSnapshot> Players,
  IReadOnlyList<NpcNpcTargetSnapshot> Npcs)
{
  /// <summary>
  /// The target slot visible when selection begins. Normal and Wall of Flesh
  /// retain it when no eligible player is found; a tank pet never changes it to
  /// the projectile owner's slot.
  /// </summary>
  public int CurrentTarget { get; init; } = int.MinValue;

  /// <summary>
  /// Optional distance origin used by TargetClosestUpgraded. Direction and pet
  /// line of sight continue to use the NPC's own center.
  /// </summary>
  public Vector2? CheckPosition { get; init; }
}
