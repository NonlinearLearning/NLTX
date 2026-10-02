namespace Terraria.WorldSession.NpcProgression.MoonLord;

public readonly record struct MoonLordCountdownTickResult(
  MoonLordCountdownTickStatus Status,
  int Countdown)
{
  public bool SpawnRequested => Status == MoonLordCountdownTickStatus.SpawnRequested;
}
