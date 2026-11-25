using System;
using ArchWorld = Arch.Core.World;

namespace Terraria.Unshared.Projectiles;

/// <summary>Enqueue seam for the projectile spawn queue, kept inside this assembly.</summary>
/// <remarks>
/// 队列组件挂在专职的世界级实体上，定位它的 <see cref="ProjectileSpawnQueue"/> 是 <c>internal</c>。
/// 同程序集的生产者（<see cref="ProjectileItemUseSpawnSystem"/>）经由本类入队，不直接依赖
/// 单例定位约定。
///
/// 请求在本 tick 内由 <see cref="ProjectileSpawnRequestSystem"/> 排空；生成的实体已经装配好
/// 空间运动组件，当帧即可被共享空间管线驱动。
/// </remarks>
internal static class ProjectileSpawnRequests
{
  /// <summary>Enqueues one spawn request for the given tick.</summary>
  /// <exception cref="ArgumentNullException"><paramref name="world"/> is null.</exception>
  /// <exception cref="ArgumentOutOfRangeException"><paramref name="tick"/> is negative.</exception>
  /// <exception cref="InvalidOperationException">No spawn queue entity exists in the world.</exception>
  public static void Enqueue(ArchWorld world, in ProjectileSpawnRequest request, long tick)
  {
    ArgumentNullException.ThrowIfNull(world);
    if (tick < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(tick));
    }

    ProjectileSpawnQueue.Require(world).Enqueue(request, tick);
  }
}
