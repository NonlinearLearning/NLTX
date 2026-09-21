using System.Collections.Generic;

namespace Terraria.Network.Transport;

public sealed class NetworkBufferPoolAdapter
{
  public const int SmallBufferSize = 256;
  public const int MediumBufferSize = 1024;
  public const int LargeBufferSize = 16384;

  private readonly object _bufferLock = new();
  private readonly Queue<byte[]> _smallBufferQueue = new();
  private readonly Queue<byte[]> _mediumBufferQueue = new();
  private readonly Queue<byte[]> _largeBufferQueue = new();
  private int _smallBufferCount;
  private int _mediumBufferCount;
  private int _largeBufferCount;
  private int _customBufferCount;

  public int SmallAvailableCount
  {
    get
    {
      lock (_bufferLock)
      {
        return _smallBufferQueue.Count;
      }
    }
  }

  public int MediumAvailableCount
  {
    get
    {
      lock (_bufferLock)
      {
        return _mediumBufferQueue.Count;
      }
    }
  }

  public int LargeAvailableCount
  {
    get
    {
      lock (_bufferLock)
      {
        return _largeBufferQueue.Count;
      }
    }
  }

  public int SmallAllocatedCount
  {
    get
    {
      lock (_bufferLock)
      {
        return _smallBufferCount;
      }
    }
  }

  public int MediumAllocatedCount
  {
    get
    {
      lock (_bufferLock)
      {
        return _mediumBufferCount;
      }
    }
  }

  public int LargeAllocatedCount
  {
    get
    {
      lock (_bufferLock)
      {
        return _largeBufferCount;
      }
    }
  }

  public int CustomBufferCount
  {
    get
    {
      lock (_bufferLock)
      {
        return _customBufferCount;
      }
    }
  }

  public NetworkBufferLease Rent(int size)
  {
    if (size <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(size));
    }

    lock (_bufferLock)
    {
      NetworkBufferBucket bucket = SelectBucket(size);
      byte[] buffer = bucket switch
      {
        NetworkBufferBucket.Small => RentSmallBuffer(),
        NetworkBufferBucket.Medium => RentMediumBuffer(),
        NetworkBufferBucket.Large => RentLargeBuffer(),
        NetworkBufferBucket.Custom => RentCustomBuffer(size),
        _ => throw new InvalidOperationException("Unsupported network buffer bucket.")
      };

      return new NetworkBufferLease(this, buffer, bucket, size);
    }
  }

  public bool Return(NetworkBufferLease? lease)
  {
    if (lease is null)
    {
      return false;
    }

    lock (_bufferLock)
    {
      if (!ReferenceEquals(lease.Owner, this) || lease.IsReturned)
      {
        return false;
      }

      lease.IsReturned = true;
      switch (lease.Bucket)
      {
        case NetworkBufferBucket.Small:
          _smallBufferQueue.Enqueue(lease.Buffer);
          break;
        case NetworkBufferBucket.Medium:
          _mediumBufferQueue.Enqueue(lease.Buffer);
          break;
        case NetworkBufferBucket.Large:
          _largeBufferQueue.Enqueue(lease.Buffer);
          break;
        case NetworkBufferBucket.Custom:
          _customBufferCount--;
          break;
        default:
          throw new InvalidOperationException("Unsupported network buffer bucket.");
      }

      return true;
    }
  }

  private static NetworkBufferBucket SelectBucket(int size)
  {
    if (size <= SmallBufferSize)
    {
      return NetworkBufferBucket.Small;
    }

    if (size <= MediumBufferSize)
    {
      return NetworkBufferBucket.Medium;
    }

    return size <= LargeBufferSize
      ? NetworkBufferBucket.Large
      : NetworkBufferBucket.Custom;
  }

  private byte[] RentSmallBuffer()
  {
    if (_smallBufferQueue.Count > 0)
    {
      return _smallBufferQueue.Dequeue();
    }

    _smallBufferCount++;
    return new byte[SmallBufferSize];
  }

  private byte[] RentMediumBuffer()
  {
    if (_mediumBufferQueue.Count > 0)
    {
      return _mediumBufferQueue.Dequeue();
    }

    _mediumBufferCount++;
    return new byte[MediumBufferSize];
  }

  private byte[] RentLargeBuffer()
  {
    if (_largeBufferQueue.Count > 0)
    {
      return _largeBufferQueue.Dequeue();
    }

    _largeBufferCount++;
    return new byte[LargeBufferSize];
  }

  private byte[] RentCustomBuffer(int size)
  {
    _customBufferCount++;
    return new byte[size];
  }
}
