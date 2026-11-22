using System;
using ArchWorld = Arch.Core.World;

namespace Terraria.Projectile;

/// <summary>Public entry point for requesting projectile spawns from outside the component domain.</summary>
/// <remarks>
/// 组件库发布能力，宿主决定接线——这与 <c>CombatSystemRegistration</c> 的既有约定一致：
/// 库负责定义系统与数据结构，谁调用、按什么顺序调用由宿主决定。
///
/// 之所以需要这个公开入口：队列组件挂在专职的世界级实体上，而定位它的
/// <see cref="ProjectileSpawnQueue"/> 是 <c>internal</c>（不允许外部程序集直接依赖
/// 单例定位约定）。迁移期的旧投射物实现位于另一个程序集，需要一条不依赖内部约定的入队路径。
///
/// 请求在本 tick 内由 <see cref="ProjectileSpawnRequestSystem"/> 排空；生成的实体已经装配好
/// 空间运动组件，当帧即可被共享空间管线驱动。
/// </remarks>
public static class ProjectileSpawnRequests
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
