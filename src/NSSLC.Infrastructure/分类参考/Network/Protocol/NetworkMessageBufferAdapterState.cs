namespace Terraria.Network.Protocol;

public sealed class NetworkMessageBufferAdapterState
{
  private const int FrameLengthPrefixSize = 2;
  private const int MinimumFrameLength = 3;

  private readonly object _stateLock = new();
  private readonly byte[] _readBuffer;
  private readonly byte[] _writeBuffer;
  private int _totalData;
  private int _writeData;
  private bool _checkBytes;
  private bool _writeLocked;

  public NetworkMessageBufferAdapterState(
    int connectionSlot,
    int readBufferCapacity,
    int writeBufferCapacity)
  {
    if (readBufferCapacity <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(readBufferCapacity));
    }

    if (writeBufferCapacity <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(writeBufferCapacity));
    }

    WhoAmI = connectionSlot;
    _readBuffer = new byte[readBufferCapacity];
    _writeBuffer = new byte[writeBufferCapacity];
  }

  public bool CheckBytes
  {
    get
    {
      lock (_stateLock)
      {
        return _checkBytes;
      }
    }
  }

  public int ReadBufferCapacity => _readBuffer.Length;

  public int RemainingReadBufferLength
  {
    get
    {
      lock (_stateLock)
      {
        return _readBuffer.Length - _totalData;
      }
    }
  }

  public int TotalData
  {
    get
    {
      lock (_stateLock)
      {
        return _totalData;
      }
    }
  }

  public int WhoAmI { get; }

  public int WriteBufferCapacity => _writeBuffer.Length;

  public bool WriteLocked
  {
    get
    {
      lock (_stateLock)
      {
        return _writeLocked;
      }
    }
  }

  public bool TryAcquireWrite()
  {
    lock (_stateLock)
    {
      if (_writeLocked)
      {
        return false;
      }

      _writeLocked = true;
      return true;
    }
  }

  public bool TryAppendReadBytes(byte[] bytes)
  {
    ArgumentNullException.ThrowIfNull(bytes);

    lock (_stateLock)
    {
      if (bytes.Length > _readBuffer.Length - _totalData)
      {
        return false;
      }

      Array.Copy(bytes, 0, _readBuffer, _totalData, bytes.Length);
      _totalData += bytes.Length;
      _checkBytes = true;
      return true;
    }
  }

  public bool TryConsumeReadFrame(out byte[] frame)
  {
    lock (_stateLock)
    {
      frame = Array.Empty<byte>();
      if (!TryReadFrameLength(out int messageLength) ||
          messageLength < MinimumFrameLength ||
          messageLength > _readBuffer.Length ||
          messageLength > _totalData)
      {
        return false;
      }

      frame = new byte[messageLength];
      Array.Copy(_readBuffer, 0, frame, 0, messageLength);
      int remainingData = _totalData - messageLength;
      if (remainingData > 0)
      {
        Array.Copy(_readBuffer, messageLength, _readBuffer, 0, remainingData);
      }

      Array.Clear(_readBuffer, remainingData, messageLength);
      _totalData = remainingData;
      _checkBytes = remainingData > 0;
      return true;
    }
  }

  public bool TryPeekMessageLength(out int messageLength)
  {
    lock (_stateLock)
    {
      if (!TryReadFrameLength(out messageLength))
      {
        return false;
      }

      return messageLength >= MinimumFrameLength &&
        messageLength <= _readBuffer.Length;
    }
  }

  public bool TryWriteBytes(byte[] bytes)
  {
    ArgumentNullException.ThrowIfNull(bytes);

    lock (_stateLock)
    {
      if (bytes.Length > _writeBuffer.Length - _writeData)
      {
        return false;
      }

      Array.Copy(bytes, 0, _writeBuffer, _writeData, bytes.Length);
      _writeData += bytes.Length;
      return true;
    }
  }

  public void ReleaseWrite()
  {
    lock (_stateLock)
    {
      _writeLocked = false;
    }
  }

  public void Reset()
  {
    lock (_stateLock)
    {
      Array.Clear(_readBuffer);
      Array.Clear(_writeBuffer);
      _totalData = 0;
      _writeData = 0;
      _checkBytes = false;
      _writeLocked = false;
    }
  }

  private bool TryReadFrameLength(out int messageLength)
  {
    if (_totalData < FrameLengthPrefixSize)
    {
      messageLength = 0;
      return false;
    }

    messageLength = _readBuffer[0] | (_readBuffer[1] << 8);
    return true;
  }
}
