using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.Protocol;

public sealed record InboundRawFrame(
  SessionEpoch Epoch,
  PacketSequence Sequence,
  DateTimeOffset ReceivedAt,
  RawPacketFrame Frame);
