namespace Terraria.Npc;

public readonly record struct NpcNpcTargetSnapshot(
  int Slot,
  int TypeId,
  NpcTargetGeometrySnapshot Geometry,
  bool IsActive)
{
  public bool IsSelectable => IsActive && TypeId == 548;
}
