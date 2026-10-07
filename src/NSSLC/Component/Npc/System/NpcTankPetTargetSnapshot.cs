namespace Terraria.Npc;

public readonly record struct NpcTankPetTargetSnapshot(
  int ProjectileSlot,
  NpcTargetGeometrySnapshot Geometry,
  bool? CanHit)
{
  /// <summary>
  /// Owner slot captured from the projectile identity. Normal target selection
  /// does not use this value to rewrite the NPC's target slot.
  /// </summary>
  public int OwnerSlot { get; init; } = -1;
}
