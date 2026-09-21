using System.Buffers.Binary;
using Terraria.Network.Transport;

namespace Terraria.Network.Protocol;

public sealed class NetworkPacketEnvelope
{
  public const int HeaderSize = 5;

  public const byte PacketMarker = 82;

  private readonly NetworkBufferPoolAdapter _bufferPool;
  private readonly NetworkBufferLease _bufferLease;
  private readonly int _payloadCapacity;
  private bool _isRecycled;
  private int _payloadLength;

  public NetworkPacketEnvelope(
    int id,
    int payloadCapacity,
    NetworkBufferPoolAdapter bufferPool)
  {
    if (id < ushort.MinValue || id > ushort.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(id));
    }

    if (payloadCapacity < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(payloadCapacity));
    }

    if (payloadCapacity > ushort.MaxValue - HeaderSize)
    {
      throw new ArgumentOutOfRangeException(nameof(payloadCapacity));
    }

    ArgumentNullException.ThrowIfNull(bufferPool);

    Id = id;
    _payloadCapacity = payloadCapacity;
    _bufferPool = bufferPool;
    Length = HeaderSize + payloadCapacity;
    _bufferLease = bufferPool.Rent(Length);

    Span<byte> header = _bufferLease.Buffer.AsSpan(0, HeaderSize);
    BinaryPrimitives.WriteUInt16LittleEndian(header, checked((ushort)Length));
    header[2] = PacketMarker;
    BinaryPrimitives.WriteUInt16LittleEndian(header[3..], checked((ushort)id));
  }

  public ReadOnlyMemory<byte> Buffer => _bufferLease.Buffer.AsMemory(0, Length);

  public int Id { get; }

  public int Length { get; private set; }

  public int PayloadLength => _payloadLength;

  public bool Recycle()
  {
    if (_isRecycled)
    {
      return false;
    }

    _isRecycled = true;
    return _bufferPool.Return(_bufferLease);
  }

  public void ShrinkToFit()
  {
    if (_isRecycled)
    {
      throw new InvalidOperationException("A recycled packet cannot be resized.");
    }

    Length = HeaderSize + _payloadLength;
    BinaryPrimitives.WriteUInt16LittleEndian(
      _bufferLease.Buffer.AsSpan(0, sizeof(ushort)),
      checked((ushort)Length));
  }

  public bool TryWritePayload(byte[] payload)
  {
    ArgumentNullException.ThrowIfNull(payload);

    if (_isRecycled || payload.Length > _payloadCapacity - _payloadLength)
    {
      return false;
    }

    payload.AsSpan().CopyTo(_bufferLease.Buffer.AsSpan(HeaderSize + _payloadLength));
    _payloadLength += payload.Length;
    return true;
  }
}
