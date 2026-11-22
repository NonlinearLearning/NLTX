using System;
using Arch.Core;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;

namespace Terraria.Combat;

/// <summary>Locates the single entity that carries the tick-scoped attack intent queue.</summary>
/// <remarks>
/// Producers in every domain resolve the queue through this accessor rather than receiving it as a
/// constructor argument, so no domain holds another domain's mutable queue.
/// </remarks>
public static class CombatAttackIntentQueue
{
  private static readonly QueryDescription QueueQuery =
      new QueryDescription().WithAll<CombatAttackIntentQueueComponent>();

  public static CombatAttackIntentQueueComponent Require(ArchWorld world)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArchEntity queueEntity = default;
    bool found = false;
    world.Query(
        in QueueQuery,
        (ArchEntity candidate) =>
        {
          queueEntity = candidate;
          found = true;
        });

    if (!found)
    {
      throw new InvalidOperationException(
          "A CombatAttackIntentQueueComponent entity must exist before attack intents are produced.");
    }

    return world.Get<CombatAttackIntentQueueComponent>(queueEntity);
  }
}
