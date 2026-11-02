using ArchWorld = Arch.Core.World;
using Terraria.Entity;

namespace Terraria.Combat;

/// <summary>Locates the single entity that carries the tick-scoped damage request queue.</summary>
internal static class CombatDamageRequestQueue
{
  public static CombatDamageRequestQueueComponent Require(ArchWorld world)
    => SingletonComponent<CombatDamageRequestQueueComponent>.Require(
        world,
        "A CombatDamageRequestQueueComponent entity must exist before damage requests are produced.");
}
