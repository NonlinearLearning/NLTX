using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct ShadowOrbBreakProgression(
  bool ShadowOrbSmashed,
  int NextShadowOrbCount,
  bool ShouldAttemptBossSpawn);
