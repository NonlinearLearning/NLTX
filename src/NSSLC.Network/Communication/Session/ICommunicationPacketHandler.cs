namespace Terraria.Network;

/// <summary>Handles one already-framed inbound message for a single session.</summary>
/// <remarks>
/// 处理器只产出出站意图，不直接写别人的领域状态；真正的权威提交由各领域系统在后续阶段完成。
/// </remarks>
public interface ICommunicationPacketHandler
{
  IReadOnlyList<OutboundPacketIntent> Handle(
      InboundCommunicationPacket packet,
      CommunicationSessionComponent session,
      long tick);
}
