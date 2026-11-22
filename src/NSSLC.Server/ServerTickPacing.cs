using System.Diagnostics;

namespace NSSLC.Server;

/// <summary>
/// 维护固定的 60 Hz tick 网格，让权威位置快照保持稳定节拍。
/// </summary>
/// <remarks>
/// 副作用登记：
/// 读取：无。时间戳由调用方传入，本类型不读系统时钟。
/// 写入：无。只做确定性计算，便于单独验证。
/// 责任边界：调用方（<see cref="Program.RunTicksAsync"/>）负责读取 <see cref="Stopwatch"/> 并执行 tick。
/// 顺序：每个 tick 结束后调用一次 <see cref="AdvanceDeadline"/>，推进到下一个格点。
/// 失败：不抛业务异常；周期非正数时抛 <see cref="ArgumentOutOfRangeException"/>。
/// 重复：幂等。同一组 (currentDeadline, now, period) 总是得到同一结果。
///
/// 设计动机：客户端在 serverAuthoritative 模式下把 <c>netOffset</c> 强制归零且不跑本地 AI，
/// 画面严格等于最后一次包 23 的位置，因此快照“什么时候到”比“什么时候算”更影响观感。
/// PeriodicTimer 会在某次 tick 超时后立刻补发下一条（把错过的格点合并成突发），
/// 使包 23 的到达间隔在 10 ms 与 25 ms 之间摆动；本类型改为丢弃错过的格点、始终落在网格上，
/// 宁可让模拟在过载时整体变慢，也不让快照成批补发。
/// </remarks>
public static class ServerTickPacing
{
  /// <summary>目标 tick 频率（Hz）。</summary>
  public const int TargetTicksPerSecond = 60;

  /// <summary>一个 tick 的 <see cref="Stopwatch"/> 时间戳长度。</summary>
  public static long PeriodTimestamp =>
      (long)(Stopwatch.Frequency / (double)TargetTicksPerSecond);

  /// <summary>
  /// 计算下一个 tick 的格点时间戳。
  /// </summary>
  /// <param name="currentDeadline">本 tick 所在格点的时间戳。</param>
  /// <param name="now">本 tick 结束时的单调时间戳。</param>
  /// <param name="period">tick 周期，单位为 <see cref="Stopwatch"/> 时间戳。</param>
  /// <param name="skipped">本次因超时被丢弃的格点数；未超时时为 0。</param>
  /// <returns>严格晚于 <paramref name="now"/> 的下一个格点时间戳。</returns>
  public static long AdvanceDeadline(long currentDeadline, long now, long period, out long skipped)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(period);

    long next = currentDeadline + period;
    if (next > now)
    {
      skipped = 0;
      return next;
    }

    // 已经错过至少一个格点：跳过整格，保证两个 tick 之间不会出现零间隔的补发。
    long missed = ((now - next) / period) + 1;
    skipped = missed;
    return next + (missed * period);
  }
}
