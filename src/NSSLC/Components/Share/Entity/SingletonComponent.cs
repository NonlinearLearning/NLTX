using System;
using Arch.Core;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;

namespace Terraria.Entity;

/// <summary>Locates the single entity that carries a world-scoped singleton component.</summary>
/// <remarks>
/// World-scoped queues are attached to one dedicated entity so that no domain receives another
/// domain's mutable object through its constructor. The query description is immutable and built
/// once per component type, so this holds no mutable shared state.
/// </remarks>
internal static class SingletonComponent<T>
    where T : class
{
  private static readonly QueryDescription Query = new QueryDescription().WithAll<T>();

  public static T Require(ArchWorld world, string missingMessage)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArchEntity componentEntity = default;
    bool found = false;
    world.Query(
        in Query,
        (ArchEntity candidate) =>
        {
          componentEntity = candidate;
          found = true;
        });

    if (!found)
    {
      throw new InvalidOperationException(missingMessage);
    }

    return world.Get<T>(componentEntity);
  }
}
