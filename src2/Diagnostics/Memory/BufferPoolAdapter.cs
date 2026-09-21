using System.Collections.Generic;

namespace Terraria.NonAuthoritative.Diagnostics;

public sealed class BufferPoolAdapter
{
  private const int SmallBufferSize = 32;
  private const int MediumBufferSize = 256;
  private const int LargeBufferSize = 16384;
  private const int HugeBufferSize = 65536;

  private readonly object _bufferLock = new();
  private readonly Queue<CachedBufferLease> _smallBuffers = new();
  private readonly Queue<CachedBufferLease> _mediumBuffers = new();
  private readonly Queue<CachedBufferLease> _largeBuffers = new();
  private readonly Queue<CachedBufferLease> _hugeBuffers = new();

  public CachedBufferLease Request(int size)
  {
    if (size < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(size));
    }

    lock (_bufferLock)
    {
      Queue<CachedBufferLease>? queue = SelectQueue(size);
      int capacity = SelectCapacity(size);
      if (queue is null)
      {
        return new CachedBufferLease(new byte[size]);
      }

      if (queue.Count == 0)
      {
        return new CachedBufferLease(new byte[capacity]);
      }

      return queue.Dequeue().Activate();
    }
  }

  public void Recycle(CachedBufferLease buffer)
  {
    ArgumentNullException.ThrowIfNull(buffer);
    if (!buffer.IsActive)
    {
      return;
    }

    buffer.Deactivate();
    lock (_bufferLock)
    {
      Queue<CachedBufferLease>? queue = SelectQueue(buffer.Length);
      queue?.Enqueue(buffer);
    }
  }

  private Queue<CachedBufferLease>? SelectQueue(int size)
  {
    if (size <= SmallBufferSize)
    {
      return _smallBuffers;
    }

    if (size <= MediumBufferSize)
    {
      return _mediumBuffers;
    }

    if (size <= LargeBufferSize)
    {
      return _largeBuffers;
    }

    if (size <= HugeBufferSize)
    {
      return _hugeBuffers;
    }

    return null;
  }

  private static int SelectCapacity(int size)
  {
    if (size <= SmallBufferSize)
    {
      return SmallBufferSize;
    }

    if (size <= MediumBufferSize)
    {
      return MediumBufferSize;
    }

    if (size <= LargeBufferSize)
    {
      return LargeBufferSize;
    }

    return HugeBufferSize;
  }
}
