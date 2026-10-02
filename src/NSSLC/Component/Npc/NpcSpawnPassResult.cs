namespace Terraria.Npc;

public readonly record struct NpcSpawnPassResult(
  int? LoopControlPlayerIndex,
  NpcSpawnCreationObservation CreationObservation)
{
  public bool ContinuationWasInvoked => LoopControlPlayerIndex.HasValue;
}
