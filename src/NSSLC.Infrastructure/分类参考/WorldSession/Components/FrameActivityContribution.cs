namespace Terraria.WorldSession.Components;

public readonly record struct FrameActivityContribution(
  int ActivePlayerCount,
  int SleepingPlayerCount,
  bool AnyActiveBoss,
  bool HadActiveInteractableProjectile);
