namespace Terraria.Npc;

public readonly record struct NpcSpawnEntryResult(
  bool NoSpawnCycleWasConsumed,
  bool RespawnCheckRan,
  NpcSpawnPassResult Pass)
{
  public bool NaturalSpawnPassRan => RespawnCheckRan;
}
