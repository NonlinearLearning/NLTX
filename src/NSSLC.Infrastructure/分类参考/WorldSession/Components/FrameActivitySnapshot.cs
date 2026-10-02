namespace Terraria.WorldSession.Components;

public readonly record struct FrameActivitySnapshot(
  int ActivePlayerCount,
  int SleepingPlayerCount,
  bool AnyActiveBoss,
  bool HadActiveInteractableProjectile);
