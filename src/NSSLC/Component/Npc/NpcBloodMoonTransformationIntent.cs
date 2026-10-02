namespace Terraria.Npc;

public readonly record struct NpcBloodMoonTransformationIntent(
  NpcTypeId SourceType,
  NpcTypeId TargetType,
  bool RestoreValueToZero);
