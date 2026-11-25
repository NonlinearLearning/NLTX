using System;
using System.Diagnostics;
using Arch.Core;
using Arch.System;
using ArchWorld = Arch.Core.World;
using Terraria.Share.Entities;

namespace Terraria.Unshared.World;

/// <summary>
/// Advances one authoritative <see cref="WorldRuntime" /> from the ECS tick context.
/// </summary>
/// <remarks>
/// Register this system before interaction commit when the host wants world time and revision to
/// advance once per server tick. The host still owns the clock and calls <see cref="EcsServer"/>.
/// 副作用登记：本系统每 tick 修改世界时间与液体状态，并记录 Tile 变更；不产生 I/O。
/// </remarks>
public sealed class WorldTickSystem : BaseSystem<ArchWorld, EcsServerTickContext>
{
  private readonly WorldRuntime _world;

  public WorldTickSystem(ArchWorld ecsWorld, WorldRuntime world)
      : base(ecsWorld ?? throw new ArgumentNullException(nameof(ecsWorld)))
  {
    _world = world ?? throw new ArgumentNullException(nameof(world));
  }

  // 不暴露 _world：任何持有本系统的域都能借它取回具体 WorldRuntime 并调用 TryBreakTile，
  // 那正是输入边界约束要堵住的跨域写路径。
  public WorldTickResult LastResult { get; private set; } = WorldTickResult.NotRun;

  /// <summary>上一次 <see cref="Update"/> 中时间推进（<c>AdvanceTick</c>）的耗时，毫秒。</summary>
  public double LastClockMilliseconds { get; private set; }

  /// <summary>上一次 <see cref="Update"/> 中液体模拟（<c>AdvanceLiquidSimulation</c>）的耗时，毫秒。</summary>
  public double LastLiquidMilliseconds { get; private set; }

  /// <summary>上一次液体步实际执行的操作数，用于判断是否触到预算上限。</summary>
  public int LastLiquidOperations { get; private set; }

  public override void Update(in EcsServerTickContext context)
  {
    long clockStarted = Stopwatch.GetTimestamp();
    LastResult = _world.AdvanceTick(context.Tick, context.DeltaTime);
    if (!LastResult.Applied)
    {
      throw new InvalidOperationException(LastResult.Failure);
    }

    LastClockMilliseconds =
        (Stopwatch.GetTimestamp() - clockStarted) * 1000d / Stopwatch.Frequency;

    long liquidStarted = Stopwatch.GetTimestamp();
    LastLiquidOperations = _world.AdvanceLiquidSimulation();
    LastLiquidMilliseconds =
        (Stopwatch.GetTimestamp() - liquidStarted) * 1000d / Stopwatch.Frequency;
  }
}
