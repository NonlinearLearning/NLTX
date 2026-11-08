using System;
using System.Collections.Generic;
using Arch.Core;
using Arch.System;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Entity;
using Terraria.Npc;

namespace Terraria.Network;

/// <summary>
/// Binds a freshly spawned creature to a client NPC slot.
/// </summary>
/// <remarks>
/// The NPC domain creates the entity and states its protocol type; the slot is a protocol
/// projection, so it is assigned here rather than inside the spawner. Slots are allocated from the
/// lowest free index in <c>[0, Main.maxNPCs)</c>, which is the same range the client keeps, and the
/// generation stays at one until slot reuse is modelled.
/// </remarks>
public sealed class NpcNetworkIdentitySystem : BaseSystem<ArchWorld, EcsServerTickContext>
{
  /// <summary>Version4 <c>Main.maxNPCs</c>.</summary>
  public const int MaximumNpcs = 200;

  private readonly QueryDescription _identityQuery =
      new QueryDescription().WithAll<NpcNetworkIdentityComponent>();

  public NpcNetworkIdentitySystem(ArchWorld world)
      : base(world ?? throw new ArgumentNullException(nameof(world)))
  {
  }

  public override void Update(in EcsServerTickContext context)
  {
    NpcSpawnFactQueueComponent queue = NpcSpawnFactQueue.Require(World);
    IReadOnlyList<NpcSpawnedFact> facts = queue.Drain();
    if (facts.Count == 0)
    {
      return;
    }

    bool[] used = new bool[MaximumNpcs];
    World.Query(
        in _identityQuery,
        (ref NpcNetworkIdentityComponent identity) =>
        {
          if (identity.Slot < used.Length)
          {
            used[identity.Slot] = true;
          }
        });

    int nextFree = 0;
    for (int index = 0; index < facts.Count; index++)
    {
      NpcSpawnedFact fact = facts[index];
      if (fact.Entity == ArchEntity.Null || !World.IsAlive(fact.Entity))
      {
        continue;
      }

      while (nextFree < MaximumNpcs && used[nextFree])
      {
        nextFree++;
      }

      if (nextFree >= MaximumNpcs)
      {
        // 槽位耗尽：实体保留在 ECS 中，投影层本 tick 无法表达它。
        return;
      }

      used[nextFree] = true;
      World.Add(fact.Entity, new NpcNetworkIdentityComponent((byte)nextFree, 1, fact.NetworkType));
    }
  }
}
