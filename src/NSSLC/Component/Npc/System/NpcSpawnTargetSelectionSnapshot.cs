namespace Terraria.Npc;

// status: implementation-started
// sourceMembers: defaultTarget
// lifecycle: one spawn evaluation; not durable target state
public readonly record struct NpcSpawnTargetSelectionSnapshot(
  int DefaultTarget);
