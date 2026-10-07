using System;

using Terraria.Content;

namespace Terraria.Npc;

public static class NpcSpawnDefinitionResolutionSystem
{
  public static NpcSpawnDefinitionResolutionResult Resolve(
    in NpcSpawnPreCommitResult preCommit,
    INpcDefinitionQuery definitions)
  {
    ArgumentNullException.ThrowIfNull(definitions);

    NpcSpawnEntityRequest resolvedRequest = preCommit.ResolvedRequest;
    if (!preCommit.SlotAcquisition.Found)
    {
      return new NpcSpawnDefinitionResolutionResult(
        NpcSpawnDefinitionResolutionStatus.NoSlotAvailable,
        resolvedRequest,
        null);
    }

    int requestedType = resolvedRequest.Type.Value;
    if (requestedType == 0)
    {
      return new NpcSpawnDefinitionResolutionResult(
        NpcSpawnDefinitionResolutionStatus.InvalidType,
        resolvedRequest,
        null);
    }

    NpcDefinition definition;
    if (new NpcNetId(requestedType).IsVariant)
    {
      if (!definitions.TryGetByNetId(requestedType, out definition!) ||
        definition is null)
      {
        return new NpcSpawnDefinitionResolutionResult(
          NpcSpawnDefinitionResolutionStatus.DefinitionNotFound,
          resolvedRequest,
          null);
      }

      if (definition.NetId != requestedType || definition.TypeId <= 0)
      {
        return new NpcSpawnDefinitionResolutionResult(
          NpcSpawnDefinitionResolutionStatus.DefinitionIdentityMismatch,
          resolvedRequest,
          null);
      }
    }
    else
    {
      if (!definitions.TryGetByTypeId(requestedType, out definition!) ||
        definition is null)
      {
        return new NpcSpawnDefinitionResolutionResult(
          NpcSpawnDefinitionResolutionStatus.DefinitionNotFound,
          resolvedRequest,
          null);
      }

      if (definition.TypeId != requestedType)
      {
        return new NpcSpawnDefinitionResolutionResult(
          NpcSpawnDefinitionResolutionStatus.DefinitionIdentityMismatch,
          resolvedRequest,
          null);
      }
    }

    return new NpcSpawnDefinitionResolutionResult(
      NpcSpawnDefinitionResolutionStatus.Resolved,
      resolvedRequest,
      definition);
  }
}
