using System;
using System.Collections.Generic;

namespace Terraria.Npc;

public sealed class NpcDamageSingleTypeStrategy : INpcDamageTrackingStrategy
{
  private readonly NpcTypeId _npcType;
  private readonly HashSet<NpcTypeId> _includedTypes;
  private bool _isKilled;

  public NpcDamageSingleTypeStrategy(
    NpcTypeId npcType,
    IEnumerable<NpcTypeId>? includedTypes = null)
  {
    if (!npcType.IsValid)
    {
      throw new ArgumentException("NPC type must be valid.", nameof(npcType));
    }

    _npcType = npcType;
    _includedTypes = new HashSet<NpcTypeId> { npcType };
    if (includedTypes is null)
    {
      return;
    }

    foreach (NpcTypeId includedType in includedTypes)
    {
      if (!includedType.IsValid)
      {
        throw new ArgumentException(
          "An included NPC type must be valid.",
          nameof(includedTypes));
      }

      _includedTypes.Add(includedType);
    }
  }

  public bool IsKilled => _isKilled;

  public NpcTypeId TrackerNpcType => _npcType;

  public bool Includes(NpcTypeId npcType)
  {
    return _includedTypes.Contains(npcType);
  }

  public bool IsStillActive(IReadOnlySet<NpcTypeId> activeNpcTypes)
  {
    ArgumentNullException.ThrowIfNull(activeNpcTypes);
    return activeNpcTypes.Contains(_npcType);
  }

  public void OnNpcKilled(NpcTypeId npcType)
  {
    if (Includes(npcType))
    {
      _isKilled = true;
    }
  }
}
