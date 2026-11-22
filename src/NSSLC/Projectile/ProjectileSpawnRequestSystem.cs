using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Arch.System;
using ArchWorld = Arch.Core.World;
using Terraria.Entity;

namespace Terraria.Projectile;

/// <summary>Drains queued spawn requests into the lifecycle owner once per tick.</summary>
/// <remarks>
/// 这是组件域投射物的生成接缝。放在 Preparation 相位记录生成意图；服务器路径的实体结构
/// 在 Commit barrier 完成后才进入世界，因此从下一个 Simulation tick 起由空间协调系统驱动。
///
/// 排空结果的 <see cref="ProjectileSpawnResult"/> 逐条保留在 <see cref="Results"/> 里，
/// 失败不会抛出也不会静默丢弃——调用方或日志侧可以据此发现"请求了但没生成"。
/// </remarks>
public sealed class ProjectileSpawnRequestSystem : BaseSystem<ArchWorld, EcsServerTickContext>
{
  private readonly ProjectileLifecycleSystem _lifecycle;
  private readonly List<ProjectileSpawnResult> _results = [];

  public ProjectileSpawnRequestSystem(
      ArchWorld world,
      ProjectileLifecycleSystem lifecycle)
      : base(world ?? throw new ArgumentNullException(nameof(world)))
  {
    _lifecycle = lifecycle ?? throw new ArgumentNullException(nameof(lifecycle));
  }

  /// <summary>The results produced by the most recent drain.</summary>
  public IReadOnlyList<ProjectileSpawnResult> Results => _results;

  public override void Update(in EcsServerTickContext context)
  {
    _results.Clear();
    ProjectileSpawnQueueComponent queue = ProjectileSpawnQueue.Require(World);
    IReadOnlyList<ProjectileSpawnRequest> requests = queue.Drain(context.Tick);
    for (int index = 0; index < requests.Count; index++)
    {
      _results.Add(_lifecycle.TrySpawn(requests[index], context.Commands));
    }
  }
}
