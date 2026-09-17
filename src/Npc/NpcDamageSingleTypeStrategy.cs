using System;
using System.Collections.Generic;

namespace Terraria.Npc;

public sealed class NpcDamageSingleTypeStrategy : INpcDamageTrackingStrategy
{
  private readonly NpcTypeId _npcType;
  private bool _isKilled;

  public NpcDamageSingleTypeStrategy(NpcTypeId npcType)
  {
    if (!npcType.IsValid)
    {
      throw new ArgumentException("NPC type must be valid.", nameof(npcType));
    }

    _npcType = npcType;
  }

  public bool IsKilled => _isKilled;

  public bool Includes(NpcTypeId npcType)
  {
    return npcType == _npcType;
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
