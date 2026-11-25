using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Terraria.Share.Emission;

/// <summary>Tick 级发射意图队列，由投射物域的排空系统每 tick 消费一次。</summary>
/// <remarks>
/// 与 <c>ProjectileSpawnQueueComponent</c> 同形：队列挂在专职的世界级实体上（单例），
/// 生产方只入队，排空方每 tick 排空一次，生成不发生在任何查询回调里。
/// <para>
/// <b>严格同 tick</b>：<see cref="Drain"/> 只返回本 tick 入队的请求，跨 tick 未排空的直接丢弃。
/// 这不是可调参数——请求由排空方转成生成请求后进入投射物域自己的队列，而后者本身就按 tick
/// 号排空；两级队列若采用不同语义，「滞留一次」会在第二级被静默吞掉。
/// </para>
/// <para>
/// 后果是硬约束：任何发射者必须与排空方处于<b>同一相位</b>（Preparation）。图格与电路在
/// Simulation 产生的发射事实不能直接入队，须先经跨相位事实队列。
/// </para>
/// </remarks>
public sealed class EmissionQueueComponent
{
  private readonly List<EmissionRequest> _pending = [];
  private long _tick = -1;

  /// <summary>Number of requests queued for the current tick.</summary>
  public int Count => _pending.Count;

  /// <summary>Binds the queue to a tick, discarding requests queued for an earlier one.</summary>
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

  /// <summary>Queues one emission intent for the given tick.</summary>
  public void Enqueue(in EmissionRequest request, long tick)
  {
    BeginTick(tick);
    _pending.Add(request);
  }

  /// <summary>
  /// Takes every request queued for <paramref name="tick"/> and clears the queue.
  /// Requests from another tick are not returned: the queue is tick-scoped by design.
  /// </summary>
  public IReadOnlyList<EmissionRequest> Drain(long tick)
  {
    if (_tick != tick)
    {
      return Array.Empty<EmissionRequest>();
    }

    EmissionRequest[] requests = _pending.ToArray();
    _pending.Clear();
    return Array.AsReadOnly(requests);
  }

  /// <summary>Drops every pending request and forgets the bound tick.</summary>
  public void Clear()
  {
    _pending.Clear();
    _tick = -1;
  }
}
