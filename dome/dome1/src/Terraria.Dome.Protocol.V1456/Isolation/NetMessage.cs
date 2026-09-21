using System;
using Terraria.Dome.Protocol.V1456.Protocol;

namespace Terraria.Dome.Protocol.V1456.Isolation;

public sealed class NetMessage
{
  private readonly DomeNetworkIsolation _isolation;

  public NetMessage(DomeNetworkIsolation isolation)
  {
    _isolation = isolation ?? throw new ArgumentNullException(nameof(isolation));
  }

  public bool TrySendData(
    TerrariaMessageId messageId,
    byte playerSlot,
    int targetClient,
    ReadOnlySpan<byte> payload)
  {
    return TrySendData(
      messageId,
      playerSlot,
      targetClient,
      ignoreClient: -1,
      payload: payload);
  }

  public bool TrySendData(
    TerrariaMessageId messageId,
    byte playerSlot,
    int targetClient,
    int ignoreClient,
    ReadOnlySpan<byte> payload)
  {
    NetworkOutboundEnvelope envelope = NetworkOutboundEnvelope.Frame(
      playerSlot,
      targetClient,
      ignoreClient,
      messageId,
      payload);
    return _isolation.EnqueueOutbound(envelope);
  }

  public ProtocolCommandResult SendData(
    TerrariaMessageId messageId,
    byte playerSlot,
    int targetClient,
    ReadOnlySpan<byte> payload)
  {
    return SendData(
      messageId,
      playerSlot,
      targetClient,
      ignoreClient: -1,
      payload: payload);
  }

  public ProtocolCommandResult SendData(
    TerrariaMessageId messageId,
    byte playerSlot,
    int targetClient,
    int ignoreClient,
    ReadOnlySpan<byte> payload)
  {
    return TrySendData(messageId, playerSlot, targetClient, ignoreClient, payload)
      ? ProtocolCommandResult.Accepted
      : ProtocolCommandResult.Rejected;
  }
}
