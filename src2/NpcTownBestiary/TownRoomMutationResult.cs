namespace Terraria.NpcTownBestiary;

public readonly record struct TownRoomMutationResult(
  bool Applied,
  bool Conflict,
  ulong Revision);
