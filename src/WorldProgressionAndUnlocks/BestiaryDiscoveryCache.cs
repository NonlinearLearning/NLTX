using System.Collections.Generic;
using EntityEcs.Queries;
using Terraria.Npc;

namespace Terraria.WorldProgressionAndUnlocks;

public sealed class BestiaryDiscoveryCache
{
  public List<EntityHitbox> PlayerBestiaryBounds { get; } = new();

  public List<NpcNetId> SeenNpcNetworkIds { get; } = new();
}
