namespace Terraria.NpcTownBestiary;

public readonly record struct ConditionalDialogueConsumeResult(
  bool Applied,
  bool Conflict,
  ulong Revision);
