namespace Terraria.WorldSession.NpcProgression.Invasion;

public readonly record struct InvasionWaveProgressSyncView(
  float TotalInvasionPoints,
  float WaveKills,
  int WaveNumber);
