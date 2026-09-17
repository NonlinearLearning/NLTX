using System;
using System.Collections.Generic;

namespace Terraria.Npc;

public sealed class NpcDamageCompositeStrategy : INpcDamageTrackingStrategy
{
  private readonly HashSet<NpcTypeId> _npcTypes;
  private bool _isKilled;

  public NpcDamageCompositeStrategy(params NpcTypeId[] npcTypes)
  {
    ArgumentNullException.ThrowIfNull(npcTypes);
    if (npcTypes.Length == 0)
    {
      throw new ArgumentException(
        "A composite strategy requires at least one NPC type.",
        nameof(npcTypes));
    }

    _npcTypes = new HashSet<NpcTypeId>();
    for (int index = 0; index < npcTypes.Length; index++)
    {
      if (!npcTypes[index].IsValid)
      {
        throw new ArgumentException(
          "A composite strategy cannot contain an invalid NPC type.",
          nameof(npcTypes));
      }

      _npcTypes.Add(npcTypes[index]);
    }

    if (_npcTypes.Count != npcTypes.Length)
    {
      throw new ArgumentException(
        "A composite strategy cannot contain duplicate NPC types.",
        nameof(npcTypes));
    }
  }

  public bool IsKilled => _isKilled;

  public bool Includes(NpcTypeId npcType)
  {
    return _npcTypes.Contains(npcType);
  }

  public bool IsStillActive(IReadOnlySet<NpcTypeId> activeNpcTypes)
  {
    ArgumentNullException.ThrowIfNull(activeNpcTypes);
    foreach (NpcTypeId npcType in _npcTypes)
    {
      if (activeNpcTypes.Contains(npcType))
      {
        return true;
      }
    }

    return false;
  }

  public void OnNpcKilled(NpcTypeId npcType)
  {
    if (Includes(npcType))
    {
      _isKilled = true;
    }
  }
}
