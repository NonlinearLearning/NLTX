using System.Buffers.Binary;
using Terraria.Network;

namespace NSSLC.Infrastructure.Network;

internal sealed class PacketFrameAssembler : IDisposable {
  private readonly byte[] _header = new byte[2];
  private readonly int _maximum;
  private readonly PacketByteBudget _local;
  private readonly PacketByteBudget _global;
  private byte[]? _frame;
  private int _count;

  public bool HasPartial => _count != 0;

  public PacketFrameAssembler(int maximum, PacketByteBudget local, PacketByteBudget global) {
    _maximum = maximum;
    _local = local;
    _global = global;
  }

  public void Append(ReadOnlySpan<byte> data, Action<byte[]> publish) {
    while (!data.IsEmpty) {
      if (_frame is null) {
        int size = Math.Min(2 - _count, data.Length);
        data[..size].CopyTo(_header.AsSpan(_count));
        _count += size;
        data = data[size..];
        if (_count < 2) {
          return;
        }
        int length = BinaryPrimitives.ReadUInt16LittleEndian(_header);
        if (length < 3 || length > _maximum) {
          throw new PacketProtocolException("InvalidFrameLength");
        }
        if (!_local.TryReserve(length)) {
          throw new PacketProtocolException("ReceiveCapacityExceeded");
        }
        if (!_global.TryReserve(length)) {
          _local.Release(length);
          throw new PacketProtocolException("GlobalCapacityExceeded");
        }
        _frame = new byte[length];
        _header.CopyTo(_frame, 0);
      }
      int amount = Math.Min(_frame.Length - _count, data.Length);
      data[..amount].CopyTo(_frame.AsSpan(_count));
      _count += amount;
      data = data[amount..];
      if (_count == _frame.Length) {
        byte[] complete = _frame;
        _frame = null;
        _count = 0;
        publish.Invoke(complete);
      }
    }
  }

  public void Release(byte[] frame) {
    _local.Release(frame.Length);
    _global.Release(frame.Length);
  }

  public void Dispose() {
    if (_frame is not null) {
      Release(_frame);
      _frame = null;
    }
    _count = 0;
  }
}
