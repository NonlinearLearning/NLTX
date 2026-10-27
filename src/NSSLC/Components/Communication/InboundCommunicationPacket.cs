namespace Terraria.Communication;

/// <summary>Decoded or undecoded packet data delivered to the ECS ingress boundary.</summary>
public sealed record InboundCommunicationPacket(
  InboundPacketEnvelope Envelope,
  RawPacketFrame Frame);
