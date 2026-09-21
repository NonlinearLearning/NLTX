namespace Terraria.WorldSession.Components;

public readonly record struct WorldTimeSkipSnapshot(
  bool FastForwardToDawn,
  int SundialCooldownTicks,
  bool FastForwardToDusk,
  int MoondialCooldownTicks);
