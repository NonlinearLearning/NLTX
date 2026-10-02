using Terraria.Content;

namespace Terraria.Npc;

public readonly record struct NpcSpawnDefinitionResolutionResult(
  NpcSpawnDefinitionResolutionStatus Status,
  NpcSpawnEntityRequest ResolvedRequest,
  NpcDefinition? Definition)
{
  public bool IsResolved =>
    Status == NpcSpawnDefinitionResolutionStatus.Resolved && Definition is not null;
}
