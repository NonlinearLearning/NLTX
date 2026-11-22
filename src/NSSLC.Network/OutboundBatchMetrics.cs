namespace Terraria.Network;

/// <summary>出站 tick 批的运行计数，用于判断发送侧是否跟不上。</summary>
/// <param name="Batches">自启动以来闭合的批总数。</param>
/// <param name="MeanBatchSize">平均每批帧数（`batch_size`）。</param>
/// <param name="BatchWaitP99Ms">从批闭合到连接放行普通流量的 p99 耗时（`batch_wait_p99`）。</param>
/// <param name="MandatoryDropped">被丢弃的 A 类帧数（`mandatory_dropped`，P1 落地前恒为 0）。</param>
/// <param name="BatchSuperseded">被更新批取代的批数（`batch_superseded`，P1 落地前恒为 0）。</param>
public readonly record struct OutboundBatchMetrics(
    long Batches,
    double MeanBatchSize,
    double BatchWaitP99Ms,
    long MandatoryDropped,
    long BatchSuperseded);
