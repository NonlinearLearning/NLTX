
namespace Terraria.Network;

/// <summary>Host transport seam used by the outbound delivery pump.</summary>
/// <remarks>
/// 副作用登记：实现方把意图写入具体连接队列；<see cref="BeginTickBatch"/> 与
/// <see cref="SealTickBatchAsync"/> 只调整批门状态，不写 socket。
/// </remarks>
public interface ICommunicationTransportPort
{
  ValueTask<IReadOnlyList<SendReceipt>> SendAsync(
      OutboundPacketIntent intent,
      CancellationToken cancellationToken = default);

  /// <summary>开启 tick 批：在闭合前，连接不得取出普通流量。</summary>
  void BeginTickBatch(long tick);

  /// <summary>闭合 tick 批：连接恢复普通流量；已入队的实时帧仍然优先。</summary>
  ValueTask SealTickBatchAsync(long tick, CancellationToken cancellationToken = default);
}
