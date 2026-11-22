using Terraria.Communication;

namespace Terraria.Network;

public sealed record PacketTcpConnectionOptions
{
  public const int MaximumTransportRawBytes = 64 * 1024 * 1024;
  public const int DefaultMaximumReceiveBufferedBytes = 128 * 1024;
  public const int DefaultMaximumPendingReceiveFrames = 64;
  public const int DefaultReceiveBufferBytes = 8 * 1024;
  public const int DefaultMaximumPendingSendBytes = 512 * 1024;
  public const int DefaultMaximumPendingSendFrames = 128;
  public const int DefaultMaximumPendingSendRequests = 64;
  public const int DefaultMaximumPendingRealtimeSendBytes = 128 * 1024;
  public const int DefaultMaximumPendingRealtimeSendFrames = 32;
  public const int DefaultMaximumPendingRealtimeSendRequests = 16;

  public int MaximumReceiveBufferedBytes { get; init; } = DefaultMaximumReceiveBufferedBytes;
  /// <summary>Bounds assembler frames awaiting serial dispatch, before owner queue transfer.</summary>
  public int MaximumPendingReceiveFrames { get; init; } = DefaultMaximumPendingReceiveFrames;
  public int ReceiveBufferBytes { get; init; } = DefaultReceiveBufferBytes;
  public int MaximumPendingSendBytes { get; init; } = DefaultMaximumPendingSendBytes;
  public int MaximumPendingSendFrames { get; init; } = DefaultMaximumPendingSendFrames;
  /// <summary>Bounds send calls undergoing admission, before transfer to the send queue.</summary>
  public int MaximumPendingSendRequests { get; init; } = DefaultMaximumPendingSendRequests;
  /// <summary>
  /// Capacity reserved for realtime frames. Normal traffic cannot consume this budget, so a tile
  /// backlog can never delay, reject or evict a realtime position snapshot.
  /// </summary>
  public int MaximumPendingRealtimeSendBytes { get; init; } = DefaultMaximumPendingRealtimeSendBytes;
  /// <summary>Realtime frame slots, set aside from the normal frame allowance.</summary>
  public int MaximumPendingRealtimeSendFrames { get; init; } = DefaultMaximumPendingRealtimeSendFrames;
  /// <summary>Realtime admissions in flight, set aside from the normal admission allowance.</summary>
  public int MaximumPendingRealtimeSendRequests { get; init; } =
      DefaultMaximumPendingRealtimeSendRequests;
  public TimeSpan ReceiveNoProgressDeadline { get; init; } = TimeSpan.FromSeconds(5);
  public TimeSpan SendDeadline { get; init; } = TimeSpan.FromSeconds(10);

  public void Validate()
  {
    if (MaximumReceiveBufferedBytes < PacketFrameCodec.MaximumFrameBytes)
    {
      throw new InvalidOperationException(
        "MaximumReceiveBufferedBytes must fit one maximum-size Version4 frame.");
    }

    if (ReceiveBufferBytes <= 0 || ReceiveBufferBytes > MaximumReceiveBufferedBytes)
    {
      throw new InvalidOperationException(
        "ReceiveBufferBytes must be positive and fit within the raw receive budget.");
    }

    if ((long)MaximumReceiveBufferedBytes + ReceiveBufferBytes +
        PacketFrameCodec.MaximumFrameBytes > MaximumTransportRawBytes) {
      throw new InvalidOperationException(
        "The receive buffers must fit within the shared transport raw byte budget.");
    }

    if (MaximumPendingReceiveFrames <= 0) {
      throw new InvalidOperationException("MaximumPendingReceiveFrames must be positive.");
    }

    if (MaximumPendingSendBytes < PacketFrameCodec.MaximumFrameBytes)
    {
      throw new InvalidOperationException(
        "MaximumPendingSendBytes must fit one maximum-size Version4 frame.");
    }

    if (MaximumPendingSendFrames <= 0)
    {
      throw new InvalidOperationException("MaximumPendingSendFrames must be positive.");
    }

    if (MaximumPendingSendRequests <= 0)
    {
      throw new InvalidOperationException("MaximumPendingSendRequests must be positive.");
    }

    if (MaximumPendingRealtimeSendBytes < PacketFrameCodec.MaximumFrameBytes)
    {
      throw new InvalidOperationException(
          "MaximumPendingRealtimeSendBytes must fit one maximum-size Version4 frame.");
    }

    if (MaximumPendingRealtimeSendFrames <= 0)
    {
      throw new InvalidOperationException(
          "MaximumPendingRealtimeSendFrames must be positive.");
    }

    if (MaximumPendingRealtimeSendRequests <= 0)
    {
      throw new InvalidOperationException(
          "MaximumPendingRealtimeSendRequests must be positive.");
    }

    if (ReceiveNoProgressDeadline <= TimeSpan.Zero)
    {
      throw new InvalidOperationException("ReceiveNoProgressDeadline must be positive.");
    }

    if (SendDeadline <= TimeSpan.Zero)
    {
      throw new InvalidOperationException("SendDeadline must be positive.");
    }
  }
}
