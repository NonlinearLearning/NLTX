using Terraria.Communication;

namespace Terraria.Network;

/// <summary>Associates one received frame with its session-owned sequence and timestamp.</summary>
public sealed record InboundRawFrame(
  SessionEpoch Epoch,
  PacketSequence Sequence,
  DateTimeOffset ReceivedAt,
  RawPacketFrame Frame);
