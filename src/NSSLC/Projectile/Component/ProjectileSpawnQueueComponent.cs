using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Terraria.Unshared.Projectiles;

/// <summary>Tick-scoped queue of spawn requests consumed by the lifecycle owner.</summary>
/// <remarks>
/// 与 <see cref="ProjectileTerminationQueueComponent"/> 同形：队列挂在专职的世界级实体上，
/// 生产方（迁移期是旧的投射物包处理层，之后是各域自己的生成点）只入队，生命周期所有者
/// 每 tick 排空一次。这样生成不会发生在某个查询回调里，也不需要把可变缓冲交给任何一方。
///
/// 内容方只提供"在哪、朝哪、属于谁、用哪个定义"，其余装配由 <see cref="ProjectileLifecycleSystem"/> 负责。
/// </remarks>
public sealed class ProjectileSpawnQueueComponent
{
  private readonly List<ProjectileSpawnRequest> _pending = [];
  private long _tick = -1;

  public int Count => _pending.Count;

  public void BeginTick(long tick)
  {
    if (tick < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(tick));
    }

    if (_tick != tick)
    {
      _pending.Clear();
      _tick = tick;
    }
  }

  public void Enqueue(in ProjectileSpawnRequest request, long tick)
  {
    BeginTick(tick);
    _pending.Add(request);
  }

  public IReadOnlyList<ProjectileSpawnRequest> Drain(long tick)
  {
    if (_tick != tick)
    {
      return Array.Empty<ProjectileSpawnRequest>();
    }

    ProjectileSpawnRequest[] requests = _pending.ToArray();
    _pending.Clear();
    return Array.AsReadOnly(requests);
  }

  public void Clear()
  {
    _pending.Clear();
    _tick = -1;
  }
}
