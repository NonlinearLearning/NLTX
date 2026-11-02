using ArchWorld = Arch.Core.World;
using Terraria.Entity;

namespace Terraria.Combat;

/// <summary>Locates the single entity that carries the tick-scoped damage result queue.</summary>
internal static class CombatDamageResultQueue
{
  public static CombatDamageResultQueueComponent Require(ArchWorld world)
    => SingletonComponent<CombatDamageResultQueueComponent>.Require(
        world,
        "A CombatDamageResultQueueComponent entity must exist before damage results are consumed.");
}
