using ArchWorld = Arch.Core.World;
using Terraria.Share.Entities;

namespace Terraria.Share.Combat;

/// <summary>Locates the single entity that carries the tick-scoped damage request queue.</summary>
public static class CombatDamageRequestQueue
{
  public static CombatDamageRequestQueueComponent Require(ArchWorld world)
    => SingletonComponent<CombatDamageRequestQueueComponent>.Require(
        world,
        "A CombatDamageRequestQueueComponent entity must exist before damage requests are produced.");
}
