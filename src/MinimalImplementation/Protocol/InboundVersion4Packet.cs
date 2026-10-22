using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.Protocol;

public sealed record InboundVersion4Packet(
  InboundPacketEnvelope Envelope,
  Version4PacketHandling Handling,
  object? DecodedPacket,
  string Reason);
