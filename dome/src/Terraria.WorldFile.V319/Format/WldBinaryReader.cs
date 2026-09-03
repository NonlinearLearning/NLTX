using System;
using System.IO;
using System.Text;

namespace Terraria.WorldFile.V319.Format;

public sealed class WldBinaryReader : IDisposable
{
  private static readonly UTF8Encoding Utf8 = new(
    encoderShouldEmitUTF8Identifier: false,
    throwOnInvalidBytes: true);

  private readonly BinaryReader _reader;
  private readonly Stream _input;
  private readonly WldReadLimits _limits;

  public WldBinaryReader(
    Stream input,
    WldReadLimits limits,
    long sectionStart,
    long sectionEnd)
  {
    if (!input.CanRead || !input.CanSeek)
    {
      throw new InvalidDataException("The WLD input stream must be readable and seekable.");
    }

    _limits = limits ?? throw new ArgumentNullException(nameof(limits));
    _limits.Validate();
    if (input.Length < 0 || input.Length > _limits.MaxFileBytes)
    {
      throw new InvalidDataException("The WLD input stream exceeds the configured file limit.");
    }

    if (sectionStart < 0 || sectionEnd < sectionStart || sectionEnd > input.Length)
    {
      throw new InvalidDataException("The WLD section bounds are invalid.");
    }

    _input = input;
    SectionStart = sectionStart;
    SectionEnd = sectionEnd;
    _input.Position = sectionStart;
    _reader = new BinaryReader(_input, Utf8, leaveOpen: true);
  }

  public long Position => _input.Position;

  public long SectionEnd { get; }

  public long SectionStart { get; }

  public WldReadLimits Limits => _limits;

  public bool ReadBoolean()
  {
    EnsureAvailable(sizeof(byte));
    return _reader.ReadBoolean();
  }

  public byte ReadByte()
  {
    EnsureAvailable(sizeof(byte));
    return _reader.ReadByte();
  }

  public byte[] ReadBytes(int count)
  {
    if (count < 0)
    {
      throw new InvalidDataException("The WLD byte count cannot be negative.");
    }

    EnsureAvailable(count);
    byte[] bytes = _reader.ReadBytes(count);
    if (bytes.Length != count)
    {
      throw new InvalidDataException("The WLD input ended before the requested byte count.");
    }

    return bytes;
  }

  public int ReadInt32()
  {
    EnsureAvailable(sizeof(int));
    return _reader.ReadInt32();
  }

  public double ReadDouble()
  {
    EnsureAvailable(sizeof(double));
    return _reader.ReadDouble();
  }

  public long ReadInt64()
  {
    EnsureAvailable(sizeof(long));
    return _reader.ReadInt64();
  }

  public short ReadInt16()
  {
    EnsureAvailable(sizeof(short));
    return _reader.ReadInt16();
  }

  public float ReadSingle()
  {
    EnsureAvailable(sizeof(float));
    return _reader.ReadSingle();
  }

  public string ReadString()
  {
    int byteCount = Read7BitEncodedInt();
    if (byteCount > _limits.MaxStringByteLength)
    {
      throw new InvalidDataException("The WLD string exceeds the configured byte limit.");
    }

    return Utf8.GetString(ReadBytes(byteCount));
  }

  public uint ReadUInt32()
  {
    EnsureAvailable(sizeof(uint));
    return _reader.ReadUInt32();
  }

  public ulong ReadUInt64()
  {
    EnsureAvailable(sizeof(ulong));
    return _reader.ReadUInt64();
  }

  public ushort ReadUInt16()
  {
    EnsureAvailable(sizeof(ushort));
    return _reader.ReadUInt16();
  }

  public void RequireSectionEnd()
  {
    if (Position != SectionEnd)
    {
      throw new InvalidDataException("The WLD section contains trailing bytes.");
    }
  }

  public void Seek(long position)
  {
    if (position < SectionStart || position > SectionEnd)
    {
      throw new InvalidDataException("The requested position is outside the WLD section.");
    }

    _input.Position = position;
  }

  public void Dispose()
  {
    _reader.Dispose();
  }

  public WldBinaryReader CreateBoundedReader(long sectionStart, long sectionEnd)
  {
    return new WldBinaryReader(_input, _limits, sectionStart, sectionEnd);
  }

  public byte[] CopySectionBytes()
  {
    long originalPosition = Position;
    try
    {
      Seek(SectionStart);
      return ReadBytes(checked((int)(SectionEnd - SectionStart)));
    }
    finally
    {
      Seek(originalPosition);
    }
  }

  private void EnsureAvailable(int byteCount)
  {
    if (byteCount < 0 || Position > SectionEnd - byteCount)
    {
      throw new InvalidDataException("The WLD input ended inside a bounded section.");
    }
  }

  private int Read7BitEncodedInt()
  {
    uint value = 0;
    for (int shift = 0; shift < 35; shift += 7)
    {
      byte current = ReadByte();
      value |= (uint)(current & 0x7F) << shift;
      if ((current & 0x80) == 0)
      {
        if (value > int.MaxValue)
        {
          throw new InvalidDataException("The WLD string length is invalid.");
        }

        return (int)value;
      }
    }

    throw new InvalidDataException("The WLD string length is not a valid 7-bit integer.");
  }
}
