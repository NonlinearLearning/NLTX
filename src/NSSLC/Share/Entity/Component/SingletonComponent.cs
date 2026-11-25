using System;
using Arch.Core;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;

namespace Terraria.Share.Entities;

/// <summary>Locates the single entity that carries a world-scoped singleton component.</summary>
/// <remarks>
/// World-scoped queues are attached to one dedicated entity so that no domain receives another
/// domain's mutable object through its constructor. The query description is immutable and built
/// once per component type, so this holds no mutable shared state.
/// <para>
/// 同一组件出现两个宿主实体时直接抛错，不静默取其中一个：生产者写 A、消费者读 B 时
/// 现象是"队列永远为空"，排查成本远高于一条明确的异常。
/// </para>
/// </remarks>
internal static class SingletonComponent<T>
    where T : class
{
  private static readonly QueryDescription Query = new QueryDescription().WithAll<T>();

  public static T Require(ArchWorld world, string missingMessage)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArchEntity componentEntity = default;
    int found = 0;
    world.Query(
        in Query,
        (ArchEntity candidate) =>
        {
          componentEntity = candidate;
          found++;
        });

    if (found == 0)
    {
      throw new InvalidOperationException(missingMessage);
    }

    if (found > 1)
    {
      throw new InvalidOperationException(
          $"Exactly one entity may carry {typeof(T).Name}, but {found} do. " +
          "A world-scoped queue with two hosts splits producers from consumers.");
    }

    return world.Get<T>(componentEntity);
  }
}
