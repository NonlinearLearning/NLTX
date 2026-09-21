using System.IO;

namespace Terraria.NonAuthoritative.Diagnostics;

public sealed class CachedBufferLease
{
  private readonly MemoryStream _memoryStream;
  private bool _isActive;

  internal CachedBufferLease(byte[] data)
  {
    Data = data;
    _memoryStream = new MemoryStream(data, writable: true);
    Writer = new BinaryWriter(_memoryStream);
    Reader = new BinaryReader(_memoryStream);
    _isActive = true;
  }

  public byte[] Data { get; }

  public BinaryWriter Writer { get; }

  public BinaryReader Reader { get; }

  public int Length => Data.Length;

  public bool IsActive => _isActive;

  internal CachedBufferLease Activate()
  {
    _isActive = true;
    _memoryStream.Position = 0;
    return this;
  }

  internal void Deactivate()
  {
    if (!_isActive)
    {
      return;
    }

    _isActive = false;
    Writer.Flush();
    _memoryStream.Position = 0;
  }
}
