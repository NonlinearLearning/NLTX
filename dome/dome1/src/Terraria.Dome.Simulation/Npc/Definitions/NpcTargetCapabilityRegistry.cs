using System.Collections.Frozen;
using System.Collections.Generic;
using System;

namespace Terraria.Dome.Simulation.Npc.Definitions;

public sealed class NpcTargetCapabilityRegistry
{
  private readonly FrozenSet<int> _supportedDefinitionIds;

  public NpcTargetCapabilityRegistry(IEnumerable<int> supportedDefinitionIds)
  {
    ArgumentNullException.ThrowIfNull(supportedDefinitionIds);
    HashSet<int> ids = new();
    foreach (int definitionId in supportedDefinitionIds)
    {
      if (definitionId <= 0 || !ids.Add(definitionId))
      {
        throw new ArgumentException(
          "NPC target capability IDs must be positive and unique.",
          nameof(supportedDefinitionIds));
      }
    }

    _supportedDefinitionIds = ids.ToFrozenSet();
  }

  public int Count => _supportedDefinitionIds.Count;

  public bool Supports(int definitionId)
  {
    return _supportedDefinitionIds.Contains(definitionId);
  }

  public bool Supports(NpcDefinition definition)
  {
    return definition.SupportsNpcTargets || Supports(definition.DefinitionId);
  }

  public static NpcTargetCapabilityRegistry CreateVersion1456()
  {
    return new NpcTargetCapabilityRegistry([
      547,
      552,
      553,
      554,
      561,
      562,
      563,
      555,
      556,
      557,
      558,
      559,
      560,
      576,
      577,
      568,
      569,
      566,
      567,
      570,
      571,
      572,
      573,
      564,
      565,
      574,
      575,
      551,
      578,
      210,
      211,
      620,
      668]);
  }
}
