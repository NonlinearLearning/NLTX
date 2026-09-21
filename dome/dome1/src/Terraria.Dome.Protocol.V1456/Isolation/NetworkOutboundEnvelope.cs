using System;
using Terraria.Dome.Protocol.V1456.Protocol;

namespace Terraria.Dome.Protocol.V1456.Isolation;

public sealed class NetworkOutboundEnvelope
{
  private NetworkOutboundEnvelope(
    byte playerSlot,
    int targetClient,
    int ignoreClient,
    TerrariaMessageId messageId,
    ReadOnlyMemory<byte> payload)
  {
    PlayerSlot = playerSlot;
    TargetClient = targetClient;
    IgnoreClient = ignoreClient;
    MessageId = messageId;
    Payload = payload;
    FrameBytes = TerrariaFrameCodec.Encode(new TerrariaFrame(messageId, payload));
  }

  public long Sequence { get; internal set; }

  public byte PlayerSlot { get; }

  public int TargetClient { get; }

  public int IgnoreClient { get; }

  public TerrariaMessageId MessageId { get; }

  public ReadOnlyMemory<byte> Payload { get; }

  public ReadOnlyMemory<byte> FrameBytes { get; }

  public static NetworkOutboundEnvelope Frame(
    byte playerSlot,
    int targetClient,
    TerrariaMessageId messageId,
    ReadOnlySpan<byte> payload)
  {
    return Frame(
      playerSlot,
      targetClient,
      ignoreClient: -1,
      messageId,
      payload: payload);
  }

  public static NetworkOutboundEnvelope Frame(
    byte playerSlot,
    int targetClient,
    int ignoreClient,
    TerrariaMessageId messageId,
    ReadOnlySpan<byte> payload)
  {
    return new NetworkOutboundEnvelope(
      playerSlot,
      targetClient,
      ignoreClient,
      messageId,
      payload.ToArray());
  }
}
