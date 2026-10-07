namespace Terraria.Npc;

public readonly record struct NpcPlayerTargetSnapshot(
  int Slot,
  NpcTargetGeometrySnapshot Geometry,
  bool IsActive,
  bool IsDead,
  bool IsGhost,
  int Aggro,
  bool NoAggro,
  bool Gross,
  int ItemAnimation,
  NpcTankPetTargetSnapshot? TankPet)
{
  public bool IsSelectable => IsActive && !IsDead && !IsGhost;
}
