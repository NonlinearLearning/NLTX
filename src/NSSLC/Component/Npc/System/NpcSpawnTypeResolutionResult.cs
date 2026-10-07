namespace Terraria.Npc;

public readonly record struct NpcSpawnTypeResolutionResult(
  NpcTypeId RequestedType,
  NpcTypeId ResolvedType,
  bool GoodWorldRollConsumed,
  bool GoodWorldRollPassed)
{
  public bool TypeWasRemapped => RequestedType != ResolvedType;
}
