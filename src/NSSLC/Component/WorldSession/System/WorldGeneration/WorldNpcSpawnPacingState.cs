using System;

namespace Terraria.WorldGeneration.Components;

public sealed class WorldNpcSpawnPacingState
{
  public int NpcSpawnDelay { get; private set; }

  public int NpcSpawnPeriod { get; private set; }

  public void Replace(int npcSpawnDelay, int npcSpawnPeriod)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(npcSpawnDelay);
    ArgumentOutOfRangeException.ThrowIfNegative(npcSpawnPeriod);

    NpcSpawnDelay = npcSpawnDelay;
    NpcSpawnPeriod = npcSpawnPeriod;
  }

  public void Reset()
  {
    NpcSpawnDelay = 0;
    NpcSpawnPeriod = 0;
  }
}
