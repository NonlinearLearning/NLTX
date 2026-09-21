namespace Terraria.Npc.Queries;

public readonly record struct NpcActivePresenceScanEntry(
  int NpcType,
  bool IsActive);
