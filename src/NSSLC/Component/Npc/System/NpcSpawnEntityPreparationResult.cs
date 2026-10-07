namespace Terraria.Npc;

public readonly record struct NpcSpawnEntityPreparationResult(
  NpcSpawnEntityRequest Request,
  NpcTypeId FromNetIdType,
  NpcSpawnEntityRequest PreparedRequest,
  bool CommonVariantRollConsumed,
  bool AnniversaryVariantRollConsumed);
