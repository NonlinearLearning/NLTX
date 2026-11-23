namespace Terraria.Network;

/// <summary>tick 批准时性：该 tick 的 A 批是否在自己的 tick 窗口内写完。</summary>
/// <param name="SealedBatches">已闭合的 A 批总数。</param>
/// <param name="OverdueBatches">写线程取到闭合标记时已超过一个 tick 周期的批数（跨期）。</param>
/// <param name="MaxLatencyMilliseconds">观察到的最大批延迟。</param>
/// <remarks>
/// “跨期”指第 N tick 的 A 批在第 N+1 tick 开始之后才写出。它比平均间隔更能反映客户端
/// 看到的位置是否准时，因为均匀的间隔也可能每一帧都错过各自的 tick。
/// </remarks>
public readonly record struct TickBatchTimeliness(
    long SealedBatches,
    long OverdueBatches,
    double MaxLatencyMilliseconds)
{
  /// <summary>跨期批占已闭合批的比例；无样本时为 0。</summary>
  public double OverdueRatio => SealedBatches == 0 ? 0 : OverdueBatches / (double)SealedBatches;
}
