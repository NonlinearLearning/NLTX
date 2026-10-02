namespace Terraria.Npc;

public readonly record struct NpcShimmerTransparencyResult(
  bool Applied,
  bool RequiredInputMissing,
  float PreviousTransparency,
  float CurrentTransparency);
