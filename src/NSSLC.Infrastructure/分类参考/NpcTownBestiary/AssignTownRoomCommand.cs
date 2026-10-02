namespace Terraria.NpcTownBestiary;

public readonly record struct AssignTownRoomCommand(
  int NpcType,
  TownRoomTilePoint Room,
  ulong ExpectedRevision);
