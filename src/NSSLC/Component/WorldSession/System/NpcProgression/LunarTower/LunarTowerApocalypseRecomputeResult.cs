namespace Terraria.WorldSession.NpcProgression.LunarTower;

public readonly record struct LunarTowerApocalypseRecomputeResult(
  bool WasApocalypseActive,
  bool DeactivatedMissingTowers,
  bool StartedImpendingDoom,
  int ImpendingDoomCountdownTime);
