namespace Terraria.NpcTownBestiary;

public readonly record struct EvictTownResidentCommand(
  int NpcType,
  ulong ExpectedRevision);
