using System.Collections.Generic;

namespace Terraria.Npc;

public interface INpcDamageTrackingStrategy
{
  NpcTypeId TrackerNpcType { get; }

  bool IsKilled { get; }

  bool Includes(NpcTypeId npcType);

  bool IsStillActive(IReadOnlySet<NpcTypeId> activeNpcTypes);

  void OnNpcKilled(NpcTypeId npcType);
}
