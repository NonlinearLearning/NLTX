using System;
using ArchWorld = Arch.Core.World;
using Terraria.Share.Entities;

namespace Terraria.Share.Emission;

/// <summary>发射队列的定位与入队入口。</summary>
/// <remarks>
/// 队列组件挂在专职的世界级实体上，宿主在启动时创建一个；定位走共享
/// <c>SingletonComponent</c>，同组件出现两个宿主时直接抛错，而不是静默取其中一个——
/// 生产者写 A、消费者读 B 的现象是「队列永远为空」，排查成本远高于一条明确异常。
/// <para>
/// 入队不校验发射者是否已注册：那是投射物域排空时的职责。本类型只保证请求被记进本 tick。
/// </para>
/// </remarks>
public static class EmissionQueue
{
  /// <summary>Locates the single entity that carries the emission queue.</summary>
  /// <exception cref="ArgumentNullException"><paramref name="world"/> is null.</exception>
  /// <exception cref="InvalidOperationException">No emission queue entity exists in the world.</exception>
  public static EmissionQueueComponent Require(ArchWorld world)
  {
    ArgumentNullException.ThrowIfNull(world);
    return SingletonComponent<EmissionQueueComponent>.Require(
        world,
        "An EmissionQueueComponent entity must exist before emissions are recorded.");
  }

  /// <summary>Queues one emission intent for the given tick.</summary>
  /// <exception cref="ArgumentNullException"><paramref name="world"/> is null.</exception>
  /// <exception cref="ArgumentOutOfRangeException"><paramref name="tick"/> is negative.</exception>
  /// <exception cref="InvalidOperationException">No emission queue entity exists in the world.</exception>
  public static void Enqueue(ArchWorld world, in EmissionRequest request, long tick)
  {
    ArgumentNullException.ThrowIfNull(world);
    if (tick < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(tick));
    }

    Require(world).Enqueue(request, tick);
  }
}
