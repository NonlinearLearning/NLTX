using System;
using System.Threading;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// 协调世界变换的活动计数、状态版本和等待完成流程。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.WorldGen。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/WorldGen.cs。</para>
/// <para>主要源成员：_transformingWorld（第 4145 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P16-world-lifecycle-housing-metrics-component-design.md。
/// </para>
/// <para>依据位置：第 160 行。</para>
/// <para>
/// 用途：外部世界变换流程在开始和结束时登记活动事务，其他流程可以等待全部变换结束后继续工作。
/// </para>
/// <para>
/// 状态变化：Begin 加一，End 减一，二者都在锁内推进版本；最后一个活动结束时唤醒等待者。计数或版本溢出以及无活动时结束会抛出异常。
/// </para>
/// <para>
/// 当前实现：WaitUntilIdle 在仍有活动时通过 Monitor.Wait 等待，并在等待循环中响应取消。该组件协调计数与等待，实际地形修改和事务结果由外部流程执
/// 行。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C203，L2（通用可选玩法）。分级理由：生态采样、结构保留区、地层统计、扫描和地形变换适用于可编辑／动态世界，固定小地图可减少。
/// </para>
/// </remarks>
public sealed class WorldTransformTransactionComponent
{
  private readonly object _sync = new();
  private int _activeCount;
  private ulong _revision;

  /// <summary>
  /// 加锁读取的当前活动世界变换数量。
  /// </summary>
  public int ActiveCount
  {
    get
    {
      lock (_sync)
      {
        return _activeCount;
      }
    }
  }

  /// <summary>
  /// 加锁读取的变换登记版本，每次成功开始或结束都会递增。
  /// </summary>
  public ulong Revision
  {
    get
    {
      lock (_sync)
      {
        return _revision;
      }
    }
  }

  /// <summary>
  /// 活动世界变换数量大于零时返回 true。
  /// </summary>
  public bool IsTransforming => ActiveCount > 0;

  /// <summary>
  /// 在锁内登记一个活动变换、递增版本，并返回新的活动数。
  /// </summary>
  internal int Begin()
  {
    lock (_sync)
    {
      if (_activeCount == int.MaxValue)
      {
        throw new InvalidOperationException(
          "The active world transform count cannot exceed Int32.MaxValue.");
      }

      EnsureRevisionCanAdvance();
      _activeCount++;
      _revision++;
      return _activeCount;
    }
  }

  /// <summary>
  /// 在锁内结束一个活动变换、递增版本；活动归零时唤醒等待者。
  /// </summary>
  internal int End()
  {
    lock (_sync)
    {
      if (_activeCount == 0)
      {
        throw new InvalidOperationException(
          "A world transform cannot complete without an active transaction.");
      }

      EnsureRevisionCanAdvance();
      _activeCount--;
      _revision++;
      if (_activeCount == 0)
      {
        Monitor.PulseAll(_sync);
      }

      return _activeCount;
    }
  }

  /// <summary>
  /// 等待活动变换全部结束；仍有活动时检查取消并等待状态通知。
  /// </summary>
  internal void WaitUntilIdle(CancellationToken cancellationToken)
  {
    using CancellationTokenRegistration registration = cancellationToken.Register(
      static state => ((WorldTransformTransactionComponent)state!).PulseWaiters(),
      this);
    lock (_sync)
    {
      while (_activeCount > 0)
      {
        cancellationToken.ThrowIfCancellationRequested();
        Monitor.Wait(_sync);
      }
    }
  }

  private void PulseWaiters()
  {
    lock (_sync)
    {
      Monitor.PulseAll(_sync);
    }
  }

  private void EnsureRevisionCanAdvance()
  {
    if (_revision == ulong.MaxValue)
    {
      throw new InvalidOperationException(
        "The world transform revision cannot exceed UInt64.MaxValue.");
    }
  }
}
