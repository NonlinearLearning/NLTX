using System;
using System.Buffers.Binary;
using System.Collections.Generic;

namespace MinimalImplementation.Server.Protocol;

/// <summary>Assembles bounded Version4 frames from arbitrary TCP receive chunks.</summary>
public sealed class PacketFrameAssembler : IDisposable
{
  private byte[] _partialFrame = Array.Empty<byte>();
  private readonly Queue<RawPacketFrame> _completedFrames = new();
  private readonly int _maximumBufferedBytes;
  private readonly int _maximumBufferedFrames;
  private int _partialLength;
  private int _bufferedByteCount;
  private bool _disposed;

  public PacketFrameAssembler(
      int maximumBufferedBytes,
      int maximumBufferedFrames = PacketTcpConnectionOptions.DefaultMaximumPendingReceiveFrames)
  {
    if (maximumBufferedBytes < PacketFrameCodec.MaximumFrameBytes)
    {
      throw new ArgumentOutOfRangeException(
        nameof(maximumBufferedBytes),
        "The receive budget must fit one maximum-size Version4 frame.");
    }

    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumBufferedFrames);
    _maximumBufferedBytes = maximumBufferedBytes;
    _maximumBufferedFrames = maximumBufferedFrames;
  }

  public int BufferedByteCount => _bufferedByteCount;
  public bool HasPartialFrame => _partialLength != 0;

  /// <summary>Appends the whole chunk or throws when draining is needed to free capacity.</summary>
  public void Append(ReadOnlySpan<byte> bytes)
  {
    if (AppendAvailable(bytes) != bytes.Length) {
      throw new PacketFrameFormatException(
        PacketFrameErrorCode.BufferLimitExceeded,
        "The connection receive buffer exceeded its configured byte or frame budget.");
    }
  }

  /// <summary>Consumes only the prefix that fits; the caller drains frames before retrying.</summary>
  internal int AppendAvailable(ReadOnlySpan<byte> bytes) {
    ObjectDisposedException.ThrowIf(_disposed, this);
    if (_partialFrame.Length == 0 && !bytes.IsEmpty) {
      _partialFrame = new byte[PacketFrameCodec.MaximumFrameBytes];
    }

    int inputLength = bytes.Length;
    while (!bytes.IsEmpty)
    {
      if (_bufferedByteCount >= _maximumBufferedBytes ||
          _completedFrames.Count >= _maximumBufferedFrames) {
        break;
      }

      int targetFrameLength = _partialLength < PacketFrameCodec.LengthPrefixBytes
        ? PacketFrameCodec.LengthPrefixBytes
        : GetExpectedFrameLength();
      int remainingFrameBytes = targetFrameLength - _partialLength;
      int remainingBudget = _maximumBufferedBytes - _bufferedByteCount;
      int copiedBytes = Math.Min(Math.Min(remainingFrameBytes, bytes.Length), remainingBudget);
      bytes[..copiedBytes].CopyTo(_partialFrame.AsSpan(_partialLength));
      _partialLength += copiedBytes;
      _bufferedByteCount += copiedBytes;
      bytes = bytes[copiedBytes..];

      if (_partialLength >= PacketFrameCodec.LengthPrefixBytes)
      {
        int expectedFrameLength = GetExpectedFrameLength();
        if (expectedFrameLength < PacketFrameCodec.MinimumFrameBytes)
        {
          ResetPartialFrame();
          ValidateFrameLength(expectedFrameLength);
        }

        if (_partialLength == expectedFrameLength)
        {
          byte[] frameBytes = _partialFrame.AsSpan(0, expectedFrameLength).ToArray();
          _completedFrames.Enqueue(PacketFrameCodec.ParseOwnedFrame(frameBytes));
          _partialLength = 0;
        }
      }
    }

    return inputLength - bytes.Length;
  }

  /// <summary>
  /// Examines frame boundaries without allocating frames. This includes the unread suffix when
  /// admission pauses, so a trailing incomplete frame's deadline still covers callback waits.
  /// </summary>
  internal bool HasPartialFrameAfterAppend(ReadOnlySpan<byte> bytes) {
    ObjectDisposedException.ThrowIf(_disposed, this);
    if (_partialLength != 0) {
      if (bytes.IsEmpty) {
        return true;
      }

      int frameLength = _partialLength == 1
        ? _partialFrame[0] | (bytes[0] << 8)
        : GetExpectedFrameLength();
      ValidateFrameLength(frameLength);
      int remainingFrameBytes = frameLength - _partialLength;
      if (bytes.Length < remainingFrameBytes) {
        return true;
      }

      bytes = bytes[remainingFrameBytes..];
    }

    while (!bytes.IsEmpty) {
      if (bytes.Length < PacketFrameCodec.LengthPrefixBytes) {
        return true;
      }

      int frameLength = BinaryPrimitives.ReadUInt16LittleEndian(bytes);
      ValidateFrameLength(frameLength);
      if (bytes.Length < frameLength) {
        return true;
      }

      bytes = bytes[frameLength..];
    }

    return false;
  }

  public bool TryReadFrame(out RawPacketFrame? frame)
  {
    ObjectDisposedException.ThrowIf(_disposed, this);
    if (!_completedFrames.TryDequeue(out frame))
    {
      frame = null;
      return false;
    }

    _bufferedByteCount -= frame.TotalLength;
    return true;
  }

  public void Dispose()
  {
    if (_disposed)
    {
      return;
    }

    _disposed = true;
    _completedFrames.Clear();
    _partialFrame = Array.Empty<byte>();
    _partialLength = 0;
    _bufferedByteCount = 0;
  }

  private int GetExpectedFrameLength()
  {
    return BinaryPrimitives.ReadUInt16LittleEndian(_partialFrame);
  }

  private static void ValidateFrameLength(int frameLength) {
    if (frameLength < PacketFrameCodec.MinimumFrameBytes) {
      throw new PacketFrameFormatException(
        PacketFrameErrorCode.InvalidLengthPrefix,
        "A Version4 packet frame is shorter than its header.");
    }
  }

  private void ResetPartialFrame()
  {
    _bufferedByteCount -= _partialLength;
    _partialLength = 0;
  }
}
