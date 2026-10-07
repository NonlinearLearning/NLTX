using System;
using System.Threading;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// 保存世界变换的并发占用和变换版本。
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
/// </remarks>
public sealed class WorldTransformTransactionComponent
{
  private readonly object _sync = new();
  private int _activeCount;
  private ulong _revision;

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

  public bool IsTransforming => ActiveCount > 0;

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
