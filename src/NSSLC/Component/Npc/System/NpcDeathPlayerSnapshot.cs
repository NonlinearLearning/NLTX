namespace Terraria.Npc;

public readonly record struct NpcDeathPlayerSnapshot(
  bool IsActive,
  bool IsDead,
  bool KillClothier);
