namespace Terraria.Npc;

public enum NpcSpawnBranchSelectionStatus : byte
{
  RequestProduced,
  HandledWithoutRequest,
  UnknownInvasionTypeEarlyReturn,
  EarlierUnmodeledBranchesUnresolved,
  NoModeledBranchMatched,
}
