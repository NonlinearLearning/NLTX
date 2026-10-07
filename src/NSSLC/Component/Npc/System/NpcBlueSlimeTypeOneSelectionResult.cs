namespace Terraria.Npc;

public readonly record struct NpcBlueSlimeTypeOneSelectionResult(
  bool Attempted,
  float ItemState,
  bool NetUpdateRequested);
