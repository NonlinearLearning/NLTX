namespace Terraria.Npc;

public readonly record struct NpcStatusTickResult(
  bool SkippedInactive,
  NpcBuffStateUpdateResult BuffFlags,
  NpcSoulDrainVisualEffectResult? SoulDrainEffects,
  NpcBuffStateUpdateResult? ExpiredBuffs,
  NpcLifeRegenerationResult? LifeRegeneration,
  NpcLifeRegenerationCommitResult? LifeRegenerationCommit,
  NpcBloodMoonTransformationIntent? BloodMoonTransformation = null,
  NpcGravityResult? Gravity = null,
  bool GravityDeferredUntilPostTransformationSnapshot = false,
  NpcShimmerTransparencyResult? ShimmerTransparency = null);
