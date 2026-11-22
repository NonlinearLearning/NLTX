namespace Terraria.Network;

/// <summary>Nonblocking seam from ECS projection to the host delivery pump.</summary>
/// <remarks>
/// 副作用登记：实现方负责把意图或批边界写入宿主投递队列；本接口自身不产生 I/O。
/// 批边界只改变“何时允许普通流量出队”，不改变任何帧的字节内容。
/// </remarks>
public interface ICommunicationOutboundPort
{
  bool TrySubmit(OutboundPacketIntent intent);

  /// <summary>
  /// 开启 tick 批：此后提交的实时帧属于同一批，宿主在其闭合前不得发送普通流量。
  /// </summary>
  void BeginTickBatch(long tick);

  /// <summary>
  /// 闭合 tick 批。闭合信号与实时帧走同一条有序通道，因此宿主一定在批内帧全部入队后才
  /// 恢复普通流量；无实时帧时立即恢复。
  /// </summary>
  void SealTickBatch(long tick);
}
