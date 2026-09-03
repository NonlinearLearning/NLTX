using System;
using System.Collections.Generic;
using System.IO;
using Terraria.Dome.Protocol.V1456.Protocol;

namespace Terraria.Dome.Protocol.V1456.Isolation;

public sealed class MessageBuffer
{
  private readonly DomeNetworkIsolation _isolation;
  private readonly List<byte> _pendingBytes = new();
  private readonly byte _playerSlot;

  public MessageBuffer(byte playerSlot, DomeNetworkIsolation isolation)
  {
    _playerSlot = playerSlot;
    _isolation = isolation ?? throw new ArgumentNullException(nameof(isolation));
  }

  public bool CheckBytes(ReadOnlySpan<byte> frameBytes)
  {
    if (frameBytes.Length < sizeof(ushort))
    {
      return false;
    }

    int frameLength = frameBytes[0] | frameBytes[1] << 8;
    return frameLength >= TerrariaProtocolVersion.MinimumFrameLength &&
      frameLength <= TerrariaProtocolVersion.MaximumFrameLength &&
      frameBytes.Length >= frameLength;
  }

  public MessageBufferReceiveResult ReceiveBytes(
    ReadOnlySpan<byte> bytes,
    out int acceptedBytes)
  {
    acceptedBytes = 0;
    if (bytes.Length == 0)
    {
      return MessageBufferReceiveResult.Incomplete;
    }

    for (int index = 0; index < bytes.Length; index++)
    {
      _pendingBytes.Add(bytes[index]);
    }

    while (_pendingBytes.Count >= 2)
    {
      int frameLength = _pendingBytes[0] | _pendingBytes[1] << 8;
      if (frameLength < TerrariaProtocolVersion.MinimumFrameLength ||
          frameLength > TerrariaProtocolVersion.MaximumFrameLength)
      {
        _pendingBytes.Clear();
        return MessageBufferReceiveResult.Rejected;
      }

      if (_pendingBytes.Count < frameLength)
      {
        return acceptedBytes > 0
          ? MessageBufferReceiveResult.Accepted
          : MessageBufferReceiveResult.Incomplete;
      }

      byte[] frameBytes = _pendingBytes.GetRange(0, frameLength).ToArray();
      _pendingBytes.RemoveRange(0, frameLength);
      NetworkInboundEnvelope envelope;
      try
      {
        envelope = NetworkInboundEnvelope.FromFrame(_playerSlot, frameBytes);
      }
      catch (InvalidDataException)
      {
        _pendingBytes.Clear();
        return MessageBufferReceiveResult.Rejected;
      }

      if (!_isolation.EnqueueInbound(envelope))
      {
        return MessageBufferReceiveResult.Rejected;
      }

      acceptedBytes += frameLength;
    }

    return acceptedBytes > 0
      ? MessageBufferReceiveResult.Accepted
      : MessageBufferReceiveResult.Incomplete;
  }
}
