namespace Terraria.Npc;

public readonly record struct NpcShimmerTransparencyInput(
  bool CanDisplayBuffs,
  bool Shimmering,
  bool JustHit,
  bool? IsImmuneToShimmeringBuff);
